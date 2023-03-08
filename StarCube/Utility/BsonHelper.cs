using System.Diagnostics.CodeAnalysis;
using System.IO;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace StarCube.Utility
{
    public static class BsonHelper
    {
        public static bool TryReadFromStreamSync(FileStream stream, [NotNullWhen(true)] out BsonDocument? bson)
        {
            RawBsonDocument rawBson = new RawBsonDocument(new BinaryReader(stream).ReadBytes((int)stream.Length));
            bson = rawBson.ToBsonDocument();
            return true;
        }
    }
}
