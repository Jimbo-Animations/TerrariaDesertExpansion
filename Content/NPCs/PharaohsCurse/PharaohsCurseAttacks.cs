using Terraria.Graphics.CameraModifiers;
using TerrariaDesertExpansion.Content.NPCs.DustDevil;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    partial class PharaohsCurse : ModNPC
    {
        public void Intro()
        {

        }

        public void Reposition(bool phase2 = false) // Strafe the player briefly
        {
            if (AITimer == 1)
            {
                int direction = Main.rand.NextBool() ? 1 : -1;

                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, NPC.Center);
                NPC.velocity = new Vector2(0, Main.rand.NextFloat(6, 13) * direction).RotatedBy(NPC.DirectionTo(target.Center).ToRotation() - (MathHelper.PiOver4 * direction));
                NPC.netUpdate = true;
            }

            if (AITimer > 50)
            {
                AITimer = 0;
                PickAttack();
            }

            NPC.rotation = NPC.rotation.AngleTowards(MathHelper.Clamp(NPC.velocity.X * .05f, -.75f, .75f), .2f);
            NPC.velocity *= .98f;
        }

        public void ChargeAttack(bool phase2 = false)
        {
            if (AITimer <= 35) // The boss backpedals and telegraphs its attack.
            {
                if (AITimer == 1)
                {
                    Main.NewText("Charge");
                    SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = 1.5f, Pitch = -.3f }, NPC.Center);

                    if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                    NPC.velocity = new Vector2(5, 0).RotatedBy(NPC.DirectionTo(target.Center + new Vector2(-400 * NPC.direction, 0)).ToRotation());
                    eyeMode = true;
                    NPC.netUpdate = true;
                }

                NPC.velocity *= .97f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), .2f);
            }

            if (AITimer == 45)
            {
                NPC.velocity = new Vector2(30, 0).RotatedBy(NPC.rotation);
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Volume = 1.5f, Pitch = -.3f }, NPC.Center);
                useContactDamage = true;
                changeDirection = false;
                NPC.netUpdate = true;
            }

            if (AITimer > 55 && !BasicUtils.CloseTo(NPC.velocity.ToRotation(), NPC.DirectionTo(target.Center).ToRotation(), MathHelper.PiOver2) && NPC.Distance(target.Center) > 100)
            {
                NPC.velocity *= .9f;
                AITimer2++;

                if (AITimer2 > 30)
                {
                    AITimer = 0;
                    AITimer2 = 0;
                    eyeMode = false;
                    changeDirection = true;
                    useContactDamage = false;

                    if (NPC.direction == 1) NPC.rotation = 0;

                    PickAttack();
                }
            }
        }

        public void GroundPound(bool phase2 = false)
        {
            if (AITimer == 1) // Ready for ground pound.
            {
                Main.NewText("Ground Pound");

                changeDirection = false;
                eyeMode = true;
                if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                float velocityAdj = MathHelper.Clamp((800 - BasicUtils.CastLength(NPC.Center, new Vector2(NPC.direction, 2), 700, target.Bottom.Y < NPC.Top.Y)) * .01f, 1, 8);

                NPC.velocity = new Vector2(NPC.direction, -2 * velocityAdj);

                SoundEngine.PlaySound(SoundID.Zombie39 with { Volume = 2f, Pitch = -.2f }, NPC.Center);
                NPC.netUpdate = true;
            }

            NPC.rotation = NPC.rotation.AngleTowards(new Vector2(-NPC.direction * 2, 3).ToRotation(), .2f);
            if (NPC.noTileCollide) NPC.velocity *= .98f; // Only lose velocity when not actively ground-pounding.

            if (AITimer == 40) // Thrust downwards at an angle.
            {
                NPC.velocity = new Vector2(-NPC.direction * 10, 20);
                useContactDamage = true;
                NPC.noTileCollide = false;
            }

            if ((NPC.velocity.Y == 0 || NPC.collideY) && !NPC.noTileCollide && AITimer > 40 && AITimer <= 90) // Bounce when hitting the ground and create rubble.
            {
                NPC.position = NPC.oldPosition;

                SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(0f, 1f), 10f, 10f, 30, 1000f, "PharaohCurse");
                Main.instance.CameraModifiers.Add(modifier);

                for (int i = 0; i < 4; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), BasicUtils.findGroundUnder(NPC.Center + new Vector2(-NPC.direction * 32 * i, 0)),
                        new Vector2(-NPC.direction * i * 2, -1.5f + ( i * .5f)), ProjectileType<SandBarrier>(), 15, 1, target.whoAmI);
                        Main.projectile[proj].ai[2] = 1;
                    }
                }

                int numDusts = 40;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(BasicUtils.findGroundUnder(NPC.Center + new Vector2(Main.rand.Next(1, 4) * i * -NPC.direction, 0)), 0, 0, 32, Scale: 2f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].velocity.X = Main.rand.NextFloat(-1, 2);
                    Main.dust[dust].velocity.Y -= Main.rand.NextFloat(2, 6);
                }

                NPC.velocity = new Vector2(NPC.velocity.X * .5f, NPC.velocity.Y * -.5f);
                useContactDamage = false;
                NPC.noTileCollide = true;
                NPC.netUpdate = true;
            }

            if (AITimer > 100)
            {
                AITimer = 0;
                eyeMode = false;
                changeDirection = true;
                NPC.noTileCollide = true; 

                if (NPC.direction == 1) NPC.rotation = 0;

                PickAttack();
            }
        }

        public void FanBarrage(bool phase2 = false)
        {
            if (AITimer <= 20) // Move closer to the arena center and ready to shoot.
            {
                if (AITimer == 1) 
                {
                    Main.NewText("Fan Barrage");

                    eyeMode = true;
                    if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                    if (NPC.Center != ArenaCenter) NPC.velocity = new Vector2(15, 0).RotatedBy(NPC.DirectionTo(ArenaCenter).ToRotation());

                    SoundEngine.PlaySound(SoundID.NPCHit57 with { Volume = 2f, Pitch = -.2f }, NPC.Center);
                    NPC.netUpdate = true;
                }

                NPC.velocity *= .9f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), .2f);

                if (AITimer == 20) changeDirection = false;
            }

            if (AITimer >= 45 && AITimer <= 90 && AITimer % 3 == 0) // Launch homing projectiles. Projectiles get slower and more inaccurate as the barrage continues.
            {
                SoundEngine.PlaySound(SoundID.Item116 with { Volume = 2f, Pitch = -.2f }, NPC.Center);

                var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation + MathHelper.ToRadians(AITimer2 % 2 == 0 ? AITimer2 * 3 : -AITimer2 * 3)), ProjectileType<CurseBlast>(), 18, 1, target.whoAmI);
                Main.projectile[proj].ai[0] = 25 - (AITimer2 * .5f);
                Main.projectile[proj].ai[1] = Main.rand.NextBool() ? -1 : 1;
                Main.projectile[proj].ai[2] = 15;

                AITimer2++;
                NPC.netUpdate = true;
            }

            if (AITimer > 100)
            {
                AITimer = 0;
                AITimer2 = 0;
                eyeMode = false;
                changeDirection = true;

                if (NPC.direction == 1) NPC.rotation = 0;

                PickAttack();
            }
        }

        public void TeleportAttack(bool phase2 = false)
        {
            if (AITimer == 1) // Teleport to 1 of 4 sides of the map
            {
                Main.NewText("Teleport Attack");
                SoundEngine.PlaySound(SoundID.NPCDeath18 with { Volume = 2f, Pitch = -.1f }, NPC.Center);

                eyeMode = true;
                changeDirection = false;

                ArenaDir = Main.rand.Next(0, 4);
                NPC.position = ArenaCenter + new Vector2(350, 0).RotatedBy(MathHelper.PiOver2 * ArenaDir) - NPC.Size / 2;

                NPC.rotation = NPC.DirectionTo(ArenaCenter).ToRotation();

                NPC.netUpdate = true;
            }

            NPC.direction = NPC.spriteDirection = ArenaCenter.X > NPC.Center.X ? -1 : 1;

            if (AITimer % 40 == 30) // Shoot a spread of 3-4 big projectiles 
            {
                SoundEngine.PlaySound(SoundID.Item116 with { Volume = 2f, Pitch = -.2f }, NPC.Center);

                int projCount = 3 + (int)AITimer2;

                for (int i = -projCount / 2; i < projCount / 2; i++)
                {
                    var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation + (MathHelper.PiOver2 / projCount * i)), ProjectileType<CurseBlast>(), 20, 1, target.whoAmI);
                    Main.projectile[proj].ai[0] = 15;
                    Main.projectile[proj].ai[1] = Main.rand.NextBool() ? -1 : 1;
                    Main.projectile[proj].ai[2] = 30;
                }

                NPC.netUpdate = true;
            }

            if (AITimer % 40 == 1 && AITimer > 1) // if repeated enough times, end attack. Otherwise, repeat.
            {
                if (AITimer2 > (phase2 ? 1 : 0))
                {
                    AITimer = 0;
                    AITimer2 = 0;
                    eyeMode = false;
                    changeDirection = true;

                    if (NPC.direction == 1) NPC.rotation = 0;

                    PickAttack();
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath18 with { Volume = 2f, Pitch = -.1f }, NPC.Center);

                    AITimer2++;
                    ArenaDir = Main.rand.NextBool() ? -1 : 1;
                    NPC.position += NPC.DirectionTo(ArenaCenter).RotatedBy(MathHelper.PiOver4 * ArenaDir);
                    NPC.rotation = NPC.DirectionTo(ArenaCenter).ToRotation();

                    NPC.netUpdate = true;
                }
            }
        }

        public void Sandnadoes(bool phase2 = false)
        {

        }

        public void ChaseAndShoot(bool phase2 = false)
        {

        }

        public void ArenaSpawn(bool phase2 = false) // Stop and create a ring of sand tornado barriers
        {
            if (AITimer == 1)
            {
                Main.NewText("Spawn arena");
                SoundEngine.PlaySound(SoundID.Zombie40 with { Volume = 2f, Pitch = -.3f }, NPC.Center);

                ArenaDir = Main.rand.NextBool() ? 1 : -1;
                NPC.position = ArenaCenter - NPC.Size / 2;
                NPC.netUpdate = true;
            }

            shakeFrequency = MathHelper.SmoothStep(shakeFrequency, AITimer <= 75 ? .25f : .05f, .175f);

            NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 4) * shakeFrequency, .2f);
            NPC.velocity *= .9f;

            if (AITimer >= 60 && AITimer <= 150 && AITimer % 30 == 0)
            {
                int projCount = 36;

                for (int i = 0; i < projCount; i++)
                {
                    if (i % 3 == AITimer2)
                    {
                        int barrier = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(ArenaRadius, 0).RotatedBy(MathHelper.TwoPi / projCount * i), Vector2.Zero, ProjectileType<SandBarrier>(), 30, 2f, Main.myPlayer, NPC.whoAmI);
                        Main.projectile[barrier].timeLeft = ArenaDuration + 180 - (int)AITimer;
                        Main.projectile[barrier].ai[0] = ArenaDir;
                        Main.projectile[barrier].ai[1] = ArenaRadius + 16;
                    }
                }

                AITimer2++;
                NPC.netUpdate = true;
            }

            if (AITimer > 150)
            {
                ArenaTimer = 0;
                AITimer = 0;
                AITimer2 = 0;
                shakeFrequency = .05f;
                PickAttack();
            }
        }

        public void Phase2()
        {

        }

        public void Phase3()
        {

        }

        public void DefeatAnim()
        {

        }
    }
}
