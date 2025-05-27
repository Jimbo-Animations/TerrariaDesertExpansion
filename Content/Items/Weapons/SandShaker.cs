using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;

namespace TerrariaDesertExpansion.Content.Items.Weapons
{
    class SandShaker : ModItem
    {
        public override void SetDefaults()
        {
            Item.Size = new Vector2(70);
            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.useTime = Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.knockBack = 6;
            Item.autoReuse = true;
            Item.damage = 20;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileType<SandShakerProj>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);
            return false;
        }

        public override bool MeleePrefix()
        {
            return true;
        }
    }

    class SandShakerProj : ModProjectile
    {
        // We define some constants that determine the swing range of the sword
        // Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
        // You could change the values or even replace them entirely, but they are tweaked with looks in mind
        private const float SWINGRANGE = (float)Math.PI; // The angle a swing attack covers (300 deg)
        private const float FIRSTHALFSWING = 0.6f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float WINDUP = 0.1f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
        private const float UNWIND = 0.2f; // When should the sword start disappearing

        private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
        {
            Prepare,
            Execute,
            Unwind
        }
        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // reset the timer when the projectile switches states
            }
        }

        // Variables to keep track of during runtime
        private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
        private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
        private ref float Size => ref Projectile.localAI[2]; // Size of sword

        // We define timing functions for each stage, taking into account melee attack speed
        // Note that you can change this to suit the need of your projectile
        private float prepTime => 19f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float execTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float hideTime => 19f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(78);
            Projectile.friendly = true;
            Projectile.timeLeft = 10000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

            if (Projectile.spriteDirection == 1)
            {
                // However, we limit the rangle of possible directions so it does not look too ridiculous
                targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
            }
            else
            {
                if (targetAngle < 0)
                {
                    targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
                }

                targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
            }

            InitialAngle = targetAngle - (FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection); // Otherwise, we calculate the angle
            Size = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        { 
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        public override void AI()
        {
            // Extend use animation until projectile is killed
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            switch (CurrentStage)
            {
                case AttackStage.Prepare:
                    PrepareStrike();
                    break;
                case AttackStage.Execute:
                    ExecuteStrike();

                    Vector2 usePos = Projectile.Center + new Vector2(12, 0).RotatedBy(Projectile.rotation) - new Vector2(4);

                    Vector2 rotationVector = Projectile.rotation.ToRotationVector2();
                    usePos += rotationVector * 32f;

                    for (int i = 0; i < 8; i++)
                    {
                        if (Main.rand.NextBool(3))
                        {
                            Dust dust = Dust.NewDustDirect(usePos, 4, 4, 291);
                            dust.noGravity = true;
                        }

                        usePos -= rotationVector * 4f;
                    }

                    break;
                default:
                    UnwindStrike();
                    break;
            }

            SetSwordPosition();
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;

            if (Projectile.spriteDirection > 0)
            {
                origin = new Vector2(Projectile.width * .128f, Projectile.height * .872f);
                rotationOffset = MathHelper.ToRadians(45f);
                effects = SpriteEffects.None;
            }
            else
            {
                origin = new Vector2(Projectile.width * .872f, Projectile.height * .872f);
                rotationOffset = MathHelper.ToRadians(135f);
                effects = SpriteEffects.FlipHorizontally;
            }

            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);
            return false;
        }

        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * .872f * Projectile.scale);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
        }

        public override void CutTiles()
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * .872f * Projectile.scale);
            Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
        }

        public override bool? CanDamage()
        {
            if (CurrentStage == AttackStage.Prepare)
                return false;
            return base.CanDamage();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.collideX && !target.collideY || target.noGravity == true)
            {
                target.velocity.Y += 20 * target.knockBackResist;
            }
            else
            {
                PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, new Vector2(Projectile.direction, 1f), 3f, 3f, 10, 500f, "SandShaker");
                Main.instance.CameraModifiers.Add(modifier);

                for (int i = 0; i < 20; i++)
                {
                    int dust = Dust.NewDust(BasicUtils.findGroundUnder(target.Center + new Vector2(Main.rand.NextFloat(-30, 31), 0)), 0, 0, 124, Scale: 1f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

            if (!target.collideX && !target.collideY) modifiers.Knockback -= 1;

            if (target.collideX || target.collideY || (target.noGravity == false && target.velocity.Y == 0))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        // Function to easily set projectile and arm position
        public void SetSwordPosition()
        {
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; 
         
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);

            armPosition.Y += Owner.gfxOffY;
            Projectile.Center = armPosition - new Vector2(10, 0).RotatedBy(Projectile.rotation);
            Projectile.scale = Size * Owner.GetAdjustedItemScale(Owner.HeldItem);

            Owner.heldProj = Projectile.whoAmI;
        }

        private void PrepareStrike()
        {
            Progress = MathHelper.SmoothStep(0, WINDUP * SWINGRANGE, (1f - Timer / prepTime));

            if (Timer >= prepTime)
            {
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Pitch = -.5f });
                CurrentStage = AttackStage.Execute;
            }
        }

        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

            if (Timer >= execTime)
            {
                CurrentStage = AttackStage.Unwind;
            }
        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);

            if (Timer >= hideTime)
            {
                Projectile.Kill();
            }
        }
    }
}
