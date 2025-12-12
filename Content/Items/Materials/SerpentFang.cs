namespace TerrariaDesertExpansion.Content.Items.Materials
{
    internal class SerpentFang : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 24;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(copper: 50);
        }
    }

    public class SerpentFangSubstitues : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return item.type == ItemID.PoisonDart || item.type == ItemID.PoisonedKnife || item.type == ItemID.FlaskofPoison;
        }

        public override void AddRecipes()
        {
            Recipe dartRecipe = Recipe.Create(ItemID.PoisonDart, 60);
            dartRecipe.AddIngredient(ItemType<SerpentFang>());

            Recipe knifeRecipe = Recipe.Create(ItemID.PoisonedKnife, 30);
            knifeRecipe.AddIngredient(ItemType<SerpentFang>());
            knifeRecipe.AddIngredient(ItemID.ThrowingKnife, 30);

            Recipe flaskRecipe = Recipe.Create(ItemID.FlaskofPoison);
            flaskRecipe.AddIngredient(ItemType<SerpentFang>(), 2);
            flaskRecipe.AddIngredient(ItemID.BottledWater);
            flaskRecipe.AddTile(TileID.ImbuingStation);
        }
    }
}
