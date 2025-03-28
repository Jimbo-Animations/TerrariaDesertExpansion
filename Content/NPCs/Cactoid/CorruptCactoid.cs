using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using TerrariaDesertExpansion.Systems.GlobalNPCs;

namespace TerrariaDesertExpansion.Content.NPCs.Cactoid
{
    class CorruptCactoid : ModNPC
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
            Rolling = 2,
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
                new FlavorTextBestiaryInfoElement("The corruption has made the once savage cactoid into a slightly more savage cactoid. They have a fondness for rolling on the ground, which can benefit them in battle.")
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
            NPC.defense = 8;
            NPC.damage = 22;
            NPC.lifeMax = 80;
            NPC.knockBackResist = .5f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.HitSound = SoundID.NPCHit47 with { Pitch = 0.15f };
            NPC.DeathSound = SoundID.NPCDeath49 with { Pitch = 0.15f };
            NPC.value = Item.buyPrice(0, 0, 0, 5);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert && !Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water) return 0.3f;
            else return 0f;
        }

        int contactDamage;
        int animState;
        int rollDir;
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
            if (contactDamage == 0) contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 88 : 66 : 44 : 22;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (soundCooldown > 0 && NPC.ai[0] != 0) soundCooldown--;

            if (soundCooldown <= 0 && Main.rand.NextBool(100) && NPC.ai[0] != 0)
            {
                SoundEngine.PlaySound(SoundID.Zombie79 with { Pitch = 0.15f }, NPC.Center);
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
                        SoundEngine.PlaySound(SoundID.Zombie80 with { PitchVariance = 0.4f, Pitch = 0.15f }, NPC.Center);
                        CombatText.NewText(NPC.getRect(), new Color(250, 50, 50), "!", true, false);

                        ResetVars();
                    }

                    break;

                case AttackPattern.Walking:

                    AITimer++;

                    ImprovedFighterAI.CustomizableFighterAI(NPC, target, 2.2f, 0.16f, 0.995f, 5, true, 75, 45, 90);

                    if (AITimer > 200 + AIRandomizer && (NPC.collideY || NPC.velocity.Y == 0))
                    {
                        NPC.ai[0] = 2;
                        ResetVars();
                    }

                    break;

                case AttackPattern.Rolling:

                    AITimer++;

                    if (AITimer == 1)
                    {
                        NPC.damage = 0;
                        NPC.dontTakeDamage = true;
                        rollDir = NPC.Distance(target.Center) <= 150 && Main.rand.NextBool(2) ? -NPC.direction : NPC.direction;

                        NPC.velocity.X = 6.4f * rollDir;

                        AfterImageOpacity = 1;
                        SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Pitch = -0.3f }, NPC.Center);
                    }

                    NPC.rotation += .2512f * rollDir;
                    NPC.velocity.Y += .1f;

                    if (Main.rand.NextBool())
                    {
                        int dust = Dust.NewDust(BasicUtils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(-30, 31), 0)), 0, 0, 14, Scale: 1f);
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));
                    }

                    if (Collision.SolidCollision(NPC.position + new Vector2(NPC.width * NPC.direction, 0), NPC.width, NPC.height)) NPC.position.Y -= 8;

                    if (NPC.rotation > MathHelper.TwoPi || NPC.rotation < -MathHelper.TwoPi)
                    {
                        NPC.rotation = 0;
                        NPC.damage = contactDamage;
                        NPC.dontTakeDamage = false;
                        NPC.velocity.X *= .5f;

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

            int numDusts = 5;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 17, newColor: Color.White, Scale: 1.5f);
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2.0f, 2.1f), Main.rand.NextFloat(-2.0f, 2.1f));
            }
        }

        public float stretchHeight = 1;
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
}
