namespace StarCube.Game.Items
{
    public class ItemStack
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

        public ItemStack() : this(BuiltinItems.Air, 0)
        {
        }

        public ItemStack(Item item, int count = 1)
        {
        }

        private Item item = BuiltinItems.Air;

        private int count = 0;
    }
}
