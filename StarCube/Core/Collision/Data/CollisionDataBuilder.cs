using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using StarCube.Data.DependencyResolver;
using StarCube.Utility;
using StarCube.Utility.Physics;

namespace StarCube.Core.Collision.Data
{
    public class CollisionDataBuilder : IResolvedDataBuilder<RawCollisionData, CollisionData>
    {
        public bool BuildResolvedData(RawCollisionData unresolvedData,
            IResolvedDataBuilder<RawCollisionData, CollisionData>.ResolvedDataGetter getResolvedData,
            [NotNullWhen(true)] out CollisionData? resolvedData)
        {
            resolvedData = null;

            if(!StringID.Valid(unresolvedData.parent))
            {
                resolvedData = new CollisionData(unresolvedData.id, unresolvedData.boxColliders, unresolvedData.sphereColliders, unresolvedData.capsuleColliders);
                return true;
            }

            if(!getResolvedData(unresolvedData.parent, out CollisionData? parent))
            {
                return false;
            }

            List<BoxColliderData> boxColliders;
            List<SphereColliderData> sphereColliders;
            List<CapsuleColliderData> capsuleColliders;

            if (unresolvedData.boxColliders.Count == 0)
            {
                boxColliders = unresolvedData.boxColliders;
            }
            else if (parent.boxColliders.Count == 0)
            {
                boxColliders = parent.boxColliders;
            }
            else
            {
                boxColliders = new List<BoxColliderData>(unresolvedData.boxColliders.Count + parent.boxColliders.Count);
                boxColliders.AddRange(parent.boxColliders);
                boxColliders.AddRange(unresolvedData.boxColliders);
            }

            if (unresolvedData.sphereColliders.Count == 0)
            {
                sphereColliders = unresolvedData.sphereColliders;
            }
            else if (parent.sphereColliders.Count == 0)
            {
                sphereColliders = parent.sphereColliders;
            }
            else
            {
                sphereColliders = new List<SphereColliderData>(unresolvedData.sphereColliders.Count + parent.sphereColliders.Count);
                sphereColliders.AddRange(parent.sphereColliders);
                sphereColliders.AddRange(unresolvedData.sphereColliders);
            }

            if (unresolvedData.capsuleColliders.Count == 0)
            {
                capsuleColliders = unresolvedData.capsuleColliders;
            }
            else if (parent.capsuleColliders.Count == 0)
            {
                capsuleColliders = parent.capsuleColliders;
            }
            else
            {
                capsuleColliders = new List<CapsuleColliderData>(unresolvedData.capsuleColliders.Count + parent.capsuleColliders.Count);
                capsuleColliders.AddRange(parent.capsuleColliders);
                capsuleColliders.AddRange(unresolvedData.capsuleColliders);
            }

            resolvedData = new CollisionData(unresolvedData.id, boxColliders, sphereColliders, capsuleColliders);
            return true;
        }
    }
}
