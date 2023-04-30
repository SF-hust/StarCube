using System.Collections.Concurrent;

using StarCube.Network.Packets;

namespace StarCube.Network
{
    public enum ConnectionBound
    {
        ServerToClient,
        ClientToServer
    }

    public abstract class Connection
    {
        public abstract bool Remote { get; }

        public void Send(Packet packet)
        {
            packetQueue.Enqueue(packet);
        }

        protected abstract void DoSend(Packet packet);

        public void Flush()
        {
            while(packetQueue.TryDequeue(out var packet))
            {
                DoSend(packet);
            }
        }

        public void Tick()
        {
            Flush();

            while (DoReceive(out var packet))
            {
                packet.Handle(handler);
            }
        }

        protected abstract bool DoReceive(out Packet packet);

        public Connection(ConnectionBound bound, PacketHandler handler)
        {
            this.bound = bound;
            this.handler = handler;
        }

        public readonly ConnectionBound bound;

        public readonly PacketHandler handler;

        private readonly ConcurrentQueue<Packet> packetQueue = new ConcurrentQueue<Packet>();
    }
}
