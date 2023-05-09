using System;

using StarCube.Utility.Math;
using StarCube.Client.Network.Protocol;
using StarCube.Server.Game;

namespace StarCube.Game.Levels.Chunks.Loading
{
    public sealed class ChunkUpdateListener
    {
        public Guid Guid => player.guid;

        public void HandleChunkLoad(Guid levelGuid, Chunk chunk)
        {
            Span<int> buffer = stackalloc int[Chunk.ChunkSize];
            chunk.CopyBlockStatesTo(buffer);
            Chunk copied = player.game.levels.ChunkFactory.Create(chunk.Position, buffer);
            var packet = new ClientBoundChunkLoadPacket(levelGuid, chunk);
            player.connection.Send(packet);
        }

        public void HandleChunkUnload(Guid levelGuid, ChunkPos pos)
        {
            var packet = new ClientBoundChunkUnloadPacket(levelGuid, pos);
            player.connection.Send(packet);
        }

        public ChunkUpdateListener(ServerPlayer player)
        {
            this.player = player;
        }

        private readonly ServerPlayer player;

        public AnchorData anchor;
    }
}
