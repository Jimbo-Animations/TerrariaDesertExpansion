using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using TerrariaDesertExpansion.Content.Items.Equips;
using TerrariaDesertExpansion.Content.Items.Materials;
using TerrariaDesertExpansion.Content.Items.Weapons;
using TerrariaDesertExpansion.Systems;

namespace TerrariaDesertExpansion.Content.NPCs.CactusSlime
{
    partial class MegaCactusSlime : ModNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("This gluttonous slime consumed too much cactus, and has developed a botanical form. The spikes it grows within are formed out of gel, using the fibers absorbed from its favorite food.")
            });
        }

        public override void BossHeadRotation(ref float rotation)
        {
            rotation = NPC.rotation;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            if (NPC.frame.Y >= frameHeight * 4)
            {
                NPC.frame.Y = 0;
            }
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return true;
        }

        private enum AttackPattern : byte
        {
            Initiate = 0,
            Hopping = 1,
            SpikeBarrage = 2,
            MiniBarrage = 3,
            Teleport = 4,
            RunAway = 5
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public ref float AITimer => ref NPC.ai[1];
        public ref float AIRandomizer => ref NPC.ai[2];
        public ref float MovementTracker => ref NPC.ai[3];
        public ref float AIModifier => ref NPC.localAI[0];

        public bool isGrounded;
        public int contactDamage;
        public int jumpDuration;
        public float auraAlpha;
        public float stretchWidth = 1;
        public float stretchHeight = 1;
        public Vector2 goalPosition;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(isGrounded);
            writer.Write(contactDamage);
            writer.Write(jumpDuration);
            writer.Write(auraAlpha);
            writer.Write(stretchWidth);
            writer.Write(stretchHeight);
            writer.WriteVector2(goalPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            isGrounded = reader.ReadBoolean();
            contactDamage = reader.ReadInt32();
            jumpDuration = reader.ReadInt32();
            auraAlpha = reader.ReadInt32();
            stretchWidth = reader.ReadInt32();
            stretchHeight = reader.ReadInt32();
            goalPosition = reader.ReadVector2();
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cactus, 1, 15, 20));
            npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 20, 30));
            npcLoot.Add(ItemDropRule.Common(ItemType<SandShaker>(), 1));
            npcLoot.Add(ItemDropRule.Common(ItemType<CactusLamp>(), 1));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int numDusts = 5;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, newColor: Color.LightGreen, Scale: 1);
                Main.dust[dust].noLight = true;
                Main.dust[dust].alpha = 180;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2.0f, 2.1f), Main.rand.NextFloat(-2.0f, 2.1f));              
            }

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(4, 8)).RotatedByRandom(MathHelper.PiOver2), ProjectileType<CactusSlimeSpike>(), 0, 2f, Main.myPlayer);
                }

                numDusts = 50;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.t_Slime, newColor: Color.LightGreen, Scale: 2);
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].alpha = 180;
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-1, 4));
                }

                NPC.netUpdate = true;
            }
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref Progression.DownedCactusSlime, -1);
        }

        public override void PostAI()
        {
            int goalDirection = target.Center.X < NPC.Center.X ? -1 : 1;
            NPC.velocity.Y += 0.33f;
            NPC.velocity *= 0.99f;

            NPC.EncourageDespawn(30);

            if (target.dead || !target.active || !Main.dayTime)
            {
                NPC.dontTakeDamage = true;
                NPC.ai[0] = 5;
                resetVars();
            }

            contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 60 : 50 : 40 : 30;
            AIModifier = Main.expertMode ? Main.getGoodWorld ? 0.6f : 0.8f : 1;

            if (NPC.velocity.Y > 0 && !isGrounded) 
            {
                NPC.velocity.X += 0.001f * -goalDirection;
                jumpDuration++;

                if (jumpDuration > 200)
                {
                    NPC.ai[0] = 4;
                    resetVars();
                }
            }

            if (NPC.velocity.Y > 0 && NPC.collideY && !isGrounded && NPC.ai[0] != 4)
            {
                SoundEngine.PlaySound(SoundID.DeerclopsStep with { Volume = 2 }, NPC.Center);

                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(0f, 1f), 3f, 3f, 10, 500f, "CactusSlime");
                Main.instance.CameraModifiers.Add(modifier);
                stretchWidth = 1.25f;
                stretchHeight = .75f;

                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, Vector2.Zero, ProjectileType<CactusSlimeShockwave>(), 10, 2f, Main.myPlayer);

                NPC.velocity = Vector2.Zero;
                isGrounded = true;
                NPC.netUpdate = true;
            }

            if (isGrounded)
            {
                NPC.rotation = 0;
                jumpDuration = 0;

                if (NPC.ai[0] == 2)
                {
                    SlimeSquish(1 + (float)Math.Sin(AITimer / 4) * 0.25f, 1 - (float)Math.Sin(AITimer / 4) * 0.25f);
                }
                else if (NPC.ai[0] != 4) SlimeSquish(1, 1);
            }
            else
            {
                NPC.rotation = NPC.velocity.Y < 0 ? NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.X * .04f, .01f) : NPC.rotation.AngleTowards(-NPC.velocity.X * .04f, .01f);
                SlimeSquish(MathHelper.Clamp(Math.Abs(NPC.velocity.Y) / 250, .8f, 1), MathHelper.Clamp(Math.Abs(NPC.velocity.Y) / 250, 1, 1.2f));
            }
        }

        public void resetVars()
        {
            AITimer = 0;
            AIModifier = 0;
            MovementTracker = 0;
            jumpDuration = 0;

            NPC.netUpdate = true;
        }

        public void SlimeSquish(float width, float height)
        {
            stretchWidth = MathHelper.SmoothStep(stretchWidth, width, .2f);
            stretchHeight = MathHelper.SmoothStep(stretchHeight, height, .2f);
        }

        private bool FindTeleportPoint(Player player)
        {
            //try up to 20 times
            for (int i = 0; i < 40; i++)
            {
                float direction = Main.rand.NextBool() ? -1 : 1;

                Vector2 tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(150f, 300f) * direction, Main.rand.NextFloat(-250f, 250f));
                tryGoalPoint.Y = 16 * (int)(tryGoalPoint.Y / 16);
                tryGoalPoint -= new Vector2(0, NPC.height);

                bool viable = true;

                for (int x = (int)((tryGoalPoint.X) / 16); x <= (int)((tryGoalPoint.X + NPC.width) / 16); x++)
                {
                    for (int y = (int)((tryGoalPoint.Y) / 16); y <= (int)((tryGoalPoint.Y + NPC.height) / 16); y++)
                    {
                        if (Main.tile[x, y].HasUnactuatedTile)
                        {
                            viable = false;
                            break;
                        }
                    }

                    if (!viable)
                    {
                        break;
                    }
                }

                if (viable)
                {
                    for (int y = (int)((tryGoalPoint.Y + NPC.height) / 16); y < Main.maxTilesY; y++)
                    {
                        int x = (int)((tryGoalPoint.X + NPC.width / 2) / 16);
                        if (Main.tile[x, y].HasUnactuatedTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolidTop[Main.tile[x, y].TileType]))
                        {
                            goalPosition = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);

                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
