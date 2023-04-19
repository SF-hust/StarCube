using LiteDB;

namespace StarCube.Utility
{
    public static class LiteDBExtension
    {
        public static ILiteCollection<BsonDocument> GetCollectionAndEnsureIndex(this LiteDatabase database, string name, BsonAutoId autoID)
        {
            var collection = database.GetCollection<BsonDocument>(name, autoID);
            collection.EnsureIndex("_id");
            return collection;
        }
    }
}
