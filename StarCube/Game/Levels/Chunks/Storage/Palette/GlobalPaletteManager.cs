using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Utility.Logging;

namespace StarCube.Game.Levels.Chunks.Storage.Palette
{
    public sealed class GlobalPaletteManager<T>
        where T : class, IIntegerID
    {
        /// <summary>
        /// 当前正在使用的全局调色盘的 id
        /// </summary>
        public int CurrentGlobalPaletteID => currentGlobalPaletteID >= 0 ? currentGlobalPaletteID : throw new InvalidOperationException("currentGlobalPaletteID is unknown");

        /// <summary>
        /// 加载所有的调色盘
        /// </summary>
        /// <param name="collection"></param>
        public void LoadAll(ILiteCollection<BsonDocument> collection)
        {
            if (collection.AutoId != BsonAutoId.Int32)
            {
                throw new ArgumentException("palette collections must have int32 auto id");
            }

            if (idToMapper.Count != 0)
            {
                throw new InvalidOperationException("can't call LoadAll() twice");
            }

            // 加载所有旧的全局调色盘
            foreach (BsonDocument doc in collection.FindAll())
            {
                if (!doc.TryGetInt32("_id", out int id))
                {
                    LogUtil.Error($"in gamesaves(\"{gamesavesName}\"), a \"{name}\" palette document missing  \"_id\" field");
                    continue;
                }

                if (!doc.TryGetArray("entries", out BsonArray? entryArray))
                {
                    LogUtil.Error($"in gamesaves(\"{gamesavesName}\"), a \"{name}\" palette document missing  \"entries\" field");
                    continue;
                }

                GlobalPaletteMapper converter = BuildPaletteMapper(entryArray);
                // 检查这个旧调色盘与当前调色盘是否相同
                if (EqualCurrent(converter))
                {
                    currentGlobalPaletteID = id;
                    globalPaletteArray = entryArray;
                }
                idToMapper.Add(id, converter);
            }

            // 旧调色盘中没找到与当前调色盘相同的
            if (currentGlobalPaletteID == -1)
            {
                BsonDocument current = new BsonDocument();
                current.Add("entries", globalPaletteArray);
                BsonValue idValue = collection.Insert(current);
                int id = idValue.AsInt32;
                var builder = ImmutableArray.CreateBuilder<int>(globalIDMap.Count);
                for (int i = 0; i < builder.Count; i++)
                {
                    builder[i] = i;
                }
                idToMapper.Add(id, new GlobalPaletteMapper(builder.MoveToImmutable()));
                currentGlobalPaletteID = id;
            }
        }

        private GlobalPaletteMapper BuildPaletteMapper(BsonArray entryArray)
        {
            var builder = ImmutableArray.CreateBuilder<int>(entryArray.Count);
            for (int i = 0; i < entryArray.Count; ++i)
            {
                for (int j = 0; j < globalPaletteArray.Count; ++j)
                {
                    if (entryArray[i].Equals(globalPaletteArray[j]))
                    {
                        builder[j] = i;
                        break;
                    }
                }
            }

            return new GlobalPaletteMapper(builder.MoveToImmutable());
        }

        private bool EqualCurrent(GlobalPaletteMapper converter)
        {
            if (converter.data.Length != globalIDMap.Count)
            {
                return false;
            }

            for (int i = 0; i < converter.data.Length; ++i)
            {
                if (i != converter.data[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryGetMapper(int id, [NotNullWhen(true)] out GlobalPaletteMapper? palette)
        {
            return idToMapper.TryGetValue(id, out palette);
        }

        private BsonArray GenerateCurrentPaletteArray()
        {
            BsonDocument bson = new BsonDocument();
            BsonValue[] entries = new BsonValue[globalIDMap.Count];
            for (int i = 0; i < globalIDMap.Count; i++)
            {
                entries[i] = valueToBson(globalIDMap.ValueFor(i));
            }
            BsonArray entryArray = new BsonArray(entries);
            return entryArray;
        }

        public GlobalPaletteManager(IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson)
            : this (globalIDMap, valueToBson, string.Empty, string.Empty)
        {
        }

        public GlobalPaletteManager(IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson, string name, string gamesavesName)
        {
            this.globalIDMap = globalIDMap;
            this.valueToBson = valueToBson;
            this.name = name;
            this.gamesavesName = gamesavesName;
            globalPaletteArray = GenerateCurrentPaletteArray();
        }

        private readonly IIDMap<T> globalIDMap;

        private readonly Func<T, BsonValue> valueToBson;

        private readonly string name;

        private readonly string gamesavesName;

        private int currentGlobalPaletteID = -1;

        private BsonArray globalPaletteArray;

        private readonly Dictionary<int, GlobalPaletteMapper> idToMapper = new Dictionary<int, GlobalPaletteMapper>();
    }
}
