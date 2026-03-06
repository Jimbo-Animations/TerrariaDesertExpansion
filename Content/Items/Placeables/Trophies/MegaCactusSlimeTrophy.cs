using Terraria.Localization;
using Terraria.ObjectData;

namespace TerrariaDesertExpansion.Content.Items.Placeables.Trophies
{
    class MegaCactusSlimeTrophy : ModItem
    {
        public override void SetDefaults()
        {           
            Item.DefaultToPlaceableTile(TileType<MegaCactusSlimeTrophyPlaced>());

            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 1);
        }

    }

    class MegaCactusSlimeTrophyPlaced : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.FramesOnKillWall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Trophy"));
            DustType = DustID.WoodFurniture;
        }
    }
}
