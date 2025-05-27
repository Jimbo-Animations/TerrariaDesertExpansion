
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using TerrariaDesertExpansion.Systems.GlobalNPCs;
using TerrariaDesertExpansion.Content.NPCs.CactusSlime;
using Terraria.Graphics.CameraModifiers;
using Terraria.GameContent.ItemDropRules;

namespace TerrariaDesertExpansion.Content.NPCs.Cactoid
{
    [AutoloadBanner]
    class HallowedCactoid : ModNPC
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
            FlyingSlam = 2,
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
                new FlavorTextBestiaryInfoElement("Cactoids who wonder into the hallowed deserts are often attracted to the bright, flashy pixies and gastropods that roam the skies at night. Consumption of such creatures empowers the cactacious creatures, granting them temporary flight.")
            });
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
            NPCID.Sets.UsesNewTargetting[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 1;

            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 38;
            NPC.height = 52;
            NPC.defense = 20;
            NPC.damage = 50;
            NPC.lifeMax = 200;
            NPC.knockBackResist = .5f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.HitSound = SoundID.NPCHit47;
            NPC.DeathSound = SoundID.NPCDeath49;
            NPC.value = Item.buyPrice(0, 0, 0, 5);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneHallow && !Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Pearlsand && !spawnInfo.Water) return 0.3f;
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

                if (NPC.frameCounter >= 6)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;


                    if (NPC.frame.Y >= frameHeight * 10) NPC.frame.Y = 0;
                }
            }

            if (animState == 2) // Jumping
            {
                NPC.frame.X = NPC.frame.Width * 2;

                if (NPC.velocity.Y < 0) NPC.frame.Y = frameHeight;
                else NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
            if (contactDamage == 0) contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 200 : 150 : 100 : 50;
            if (knockBack == 0) knockBack = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? .35f : .4f : .45f : .5f;

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

            NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

            switch (AIstate)
            {
                case AttackPattern.Idle:

                    if (NPC.Distance(target.Center) < 300) AITimer++;

                    if (NPC.damage > 0) NPC.damage = 0;

                    if (AITimer > 200 + AIRandomizer)
                    {
                        NPC.ai[0] = 1;
                        NPC.damage = contactDamage;
                        SoundEngine.PlaySound(SoundID.Zombie80 with { PitchVariance = 0.5f }, NPC.Center);
                        CombatText.NewText(NPC.getRect(), new Color(250, 50, 50), "!", true, false);

                        ResetVars();
                    }

                    break;

                case AttackPattern.Walking:

                    AITimer++;

                    ImprovedFighterAI.CustomizableFighterAI(NPC, target, 2.5f, 0.25f, 0.98f, 8, true, 60, 60, 120);

                    if (AITimer > 300 + AIRandomizer && (NPC.collideY || NPC.velocity.Y == 0))
                    {
                        NPC.ai[0] = 2;
                        ResetVars();
                    }

                    break;

                case AttackPattern.FlyingSlam:

                    AITimer++;

                    if (NPC.localAI[0]++ >= 5)
                    {
                        int dust = Dust.NewDust(NPC.Center, NPC.width, NPC.height, 204, Scale: .5f);
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].noLight = false;
                        Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));
                    }

                    if (AITimer == 1) 
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit5 with { Pitch = -.1f }, NPC.Center);
                        NPC.noGravity = false;
                        NPC.damage = 0;

                        NPC.velocity.Y -= 8;
                    }

                    if (AITimer <= 150)
                    {
                        AfterImageOpacity = 1;

                        if (NPC.Distance(target.Center - new Vector2(0, 250)) > 50) NPC.velocity += NPC.DirectionTo(target.Center - new Vector2(0, 250)) * .5f;
                        NPC.velocity *= NPC.Distance(target.Center - new Vector2(0, 150)) > 120 ? .98f : .92f;

                        if (NPC.Distance(target.Center - new Vector2(0, 250)) <= 50)
                        {
                            AITimer = 151;
                            NPC.velocity.X *= .1f;
                            NPC.velocity.Y += 15;

                            SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Pitch = -.1f }, NPC.Center);

                            int numDusts = 18;
                            for (int i = 0; i < numDusts; i++)
                            {
                                int dust = Dust.NewDust(NPC.Bottom, 0, 0, 204, Scale: 2f);
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                                Main.dust[dust].velocity.Y *= .4f;
                            }
                        }
                    }
                    else
                    {
                        bigJumping = true;
                        NPC.damage = contactDamage;
                        NPC.velocity.X *= .95f;

                        if (AITimer > 180 || NPC.velocity.Y == 0 || NPC.collideY)
                        {
                            if (NPC.velocity.Y == 0 || NPC.collideY)
                            {
                                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(0f, 1f), 3f, 3f, 10, 500f, "HallowedCactoid");
                                Main.instance.CameraModifiers.Add(modifier);

                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, Vector2.Zero, ProjectileType<CactusSlimeShockwave>(), 25, 2f, Main.myPlayer);
                                NPC.velocity = Vector2.Zero;

                                NPC.netUpdate = true;
                            }
                            bigJumping = false;
                            NPC.ai[0] = 1;
                            ResetVars();
                        }
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

                SoundEngine.PlaySound(SoundID.Zombie80 with { PitchVariance = 0.5f }, NPC.Center);
                CombatText.NewText(NPC.getRect(), new Color(250, 50, 50), "!", true, false);

                ResetVars();
            }

            int numDusts = 3;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 47, newColor: Color.White, Scale: 1.5f);
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2.0f, 2.1f), Main.rand.NextFloat(-2.0f, 2.1f));
            }

            if (NPC.life <= 0)
            {
                int gore1 = Mod.Find<ModGore>("HallowedCactoidGore1").Type;
                int gore2 = Mod.Find<ModGore>("HallowedCactoidGore2").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position - new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore1);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.position + new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore2);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cactus, 1, 4, 8));
            npcLoot.Add(ItemDropRule.Common(ItemID.PixieDust, 1, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 2, 4));
        }

        public float AfterImageOpacity;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D textureShadow = Request<Texture2D>(Texture + "Shadow").Value;
            Texture2D textureMask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(textureShadow, NPC.oldPos[i] - NPC.position + drawPos, NPC.frame, new Color(255 - (i * 2), 234 - (20 * i), 80 + (i * 2), 150) * AfterImageOpacity * (1 - i / (float)NPC.oldPos.Length), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            AfterImageOpacity *= .95f;

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            spriteBatch.Draw(textureMask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
