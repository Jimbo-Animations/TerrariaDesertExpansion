using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Bomb;

namespace TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Djinn
{
    class HappySpirit : ModNPC
    {
        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<HappySpirit>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                new FlavorTextBestiaryInfoElement("Many spirits find themselves trapped within urns left beneath the sands of the Great Desert. Amongst these, the peaceful jinn are most favorable to those who release them from their prisons.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(42, 70);

            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0;
            NPC.value = 0;
            NPC.npcSlots = 1f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontCountMe = true;
            NPC.dontTakeDamage = true;

            NPC.HitSound = SoundID.NPCHit37 with { Volume = .5f, Pitch = .2f };
            NPC.DeathSound = SoundID.NPCDeath47 with { Volume = .5f, Pitch = .2f };
        }

        bool startAnim = false;
        public override void FindFrame(int frameHeight)
        {
            if (!startAnim)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = 0;
            }
            else
            {
                NPC.frameCounter++;

                if (NPC.frameCounter == 6)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y == 7 * frameHeight)
                    {
                        NPC.frame.Y = 5 * frameHeight;
                    }
                }
            }
        }

        Vector2 spawnPos;

        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            spawnPos = NPC.Bottom;
            NPC.direction = NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;

            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 1.5f }, NPC.Center);
        }

        public override void AI()
        {
            NPC.ai[0]++;

            if (NPC.ai[0] > 45 && NPC.ai[0] < 99) visibility = MathHelper.SmoothStep(visibility, 1, .2f); // Period where she becomes fully visible.

            if (NPC.ai[0] < 60) outlineVis = MathHelper.SmoothStep(outlineVis, 1, .11f);

            if (NPC.ai[0] == 75) startAnim = true;

            if (NPC.ai[0] >= 99) // Start disappearing 24 frames after the animation starts.
            {
                if (NPC.ai[0] == 99)
                {
                    SoundEngine.PlaySound(SoundID.AbigailCry with { Pitch = .1f, Volume = 1.5f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item176 with { Pitch = -.3f }, NPC.Center);

                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ProjectileID.CoinPortal, 0, 2f, Main.myPlayer);

                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].Distance(NPC.Center) <= 800) Main.player[i].AddBuff(BuffType<HappySpiritBuff>(), 12600, true);
                    }
                }
                NPC.velocity.Y = MathHelper.SmoothStep(NPC.velocity.Y, -1.5f, .1f);

                visibility = MathHelper.SmoothStep(visibility, 0, .125f);
                outlineVis = MathHelper.SmoothStep(outlineVis, 0, .11f);
            }

            if (NPC.localAI[0]++ >= 5) // Add dusts.
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueTorch, Scale: 1);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));

                NPC.localAI[0] = 0;
            }

            if (NPC.ai[0] > 300)
            {
                NPC.active = false;
                NPC.life = 0;
            }
        }

        float timer;
        float visibility;
        float outlineVis;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(mask, drawPos + new Vector2(4 - (2 * visibility), 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(0, 50, 255, 100) * outlineVis, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
