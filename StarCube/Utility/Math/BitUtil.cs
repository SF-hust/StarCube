﻿using System;
using System.Diagnostics;

namespace StarCube.Utility.Math
{
    public static class BitUtil
    {
        /// <summary>
        /// 计算存储一个 int 整数所需的最小二进制位数，value 必须大于等于 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int BitCount(int value)
        {
            Debug.Assert(value >= 0);

            int count = 0;
            while(value > 0)
            {
                count++;
                value >>= 1;
            }
            return count;
        }

        /// <summary>
        /// 将 data 中每个整数压缩打包为 binary，data 中每个元素在打包后数据中占 bitCount 个二进制位
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binary"></param>
        /// <param name="bitCount"></param>
        public static void Pack(Span<int> data, Span<byte> binary, int bitCount)
        {
            Debug.Assert(bitCount > 0 && bitCount < 32);
            Debug.Assert(data.Length * bitCount <= binary.Length * sizeof(byte));

            int byteIndex = 0;
            int bitIndex = 0;
            byte b = 0;
            for(int i = 0; i < data.Length; ++i)
            {
                for(int j = 0; j < bitCount; ++j)
                {
                    b |= unchecked((byte)(((data[i] >> j) & 1) << bitIndex));

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

        /// <summary>
        /// 将 binary 中前 data.Length * bitCount 个二进制位，每 bitCount 个二进制位解压缩为一个 int，并存储到 data 中
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binary"></param>
        /// <param name="bitCount"></param>
        public static void Unpack(Span<int> data, Span<byte> binary, int bitCount)
        {
            Debug.Assert(bitCount > 0 && bitCount < 32);
            Debug.Assert(data.Length * bitCount <= binary.Length * sizeof(byte));

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

        /// <summary>
        /// 使用 ZigZag 编码将一个 int32 编码为 uint32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint ZigZagEncode(int value)
        {
            return unchecked((uint)((value << 1) ^ (value >> 31)));
        }

        /// <summary>
        /// 使用 Zigzag 编码将一个 int64 编码为 uint64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong ZigZagEncode(long value)
        {
            return unchecked((ulong)((value << 1) ^ (value >> 63)));
        }

        /// <summary>
        /// 使用 ZigZag 编码将一个 uint32 解码为 int32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ZigZagDecode(uint value)
        {
            return unchecked((int)((value >> 1) ^ (uint)-(int)(value & 1U)));
        }

        /// <summary>
        /// 使用 ZigZag 编码将一个 uint64 解码为 int64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ZigZagDecode(ulong value)
        {
            return unchecked((long)((value >> 1) ^ (ulong)-(long)(value & 1LU)));
        }
    }
}
