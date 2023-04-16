using System.Collections.Generic;
using System.Collections.Immutable;

using StarCube.Utility;

namespace StarCube.Game.Levels.Chunks.Storage.Palette
{
    /// <summary>
    /// 旧调色盘到当前调色盘的转化器
    /// </summary>
    public sealed class GlobalPaletteConverter
    {
        public int Length => data.Length;

        public bool Valid(int value)
        {
            return value >= data.Length;
        }

        /// <summary>
        /// 将一个旧调色盘的值转化为当前调色盘的值，必须保证传入值在旧调色盘中有效，如果没有对应的在当前调色盘中的值，返回默认值 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ToCurrent(int value)
        {
            return data[value];
        }

        public GlobalPaletteConverter(ImmutableArray<int> data)
        {
            this.data = data;
        }

        public readonly ImmutableArray<int> data;
    }
}
