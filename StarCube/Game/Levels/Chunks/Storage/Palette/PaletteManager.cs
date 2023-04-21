using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Utility.Logging;

namespace StarCube.Game.Levels.Chunks.Storage.Palette
{
    public sealed class PaletteManager<T>
        where T : class, IIntegerID
    {
        public static PaletteManager<T> Load(string name, IIDMap<T> globalIDMap, ILiteCollection<BsonDocument> collection, Func<T, BsonValue> valueToBson, string savesName)
        {
            // 检查 collection 的 AutoId 是否正确
            if (collection.AutoId != BsonAutoId.Int32)
            {
                throw new ArgumentException("palette collections must have int32 auto id");
            }

            int currentGlobalPaletteID = -1;
            // 构建现在的全局调色盘数组
            BsonArray currentPaletteArray = BuildCurrentPaletteArray(globalIDMap, valueToBson);

            // 加载所有旧的全局调色盘
            Dictionary<int, PaletteMapper> idToMapper = new Dictionary<int, PaletteMapper>();
            foreach (BsonDocument doc in collection.FindAll())
            {
                if (!doc.TryGetInt32("_id", out int id))
                {
                    LogUtil.Error($"in gamesaves(\"{savesName}\"), a \"{name}\" palette document missing  \"_id\" field");
                    continue;
                }

                if (!doc.TryGetArray("entries", out BsonArray? paletteArray))
                {
                    LogUtil.Error($"in gamesaves(\"{savesName}\"), a \"{name}\" palette document missing  \"entries\" field");
                    continue;
                }

                PaletteMapper mapper = BuildPaletteMapper(currentPaletteArray, paletteArray);
                // 检查这个旧调色盘与当前调色盘是否相同
                if (currentGlobalPaletteID == -1 && EqualCurrent(globalIDMap.Count, mapper))
                {
                    currentGlobalPaletteID = id;
                    currentPaletteArray = paletteArray;
                }
                idToMapper.Add(id, mapper);
            }

            // 旧调色盘中没找到与当前调色盘相同的
            if (currentGlobalPaletteID == -1)
            {
                BsonDocument current = new BsonDocument();
                current.Add("entries", currentPaletteArray);
                BsonValue idValue = collection.Insert(current);
                currentGlobalPaletteID = idValue.AsInt32;
                var builder = ImmutableArray.CreateBuilder<int>(globalIDMap.Count);
                builder.AddRange(Enumerable.Repeat(0, globalIDMap.Count));
                for (int i = 0; i < builder.Count; i++)
                {
                    builder[i] = i;
                }
                idToMapper.Add(currentGlobalPaletteID, new PaletteMapper(builder.MoveToImmutable()));
            }

            return new PaletteManager<T>(name, currentGlobalPaletteID, idToMapper);
        }

        private static BsonArray BuildCurrentPaletteArray(IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson)
        {
            BsonValue[] entries = new BsonValue[globalIDMap.Count];
            for (int i = 0; i < globalIDMap.Count; i++)
            {
                entries[i] = valueToBson(globalIDMap.ValueFor(i));
            }
            BsonArray entryArray = new BsonArray(entries);
            return entryArray;
        }

        private static PaletteMapper BuildPaletteMapper(BsonArray currentPaletteArray, BsonArray paletteArray)
        {
            var builder = ImmutableArray.CreateBuilder<int>(paletteArray.Count);
            builder.AddRange(Enumerable.Repeat(0, paletteArray.Count));
            for (int i = 0; i < paletteArray.Count; ++i)
            {
                for (int j = 0; j < currentPaletteArray.Count; ++j)
                {
                    if (paletteArray[i].Equals(currentPaletteArray[j]))
                    {
                        builder[j] = i;
                        break;
                    }
                }
            }

            return new PaletteMapper(builder.MoveToImmutable());
        }

        private static bool EqualCurrent(int currentPaletteCount, PaletteMapper mapper)
        {
            if (mapper.data.Length != currentPaletteCount)
            {
                return false;
            }

            for (int i = 0; i < mapper.data.Length; ++i)
            {
                if (i != mapper.data[i])
                {
                    return false;
                }
            }

            return true;
        }


        public bool TryGetMapper(int id, [NotNullWhen(true)] out PaletteMapper? palette)
        {
            return idToMapper.TryGetValue(id, out palette);
        }

        public PaletteManager(string name, int currentPaletteID, Dictionary<int, PaletteMapper> idToMapper)
        {
            this.name = name;
            this.currentPaletteID = currentPaletteID;
            this.idToMapper = idToMapper;
        }

        public readonly string name;

        public readonly int currentPaletteID;

        private readonly Dictionary<int, PaletteMapper> idToMapper;
    }
}
