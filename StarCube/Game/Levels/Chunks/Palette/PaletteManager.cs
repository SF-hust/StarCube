using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using LiteDB;

using StarCube.Utility;
using StarCube.Utility.Container;
using StarCube.Data.Storage;

namespace StarCube.Game.Levels.Chunks.Palette
{
    public sealed class PaletteManager<T>
        where T : class, IIntegerID
    {
        public const string NextIndexField = "index";

        public const string PaletteEntriesField = "entries";

        private static BsonArray BuildPaletteEntries(IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson)
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
            return indexToMapper.TryGetValue(id, out palette);
        }

        public void Save()
        {
            if (currentPaletteDocument == null)
            {
                return;
            }

            var collection = database.Value.GetCollectionAndEnsureIndex(name, BsonAutoId.Int32);
            BsonDocument nextIndexDocument = new BsonDocument();
            nextIndexDocument[NextIndexField] = currentPaletteIndex + 1;
            collection.Upsert(0, nextIndexDocument);
            collection.Insert(currentPaletteIndex, currentPaletteDocument);
            currentPaletteDocument = null;
        }

        private void Load(IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson, out int currentPaletteIndex, out ImmutableDictionary<int, PaletteMapper> indexToMapper, out BsonDocument? currentPaletteDocument)
        {
            if (!database.Created)
            {
                currentPaletteIndex = 1;
                indexToMapper = ImmutableDictionary<int, PaletteMapper>.Empty;
                currentPaletteDocument = new BsonDocument();
                BsonArray array = BuildPaletteEntries(globalIDMap, valueToBson);
                currentPaletteDocument[PaletteEntriesField] = array;
                return;
            }

            currentPaletteDocument = null;
            currentPaletteIndex = -1;

            // 加载下一个 palette 的 index
            var collection = database.Value.GetCollectionAndEnsureIndex(name, BsonAutoId.Int32);
            int nextPaletteIndex = collection.FindById(0)[NextIndexField].AsInt32;

            // 构建现在的全局调色盘数组
            BsonArray currentPaletteEntries = BuildPaletteEntries(globalIDMap, valueToBson);

            // 加载所有旧的全局调色盘
            Dictionary<int, PaletteMapper> loadingIndexToMapper = new Dictionary<int, PaletteMapper>();
            foreach (BsonDocument doc in collection.FindAll())
            {
                int index = doc["_id"].AsInt32;
                BsonArray paletteArray = doc[PaletteEntriesField].AsArray;
                PaletteMapper mapper = BuildPaletteMapper(currentPaletteEntries, paletteArray);
                // 检查这个旧调色盘与当前调色盘是否相同
                if (currentPaletteIndex == -1 && EqualCurrent(globalIDMap.Count, mapper))
                {
                    currentPaletteIndex = index;
                    currentPaletteEntries = paletteArray;
                }
                loadingIndexToMapper.Add(index, mapper);
            }

            // 旧调色盘中没找到与当前调色盘相同的
            if (currentPaletteIndex == -1)
            {
                currentPaletteIndex = nextPaletteIndex;
                currentPaletteDocument = new BsonDocument();
                currentPaletteDocument[PaletteEntriesField] = currentPaletteEntries;

                var builder = ImmutableArray.CreateBuilder<int>(globalIDMap.Count);
                for (int i = 0; i < globalIDMap.Count; i++)
                {
                    builder.Add(i);
                }
                loadingIndexToMapper.Add(currentPaletteIndex, new PaletteMapper(builder.MoveToImmutable()));
            }

            indexToMapper = loadingIndexToMapper.ToImmutableDictionary();
        }

        public PaletteManager(string name, StorageDatabase database, IIDMap<T> globalIDMap, Func<T, BsonValue> valueToBson)
        {
            this.name = name;
            this.database = database;
            Load(globalIDMap, valueToBson, out currentPaletteIndex, out indexToMapper, out currentPaletteDocument);
        }

        public readonly string name;

        private readonly StorageDatabase database;

        public readonly int currentPaletteIndex;

        private readonly ImmutableDictionary<int, PaletteMapper> indexToMapper;

        private BsonDocument? currentPaletteDocument;
    }
}
