using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using TerrariaDesertExpansion.Systems.GlobalNPCs;
using Terraria.GameContent.ItemDropRules;

namespace TerrariaDesertExpansion.Content.NPCs.Cactoid
{
    [AutoloadBanner]
    class CrimsonCactoid : ModNPC
    {
        public ref float AITimer => ref NPC.ai[1];
        public ref float AIRandomizer => ref NPC.ai[2];
        public ref float soundCooldown => ref NPC.ai[3];

        public Player target
        {
            get => Main.player[NPC.target];
        }

        private enum AttackPattern : byte
        {
            Idle = 0,
            Walking = 1,
            Spitting = 2,
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("Cactoid specimens mutated by the Crimson. Their watery insides have become much more thick and juicy, granting them tasty goodness at the cost of their mobility.")
            });
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
            NPCID.Sets.UsesNewTargetting[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 1;

            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 38;
            NPC.height = 52;
            NPC.defense = Main.hardMode && Main.expertMode ? 16 : 8;
            NPC.damage = Main.hardMode && Main.expertMode ? 40 : 20;
            NPC.lifeMax = Main.hardMode && Main.expertMode ? 180 : 90;
            NPC.knockBackResist = .48f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.HitSound = SoundID.NPCHit47 with { Pitch = -0.15f };
            NPC.DeathSound = SoundID.NPCDeath49 with { Pitch = -0.15f };
            NPC.value = Item.buyPrice(0, 0, 0, 5);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneCrimson && !Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Crimsand && !spawnInfo.Water) return 0.3f;
            else return 0f;
        }

        int contactDamage;
        float knockBack;
        int animState;
        public bool bigJumping;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;
            NPC.frameCounter++;

            if (animState == 0) // Standing
            {
                NPC.frame.X = NPC.frame.Width;

                if (NPC.frameCounter >= 7)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 6) NPC.frame.Y = 0;
                }
            }

            if (animState == 1) // Walking
            {
                NPC.frame.X = 0;

                if (NPC.frameCounter >= 7)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;


                    if (NPC.frame.Y >= frameHeight * 10) NPC.frame.Y = 0;
                }
            }

            if (animState == 2) // Jumping
            {
                NPC.frameCounter = 0;
                NPC.frame.X = NPC.frame.Width * 2;

                if (NPC.velocity.Y < 0) NPC.frame.Y = frameHeight;
                else NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
            if (contactDamage == 0) 
            {
                contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 80 : 60 : 40 : 20;
                if (Main.hardMode && Main.expertMode) contactDamage *= 2;
            }
            if (knockBack == 0) knockBack = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? .33f : .38f : .43f : 48f;

            NPC.knockBackResist = bigJumping ? 0 : knockBack;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (soundCooldown > 0 && NPC.ai[0] != 0) soundCooldown--;

            if (soundCooldown <= 0 && Main.rand.NextBool(100) && NPC.ai[0] != 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie79, NPC.Center);
                soundCooldown = 300;
                NPC.netUpdate = true;
            }

            if (NPC.collideY || NPC.velocity.Y == 0)
            {
                if (NPC.ai[0] == 1 && NPC.velocity.X != 0) animState = 1;
                else animState = 0;
                NPC.rotation = NPC.rotation.AngleTowards(0, .01f);

                if (bigJumping)
                {
                    SoundEngine.PlaySound(SoundID.DeerclopsStep with { Pitch = .2f }, NPC.Center);
                    bigJumping = false;
                }
            }
            else
            {
                animState = 2;
                NPC.rotation = NPC.velocity.Y < 0 ? NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.X * .04f, .01f) : NPC.rotation.AngleTowards(-NPC.velocity.X * .04f, .01f);
            }

            NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1; if (contactDamage == 0) contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 88 : 66 : 44 : 22;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (soundCooldown > 0 && NPC.ai[0] != 0) soundCooldown--;

            if (soundCooldown <= 0 && Main.rand.NextBool(100) && NPC.ai[0] != 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie79 with { Pitch = -0.15f }, NPC.Center);
                soundCooldown = 300;
                NPC.netUpdate = true;
            }

            if (NPC.ai[0] == 2) animState = 2;
            else
            {
                if (NPC.collideY || NPC.velocity.Y == 0)
                {
                    if (NPC.ai[0] == 1 && NPC.velocity.X != 0) animState = 1;
                    else animState = 0;
                    NPC.rotation = NPC.rotation.AngleTowards(0, .01f);
                }
                else
                {
                    animState = 2;
                    NPC.rotation = NPC.velocity.Y < 0 ? NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.X * .04f, .01f) : NPC.rotation.AngleTowards(-NPC.velocity.X * .04f, .01f);
                }

                NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            }

            switch (AIstate)
            {
                case AttackPattern.Idle:

                    if (NPC.Distance(target.Center) < 250) AITimer++;

                    if (NPC.damage > 0) NPC.damage = 0;

                    if (AITimer > 120 + AIRandomizer)
                    {
                        NPC.ai[0] = 1;
                        NPC.damage = contactDamage;
                        SoundEngine.PlaySound(SoundID.Zombie80 with { PitchVariance = 0.4f, Pitch = -0.15f }, NPC.Center);
                        CombatText.NewText(NPC.getRect(), new Color(250, 50, 50), "!", true, false);

                        ResetVars();
                    }

                    break;

                case AttackPattern.Walking:

                    AITimer++;

                    ImprovedFighterAI.CustomizableFighterAI(NPC, target, 1.5f, 0.12f, 0.99f, 6, true, 75, 45, 90);

                    if (AITimer > 200 + AIRandomizer && (NPC.collideY || NPC.velocity.Y == 0))
                    {
                        NPC.ai[0] = 2;
                        ResetVars();
                    }

                    break;

                case AttackPattern.Spitting:

                    AITimer++;

                    if (AITimer == 1) SoundEngine.PlaySound(SoundID.Item154, NPC.Center);

                    if (AITimer < 20) NPC.velocity.X *= .9f;

                    if (AITimer >= 20 && !bigJumping)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, NPC.Center);

                        bigJumping = true;
                        NPC.velocity += new Vector2(NPC.direction, -8);

                        for (int i = 0; i < 10; i++)
                        {
                            int dust = Dust.NewDust(BasicUtils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(-30, 31), 0)), 0, 0, 115, Scale: 1f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));
                        }
                    }

                    if (bigJumping && NPC.velocity.Y >= 0)
                    {
                        NPC.velocity.X -= NPC.direction * 2;
                        SoundEngine.PlaySound(SoundID.NPCDeath1 with { PitchVariance = 0.4f, Pitch = -0.15f }, NPC.Center);
                        AfterImageOpacity = 1;

                        for (int i = -1; i < 2; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.direction * 10, 2), NPC.DirectionTo(target.Top).RotatedBy(MathHelper.ToRadians(5 * i)) * Main.rand.NextFloat(8, 11), ProjectileType<BloodSpit>(), 11, 2f, Main.myPlayer);
                        }

                        NPC.netUpdate = true;
                        NPC.ai[0] = 1;
                        ResetVars();
                    }

                    break;
            }
        }

        void ResetVars()
        {
            AITimer = 0;
            AIRandomizer = Main.rand.Next(0, 61);
            NPC.frameCounter = 0;
            NPC.netUpdate = true;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.ai[0] == 0)
            {
                NPC.ai[0] = 1;
                NPC.damage = contactDamage;

                SoundEngine.PlaySound(SoundID.Zombie80 with { PitchVariance = 0.5f, Pitch = -0.3f }, NPC.Center);
                CombatText.NewText(NPC.getRect(), new Color(250, 50, 50), "!", true, false);

                ResetVars();
            }

            int numDusts = 3;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 125, newColor: Color.White, Scale: 1.5f);
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2.0f, 2.1f), Main.rand.NextFloat(-2.0f, 2.1f));
            }

            if (NPC.life <= 0)
            {
                int gore1 = Mod.Find<ModGore>("CrimsonCactoidGore1").Type;
                int gore2 = Mod.Find<ModGore>("CrimsonCactoidGore2").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position - new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore1);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position + new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore2);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cactus, 1, 4, 8));
            npcLoot.Add(ItemDropRule.Common(ItemID.BloodOrange, 3, 1, 1));
        }

        public float AfterImageOpacity;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + drawPos, NPC.frame, drawColor * AfterImageOpacity * (.8f - i / (float)NPC.oldPos.Length), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            AfterImageOpacity *= .95f;

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }

    class BloodSpit : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(26);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.2f;
            Projectile.velocity *= 0.996f;

            if (Projectile.timeLeft <= 10)
            {
                Projectile.alpha += 25;
            }

            if (Projectile.ai[0]++ >= 4)
            {
                Projectile.frame++;
                Projectile.ai[0] = 0;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 123, Scale: .8f);
                dust.noGravity = false;
                dust.alpha = 140;
                dust.fadeIn = 1.2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, lightColor * .9f * (1 - i / (float)Projectile.oldPos.Length), Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * 0.98f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * .9f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item87, Projectile.Center);

            // Spawn some dusts upon javelin death
            for (int i = 0; i < 8; i++)
            {
                // Create a new dust
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 123, Scale: 1.2f);
                dust.velocity = new Vector2(Main.rand.NextFloat(5), 0).RotatedBy(i * MathHelper.TwoPi / 8);
                dust.alpha = 140;
                dust.fadeIn = 1.2f;
                dust.noGravity = false;
            }
        }
    }
}
