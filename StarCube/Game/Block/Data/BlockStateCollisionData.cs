using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Core.Collision.Data;

namespace StarCube.Game.Block.Data
{
    public class BlockStateCollisionData : BlockStateDataBase
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "blockstate/collision");

        public static readonly IDataReader<BlockStateCollisionData> DataReader = new DataReaderWrapper<BlockStateCollisionData, JObject>(RawDataReaders.JSON, TryParseFromJson);


        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out BlockStateCollisionData? data)
        {
            data = null;
            List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry = new List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>>();

            // 获取本文件中定义的默认值
            CollisionDataEntry defaultEntry = CollisionDataEntry.ParseFromJson(json, CollisionDataEntry.DEFAULT);

            // 按顺序遍历解析出所有匹配规则与其对应的值
            if (json.TryGetArray("match", out JArray? matcherToEntryArray))
            {
                foreach (JToken token in matcherToEntryArray)
                {
                    if(!(token is JObject matcherToEntryObject))
                    {
                        return false;
                    }

                    if(!BlockStatePropertyMatcher.TryParseFromJson(matcherToEntryObject, out BlockStatePropertyMatcher matcher))
                    {
                        return false;
                    }

                    CollisionDataEntry entry = CollisionDataEntry.ParseFromJson(matcherToEntryObject, defaultEntry);

                    matcherToEntry.Add(new KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>(matcher, entry));
                }
            }

            // 在最后添加一个空的匹配规则与默认值
            matcherToEntry.Add(new KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>(BlockStatePropertyMatcher.EMPTY, defaultEntry));

            data = new BlockStateCollisionData(id, matcherToEntry);
            return true;
        }


        public readonly struct CollisionDataEntry
        {
            public static readonly CollisionDataEntry DEFAULT = new CollisionDataEntry(RawCollisionData.AIR_COLLISION_ID, 0, 0, 0);

            public static CollisionDataEntry ParseFromJson(JObject json, in CollisionDataEntry defaultEntry)
            {
                if(!json.TryGetStringID("collision", out StringID collisionID))
                {
                    collisionID = defaultEntry.collisionID;
                }
                if (!json.TryGetInt32("x", out int xRot))
                {
                    xRot = defaultEntry.xRot;
                }
                if (!json.TryGetInt32("y", out int yRot))
                {
                    yRot = defaultEntry.yRot;
                }
                if (!json.TryGetInt32("z", out int zRot))
                {
                    zRot = defaultEntry.zRot;
                }

                return new CollisionDataEntry(collisionID, xRot, yRot, zRot);
            }

            public CollisionDataEntry(StringID collisionID, int xRot, int yRot, int zRot)
            {
                this.collisionID = collisionID;
                this.xRot = xRot;
                this.yRot = yRot;
                this.zRot = zRot;
            }

            public readonly StringID collisionID;
            public readonly int xRot;
            public readonly int yRot;
            public readonly int zRot;
        }


        /// <summary>
        /// 获取此
        /// </summary>
        /// <param name="blockState"></param>
        /// <returns></returns>
        public List<CollisionDataEntry> GetMatchingCollisionDataEntriesFor(BlockState blockState)
        {
            List<CollisionDataEntry> matchingEntries = new List<CollisionDataEntry>();

            if (!blockState.Block.RegistryEntryData.ID.Equals(id))
            {
                return matchingEntries;
            }

            foreach (KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry> pair in matcherToEntry)
            {
                if(pair.Key.Match(blockState))
                {
                    matchingEntries.Add(pair.Value);
                }
            }

            return matchingEntries;
        }

        public BlockStateCollisionData(StringID id, List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry) : base(id)
        {
            this.matcherToEntry = matcherToEntry;
        }

        public readonly List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry;
    }
}
