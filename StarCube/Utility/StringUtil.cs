using System;
using System.Text;

namespace StarCube.Utility
{
    public static class StringUtil
    {
        /// <summary>
        /// 获取一个 ThreadLocal 的全局静态空 StringBuilder，
        /// 警告 : 不要在使用完毕此 StringBuilder 之前调用其他使用此属性的方法
        /// </summary>
        public static StringBuilder StringBuilder => stringBuilder.Clear();

        [ThreadStatic]
        private static readonly StringBuilder stringBuilder = new StringBuilder(64);

        /// <summary>
        /// 最简单版本的 IndexOf
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int SimpleIndexOf(this string str, char c)
        {
            for(int i = 0; i < str.Length; ++i)
            {
                if(str[i] == c)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 最简单版本的 IndexOf，可指定起始位置和长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int SimpleIndexOf(this string str, char c, int start, int length)
        {
            for (int i = start; i < start + length; ++i)
            {
                if (str[i] == c)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
