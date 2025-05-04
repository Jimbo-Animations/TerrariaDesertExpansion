using Terraria.GameContent.Bestiary;

namespace TerrariaDesertExpansion.Content.NPCs.TheSun
{
    partial class TheSun : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(94);

            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 1000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.npcSlots = 25f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.dontTakeDamage = false;
        }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D texture2 = Request<Texture2D>(Texture + "Ring").Value;
            Texture2D glow1 = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_1").Value;

            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = SpriteEffects.FlipHorizontally;
            Rectangle RingRect = new Rectangle(0, ringFrame, NPC.frame.Width, NPC.frame.Height);

            float glowMult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.15f;
            rotationTimer += .075f;
            if (rotationTimer >= MathHelper.Pi) rotationTimer = 0f;

            //Makes sure it does not draw its normal code for its bestiary entry.
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(glow1, drawPos, null, new Color(250, 175, 100) * (glowMult + 1.3f), NPC.rotation, glow1.Size() / 2, NPC.scale * (glowMult + 1.3f), SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(texture2, drawPos, RingRect, Color.White, NPC.rotation + rotationTimer, drawOrigin, NPC.scale, effects, 0f);
                spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                spriteBatch.Draw(glow1, drawPos, null, new Color(225, 125, 175) * (glowMult + .65f), NPC.rotation, glow1.Size() / 2, NPC.scale * (glowMult + .65f), SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }

            return false;
        }
    }
}
