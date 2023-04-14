using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StarCube.Game.Levels.Chunks
{
    /// <summary>
    /// 可以以压缩形式存储 Chunk.ChunkSize 个整数的结构
    /// </summary>
    /// 此结构记录有一个整形 level，一个整形 singleValue，一个 uint 数组 data
    /// level 有 7 种合法取值 (0 ~ 6)，代表了数据的压缩级别，压缩级别随着 level 的增大而降低
    /// level = 0 时，data 长度为 0，认为数组中所有元素均取值 singleValue，singleValue 取值为 [0, int.MaxValue - 1]
    /// level = 1 ~ 6 时，data 长度分别为 LevelToDataLength[level]，每个元素占用 LevelToBitSize[level] 个比特位(注意 level = 6 时，实际只有低位的 31 起作用)，
    /// 每个元素取值范围为 [0, LevelToMaxValue[level]]，singleValue = -1
    public struct PooledCompressedIntArray : IEnumerable<int>
    {
        public static readonly ImmutableArray<int> LevelToBitSize = ImmutableArray.Create(0, 1, 2, 4, 8, 16, 32);
        public static readonly ImmutableArray<int> LevelToMaxValue = ImmutableArray.Create(0, 1, 3, 15, byte.MaxValue, ushort.MaxValue, int.MaxValue - 1);
        public static readonly ImmutableArray<int> LevelToDataLength = ImmutableArray.Create(0, Chunk.ChunkSize / 32, Chunk.ChunkSize / 16, Chunk.ChunkSize / 8, Chunk.ChunkSize / 4, Chunk.ChunkSize / 2, Chunk.ChunkSize);

        public struct Enumerator : IEnumerator<int>
        {
            public int Current => data.Get(index);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                index++;
                return index != Chunk.ChunkSize;
            }

            public void Reset()
            {
                index = -1;
            }

            public Enumerator(PooledCompressedIntArray data)
            {
                this.data = data;
                index = -1;
            }

            private PooledCompressedIntArray data;
            private int index;
        }

        public static readonly PooledCompressedIntArray Empty = new PooledCompressedIntArray
        {
            level = 0,
            singleValue = 0,
            data = Array.Empty<uint>()
        };

        /// <summary>
        /// 数组存储最大值为 maxValue 的数据时，所需的最小 level
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns>所需的 level</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 maxValue 超出范围 [0, int.MaxValue - 1] 时</exception>
        public static int GetRequiredLevel(int maxValue)
        {
            for (int i = 0; i < LevelToMaxValue.Length; ++i)
            {
                if (maxValue <= LevelToMaxValue[i])
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException($"PooledCompressedIntArray : can't hold {maxValue} values");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetImpl(uint[] data, int level, int singleValue, int index)
        {
            return level switch
            {
                0 => singleValue,
                1 => (int)(data[index >> 5] >> (index & 31)) & 0x1,
                2 => (int)(data[index >> 4] >> ((index & 15) << 1)) & 0x3,
                3 => (int)(data[index >> 3] >> ((index & 7) << 2)) & 0xF,
                4 => (int)(data[index >> 2] >> ((index & 3) << 3)) & 0xFF,
                5 => (int)(data[index >> 1] >> ((index & 1) << 4)) & 0xFFFF,
                6 => (int)data[index],
                _ => -1,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetImpl(uint[] data, int level, int index, int value)
        {
            switch (level)
            {
            case 1:
            {
                uint oldPacked = data[index >> 5];
                uint clearMask = ~(0x1u << (index & 31));
                uint valueMask = ((uint)value & 0x1u) << (index & 31);
                uint newPacked = oldPacked & clearMask | valueMask;
                data[index >> 5] = newPacked;
            }
            break;
            case 2:
            {
                uint oldPacked = data[index >> 4];
                uint clearMask = ~(0x3u << ((index & 15) << 1));
                uint valueMask = ((uint)value & 0x3u) << ((index & 15) << 1);
                uint newPacked = oldPacked & clearMask | valueMask;
                data[index >> 4] = newPacked;
            }
            break;
            case 3:
            {
                uint oldPacked = data[index >> 3];
                uint clearMask = ~(0xFu << ((index & 7) << 2));
                uint valueMask = ((uint)value & 0x7u) << ((index & 7) << 2);
                uint newPacked = oldPacked & clearMask | valueMask;
                data[index >> 3] = newPacked;
            }
            break;
            case 4:
            {
                uint oldPacked = data[index >> 2];
                uint clearMask = ~(0xFFu << ((index & 3) << 4));
                uint valueMask = ((uint)value & 0xFFu) << ((index & 3) << 4);
                uint newPacked = oldPacked & clearMask | valueMask;
                data[index >> 2] = newPacked;
            }
            break;
            case 5:
            {
                uint oldPacked = data[index >> 1];
                uint clearMask = ~(0xFFFFu << ((index & 1) << 16));
                uint valueMask = ((uint)value & 0xFFFFu) << ((index & 1) << 16);
                uint newPacked = oldPacked & clearMask | valueMask;
                data[index >> 1] = newPacked;
            }
            break;
            case 6:
                data[index] = (uint)value;
                break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillImpl(uint[] data, int level, int value)
        {
            for (int i = 0; i < (32 / LevelToBitSize[level]); ++i)
            {
                SetImpl(data, level, i, value);
            }
            uint fillValue = data[0];
            data.AsSpan().Fill(fillValue);
        }


        public int Level => data.Length;

        public int SingleValue => singleValue;

        public int MaxValue => LevelToMaxValue[level];

        public int this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        /// <summary>
        /// 获取数组指定下标的值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(int index)
        {
            return GetImpl(data, level, singleValue, index);
        }

        /// <summary>
        /// 设置数组指定下标的值，这将导致值被截断
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, int value)
        {
            SetImpl(data, level, index, value);
        }

        /// <summary>
        /// 将数组中的值复制到 buffer 中
        /// </summary>
        /// <param name="buffer"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<int> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            if (level == 0)
            {
                buffer.Fill(singleValue);
                return;
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Get(i);
            }
        }

        /// <summary>
        /// 从 buffer 中复制值到数组中，这将导致值被截断
        /// </summary>
        /// <param name="buffer"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<int> buffer)
        {
            Debug.Assert(buffer.Length == Chunk.ChunkSize);

            if (level == 0)
            {
                return;
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                Set(i, buffer[i]);
            }
        }

        /// <summary>
        /// 使用值填充数组，这将导致值被截断
        /// </summary>
        /// <param name="value"></param>
        public void Fill(int value)
        {
            if (level == 0)
            {
                singleValue = value;
                return;
            }

            FillImpl(data, level, value);
        }

        /// <summary>
        /// 将数据清空为全 0
        /// </summary>
        public void Clear()
        {
            if (level == 0)
            {
                singleValue = 0;
                return;
            }

            data.AsSpan().Clear();
        }

        /// <summary>
        /// 将数组扩张到指定 level 并复制原有数据，注意原 level 为 0 时原有数据可能会被截断
        /// </summary>
        /// <param name="newLevel"></param>
        /// <param name="pool"></param>
        public void ExpandToLevel(int newLevel, PalettedChunkDataPool pool)
        {
            Debug.Assert(newLevel > level && newLevel <= 6, "newLevel out of range");

            uint[] newData = pool.Get(newLevel);
            // 原 level 是 0，使用原 singleValue 填充新缓冲区，这可能会导致数据被截断
            if (level == 0)
            {
                FillImpl(newData, level, singleValue);
            }
            // 原 level 不是 0，复制原缓冲区中的数据至新缓冲区
            else
            {
                for (int i = 0; i < Chunk.ChunkSize; ++i)
                {
                    int value = GetImpl(data, level, singleValue, i);
                    SetImpl(newData, newLevel, i, value);
                }
            }
            // 释放原数据缓冲区
            pool.Release(level, data);
            // 设置新值
            level = newLevel;
            singleValue = -1;
            data = newData;
        }

        /// <summary>
        /// 将数组扩张到指定 level，使用一个线性调色盘转换并复制原有数据
        /// </summary>
        /// <param name="newLevel"></param>
        /// <param name="pool"></param>
        /// <param name="linearPalette"></param>
        public void ExpandToLevelWithLinearPalette(int newLevel, PalettedChunkDataPool pool, List<int> linearPalette)
        {
            Debug.Assert(newLevel > level && newLevel <= 6, "newLevel out of range");

            uint[] newData = pool.Get(newLevel);
            for (int i = 0; i < Chunk.ChunkSize; ++i)
            {
                int value = GetImpl(data, level, singleValue, i);
                SetImpl(newData, newLevel, i, linearPalette[value]);
            }
            // 释放原数据缓冲区
            pool.Release(level, data);
            // 设置新值
            level = newLevel;
            singleValue = -1;
            data = newData;
        }

        /// <summary>
        /// 将数组扩张到指定 level 并丢弃原有数据
        /// </summary>
        /// <param name="newLevel"></param>
        /// <param name="pool"></param>
        public void ExpandToLevelWithoutData(int newLevel, PalettedChunkDataPool pool)
        {
            Debug.Assert(newLevel > level && newLevel <= 6, "newLevel out of range");

            pool.Release(level, data);
            level = newLevel;
            singleValue = -1;
            data = pool.Get(newLevel);
        }

        /// <summary>
        /// 将 level 置为 0 并释放数据缓冲区
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="singleValue"></param>
        public void Release(PalettedChunkDataPool pool, int singleValue = 0)
        {
            pool.Release(level, data);
            level = 0;
            this.singleValue = singleValue;
            data = Array.Empty<uint>();
        }

        /// <summary>
        /// 获取一份深拷贝
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public PooledCompressedIntArray Clone(PalettedChunkDataPool pool)
        {
            PooledCompressedIntArray clone = new PooledCompressedIntArray
            {
                level = this.level,
                singleValue = this.singleValue,
                data = pool.Get(this.level)
            };
            data.AsSpan().CopyTo(clone.data);
            return clone;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int level;

        private int singleValue;

        private uint[] data;
    }
}
