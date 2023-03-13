using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Newtonsoft.Json.Linq;

using StarCube.Utility;
using StarCube.Utility.Enums;
using StarCube.Utility.Physics;
using StarCube.Data.Loading;

namespace StarCube.Core.Collision.Data
{
    public class CollisionData : IStringID
    {
        public static readonly StringID DataRegistry = StringID.Create(Constants.DEFAULT_NAMESPACE, "collision");

        public static readonly IDataReader<CollisionData> DataReader = new DataReaderWrapper<CollisionData, JObject>(RawDataReaders.JSON, TryParseFromJson);

        public static readonly StringID AIR_COLLISION_ID = StringID.Create(Constants.DEFAULT_NAMESPACE, "block/air");

        public static readonly StringID CUBE_COLLISION_ID = StringID.Create(Constants.DEFAULT_NAMESPACE, "block/cube");

        /// <summary>
        /// 无碰撞
        /// </summary>
        public static readonly CollisionData AIR = new CollisionData(
            AIR_COLLISION_ID,
            new List<BoxColliderData> { BoxColliderData.FULLCUBE },
            new List<SphereColliderData>(),
            new List<CapsuleColliderData>());

        /// <summary>
        /// 完整方块的碰撞
        /// </summary>
        public static readonly CollisionData CUBE = new CollisionData(
            CUBE_COLLISION_ID,
            new List<BoxColliderData> { BoxColliderData.FULLCUBE },
            new List<SphereColliderData>(),
            new List<CapsuleColliderData>());


        public static bool TryParseFromJson(JObject json, StringID id, [NotNullWhen(true)] out CollisionData? data)
        {
            data = null;

            List<BoxColliderData> boxColliders = new List<BoxColliderData>();
            List<SphereColliderData> sphereColliders = new List<SphereColliderData>();
            List<CapsuleColliderData> capsuleColliders = new List<CapsuleColliderData>();
            if (json.TryGetArray("colliders", out JArray? colliderArray))
            {
                foreach (JToken token in colliderArray)
                {
                    if (!(token is JObject colliderObject))
                    {
                        return false;
                    }

                    if(!colliderObject.TryGetString("type", out string type))
                    {
                        return false;
                    }

                    if (type == "box")
                    {
                        if(!TryParseBoxCollider(colliderObject, out BoxColliderData boxCollider))
                        {
                            return false;
                        }
                        boxColliders.Add(boxCollider);
                    }
                    else if (type == "sphere")
                    {
                        if (!TryParseSphereCollider(colliderObject, out SphereColliderData sphereCollider))
                        {
                            return false;
                        }
                        sphereColliders.Add(sphereCollider);
                    }
                    else if (type == "capsule")
                    {
                        if (!TryParseCapsuleCollider(colliderObject, out CapsuleColliderData capsuleCollider))
                        {
                            return false;
                        }
                        capsuleColliders.Add(capsuleCollider);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            data = new CollisionData(id, boxColliders, sphereColliders, capsuleColliders);
            return true;
        }

        private static bool TryParseBoxCollider(JObject json, out BoxColliderData boxCollider)
        {
            boxCollider = new BoxColliderData();
            if(json.TryGetArray("center", out JArray? centerArray) &&
                centerArray.ToFloatArray(out float[] centers) &&
                centers.Length == 3)
            {
                boxCollider.center = new Vector3(centers[0], centers[1], centers[2]);
            }
            else
            {
                return false;
            }

            if (json.TryGetArray("size", out JArray? sizeArray) &&
                sizeArray.ToFloatArray(out float[] sizes) &&
                sizes.Length == 3)
            {
                boxCollider.size = new Vector3(sizes[0], sizes[1], sizes[2]);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool TryParseSphereCollider(JObject json, out SphereColliderData sphereCollider)
        {
            sphereCollider = new SphereColliderData();
            if (json.TryGetArray("center", out JArray? centerArray) &&
                centerArray.ToFloatArray(out float[] centers) &&
                centers.Length == 3)
            {
                sphereCollider.center = new Vector3(centers[0], centers[1], centers[2]);
            }
            else
            {
                return false;
            }

            if (json.TryGetFloat("radius", out float radius))
            {
                sphereCollider.radius = radius;
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool TryParseCapsuleCollider(JObject json, out CapsuleColliderData capsule)
        {
            capsule = new CapsuleColliderData();
            if (json.TryGetArray("center", out JArray? centerArray) &&
                centerArray.ToFloatArray(out float[] centers) &&
                centers.Length == 3)
            {
                capsule.center = new Vector3(centers[0], centers[1], centers[2]);
            }
            else
            {
                return false;
            }

            if (json.TryGetFloat("radius", out float radius))
            {
                capsule.radius = radius;
            }
            else
            {
                return false;
            }

            if (json.TryGetFloat("height", out float height))
            {
                capsule.height = height;
            }
            else
            {
                return false;
            }

            if (json.TryGetString("axis", out string axisString))
            {
                capsule.axis = AxisExtension.Parse(axisString);
                if(capsule.axis == Axis.None)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        StringID IStringID.ID => id;

        public CollisionData(StringID id, List<BoxColliderData> boxColliders, List<SphereColliderData> sphereColliders, List<CapsuleColliderData> capsuleColliders)
        {
            this.id = id;
            this.boxColliders = boxColliders;
            this.sphereColliders = sphereColliders;
            this.capsuleColliders = capsuleColliders;
        }

        public readonly StringID id;

        public readonly List<BoxColliderData> boxColliders;
        public readonly List<SphereColliderData> sphereColliders;
        public readonly List<CapsuleColliderData> capsuleColliders;
    }
}
