namespace StarCube.Network.Packets
{
    public abstract class PacketHandler
    {

        public PacketHandler(Connection connection)
        {
            this.connection = connection;
        }

        public readonly Connection connection;
    }

    public abstract class PacketHandler<H> : PacketHandler
        where H : PacketHandler<H>
    {
        public PacketHandler(Connection connection) : base(connection)
        {
        }
    }
}
