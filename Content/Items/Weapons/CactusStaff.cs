using System.Collections.Generic;
using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.Items.Weapons
{
    class CactusStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.knockBack = 2f;
            Item.mana = 10;
            Item.Size = new Vector2(48);
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.sentry = true;

            Item.shoot = ProjectileType<CactusStaffSentry>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 groundPos = Main.MouseWorld.findGroundUnder();

            position = new Vector2(groundPos.X, groundPos.Y - 24);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            player.UpdateMaxTurrets();

            return false;
        }
    }

    class CactusStaffSentry : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.Size = new Vector2(30, 50);
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 35;
        }

        int blinkFrame = 0;
        int blinkTimer = 0;
        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(target.Center) * 10, ProjectileType<CactusStaffSpike>(), 1, 2, Projectile.owner);
            Projectile.netUpdate = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            int numDusts = 12;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 291, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.ai[1]++;

            Projectile.position = Projectile.position.findGroundUnder() - new Vector2(0, 50);

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            if (foundTarget) Projectile.spriteDirection = targetCenter.X > Projectile.Center.X ? 1 : -1;

            int index = 0;
            for (int i = 0; i < Projectile.whoAmI; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    index++;
                }
            }

            if (Projectile.ai[1] % 5 == 1)
            {
                blinkFrame = 0;
                blinkTimer++;

                if (Main.rand.NextBool(10) && blinkTimer > 12)
                {
                    blinkFrame = 1;
                    blinkTimer = 0;
                }

                Projectile.frame++;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 500f;
            targetCenter = Projectile.position;
            foundTarget = false;

            // This code is required if your minion weapon has the targeting feature
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);

                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 1000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 500f;

                        if ((closest && inRange || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameWidth = texture.Width / 2;
            int frameY = frameHeight * Projectile.frame;
            int frameX = frameWidth * blinkFrame;

            Rectangle sourceRectangle = new Rectangle(frameX, frameY, frameWidth, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;

            if (Projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            else effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor, 0, origin, 1, effects, 0);

            return false;
        }
    }

    class CactusStaffSpike : ModProjectile
    {
        // Are we sticking to a target?
        public bool IsStickingToTarget
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        // Index of the current target
        public int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public float StickTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(10);

            Projectile.friendly = true;
            Projectile.timeLeft = 5;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (IsStickingToTarget)
            {
                StickTimer++;
                Projectile.timeLeft = 2;

                int npcTarget = TargetWhoAmI;
                if (StickTimer >= 300 || npcTarget < 0 || npcTarget >= 200) Projectile.Kill();

                else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
                {
                    Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
                }
                else Projectile.Kill();
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
            }
        }

        private const int MaxStickingJavelin = 3;
        private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin];

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            IsStickingToTarget = true; // we are sticking to a target
            TargetWhoAmI = target.whoAmI; // Set the target whoAmI
            Projectile.velocity = (target.Center - Projectile.Center) * .75f;
            Projectile.netUpdate = true;
            Projectile.damage = 0;

            Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, stickingJavelins);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Vector2 usePos = Projectile.position;

            Vector2 rotationVector = (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2();
            usePos += rotationVector * 16f;

            // Spawn some dusts upon javelin death
            for (int i = 0; i < 5; i++)
            {
                // Create a new dust
                Dust dust = Dust.NewDustDirect(usePos, Projectile.width, Projectile.height, 291);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.velocity += rotationVector * 2f;
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                usePos -= rotationVector * 8f;
            }
        }
    }
}
