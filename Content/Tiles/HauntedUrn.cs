using Terraria.ObjectData;
using TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Scarab;

namespace TerrariaDesertExpansion.Content.Tiles
{
    public class HauntedUrn : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileCut[Type] = true;
            //(ItemType<HauntedUrnPlaceable>());

            DustType = DustID.DesertPot;
            HitSound = SoundID.Shatter;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.StyleHorizontal = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(102, 255, 198));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Tile tile = Main.tile[i, j];

            if (frameY == 0)
            {
                NPC.NewNPC(null, i * 16 + 16, j * 16 + 16, NPCType<ScarabSpirit>());
            }
        }
    }
}
