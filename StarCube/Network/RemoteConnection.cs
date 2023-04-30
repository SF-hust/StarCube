using System;
using System.Net.Sockets;

using StarCube.Network.Packets;

namespace StarCube.Network
{
    /// <summary>
    /// 远程连接，在不同进程间通信，使用 socket 发送和接收 packet
    /// </summary>
    public sealed class RemoteConnection : Connection
    {
        public override bool Remote => true;

        protected override void DoSend(Packet packet)
        {
            throw new NotImplementedException();
        }

        protected override bool DoReceive(out Packet packet)
        {
            throw new NotImplementedException();
        }

        public RemoteConnection(ConnectionBound bound, PacketHandler handler, Socket socket)
            : base(bound, handler)
        {
            this.socket = socket;
        }

        private readonly Socket socket;
    }
}
