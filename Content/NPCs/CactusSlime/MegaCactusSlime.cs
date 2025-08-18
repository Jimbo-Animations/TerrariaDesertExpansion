
namespace TerrariaDesertExpansion.Content.NPCs.CactusSlime
{
    [AutoloadBossHead]
    partial class MegaCactusSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 3;

            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

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
            NPC.defense = 5;
            NPC.lifeMax = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 1760 : 1350 : 1040 : 800;
            NPC.knockBackResist = 0f;
            NPC.scale = Main.getGoodWorld ? 1.25f : 1;
            NPC.value = Item.buyPrice(0, 0, 25, 0);
            NPC.boss = true;
            NPC.npcSlots = 50f;

            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1 with { Volume = 2, Pitch = -.2f };
            NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 2, Pitch = -.2f };

            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/Deitys_Duel");
        }

        public override void AI()
        {
            int goalDirection = target.Center.X < NPC.Center.X ? -1 : 1;
            bool phase2 = NPC.life <= NPC.lifeMax * (Main.expertMode ? Main.getGoodWorld ? .667f : .625f : .5f);

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            switch (AIstate)
            {
                case AttackPattern.Initiate:

                    NPC.velocity = new Vector2(5 * -goalDirection, -10);
                    NPC.spriteDirection = -goalDirection;
                    isGrounded = false;

                    NPC.ai[0] = 1;
                    NPC.netUpdate = true;

                    break;

                case AttackPattern.Hopping:

                    if (AITimer == 0)
                    {
                        AIRandomizer = Main.rand.Next(60, 76);
                        NPC.netUpdate = true;
                    }

                    NPC.damage = NPC.velocity.Y > 0 && !isGrounded ? contactDamage : 0;

                    if (AITimer > AIRandomizer * AIModifier && isGrounded)
                    {
                        AITimer = 0;
                        NPC.velocity = new Vector2(MathHelper.Clamp((NPC.Center.X - target.Center.X) / (75 * -goalDirection) * (MovementTracker % 2 == 1 ? 2.4f : 1), 1, 7) * goalDirection, MathHelper.Clamp((NPC.Center.Y - target.Center.Y) / -25, -25, -12.5f) * (MovementTracker % 2 == 1 ? 0.6f : 1));
                        NPC.spriteDirection = -goalDirection;
                        isGrounded = false;
                        MovementTracker++;

                        NPC.netUpdate = true;
                    }

                    if (isGrounded)
                    {
                        if (MovementTracker > (Main.getGoodWorld ? 5 : 4))
                        {
                            NPC.ai[0] = phase2 ? 3 : 2;
                            resetVars();
                        }
                        else AITimer++;
                    }

                    break;

                case AttackPattern.SpikeBarrage:

                    AITimer++;

                    if (AITimer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item150 with { Pitch = -.1f, Volume = 2 }, NPC.Center);

                        int numDusts = 12;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center, 0, 0, 291, Scale: 2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(5, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                        }
                    }

                    if (AITimer > 15 && AITimer % (15 * AIModifier) == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item39 with { Pitch = -.1f, Volume = 2 }, NPC.Center);

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(Main.rand.NextFloat(2, 13), MathHelper.Clamp((NPC.Center.Y - target.Center.Y) / -25, -30, -15)) + new Vector2(0, Main.rand.NextFloat(-0.5f, 0.6f)), ProjectileType<CactusSlimeSpike>(), 10, 2f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(Main.rand.NextFloat(-12, -1), MathHelper.Clamp((NPC.Center.Y - target.Center.Y) / -25, -30, -15)) + new Vector2(0, Main.rand.NextFloat(-0.5f, 0.6f)), ProjectileType<CactusSlimeSpike>(), 10, 2f, Main.myPlayer);
                        NPC.netUpdate = true;
                    }

                    if (AITimer > 255 * AIModifier)
                    {
                        NPC.ai[0] = 4;
                        resetVars();
                    }

                    break;

                case AttackPattern.MiniBarrage:

                    if (MovementTracker == 0) AITimer++;

                    if (AITimer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item174 with { Pitch = -.1f, Volume = 2 }, NPC.Center);
                        stretchWidth = 1.1f;
                        stretchHeight = .9f;
                        CombatText.NewText(NPC.getRect(), new Color(100, 255, 100), "!!!", true, false);
                    }

                    if (AITimer > 15 * AIModifier && isGrounded)
                    {
                        AITimer = 0;
                        NPC.velocity = new Vector2(6 * -goalDirection, MathHelper.Clamp((NPC.Center.Y - target.Center.Y) / -25, -30, -15f));
                        NPC.spriteDirection = -goalDirection;
                        isGrounded = false;
                        MovementTracker++;

                        NPC.netUpdate = true;
                    }

                    if (!isGrounded && NPC.velocity.Y >= 0 && MovementTracker > 0 && MovementTracker <= 1 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item42 with { Pitch = -.1f, Volume = 2.5f }, NPC.Center);
                        MovementTracker++;

                        for (int i = -5; i < 5; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(target.Center).RotatedBy(MathHelper.Pi * i / 10) * 8, ProjectileType<CactusSlimeSpike>(), 10, 2f, Main.myPlayer);
                        }

                        NPC.netUpdate = true;
                    }

                    if (isGrounded && MovementTracker > 1)
                    {
                        NPC.ai[0] = 2;
                        resetVars();
                    }

                    break;

                case AttackPattern.Teleport:

                    AITimer++;

                    int dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, newColor: Color.LightGreen, Scale: 1.2f);
                    Main.dust[dust2].noLight = true;
                    Main.dust[dust2].alpha = 180;
                    Main.dust[dust2].noGravity = true;

                    if (AITimer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie116 with { Pitch = -.1f, Volume = 2 }, NPC.Center);
                        FindTeleportPoint(target);
                        NPC.dontTakeDamage = true;
                    }

                    if (AITimer <= 80) SlimeSquish(1 - (AITimer / 80), 1 - (AITimer / 80));
                    else SlimeSquish(0 + (AITimer / 160), 0 + (AITimer / 160));

                    if (AITimer == 80)
                    {
                        NPC.position = goalPosition;
                        NPC.netUpdate = true;
                    }

                    if (AITimer >= 160)
                    {
                        NPC.ai[0] = 1;
                        NPC.dontTakeDamage = false;
                        resetVars();
                    }

                    break;

                case AttackPattern.RunAway:

                    NPC.noTileCollide = true;

                    break;
            }
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
            auraAlpha = MathHelper.SmoothStep(auraAlpha, NPC.damage > 0 ? 1 : 0, 0.175f);

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, NPC.oldPos[i] - NPC.position + drawPos, NPC.frame, new Color(255, 50, 50) * auraAlpha * (0.5f - i / (float)NPC.oldPos.Length) * .85f, NPC.rotation, drawOrigin, new Vector2(SpriteWidth * 1.1f, SpriteHeight * 1.1f), effects, 0);
            }

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), effects, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), effects, 0f);

            return false;
        }
    }
}
