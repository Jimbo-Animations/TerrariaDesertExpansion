
namespace TerrariaDesertExpansion.Content.Items.Materials
{
    internal class DesertAloe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 5);
        }
    }
}
