using System;
using System.Collections.Generic;
using System.Text;
using StarCube.Network;
using StarCube.Utility;

namespace StarCube.Game.Players
{
    public abstract class Player : IGuid
    {
        public abstract bool ClientSide { get; }

        Guid IGuid.Guid => guid;

        public Player(Guid guid, Connection connection)
        {
            this.guid = guid;
            this.connection = connection;
        }

        public readonly Guid guid;

        public readonly Connection connection;
    }
}
