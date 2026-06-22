using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using TerrariaDesertExpansion.Systems.GlobalNPCs;

namespace TerrariaDesertExpansion.Content.NPCs.Cactoid
{
    [AutoloadBanner]
    class Cactoid : ModNPC
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
            Jumping = 2,
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
                new FlavorTextBestiaryInfoElement("Nocturnal desert wanderers. Cactoids are known for their sporadic, unpredictable nature and deceptively dexterous capabilities. It's best to keep your distance from them if you can.")
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
            NPC.defense = Main.hardMode ? 16 : 8;
            NPC.damage = Main.hardMode ? 40 : 20;
            NPC.lifeMax = Main.hardMode ? 160 : 80;
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
            if (spawnInfo.Player.ZoneDesert && !Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water) return 0.3f;
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
            if (contactDamage == 0) 
            {
                contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 80 : 60 : 40 : 20;
                if (Main.hardMode && Main.expertMode) contactDamage *= 2;
            }
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

                    if (NPC.Distance(target.Center) < 200) AITimer++;

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

                    ImprovedFighterAI.CustomizableFighterAI(NPC, target, 2, 0.2f, 0.99f, 4, true, 60, 60, 120);

                    if (AITimer > 300 + AIRandomizer && (NPC.collideY || NPC.velocity.Y == 0))
                    {
                        NPC.ai[0] = 2;
                        ResetVars();
                    }

                    break;

                case AttackPattern.Jumping:

                    AITimer++;

                    if (AITimer == 1) SoundEngine.PlaySound(SoundID.Item154, NPC.Center);

                    if (AITimer >= 20)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, NPC.Center);
                        AfterImageOpacity = 1;
                        bigJumping = true;

                        NPC.velocity = new Vector2(MathHelper.Clamp((NPC.Center.X - target.Center.X) / (75 * -NPC.direction), 4, 8) * NPC.direction, MathHelper.Clamp((NPC.Center.Y - target.Center.Y) / -25, -15, -10f));
                        NPC.ai[0] = 1;

                        for (int i = 0; i < 20; i++)
                        {
                            int dust = Dust.NewDust(BasicUtils.findGroundUnder(NPC.Center + new Vector2(Main.rand.NextFloat(-30, 31), 0)) , 0, 0, 124, Scale: 1f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));
                        }

                        ResetVars();
                    }
                    else NPC.velocity.X *= .9f;

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
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 40, newColor: Color.White, Scale: 1.5f);
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2.0f, 2.1f), Main.rand.NextFloat(-2.0f, 2.1f));
            }
            
            if (NPC.life <= 0)
            {
                int gore1 = Mod.Find<ModGore>("CactoidGore1").Type;
                int gore2 = Mod.Find<ModGore>("CactoidGore2").Type;
                Gore.NewGore(NPC.GetSource_Death(), NPC.position - new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore1);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 6), NPC.velocity.RotatedByRandom(MathHelper.ToRadians(10)) / 2, gore2);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cactus, 1, 4, 8));
            npcLoot.Add(ItemDropRule.Common(ItemID.PinkPricklyPear, 10, 1, 1));
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
}
