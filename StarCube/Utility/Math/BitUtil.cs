using System;
using System.Collections.Generic;
using System.Text;

namespace StarCube.Utility.Math
{
    public static class BitUtil
    {
        public static int BitCount(int value)
        {
            int count = 0;
            while(value > 0)
            {
                count++;
                value >>= 1;
            }
            return count;
        }

        public static void Pack(int[] data, byte[] binary, int bitCount)
        {
            int byteIndex = 0;
            int bitIndex = 0;
            byte b = 0;
            for(int i = 0; i < data.Length; ++i)
            {
                for(int j = 0; j < bitCount; ++j)
                {
                    b |= (byte)(((data[i] >> j) & 1) << bitIndex);

                    bitIndex++;
                    if (bitIndex == 8)
                    {
                        byteIndex += 1;
                        bitIndex = 0;
                        binary[byteIndex] = b;
                        b = 0;
                    }
                }
            }
        }

        public static void Unpack(int[] data, byte[] binary, int bitCount)
        {
            int byteIndex = 0;
            int bitIndex = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                int val = 0;
                for (int j = 0; j < bitCount; ++j)
                {
                    val |= binary[byteIndex] >> bitIndex;

                    bitIndex++;
                    bitIndex += bitIndex / 8;
                    bitIndex %= 8;
                }
                data[i] = val;
            }
        }
    }
}
