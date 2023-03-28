using LiteDB;

namespace StarCube.Game.Items
{
    public sealed class ItemStack
    {
        public Item Item
        {
            get => item;
            set
            {
                item = value;
            }
        }

        public int Count
        {
            get => count;
            set
            {
                count = value;
            }
        }


        public bool EqualsWithoutAdditional(ItemStack other)
        {
            return ReferenceEquals(item, other.item) && count == other.count;
        }

        public bool EqualsWithAdditional(ItemStack other)
        {
            return EqualsWithoutAdditional(other) && additionalData.Equals(other.additionalData);
        }


        public ItemStack() : this(BuiltinItems.Air, 0)
        {
        }

        public ItemStack(Item item, int count = 1)
        {
            this.item = item;
            this.count = count;
            additionalData = new BsonDocument();
        }

        public ItemStack(Item item, int count, BsonDocument additionalData, bool copy = false)
        {
            this.item = item;
            this.count = count;
            if(copy)
            {
                additionalData = new BsonDocument(additionalData);
            }
            this.additionalData = additionalData;
        }


        private Item item;

        private int count = 0;

        public readonly BsonDocument additionalData;
    }
}
