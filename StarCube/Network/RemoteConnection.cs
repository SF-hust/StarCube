using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using StarCube.Network.ByteBuffer;
using StarCube.Network.Packets;

namespace StarCube.Network
{
    /// <summary>
    /// 远程连接，在不同进程间通信，使用 socket 发送和接收 packet
    /// </summary>
    public sealed class RemoteConnection : Connection
    {
        public override bool Remote => true;

        public async Task ReceivePacketAsync()
        {
            await receiveBuffer.ReceiveAsync(socket);
        }

        protected override void DoSend(Packet packet)
        {
        }

        protected override bool DoReceive(out Packet packet)
        {
            throw new Exception();
        }

        public RemoteConnection(ConnectionBound bound, PacketHandler handler, Socket socket)
            : base(bound, handler)
        {
            this.socket = socket;
        }

        private readonly Socket socket;

        private readonly ReceiveByteBuffer receiveBuffer = new ReceiveByteBuffer();

        private readonly SendByteBuffer sendBuffer = new SendByteBuffer();
    }
}
