using System;
using System.Collections.Generic;
using System.Text;
using StarCube.Game.Players;
using StarCube.Network;

namespace StarCube.Client.Game
{
    public class LocalPlayer : Player
    {
        public override bool ClientSide => true;

        public LocalPlayer(Guid guid, Connection connection)
            : base(guid, connection)
        {
        }
    }
}
