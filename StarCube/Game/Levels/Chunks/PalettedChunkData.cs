using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using StarCube.Utility;
using StarCube.Utility.Container;

namespace StarCube.Game.Levels.Chunks
{
    /// <summary>
    /// 将这个类拆开仅仅是为了让 static 成员只有一份
    /// </summary>
    public abstract class PalettedChunkData
    {
        public const int MaxLinearPaletteSize = 16;

        protected static readonly ThreadLocal<Dictionary<int, int>> ThreadLocalDictionary = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>());
    }

    public sealed class PalettedChunkData<T> : PalettedChunkData
        where T : class, IIntegerID
    {
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
        public T SingleValue => globalPalette.ValueFor(RawSingleValue);

        /// <summary>
        /// 当整个数据只有一个取值时，返回这个取值的原始数据
        /// </summary>
        public int RawSingleValue => compressedData.SingleValue;

        /// <summary>
        /// 将数据解压并输出到 buffer 中，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyTo(Span<T> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            // 使用全局调色盘
            if (linearPalette == null)
            {
                for (int i = 0; i < buffer.Length; ++i)
                {
                    buffer[i] = globalPalette.ValueFor(compressedData[i]);
                }
                return;
            }

            // 使用线性调色盘
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = globalPalette.ValueFor(linearPalette[compressedData[i]]);
            }
        }

        /// <summary>
        /// 将原始的整数数据解压并输出到 buffer 中，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyRawTo(Span<int> buffer)
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
        /// 从 buffer 中拷贝并压缩数据，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyFrom(ReadOnlySpan<T> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            // 收集 buffer 中值的种类
            Dictionary<int, int> dict = ThreadLocalDictionary.Value;
            dict.Clear();
            foreach (T value in buffer)
            {
                dict.TryAdd(value.IntegerID, dict.Count);
            }
            int valueCount = dict.Count;

            // buffer 中的数据只有一种取值，用这个值填充整个数据
            if (valueCount == 1)
            {
                Clear(buffer[0].IntegerID);
                return;
            }

            // buffer 中的数据种类在 [2, 16] 中，使用线性调色盘
            if (valueCount <= 16)
            {
                // 原来的数据没有使用线性调色盘，先返还原本的数据缓冲区再重新申请
                if (linearPalette == null)
                {
                    Clear();
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
                    compressedData[i] = dict[buffer[i].IntegerID];
                }
                return;
            }

            // buffer 中的数据种类超过了 16 种，使用全局调色盘

            // 原本的数据没有使用全局调色盘，重新建立
            if (linearPalette != null || compressedData.Level == 0)
            {
                Clear();
                int level = PooledCompressedIntArray.GetRequiredLevel(globalPalette.Count - 1);
                compressedData.ExpandToLevelWithoutData(level, pool);
            }
            // 复制数据
            for (int i = 0; i < buffer.Length; ++i)
            {
                compressedData.Set(i, buffer[i].IntegerID);
            }
        }

        /// <summary>
        /// 从 buffer 中拷贝并压缩原始数据，buffer 的长度必须等于 Chunk.ChunkSize
        /// </summary>
        /// <param name="buffer"></param>
        public void CopyRawFrom(ReadOnlySpan<int> buffer)
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
                Clear(buffer[0]);
                return;
            }

            // buffer 中的数据种类在 [2, 16] 中，使用线性调色盘
            if (valueCount <= 16)
            {
                // 原来的数据没有使用线性调色盘，先返还原本的数据缓冲区再重新申请
                if (linearPalette == null)
                {
                    Clear();
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
                Clear();
                int level = PooledCompressedIntArray.GetRequiredLevel(globalPalette.Count - 1);
                compressedData.ExpandToLevelWithoutData(level, pool);
            }
            // 复制数据
            compressedData.CopyFrom(buffer);
        }

        /// <summary>
        /// 压缩数据，具体实现是先将数据解压，再重新压缩
        /// </summary>
        public void Compress()
        {
            int[] buffer = Chunk.ThreadLocalChunkDataBuffer.Value;
            CopyRawTo(buffer);
            CopyRawFrom(buffer);
        }

        public T Get(int index)
        {
            return globalPalette.ValueFor(GetRaw(index));
        }

        public int GetRaw(int index)
        {
            if (linearPalette == null)
            {
                return compressedData[index];
            }

            return linearPalette[compressedData[index]];
        }

        public void Set(int index, T value)
        {
            SetRaw(index, value.IntegerID);
        }

        public void SetRaw(int index, int value)
        {
            // 一开始数据为 single
            if (Single)
            {
                // 不改变内容，什么也不做
                if (value == RawSingleValue)
                {
                    return;
                }

                int singleValue = RawSingleValue;
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
                int level = PooledCompressedIntArray.GetRequiredLevel(globalPalette.Count - 1);
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
        /// 获取一份深拷贝
        /// </summary>
        /// <returns></returns>
        public PalettedChunkData<T> Clone()
        {
            PalettedChunkData<T> clone = new PalettedChunkData<T>(globalPalette, pool);
            if (linearPalette != null)
            {
                clone.linearPalette = pool.linearPalettePool.Get();
                clone.linearPalette.AddRange(linearPalette);
            }
            clone.compressedData = compressedData.Clone(pool);
            return clone;
        }

        /// <summary>
        /// 将整份数据填充为单一值
        /// </summary>
        /// <param name="singleValue"></param>
        public void Clear(int singleValue = 0)
        {
            compressedData.Release(pool, singleValue);
            if (linearPalette != null)
            {
                pool.linearPalettePool.Release(linearPalette);
                linearPalette = null;
            }
        }

        public PalettedChunkData(IIDMap<T> globalPalette, PalettedChunkDataPool pool)
        {
            this.globalPalette = globalPalette;
            this.pool = pool;
            linearPalette = null;
            compressedData = PooledCompressedIntArray.Empty;
        }

        public readonly IIDMap<T> globalPalette;

        public readonly PalettedChunkDataPool pool;

        private List<int>? linearPalette;

        private PooledCompressedIntArray compressedData;
    }
}
