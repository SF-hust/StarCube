using System.Collections.Concurrent;

using LiteDB;

using StarCube.Utility;

namespace StarCube.Data.Storage.Custom
{
    public class CustomStorage
    {
        public BsonDocument GetOrCreate(StringID id)
        {
            if (idToDocumentCache.TryGetValue(id, out BsonDocument? doc))
            {
                return doc;
            }

            doc = customCollection.FindOne(Query.EQ("sid", id.idString));
            if (doc != null)
            {
                idToDocumentCache[id] = doc;
                return doc;
            }

            doc = new BsonDocument();
            doc.Add("sid", id.idString);
            idToDocumentCache[id] = doc;
            idToModifiedDocumentCache[id] = doc;
            return doc;
        }

        public void Modify(StringID id)
        {
            if (idToDocumentCache.TryGetValue(id, out BsonDocument ? doc))
            {
                idToModifiedDocumentCache[id] = doc;
            }
        }

        public void Save()
        {
            foreach (var doc in idToModifiedDocumentCache.Values)
            {
                customCollection.Upsert(doc);
            }
            idToModifiedDocumentCache.Clear();
        }

        public CustomStorage(LiteDatabase database)
        {
            customCollection = database.GetCollectionAndEnsureIndex("custom", BsonAutoId.ObjectId);
            customCollection.EnsureIndex("sid");
        }

        private readonly ILiteCollection<BsonDocument> customCollection;

        private readonly ConcurrentDictionary<StringID, BsonDocument> idToModifiedDocumentCache = new ConcurrentDictionary<StringID, BsonDocument>();

        private readonly ConcurrentDictionary<StringID, BsonDocument> idToDocumentCache = new ConcurrentDictionary<StringID, BsonDocument>();
    }
}
