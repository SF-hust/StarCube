using System;
using System.Collections.Generic;
using System.Linq;

using StarCube.Utility;
using StarCube.Network.Packets;
using StarCube.Network.ByteBuffer;

namespace StarCube.Network
{
    public sealed class NetworkRegistry
    {
        private sealed class Entry
        {
            public Entry(int index, StringID id, Func<IByteBuffer, Packet> decoder)
            {
                this.index = index;
                this.id = id;
                this.decoder = decoder;
            }

            public int index = -1;

            public readonly StringID id;

            public readonly Func<IByteBuffer, Packet> decoder;
        }


        private static readonly Lazy<NetworkRegistry> instance = new Lazy<NetworkRegistry>(true);
        public static NetworkRegistry Instance => instance.Value;


        public PacketCodec Codec => codec.Value;

        public bool TrySortCodec(List<StringID> packetIDs)
        {
            Dictionary<StringID, int> idToIndex = new Dictionary<StringID, int>();
            foreach (var id in packetIDs)
            {
                idToIndex.Add(id, idToIndex.Count);
            }
            foreach (var id in entries.Select((entry) => entry.id))
            {
                if (!idToIndex.ContainsKey(id))
                {
                    return false;
                }
            }
            entries.Sort((left, right) => idToIndex[left.id].CompareTo(idToIndex[right.id]));
            int index = 0;
            foreach (var entry in entries)
            {
                entry.index = index;
                index++;
            }
            codec = new Lazy<PacketCodec>(BuildCodec, true);
            return true;
        }

        private PacketCodec BuildCodec()
        {
            return new PacketCodec(entries.Select((entry) => entry.decoder));
        }

        public void Register<P, H>(StringID id, Func<IByteBuffer, P> decoder)
            where P : Packet<P, H>
            where H : PacketHandler<H>
        {
            Entry entry = new Entry(entries.Count, id, (buffer) => decoder(buffer));
            entries.Add(entry);
        }

        public NetworkRegistry()
        {
            codec = new Lazy<PacketCodec>(BuildCodec, true);
        }

        private Lazy<PacketCodec> codec;

        private readonly List<Entry> entries = new List<Entry>();
    }

}
