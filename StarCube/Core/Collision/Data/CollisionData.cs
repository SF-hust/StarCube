using System.Collections.Generic;
using StarCube.Utility;
using StarCube.Utility.Physics;

namespace StarCube.Core.Collision.Data
{
    public class CollisionData
    {
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
