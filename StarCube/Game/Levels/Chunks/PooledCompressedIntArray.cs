using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StarCube.Game.Levels.Chunks
{
    /// <summary>
    /// 池化的可以以压缩形式存储 4096 个整数的结构
    /// </summary>
    public struct PooledCompressedIntArray : IEnumerable<int>
    {
        public static readonly ImmutableArray<int> LevelToBitSize = new ImmutableArray<int> { 0, 1, 2, 4, 8, 16, 32 };
        public static readonly ImmutableArray<int> LevelToMaxValue = new ImmutableArray<int> { 0, 1, 3, 15, byte.MaxValue, ushort.MaxValue, int.MaxValue - 1 };
        public static readonly ImmutableArray<int> LevelToDataLength = new ImmutableArray<int> { 0, 128, 256, 512, 1024, 2048, 4096 };

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
                return index != 4096;
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
            data = Array.Empty<uint>()
        };

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
        private static int GetImpl(uint[] data, int level, int index)
        {
            return level switch
            {
                0 => 0,
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


        public int Level => data.Length;

        public int MaxValue => LevelToMaxValue[level];

        public int this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(int index)
        {
            return GetImpl(data, level, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, int value)
        {
            SetImpl(data, level, index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<int> buffer)
        {
            for (int i = 0; i < 4096; i++)
            {
                buffer[i] = Get(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<int> buffer)
        {
            for (int i = 0; i < 4096; i++)
            {
                Set(i, buffer[i]);
            }
        }

        public void ExpendToLevel(int newLevel, PalettedChunkDataPool pool)
        {
            Debug.Assert(newLevel > level && newLevel <= 6, "newLevel out of range");
            uint[] newData = pool.Get(newLevel);
            if (level == 0)
            {
                newData.AsSpan().Clear();
            }
            else
            {
                for (int i = 0; i < 4096; ++i)
                {
                    int value = GetImpl(data, level, i);
                    SetImpl(newData, newLevel, i, value);
                }
            }
            pool.Release(level, data);
            level = newLevel;
            data = newData;
        }

        public void ReleaseData(PalettedChunkDataPool pool)
        {
            pool.Release(level, data);
            level = 0;
            data = Array.Empty<uint>();
        }

        public PooledCompressedIntArray Clone(PalettedChunkDataPool pool)
        {
            PooledCompressedIntArray array = new PooledCompressedIntArray
            {
                level = this.level,
                data = pool.Get(this.level)
            };
            data.AsSpan().CopyTo(array.data);
            return array;
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

        private uint[] data;
    }
}
