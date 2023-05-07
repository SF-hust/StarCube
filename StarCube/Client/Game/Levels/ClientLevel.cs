using System;

using StarCube.Utility.Math;
using StarCube.Game.Levels;
using StarCube.Game.Levels.Chunks;
using StarCube.Game.Worlds;
using StarCube.Client.Game.Worlds;

namespace StarCube.Client.Game.Levels
{
    public abstract class ClientLevel : Level
    {
        public override bool Active { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override World World => world;

        public abstract void ChunkChanged(Chunk chunk);

        public abstract void ChunkRemoved(ChunkPos pos);

        public ClientLevel(Guid guid, ILevelBounding bounding, ClientWorld world) : base(guid, bounding)
        {
            this.world = world;
        }

        private readonly ClientWorld world;
    }
}
