using System;
using StarCube.Utility.Math;

namespace StarCube.Game.Levels.Chunks.Storage
{
    public static class ChunkStorage
    {
        public static bool TryDecodeBlockStates(byte[] binary, int blockBitSize, out int[] blockStates)
        {
            blockStates = Array.Empty<int>();
            if(binary.Length * sizeof(byte) < 4096 * blockBitSize)
            {
                return false;
            }

            blockStates = new int[4096];
            BitUtil.Unpack(blockStates, binary, blockBitSize);

            return true;
        }

        public static byte[] EncodeBlockStates(int[] blockStates, out int blockBitSize)
        {
            int max = 0;
            foreach (int b in blockStates)
            {
                max = Math.Max(max, b);
            }

            blockBitSize = BitUtil.BitCount(max);
            byte[] binary = new byte[4096 / sizeof(byte) * blockBitSize];
            BitUtil.Pack(blockStates, binary, blockBitSize);

            return binary;
        }
    }
}
