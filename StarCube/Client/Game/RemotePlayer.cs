using System;
using System.Collections.Generic;
using System.Text;
using StarCube.Game.Players;
using StarCube.Network;

namespace StarCube.Client.Game
{
    public class RemotePlayer : Player
    {
        public override bool ClientSide => true;



        public RemotePlayer(Guid guid, Connection connection)
            : base(guid, connection)
        {
        }
    }
}
