using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ObjectData;
using TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Bomb;
using TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Djinn;
using TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Scarab;

namespace TerrariaDesertExpansion.Content.Tiles
{
    public class HauntedUrn : ModTile
    {
        private Asset<Texture2D> glowTexture;
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

            glowTexture = Request<Texture2D>(Texture + "Mask");
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            // Shoutouts to Dylandoe21 for helping me figure out code for this glowmask

            float time = Main.GameUpdateCount * 0.02f;
            float glowbrightness = (float)MathF.Sin(j / 15f - time);

            spriteBatch.Draw(
            TextureAssets.Tile[Type].Value,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
            new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
            Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);

            spriteBatch.Draw(
            glowTexture.Value,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
            new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
            new Color(52, 205, 148, 50) * glowbrightness, 0f, default, 1f, SpriteEffects.None, 0f);

            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int urnType = 0;

            if (frameY == 0) // 0 * 18 = 0
            {
                urnType = 1;

                NPC.NewNPC(null, i * 16 + 16, j * 16 + 16, NPCType<ScarabSpirit>());
            }

            if (frameY == 54) // 3 * 18 = 54
            {
                urnType = 2;

                NPC.NewNPC(null, i * 16 + 16, j * 16 + 16, NPCType<BombSpirit>());
            }

            if (frameY == 108) // 6 * 18 = 108
            {
                urnType = 3;

                NPC.NewNPC(null, i * 16 + 16, j * 16 + 16, NPCType<HappySpirit>());
            }

            int gore1 = Mod.Find<ModGore>("UrnGore" + urnType + "-1").Type;
            int gore2 = Mod.Find<ModGore>("UrnGore" + urnType + "-2").Type;
            int gore3 = Mod.Find<ModGore>("UrnGore" + urnType + "-3").Type;
            int gore4 = Mod.Find<ModGore>("UrnGore" + urnType + "-4").Type;
            int gore5 = Mod.Find<ModGore>("UrnGore" + urnType + "-5").Type;
            int gore6 = Mod.Find<ModGore>("UrnGore" + urnType + "-6").Type;

            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(2).RotatedByRandom(MathHelper.TwoPi), gore1);
            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(2).RotatedByRandom(MathHelper.TwoPi), gore2);
            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(3).RotatedByRandom(MathHelper.TwoPi), gore3);
            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(3).RotatedByRandom(MathHelper.TwoPi), gore4);
            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(1).RotatedByRandom(MathHelper.TwoPi), gore5);
            Gore.NewGore(null, new Vector2(i * 16 + 16, j * 16 + 16), new Vector2(1).RotatedByRandom(MathHelper.TwoPi), gore6);
        }
    }
}
