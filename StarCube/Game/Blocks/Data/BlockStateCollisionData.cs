using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Data.Loading;
using StarCube.Core.Collision.Data;

namespace StarCube.Game.Blocks.Data
{
    /// <summary>
    /// 定义 Block 的每个 BlockState 所使用的碰撞资源
    /// </summary>
    public class BlockStateCollisionData : BlockStateDataBase
    {
        /// <summary>
        /// "starcube:blockstate/collision"
        /// </summary>
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "blockstate/collision");

        public static readonly IDataReader<BlockStateCollisionData> DataReader = new DataReaderWrapper<BlockStateCollisionData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        /// <summary>
        /// 创建一份默认的数据，对任意状态有一个完整方块的碰撞体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BlockStateCollisionData CreateSolid(StringID id)
        {
            KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry> pair = 
                new KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>(BlockStatePropertyMatcher.ANY, CollisionDataEntry.SOLID);
            return new BlockStateCollisionData(id, false, new List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> { pair });
        }

        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out BlockStateCollisionData? data)
        {
            data = null;
            List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry = new List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>>();

            bool multipart;
            if (json.TryGetArray("variants", out JArray? matcherToEntryArray))
            {
                multipart = false;
            }
            else if(json.TryGetArray("multiparts", out matcherToEntryArray))
            {
                multipart = true;
            }
            else
            {
                multipart = false;
                // 将整个文件作为一个 entry
                CollisionDataEntry singleEntry = CollisionDataEntry.ParseFromJson(json);
                matcherToEntry.Add(new KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>(BlockStatePropertyMatcher.ANY, singleEntry));
            }

            // 按顺序遍历解析出所有匹配规则与其对应的值
            if(matcherToEntryArray != null)
            {
                foreach (JToken token in matcherToEntryArray)
                {
                    if ((token is JObject matcherToEntryObject) == false)
                    {
                        return false;
                    }

                    BlockStatePropertyMatcher matcher = BlockStatePropertyMatcher.ANY;
                    if (matcherToEntryObject.TryGetJObject("when", out JObject? matcherObject))
                    {
                        if (!BlockStatePropertyMatcher.TryParseFromJson(matcherObject, out matcher))
                        {
                            return false;
                        }
                    }

                    CollisionDataEntry entry = CollisionDataEntry.ParseFromJson(matcherToEntryObject);

                    matcherToEntry.Add(new KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>(matcher, entry));
                }
            }

            data = new BlockStateCollisionData(id, multipart, matcherToEntry);
            return true;
        }


        /// <summary>
        /// 其中包含对 CollisionData 的引用，以及各轴上的旋转
        /// </summary>
        public readonly struct CollisionDataEntry
        {
            /// <summary>
            /// 无碰撞
            /// </summary>
            public static readonly CollisionDataEntry AIR = new CollisionDataEntry(CollisionData.AIR_COLLISION_ID, 0, 0, 0);

            /// <summary>
            /// 完整方块碰撞
            /// </summary>
            public static readonly CollisionDataEntry SOLID = new CollisionDataEntry(CollisionData.CUBE_COLLISION_ID, 0, 0, 0);

            public static CollisionDataEntry ParseFromJson(JObject json)
            {
                if(!json.TryGetStringID("collision", out StringID collisionID))
                {
                    collisionID = CollisionData.AIR_COLLISION_ID;
                }
                if (!json.TryGetInt32("x", out int xRot))
                {
                    xRot = 0;
                }
                if (!json.TryGetInt32("y", out int yRot))
                {
                    yRot = 0;
                }
                if (!json.TryGetInt32("z", out int zRot))
                {
                    zRot = 0;
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
            /// <summary>
            /// 匹配到此项时是否要停止匹配
            /// </summary>
            public readonly int xRot;
            public readonly int yRot;
            public readonly int zRot;
        }


        /// <summary>
        /// 获取与给定 BlockState 匹配的碰撞数据项
        /// </summary>
        /// <param name="blockState"></param>
        /// <returns></returns>
        public List<CollisionDataEntry> GetMatchingCollisionDataEntriesFor(BlockState blockState)
        {
            List<CollisionDataEntry> matchingEntries = new List<CollisionDataEntry>();

            if (!blockState.Block.ID.Equals(id))
            {
                return matchingEntries;
            }

            foreach (KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry> pair in matcherToEntry)
            {
                if(pair.Key.Match(blockState))
                {
                    matchingEntries.Add(pair.Value);
                    // 如果没有 multipart 标记则停止匹配
                    if (!isMultipart)
                    {
                        break;
                    }
                }
            }

            return matchingEntries;
        }

        /// <summary>
        /// 获取此数据中所引用的所有碰撞数据 id
        /// </summary>
        /// <param name="ids"></param>
        public void GetCollisionDataReferences(HashSet<StringID> ids)
        {
            foreach (var pair in matcherToEntry)
            {
                ids.Add(pair.Value.collisionID);
            }
        }

        public BlockStateCollisionData(StringID id, bool isMultipart, List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry) : base(id)
        {
            this.isMultipart = isMultipart;
            this.matcherToEntry = matcherToEntry;
        }

        /// <summary>
        /// 是否允许匹配多个节点
        /// </summary>
        public readonly bool isMultipart;

        public readonly List<KeyValuePair<BlockStatePropertyMatcher, CollisionDataEntry>> matcherToEntry;
    }
}
