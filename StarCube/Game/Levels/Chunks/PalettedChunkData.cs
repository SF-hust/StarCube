using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunkData
    {
        public const int MaxLinearPaletteSize = 16;

        public static readonly ThreadLocal<Dictionary<int, int>> ThreadLocalDictionary = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>());

        /// <summary>
        /// 内部数组的 level
        /// </summary>
        public int Level => compressedData.Level;

        /// <summary>
        /// 整个数据是否只有一个取值
        /// </summary>
        /// 注意，即使此属性为 false，数据也有可能只有一个取值
        public bool Single => compressedData.Level == 0;

        /// <summary>
        /// 当整个数据只有一个取值时，返回这个取值
        /// </summary>
        public int SingleValue => compressedData.SingleValue;

        /// <summary>
        /// 获得指定下标的值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int Get(int index)
        {
            if (linearPalette == null)
            {
                return compressedData[index];
            }

            return linearPalette[compressedData[index]];
        }

        /// <summary>
        /// 设置指定下标的值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="pool"></param>
        public void Set(int index, int value, PalettedChunkDataPool pool)
        {
            // 一开始数据为 single
            if (Single)
            {
                // 不改变内容，什么也不做
                if (value == SingleValue)
                {
                    return;
                }

                int singleValue = SingleValue;
                // 扩张数组至 Level 3，此时可以存储 16 种不同的值
                compressedData.ExpandToLevelWithoutData(3, pool);
                // 构建 linear palette
                linearPalette = pool.linearPalettePool.Get();
                linearPalette.Add(singleValue);
                linearPalette.Add(value);
                // 填充数据
                compressedData.Clear();
                // 设置新值
                compressedData[index] = 1;
                return;
            }

            // 正在使用线性调色盘
            if (linearPalette != null)
            {
                int indexInLinearPalette = linearPalette.IndexOf(value);
                // 要设置的值在线性调色盘中，无需扩张
                if (indexInLinearPalette != -1)
                {
                    compressedData[index] = indexInLinearPalette;
                    return;
                }

                // 要设置的值不在线性调色盘中，且线性调色盘还可以继续扩张
                if (linearPalette.Count != MaxLinearPaletteSize)
                {
                    // 扩张线性调色盘
                    linearPalette.Add(value);
                    // 设置值
                    compressedData[index] = linearPalette.Count;
                    return;
                }

                // 要设置的值不在线性调色盘中，线性调色盘不可以继续扩张，改用全局调色盘
                int level = CompressedIntArray.GetRequiredLevel(globalPaletteSize - 1);
                compressedData.ExpandToLevelWithLinearPalette(level, pool, linearPalette);
                // 释放线性调色盘
                pool.linearPalettePool.Release(linearPalette);
                linearPalette = null;
                // 设置值
                compressedData[index] = value;
                return;
            }

            // 正在使用全局调色盘，直接设置值
            compressedData[index] = value;
        }

        /// <summary>
        /// 将数据解压并复制到 buffer 中，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyTo(Span<int> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            // 使用全局调色盘
            if (linearPalette == null)
            {
                compressedData.CopyTo(buffer);
                return;
            }

            // 使用线性调色盘
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = linearPalette[compressedData[i]];
            }
        }

        /// <summary>
        /// 从 buffer 中压缩并复制原始数据，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyFrom(ReadOnlySpan<int> buffer, PalettedChunkDataPool pool)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            // 收集 buffer 中值的种类
            Dictionary<int, int> dict = ThreadLocalDictionary.Value;
            dict.Clear();
            foreach (int i in buffer)
            {
                dict.TryAdd(i, dict.Count);
            }
            int valueCount = dict.Count;

            // buffer 中的数据只有一种取值，用这个值填充整个数据
            if (valueCount == 1)
            {
                Clear(buffer[0], pool);
                return;
            }

            // buffer 中的数据种类在 [2, 16] 中，使用线性调色盘
            if (valueCount <= 16)
            {
                // 原来的数据没有使用线性调色盘，先返还原本的数据缓冲区再重新申请
                if (linearPalette == null)
                {
                    Clear(0, pool);
                    linearPalette = pool.linearPalettePool.Get();
                    compressedData.ExpandToLevelWithoutData(3, pool);
                }
                // 原来的数据已经在使用线性调色盘，仅清空线性调色盘即可
                linearPalette.Clear();
                // 建立新的线性调色盘
                for (int i = 0; i < valueCount; ++i)
                {
                    linearPalette.Add(0);
                }
                foreach (var pair in dict)
                {
                    linearPalette[pair.Value] = pair.Key;
                }
                // 复制数据
                for (int i = 0; i < buffer.Length; ++i)
                {
                    compressedData[i] = dict[buffer[i]];
                }
                return;
            }

            // buffer 中的数据种类超过了 16 种，使用全局调色盘

            // 原本的数据没有使用全局调色盘，重新建立
            if (linearPalette != null || compressedData.Level == 0)
            {
                Clear(0, pool);
                int level = CompressedIntArray.GetRequiredLevel(globalPaletteSize - 1);
                compressedData.ExpandToLevelWithoutData(level, pool);
            }
            // 复制数据
            compressedData.CopyFrom(buffer);
        }

        /// <summary>
        /// 压缩数据，具体实现是先将数据解压，再重新压缩
        /// </summary>
        public void Compress(PalettedChunkDataPool pool)
        {
            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            CopyTo(buffer);
            CopyFrom(buffer, pool);
        }

        /// <summary>
        /// 获取自身的深拷贝
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public PalettedChunkData Clone(PalettedChunkDataPool pool)
        {
            PalettedChunkData clone = new PalettedChunkData(globalPaletteSize);
            if (linearPalette != null)
            {
                clone.linearPalette = pool.linearPalettePool.Get();
                clone.linearPalette.AddRange(linearPalette);
            }
            clone.compressedData = compressedData.Clone(pool);
            return clone;
        }

        /// <summary>
        /// 填充为单一值
        /// </summary>
        /// <param name="singleValue"></param>
        public void Clear(int singleValue, PalettedChunkDataPool pool)
        {
            compressedData.Release(pool, singleValue);
            if (linearPalette != null)
            {
                pool.linearPalettePool.Release(linearPalette);
                linearPalette = null;
            }
        }


        public PalettedChunkData(int globalPaletteSize)
        {
            this.globalPaletteSize = globalPaletteSize;
            linearPalette = null;
            compressedData = CompressedIntArray.Empty;
        }

        private readonly int globalPaletteSize;

        private List<int>? linearPalette;

        private CompressedIntArray compressedData;
    }

    public readonly ref struct PalettedChunkDataView
    {
        public int Level => rawData.level;

        public bool Single => rawData.level == 0;

        public int SingleValue => rawData.singleValue;

        public int Get(int index)
        {
            if (linearPalette == null)
            {
                return rawData.Get(index);
            }

            return linearPalette[rawData.Get(index)];
        }

        public void CopyTo(Span<int> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            // 使用全局调色盘
            if (linearPalette == null)
            {
                rawData.CopyTo(buffer);
                return;
            }

            // 使用线性调色盘
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = linearPalette[rawData.Get(i)];
            }
        }

        internal PalettedChunkDataView(CompressedIntArrayView rawData, List<int>? linearPalette)
        {
            this.rawData = rawData;
            this.linearPalette = linearPalette;
        }

        private readonly CompressedIntArrayView rawData;

        private readonly List<int>? linearPalette;
    }
}
