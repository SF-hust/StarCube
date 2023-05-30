using System;
using System.Collections.Generic;
using System.Numerics;

namespace StarCube.Server.Game
{
    public sealed class ServerPlayerList
    {
        public ServerPlayer AddPlayer(Guid guid)
        {
            if (game.storage.TryLoadPlayer(guid, out string name, out Guid worldGuid, out Vector3 position))
            {

            }

            throw new NotImplementedException();
        }

        public void Save(bool flush)
        {
            //foreach ()
            {

            }
        }

        public ServerPlayerList(ServerGame game)
        {
            this.game = game;
        }

        public readonly ServerGame game;

        private readonly Dictionary<Guid, ServerPlayer> guidToPlayer = new Dictionary<Guid, ServerPlayer>();
    }
}
