namespace TerrariaDesertExpansion.Content.Items.Materials
{
    class HauntedSand : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 2, copper: 50);
        }
    }

    public class HauntedSandSubstitues : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return item.type == ItemID.SandstorminaBottle;
        }

        public override void AddRecipes()
        {
            Recipe SandBottleRecipe = Recipe.Create(ItemID.SandstorminaBottle);
            SandBottleRecipe.AddIngredient(ItemType<HauntedSand>(), 3);
            SandBottleRecipe.AddIngredient(ItemID.Topaz);
            SandBottleRecipe.AddIngredient(ItemID.Bottle);
        }
    }
}

