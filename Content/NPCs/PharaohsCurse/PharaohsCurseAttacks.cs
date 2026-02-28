using Terraria.Graphics.CameraModifiers;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    partial class PharaohsCurse : ModNPC
    {
        public void Intro()
        {
            if (AITimer == 1) NPC.velocity -= new Vector2(0, 10); // Pop up, set the arena, and fight.

            if (AITimer == 30)
            {
                ArenaCenter = NPC.Center;
                ArenaTimer = ArenaDuration;
                NPC.netUpdate = true;
            }

            if (AITimer >= 75 && AITimer <= 255) // Do the roar.
            {
                outlineVis = MathHelper.Lerp(outlineVis, 1, .05f);

                if (AITimer == 75)
                {
                    SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan4") with { PitchVariance = .2f }, NPC.Center);
                    changeDirection = false;
                }

                if (AITimer % 15 == 0)
                {
                    shockwave.Add(new Tuple<Vector2, float, float, float>(NPC.Center, 0, 120f, shakeFrequency + .15f)); // Creates shockwaves to signal attack. Likely to get replaced.

                    PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(NPC.direction, 0f), 6f, 6f, 30, 1000f, "PharaohCurse");
                    Main.instance.CameraModifiers.Add(modifier);
                }
            }

            // Controls boss movement.
            
            opacity = MathHelper.Lerp(opacity, 1, .1f);

            shakeFrequency = MathHelper.SmoothStep(shakeFrequency, AITimer <= 195 && AITimer > 75 ? .25f : .05f, .175f);
            NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 2) * shakeFrequency, .2f);

            NPC.velocity *= .9f;

            if (AITimer > 270) // Ready the boss for the fight.
            {
                changeDirection = true;
                NPC.dontTakeDamage = false;
                shakeFrequency = .05f;
                AITimer = 0;

                ResetAIStates();
                PickAttack();
            }
        }

        public void Reposition() // Strafe the player briefly
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

        public void ChargeAttack()
        {
            if (AITimer <= (phase2 ? 25 : 35)) // The boss backpedals and telegraphs its attack.
            {
                if (AITimer == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan2") with { Pitch = .15f, PitchVariance = .2f }, NPC.Center);

                    if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                    NPC.velocity = new Vector2(5, 0).RotatedBy(NPC.DirectionTo(target.Center + new Vector2(-400 * NPC.direction, 0)).ToRotation());
                    eyeMode = true;
                    usemeleeVis = true;
                    NPC.netUpdate = true;
                }

                NPC.velocity *= phase2 ? .96f : .97f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), .2f);

                if (AITimer == 25 && phase3)
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -.2f }, NPC.Center);

                    for (int i = -2; i < 3; i++)
                    {
                        int sword = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<DesertBlade>(), 16, 2f, Main.myPlayer);
                        Main.projectile[sword].ai[0] = new Vector2(1, 0).RotatedBy(NPC.rotation + MathHelper.PiOver2 / 5 * i).ToRotation();
                    }                    
                }
            }

            if (AITimer == (phase2 ? 30 : 45))
            {
                NPC.velocity = new Vector2(phase3 ? 35 : 30, 0).RotatedBy(NPC.rotation);
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Volume = 1.5f, Pitch = -.3f }, NPC.Center);
                useContactDamage = true;
                changeDirection = false;
                NPC.netUpdate = true;
            }

            if (AITimer > (phase2 ? 40 : 55) && !BasicUtils.CloseTo(NPC.velocity.ToRotation(), NPC.DirectionTo(target.Center).ToRotation(), MathHelper.PiOver2) && NPC.Distance(target.Center) > 100)
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
                    usemeleeVis = false;

                    if (NPC.direction == 1) NPC.rotation = 0;

                    PickAttack();
                }
            }
        }

        public void GroundPound()
        {
            if (AITimer == 1) // Ready for ground pound.
            {
                changeDirection = false;
                eyeMode = true;
                usemeleeVis = true;
                if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                float velocityAdj = MathHelper.Clamp((800 - BasicUtils.CastLength(NPC.Center, new Vector2(NPC.direction, 2), 700, target.Bottom.Y < NPC.Top.Y)) * .01f, 1, 8);
                NPC.velocity = new Vector2(NPC.direction, -2 * velocityAdj);

                SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan1") with { PitchVariance = .3f }, NPC.Center);
                NPC.netUpdate = true;
            }

            NPC.rotation = NPC.rotation.AngleTowards(new Vector2(-NPC.direction * 2, 3).ToRotation(), .2f);
            if (!useContactDamage) NPC.velocity *= .98f; // Only lose velocity when not actively ground-pounding.

            if (AITimer == 40) // Thrust downwards at an angle.
            {
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, NPC.Center);

                NPC.velocity = new Vector2(-NPC.direction * 10, 20);
                useContactDamage = true;
            }          

            if (AITimer >= 40 && NPC.Center.Y > target.Top.Y && useContactDamage) NPC.noTileCollide = false;

            if ((NPC.velocity.Y == 0 || NPC.collideY) && !NPC.noTileCollide && AITimer > 40 && AITimer <= 90) // Bounce when hitting the ground and create rubble.
            {
                NPC.position = NPC.oldPosition;
                usemeleeVis = false;

                SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 2 }, NPC.Center);

                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(0f, 1f), 10f, 10f, 30, 1000f, "PharaohCurse");
                Main.instance.CameraModifiers.Add(modifier);

                for (int i = 0; i < (phase2 ? 5 : 4); i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), BasicUtils.findGroundUnder(NPC.Center + new Vector2(-NPC.direction * (phase2 ? 24 : 32) * i, 0)),
                        new Vector2(-NPC.direction * i * 2, -2.5f + (i * .5f)), ProjectileType<SandBarrier>(), 15, 1, target.whoAmI);
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

        public void FanBarrage()
        {
            if (AITimer <= 20) // Move closer to the arena center and ready to shoot.
            {
                if (AITimer == 1) 
                {
                    eyeMode = true;
                    if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                    if (NPC.Center != ArenaCenter) NPC.velocity = new Vector2(20, 0).RotatedBy(NPC.DirectionTo(ArenaCenter).ToRotation());

                    SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack, NPC.Center);
                    SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan3") with { PitchVariance = .3f }, NPC.Center);
                    NPC.netUpdate = true;
                }

                NPC.velocity *= .9f;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), .2f);

                if (AITimer == 20) changeDirection = false;
            }

            if (AITimer >= 45 && AITimer <= 90 && phase2) NPC.rotation = NPC.rotation.AngleTowards(NPC.DirectionTo(target.Center).ToRotation(), .02f); // Turn slowly if in phase 2.

            if (AITimer >= 45 && AITimer <= 90 && AITimer % 3 == 0) // Launch homing projectiles. Projectiles get slower and more inaccurate as the barrage continues.
            {
                SoundEngine.PlaySound(SoundID.Item116 with { Volume = 2f, Pitch = -.2f }, NPC.Center);

                var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation + MathHelper.ToRadians(AITimer2 % 2 == 0 ? AITimer2 * 2 : -AITimer2 * 2)), ProjectileType<CurseBlast>(), 16, 1, target.whoAmI);
                Main.projectile[proj].ai[0] = 25;
                Main.projectile[proj].ai[1] = Main.rand.NextBool() ? -1 : 1;
                Main.projectile[proj].ai[2] = 20;

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

        public void TeleportAttack()
        {
            if (AITimer % 50 == 1) // Teleport to 1 of 4 sides of the map a set amount of times.
            {
                if (AITimer == 1)
                {
                    eyeMode = true;
                    changeDirection = false;
                    ArenaDir = 0; // Always start from above
                    opacity = .5f;
                }

                if (AITimer2 > (phase2 ? 2 : 1))
                {
                    AITimer = 0;
                    AITimer2 = 0;
                    eyeMode = false;
                    changeDirection = true;
                    opacity = 1;

                    if (NPC.direction == 1) NPC.rotation = 0;

                    PickAttack();
                }
                else // Teleport to new location.
                {
                    SoundEngine.PlaySound(SoundID.Item46 with { Volume = 1.5f }, NPC.Center);

                    NPC.position = ArenaCenter + new Vector2(0, -350).RotatedBy(MathHelper.PiOver2 * ArenaDir) - NPC.Size / 2;
                    NPC.velocity = Vector2.Zero;

                    NPC.rotation = NPC.DirectionTo(ArenaCenter).ToRotation();

                    NPC.netUpdate = true;
                }
            }

            NPC.direction = NPC.spriteDirection = ArenaCenter.X > NPC.Center.X ? -1 : 1;
            opacity = MathHelper.Lerp(opacity, (AITimer % 50 > 30) ? 1 : .5f, .2f);

            if (AITimer % 50 == 30) // Shoot a spread of 3-4 big projectiles 
            {
                SoundEngine.PlaySound(SoundID.Item116 with { Volume = 2f, Pitch = -.2f }, NPC.Center);

                int projNum = AITimer2 % 2 == 1 ? 4 : 3; // Launch alternate between firing 3 and 4 projectiles.

                for (int i = 0; i < projNum; i++)
                {
                    var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation + (MathHelper.ToRadians(30 * projNum) / projNum * i) - MathHelper.ToRadians(30 * projNum / 3)), ProjectileType<CurseBlastBig>(), 20, 1, target.whoAmI);
                    Main.projectile[proj].ai[0] = 30;
                    Main.projectile[proj].ai[1] = Main.rand.NextBool() ? -1 : 1;
                    Main.projectile[proj].ai[2] = 12;
                }

                ArenaDir = ArenaDir == 0 ? Main.rand.Next(1, 4) : ArenaDir + (Main.rand.NextBool() ? 1 : -1); // Choose position for next teleport.

                AITimer2++;
                NPC.netUpdate = true;
            }
        }

        public void Sandnadoes()
        {
            // Orbiting variables to change the spin

            float SummonTimeMult = phase2 ? 18f : 20f;
            float orbitTimeMult = phase2 ? 4f : 5f;
            float distanceMult = 1 - (float)Math.Exp(-AITimer2 / SummonTimeMult);

            if (AITimer <= 30) // Ready for spin attack
            {
                if (AITimer == 1)
                {
                    changeDirection = false;
                    eyeMode = true;

                    if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;
                    int direction = Main.rand.NextBool() ? 1 : -1;

                    NPC.velocity = new Vector2(0, Main.rand.NextFloat(5, 11) * direction).RotatedBy(NPC.DirectionTo(target.Center).ToRotation() - (MathHelper.PiOver2 * direction));

                    SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan2") with { Pitch = -.2f }, NPC.Center);
                    NPC.netUpdate = true;
                }

                if (AITimer == 30) 
                {
                    PositionPoint = NPC.Center;

                    for (int i = 0;  i < 12; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center, 0, 0, 32, newColor: Color.Tan, Scale: 3);
                        Main.dust[dust].velocity = new Vector2(10, 0).RotatedBy(MathHelper.TwoPi / 12 * i);
                        Main.dust[dust].noGravity = true;
                    }
                }

                NPC.rotation += AITimer * .015f * NPC.direction;
                NPC.velocity *= .95f;
            }
            else
            {
                AITimer2++;
                NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation(), .2f);

                if (AITimer <= 120)
                {
                    NPC.velocity = -NPC.Center + PositionPoint + new Vector2(300 * distanceMult, 0).RotatedBy(.33f * (Main.GameUpdateCount / orbitTimeMult - distanceMult) * NPC.direction);

                    if (AITimer % 5 == 0)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 1);
                        Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
                        Main.dust[dust].noGravity = true;
                    }

                    if (AITimer % (phase2 ? 12 : 15) == 0) // Launch projectiles while spinning
                    {
                        SoundEngine.PlaySound(SoundID.Item103, NPC.Center);

                        var proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.position + new Vector2(Main.rand.Next(NPC.width + 1), Main.rand.Next(NPC.height + 1)), new Vector2 (1,0).RotatedBy(NPC.rotation), ProjectileType<DustVortex>(), 18, 1, target.whoAmI);
                        NPC.netUpdate = true;
                    }
                }
                else // Stop and slow down
                {
                    NPC.velocity *= .9f;

                    if (AITimer > 150)
                    {
                        AITimer = 0;
                        AITimer2 = 0;
                        eyeMode = false;
                        changeDirection = true;

                        if (NPC.direction == 1) NPC.rotation = 0;

                        PickAttack();
                    }
                }
            }
        }

        public void SwordRing()
        {
            if (AITimer == 1) // Stop and create a small ring of spinning blades around itself.
            {
                changeDirection = false;

                if (NPC.direction == 1) NPC.rotation = MathHelper.Pi;

                SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan1") with { Pitch = -.25f, PitchVariance = .2f}, NPC.Center);
                NPC.netUpdate = true;
            }

            shakeFrequency = MathHelper.SmoothStep(shakeFrequency, AITimer <= 75 ? .3f : .05f, .2f);
            useruneVis = AITimer < 90 ? true : false;

            NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 4) * shakeFrequency, .2f);
            NPC.velocity *= .9f;

            if (AITimer > 10 && AITimer <= 90 && AITimer % 9 == 0) // Spawns flames clockwise.
            {
                Vector2 swordPos = NPC.Center + new Vector2(0, phase2 ? 100 : 75).RotatedBy(MathHelper.TwoPi / (phase2 ? 20 : 10) * AITimer2) * NPC.direction; 

                int sword = Projectile.NewProjectile(NPC.GetSource_FromThis(), swordPos, Vector2.Zero, ProjectileType<DesertBlade>(), 16, 2f, Main.myPlayer);
                Main.projectile[sword].ai[0] = swordPos.DirectionTo(target.Center + target.velocity * NPC.Distance(target.Center) / 1.5f).ToRotation();

                if (phase2)
                {
                    swordPos = NPC.Center + new Vector2(0, 100).RotatedBy(MathHelper.TwoPi / 20 * (AITimer2 + 10)) * NPC.direction;

                    sword = Projectile.NewProjectile(NPC.GetSource_FromThis(), swordPos, Vector2.Zero, ProjectileType<DesertBlade>(), 16, 2f, Main.myPlayer);
                    Main.projectile[sword].ai[0] = swordPos.DirectionTo(target.Center + target.velocity * NPC.Distance(target.Center) / 1.5f).ToRotation();
                }

                SoundEngine.PlaySound(SoundID.Item71, swordPos);
                AITimer2++;

                NPC.netUpdate = true;
            }

            if (AITimer > 100)
            {
                AITimer = 0;
                AITimer2 = 0;
                shakeFrequency = .05f;

                eyeMode = false;
                changeDirection = true;

                if (NPC.direction == 1) NPC.rotation = 0;

                PickAttack();
            }
        }

        public void ArenaSpawn() // Stop and create a ring of sand tornado barriers
        {
            if (AITimer == 1)
            {
                SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan3") with { Pitch = -.3f }, NPC.Center);

                ArenaDir = Main.rand.NextBool() ? 1 : -1;
                NPC.position = ArenaCenter - NPC.Size / 2;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            shakeFrequency = MathHelper.SmoothStep(shakeFrequency, AITimer <= 75 ? .25f : .05f, .175f);
            NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 4) * shakeFrequency, .2f); 
      
            if (AITimer >= 60 && AITimer <= 150 && AITimer % 30 == 0)
            {
                int projCount = Main.expertMode ? 36 : 54;

                for (int i = 0; i < projCount; i++)
                {
                    if (i % 3 == AITimer2)
                    {
                        int barrier = Projectile.NewProjectile(NPC.GetSource_FromThis(), ArenaCenter + new Vector2(ArenaRadius, 0).RotatedBy(MathHelper.TwoPi / projCount * i), Vector2.Zero, ProjectileType<SandBarrier>(), 30, 2f, Main.myPlayer, NPC.whoAmI);
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

        public void PhaseTransition() // Quick phase 2 transition.
        {
            if (AITimer <= 90) // Do the roar.
            {
                if (AITimer == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan4") with { Volume = 1.5f, Pitch = -.1f }, NPC.Center);
                    changeDirection = false;
                }

                if (AITimer % 15 == 0)
                {
                    shockwave.Add(new Tuple<Vector2, float, float, float>(NPC.Center, 0, 120f, shakeFrequency + .15f)); // Creates shockwaves to signal attack. Likely to get replaced.

                    PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(NPC.direction, 0f), 6f, 6f, 30, 1000f, "PharaohCurse");
                    Main.instance.CameraModifiers.Add(modifier);
                }
            }

            // Controls boss movement.

            shakeFrequency = MathHelper.SmoothStep(shakeFrequency, AITimer <= 60 ? .25f : .05f, .2f);
            NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 2) * shakeFrequency, .2f);

            NPC.velocity *= .9f;

            if (AITimer > 150) // Transitions phases at the end. Done to make sure it can transition from phase 1 to 3 if need-be.
            {
                changeDirection = true;
                shakeFrequency = .05f;
                AITimer = 0;

                phase2 = true;
                if (NPC.life <= NPC.lifeMax * .3f) phase3 = true;

                ResetAIStates();
                 PickAttack();
            }
        }

        public void DefeatAnim() // Fade into nothingness.
        {
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/Nothing");
            outlineVis = MathHelper.Lerp(outlineVis, 0, .05f);

            NPC.velocity *= .9f;

            if (AITimer >= 120 && AITimer % 3 == 0) // Number of dusts used based on timer.
            {
                for (int i = 0; i < 3; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, newColor: Color.White, Scale: shakeFrequency * 8);
                    Main.dust[dust].velocity = new Vector2(Main.WindForVisuals, -shakeFrequency * 8);
                    Main.dust[dust].noGravity = true;
                }
            }

            if (AITimer >= 120)
            {
                shakeFrequency = MathHelper.SmoothStep(shakeFrequency, .25f, .1f);
                NPC.rotation = NPC.rotation.AngleTowards((float)Math.Sin(AITimer / 3) * shakeFrequency, .2f);

                if (AITimer == 180) SoundEngine.PlaySound(new SoundStyle("TerrariaDesertExpansion/Content/Music/MummyGroan4") with { Volume = 1.5f, Pitch = -.25f }, NPC.Center);

                if (AITimer > 180)
                {
                    opacity = MathHelper.SmoothStep(opacity, 0, .1f);

                    if (AITimer % 15 == 0)
                    {
                        shockwave.Add(new Tuple<Vector2, float, float, float>(NPC.Center, 0, 120f, .15f)); // Creates shockwaves to signal attack. Likely to get replaced.

                        PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(NPC.direction, 0f), 6f, 6f, 30, 1000f, "PharaohCurse");
                        Main.instance.CameraModifiers.Add(modifier);
                    }
                }
            }
            else NPC.rotation = NPC.rotation.AngleTowards(0, .1f);

            if (AITimer > 360)
            {
                NPC.NPCLoot();

                NPC.active = false;
                NPC.netUpdate = true;
            }
        }
    }
}
