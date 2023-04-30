using System.Collections.Concurrent;

using StarCube.Network.Packets;

namespace StarCube.Network
{
    /// <summary>
    /// 本地连接，在同一进程内通信，使用并发队列发送和接收 packet
    /// </summary>
    public sealed class LocalConnection : Connection
    {
        public override bool Remote => false;

        protected override void DoSend(Packet packet)
        {
            sendPackets.Enqueue(packet);
        }

        protected override bool DoReceive(out Packet packet)
        {
            return receivePackets.TryDequeue(out packet);
        }

        public LocalConnection(ConnectionBound bound, PacketHandler handler, ConcurrentQueue<Packet> sendPackets, ConcurrentQueue<Packet> receivePackets)
            : base(bound, handler)
        {
            this.sendPackets = sendPackets;
            this.receivePackets = receivePackets;
        }

        private readonly ConcurrentQueue<Packet> sendPackets;

        private readonly ConcurrentQueue<Packet> receivePackets;
    }
}
