using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using TerrariaDesertExpansion.Content.Items.Materials;
using TerrariaDesertExpansion.Content.NPCs.Cactoid;

namespace TerrariaDesertExpansion.Content.NPCs.DustDevil
{
    internal class DustDevil : ModNPC
    {
        public Player target
        {
            get => Main.player[NPC.target];
        }

        private enum AttackPattern : byte
        {
            FlyNShoot = 0,
            MoveBelow = 1,
            MoveUp = 2
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[1];
            set => NPC.localAI[1] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[2];

        public ref float SquishTimer => ref NPC.localAI[2];

        public ref float WhichSide => ref NPC.ai[3];

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Sandstorm,
                new FlavorTextBestiaryInfoElement("Those who are lost in the sandstorms may find their very soul trapped under the shifting sands and strong winds, creating a dehydrated Dust Devil. Hunting in groups, they seek out the living to expand their company.")
            });
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 7;
            NPCID.Sets.UsesNewTargetting[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 100;
            NPC.height = 50;
            NPC.defense = 5;
            NPC.damage = 20;
            NPC.lifeMax = 100;
            NPC.knockBackResist = .4f;
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.HitSound = SoundID.NPCHit23;
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.value = Item.buyPrice(0, 0, 0, 5);
        }

        bool SpitAttack;
        bool Surface;
        public float stretchWidth = 1;
        public float stretchHeight = 1;
        public int contactDamage;
        public int shotCount;
        public NPC swarmNPC;

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneSandstorm && spawnInfo.SpawnTileY <= Main.worldSurface)
            {
                if (Main.dayTime) return 0.1f;
                else return 0.2f;
            }
            else return 0f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 0)
            {
                for (int i = 1; i < 3; i++)
                {
                    swarmNPC = Main.npc[NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)NPC.Center.X + Main.rand.Next(-1, 2), (int)NPC.Center.Y - i, NPC.type, ai0: i)];
                    swarmNPC.frame.Y = Main.rand.Next(0, 7) * NPC.frame.Height;
                    NPC.damage = 0;
                }
            }

            NPC.frame.Y = Main.rand.Next(0, 7) * NPC.frame.Height;
            NPC.damage = 0;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 2;
            NPC.frameCounter++;

            if (NPC.frameCounter >= 7)
            {
                NPC.frameCounter = 0;

                NPC.frame.Y += frameHeight;

                if (Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 1);
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
                }

                if (NPC.frame.Y == frameHeight * 4)
                {
                    if (SpitAttack && NPC.frame.X == NPC.frame.Width)
                    {
                        SoundEngine.PlaySound(SoundID.Item65 with { PitchVariance = 0.4f, Pitch = -0.15f }, NPC.Center);

                        for (int i = 0; i < 12; i++)
                        {
                            int dust = Dust.NewDust(NPC.Center + new Vector2(NPC.direction * (2 * stretchWidth), 6 * stretchHeight), 0, 0, 124, newColor: Color.Tan, Scale: 1.5f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(2, 5.1f), 0).RotatedBy(i * MathHelper.TwoPi / 12);
                        }

                        Vector2 shootPoint = target.Top + new Vector2(0, -Math.Abs(target.Center.X - NPC.Center.X ) / 1.5f);

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.direction * (2 * stretchWidth), 6 * stretchHeight), NPC.DirectionTo(shootPoint + target.velocity * (NPC.Distance(shootPoint) / 4)) * 8, ProjectileType<HauntedSandBall>(), 16, 2f, Main.myPlayer);
                        SpitAttack = false;

                        NPC.netUpdate = true;
                    }
                    else SoundEngine.PlaySound(SoundID.Item32, NPC.Center);

                    NPC.velocity += new Vector2(.05f * (Main.rand.NextBool() ? 1 : -1), -.075f);
                }
            }

            if (NPC.frame.Y >= frameHeight * 7)
            {
                NPC.frame.X = SpitAttack ? NPC.frame.Width : 0;
                NPC.frame.Y = 0;
            }
        }

        Vector2 moveTo;

        public override void AI()
        {
            // Basic AI 

            if (contactDamage == 0) contactDamage = NPC.GetAttackDamage_ScaledByStrength(20);

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            Vector2 HybridGround = BasicUtils.findGroundUnder(new Vector2(target.Center.X, NPC.Center.Y));
            bool collision = CheckTileCollision();

            NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

            SlimeSquish(1 + (float)Math.Sin(SquishTimer / 4) * 0.1f, 1 - (float)Math.Sin(SquishTimer / 4) * 0.1f);

            SquishTimer++;

            // Enemy behaviors

            switch (AIstate)
            {
                case AttackPattern.FlyNShoot: // Follow the player

                    // Basics for choosing sides and rotation

                    NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.75f, 0.75f), 0.1f);

                    if (AITimer == 0) WhichSide = -NPC.direction;

                    if (AITimer % (Main.expertMode ? 180 : 240) == 1 && AITimer > 1)
                    {
                        WhichSide *= -1;
                        shotCount++;

                        if (shotCount == NPC.ai[0] + 1)
                        {
                            SpitAttack = true;
                            SoundEngine.PlaySound(SoundID.NPCHit52 with { Volume = 2 }, NPC.Center);
                            CombatText.NewText(NPC.getRect(), new Color(200, 150, 50), "!", false, false);
                            NPC.netUpdate = true;
                        }

                        if (shotCount > 2) shotCount = 0;
                    }

                    moveTo = new Vector2(target.Center.X + (200 * WhichSide), HybridGround.Y - 200);

                    SwarmSeparation(moveTo);

                    AITimer++;

                    if (AITimer > (Main.expertMode ? 600 : 800))
                    {
                        NPC.ai[1] = 1;
                        AITimer = 0;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.MoveBelow: // Go underground

                    NPC.rotation = NPC.spriteDirection == 1 ? NPC.velocity.ToRotation() : NPC.velocity.ToRotation() + MathHelper.Pi;

                    if (AITimer <= 10)
                    {
                        moveTo = new Vector2(target.Center.X + (target.velocity.X * 2), HybridGround.Y + 250);

                        NPC.velocity.Y += 1;
                        NPC.velocity.X *= .95f;
                    }

                    if (collision)
                    {
                        if (!Surface)
                        {
                            Surface = true;
                            SoundEngine.PlaySound(SoundID.WormDig with { Volume = 1.25f }, NPC.Center);
                        }    

                        if (Main.rand.NextBool())
                        {
                            int dust = Dust.NewDust(BasicUtils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-30, 31), 0)), 0, 0, 32, Scale: 1.5f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                        }
                    }

                    SwarmSeparation(moveTo);

                    AITimer++;

                    if (AITimer >= 180)
                    {
                        NPC.velocity *= .25f;
                        NPC.velocity.Y -= 18;

                        NPC.ai[1] = 2;
                        AITimer = 0;
                        NPC.netUpdate = true;
                    }

                    break;

                case AttackPattern.MoveUp:

                    if (AITimer == 0) NPC.damage = contactDamage;

                    NPC.rotation = NPC.spriteDirection == 1 ? NPC.velocity.ToRotation() : NPC.velocity.ToRotation() + MathHelper.Pi;

                    if (collision && Main.rand.NextBool())
                    {
                        int dust = Dust.NewDust(BasicUtils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-30, 31), 0)), 0, 0, 32, Scale: 1.5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity.Y -= Main.rand.NextFloat(1, 4);
                    }

                    if (!collision && Surface)
                    {
                        Surface = false;
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss, NPC.Center);

                        int numDusts = 8;
                        for (int i = 0; i < numDusts; i++)
                        {
                            int dust = Dust.NewDust(BasicUtils.findSurfaceAbove(NPC.Center + new Vector2(Main.rand.Next(-30, 31), 0)), 0, 0, 32, Scale: 2f);
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].velocity.X = Main.rand.NextFloat(-1, 2);
                            Main.dust[dust].velocity.Y -= Main.rand.NextFloat(2, 6);
                        }                       
                    }

                    NPC.velocity *= .98f;
                    AITimer++;

                    if (AITimer > 60)
                    {
                        NPC.ai[1] = 0;
                        AITimer = 0;
                        NPC.damage = 0;

                        NPC.netUpdate = true;
                    }

                    break;
            }
        }

        private bool CheckTileCollision()
        {
            int minTilePosX = (int)(NPC.Left.X / 16) - 1;
            int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
            int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
            int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;

            // Ensure that the tile range is within the world bounds
            if (minTilePosX < 0)
                minTilePosX = 0;
            if (maxTilePosX > Main.maxTilesX)
                maxTilePosX = Main.maxTilesX;
            if (minTilePosY < 0)
                minTilePosY = 0;
            if (maxTilePosY > Main.maxTilesY)
                maxTilePosY = Main.maxTilesY;

            bool collision = false;

            // This is the initial check for collision with tiles.
            for (int i = minTilePosX; i < maxTilePosX; ++i)
            {
                for (int j = minTilePosY; j < maxTilePosY; ++j)
                {
                    Tile tile = Main.tile[i, j];

                    // If the tile is solid or is a filled liquid, then there's valid collision
                    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] || tile.LiquidAmount > 64)
                    {
                        Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);

                        if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y && NPC.Top.Y < tileWorld.Y + 16)
                        {
                            // Collision found
                            collision = true;

                            if (Main.rand.NextBool(100))
                                WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
                        }
                    }
                }
            }

            return collision;
        }

        private void SwarmSeparation(Vector2 goalPosition)
        {
            //boids
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            int count = 0;

            foreach (NPC otherNPC in Main.ActiveNPCs)
            {
                if (otherNPC.whoAmI != NPC.whoAmI && otherNPC.type == Type && (NPC.Center - otherNPC.Center).Length() < 200f)
                {
                    count++;

                    //separation component
                    separation += 133f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

                    //alignment component
                    alignment += 1 / 9f * (otherNPC.velocity - NPC.velocity);

                    //cohesion component
                    cohesion += 1 / 20f * (otherNPC.Center - NPC.Center);
                }
            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            Vector2 goalVelocity = NPC.velocity + separation + alignment + cohesion + (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            if (goalVelocity.Length() > 8)
            {
                goalVelocity.Normalize();
                goalVelocity *= 4;
            }

            NPC.velocity *= .9875f;
            NPC.velocity += (goalVelocity - NPC.velocity) / 10;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.SandBlock, 1, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemID.AncientCloth, 2));
            npcLoot.Add(ItemDropRule.Common(ItemType<HauntedSand>(), 5));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int numDusts = 4;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 2);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
            }

            if (NPC.life <= 0)
            {
                numDusts = 16;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 2);
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.67f, 1.68f), Main.rand.NextFloat(-1.67f, 1.68f));
                }
            }
        }

        public void SlimeSquish(float width, float height)
        {
            stretchWidth = MathHelper.SmoothStep(stretchWidth, width, .2f);
            stretchHeight = MathHelper.SmoothStep(stretchHeight, height, .2f);
        }

        float timer;
        float outlineVis;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;

            float SpriteWidth = NPC.scale * stretchWidth;
            float SpriteHeight = NPC.scale * stretchHeight;

            float time = Main.GameUpdateCount * 0.05f;
            float glowbrightness = (float)MathF.Sin(NPC.whoAmI - time);

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            outlineVis = MathHelper.Lerp(outlineVis, NPC.ai[1] == 0 ? 1 : 0, .15f);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(mask, drawPos + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(180, 50, 220, 100) * glowbrightness * outlineVis, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), effects, 0f);

            return false;
        }
    }
}
