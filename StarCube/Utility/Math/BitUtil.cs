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

        public static void Pack(int[] data, int bitCount, byte[] binary)
        {

        }

        public static void Unpack(int[] data, int bitCount, byte[] binary)
        {

        }
    }
}
