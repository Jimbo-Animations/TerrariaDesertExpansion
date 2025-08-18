using TerrariaDesertExpansion.Content.NPCs.CactusSlime;
using TerrariaDesertExpansion.Systems;
using Terraria.DataStructures;
using TerrariaDesertExpansion.Content.Dusts;

namespace TerrariaDesertExpansion.Content.NPCs.CactusSlime
{
    internal class CactusSlimeIdle : ModNPC
    {
        public override string Texture => "TerrariaDesertExpansion/Content/NPCs/CactusSlime/MegaCactusSlime";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(74, 58);

            NPC.damage = 0;
            NPC.defense = 9999;
            NPC.lifeMax = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 1760 : 1350 : 1040 : 800;
            NPC.knockBackResist = 0;
            NPC.scale = Main.getGoodWorld ? 1.25f : 1;
            NPC.npcSlots = 50f;

            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1 with { Volume = 2, Pitch = -.2f };
            NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 2, Pitch = -.2f };
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frameHeight * 4;
        }

        public override bool CheckActive()
        {
            return !Main.dayTime;
        }

        public ref float AITimer => ref NPC.ai[0];

        public float stretchWidth;
        public float stretchHeight;
        public void SlimeSquish(float width, float height)
        {
            stretchWidth = MathHelper.SmoothStep(stretchWidth, width, .2f);
            stretchHeight = MathHelper.SmoothStep(stretchHeight, height, .2f);
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.spriteDirection = Main.rand.NextBool() ? 1 : -1;
        }

        public override void AI()
        {
            AITimer++;

            if (AITimer % 20 == 1 && AITimer > 1)
            {
                Dust.NewDustPerfect(NPC.Center + new Vector2(10 * NPC.spriteDirection, -20), DustType<SleepyZ>(), new Vector2(Main.rand.NextFloat(1, 2) * -NPC.spriteDirection, -2), 0, default, 1f);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            NPC.Transform(NPCType<MegaCactusSlime>());
            CombatText.NewText(NPC.getRect(), new Color(250, 150, 50), "!!!", true, false);
            NPC.netUpdate = true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe) return 0f;
            if (NPC.AnyNPCs(NPCType<MegaCactusSlime>())) return 0f;

            if (spawnInfo.Player.ZoneDesert && Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water) return Progression.DownedCactusSlime ? 0.012f : 0.12f;
            else return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height);
            Vector2 drawPos = NPC.Bottom - screenPos;

            float SpriteWidth = NPC.scale * stretchWidth;
            float SpriteHeight = NPC.scale * stretchHeight;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            SlimeSquish(1.1f + (float)Math.Sin(AITimer / 15) * .1f, .9f - (float)Math.Sin(AITimer / 16) * .1f);

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), effects, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), effects, 0f);

            return false;
        }
    }
}
