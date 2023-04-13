using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using StarCube.Utility;
using StarCube.Utility.Container;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunkData<T>
        where T : class, IIntegerID
    {
        public const int MaxLinearPaletteSize = 16;

        private static readonly ThreadLocal<Dictionary<int, int>> ThreadLocalDictionary = new ThreadLocal<Dictionary<int, int>>(() => new Dictionary<int, int>());

        public int Level => compressedData.Level;

        public void CopyTo(T[] array)
        {
            // 使用全局调色盘
            if (linearPalette == null)
            {
                for (int i = 0; i < 4096; ++i)
                {
                    array[i] = globalPalette.ValueFor(compressedData[i]);
                }
                return;
            }

            // 使用线性调色盘
            for (int i = 0; i < 4096; ++i)
            {
                array[i] = globalPalette.ValueFor(linearPalette[compressedData[i]]);
            }
        }

        public void CopyRawTo(Span<int> buffer)
        {
            // 使用全局调色盘
            if (linearPalette == null)
            {
                compressedData.CopyTo(buffer);
                return;
            }

            // 使用线性调色盘
            for (int i = 0; i < 4096; ++i)
            {
                buffer[i] = linearPalette[compressedData[i]];
            }
        }

        public void CopyFrom(T[] array)
        {
            // 清空数据
            Clear();

            // 收集 buffer 中值的种类
            Dictionary<int, int> dict = ThreadLocalDictionary.Value;
            dict.Clear();
            dict.Add(0, 0);
            foreach (T value in array)
            {
                dict.TryAdd(value.IntegerID, dict.Count);
            }
            int valueCount = dict.Count;

            // 全空
            if (valueCount == 1)
            {
                return;
            }

            // 使用线性调色盘
            if (valueCount <= 16)
            {
                linearPalette = pool.linearPalettePool.Get();
                compressedData.ExpendToLevel(3, pool);
                for (int i = 0; i < valueCount; ++i)
                {
                    linearPalette.Add(0);
                }
                foreach (var pair in dict)
                {
                    linearPalette[pair.Value] = pair.Key;
                }
                for (int i = 0; i < 4096; ++i)
                {
                    compressedData[i] = dict[array[i].IntegerID];
                }
                return;
            }

            // 使用全局调色盘
            int level = PooledCompressedIntArray.GetRequiredLevel(valueCount);
            compressedData.ExpendToLevel(level, pool);
            for (int i = 0; i < 4096; ++i)
            {
                compressedData[i] = array[i].IntegerID;
            }
        }

        public void CopyRawFrom(ReadOnlySpan<int> buffer)
        {
            Clear();
            Dictionary<int, int> dict = ThreadLocalDictionary.Value;
            dict.Clear();
            // 收集 buffer 中值的种类
            dict.Add(0, 0);
            foreach (int i in buffer)
            {
                dict.TryAdd(i, dict.Count);
            }
            int valueCount = dict.Count;

            // 全空
            if (valueCount == 1)
            {
                return;
            }

            // 使用线性调色盘
            if (valueCount <= 16)
            {
                linearPalette = pool.linearPalettePool.Get();
                compressedData.ExpendToLevel(3, pool);
                for (int i = 0; i < valueCount; ++i)
                {
                    linearPalette.Add(0);
                }
                foreach (var pair in dict)
                {
                    linearPalette[pair.Value] = pair.Key;
                }
                for (int i = 0; i < 4096; ++i)
                {
                    compressedData[i] = dict[buffer[i]];
                }
                return;
            }

            // 使用全局调色盘
            int level = PooledCompressedIntArray.GetRequiredLevel(valueCount);
            compressedData.ExpendToLevel(level, pool);
            compressedData.CopyFrom(buffer);
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
            // 一开始数据为空
            if (compressedData.Level == 0)
            {
                // 不改变内容，什么也不做
                if (value == 0)
                {
                    return;
                }

                // 扩张调色盘容量至 16
                compressedData.ExpendToLevel(3, pool);
                linearPalette = pool.linearPalettePool.Get();
                linearPalette.Add(0);
                linearPalette.Add(value);
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

                // 要设置的值不在线性调色盘中，线性调色盘还可以继续扩张
                if (linearPalette.Count != MaxLinearPaletteSize)
                {
                    linearPalette.Add(value);
                    compressedData[index] = linearPalette.Count;
                    return;
                }

                // 要设置的值不在线性调色盘中，线性调色盘不可以继续扩张，改用全局调色盘
                int level = PooledCompressedIntArray.GetRequiredLevel(globalPalette.Count - 1);
                compressedData.ExpendToLevel(level, pool);
                for (int i = 0; i < 4096; ++i)
                {
                    int v = compressedData[i];
                    compressedData[i] = linearPalette[v];
                }
                pool.linearPalettePool.Release(linearPalette);
                linearPalette = null;
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

        public void Clear()
        {
            compressedData.ReleaseData(pool);
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
