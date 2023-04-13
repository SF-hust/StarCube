using System;
using System.Diagnostics;

using StarCube.Utility.Pools;

namespace StarCube.Game.Levels.Chunks
{
    public sealed class PalettedChunkDataPool
    {
        public uint[] Get(int level)
        {
            Debug.Assert(level >= 0 && level <= 6);

            return level switch
            {
                1 => poolLevel1.Get(),
                2 => poolLevel2.Get(),
                3 => poolLevel3.Get(),
                4 => poolLevel4.Get(),
                5 => poolLevel5.Get(),
                6 => poolLevel6.Get(),
                _ => Array.Empty<uint>()
            };
        }

        public void Release(int level, uint[] array)
        {
            Debug.Assert(level >= 0 && level <= 6);

            switch (level)
            {
            case 1:
                poolLevel1.Release(array);
                break;
            case 2:
                poolLevel2.Release(array);
                break;
            case 3:
                poolLevel3.Release(array);
                break;
            case 4:
                poolLevel4.Release(array);
                break;
            case 5:
                poolLevel5.Release(array);
                break;
            case 6:
                poolLevel6.Release(array);
                break;
            }
        }

        public void Resize(int level, int newSize)
        {
            Debug.Assert(level >= 1 && level <= 6);

            switch (level)
            {
            case 1:
                poolLevel1.Resize(newSize);
                break;
            case 2:
                poolLevel2.Resize(newSize);
                break;
            case 3:
                poolLevel3.Resize(newSize);
                break;
            case 4:
                poolLevel4.Resize(newSize);
                break;
            case 5:
                poolLevel5.Resize(newSize);
                break;
            case 6:
                poolLevel6.Resize(newSize);
                break;
            }
        }

        public PalettedChunkDataPool(int sizeLevel1, int sizeLevel2, int sizeLevel3, int sizeLevel4, int sizeLevel5, int sizeLevel6, int sizeLinearPalette)
        {
            poolLevel1 = new ConcurrentFixedArrayPool<uint>(sizeLevel1, 128);
            poolLevel2 = new ConcurrentFixedArrayPool<uint>(sizeLevel2, 256);
            poolLevel3 = new ConcurrentFixedArrayPool<uint>(sizeLevel3, 512);
            poolLevel4 = new ConcurrentFixedArrayPool<uint>(sizeLevel4, 1024);
            poolLevel5 = new ConcurrentFixedArrayPool<uint>(sizeLevel5, 2048);
            poolLevel6 = new ConcurrentFixedArrayPool<uint>(sizeLevel6, 4096);
            linearPalettePool = new ConcurrentFixedListPool<int>(sizeLinearPalette, 16);
        }

        public readonly ConcurrentFixedArrayPool<uint> poolLevel1;
        public readonly ConcurrentFixedArrayPool<uint> poolLevel2;
        public readonly ConcurrentFixedArrayPool<uint> poolLevel3;
        public readonly ConcurrentFixedArrayPool<uint> poolLevel4;
        public readonly ConcurrentFixedArrayPool<uint> poolLevel5;
        public readonly ConcurrentFixedArrayPool<uint> poolLevel6;

        public readonly ConcurrentFixedListPool<int> linearPalettePool;
    }
}
