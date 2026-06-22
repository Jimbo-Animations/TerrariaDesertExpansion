using Terraria.DataStructures;
using TerrariaDesertExpansion.Content.Items.Materials;
using TerrariaDesertExpansion.Systems.GlobalNPCs;

namespace TerrariaDesertExpansion.Content.Items.Weapons
{
    class AloeStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.knockBack = 2f;
            Item.Size = new Vector2(42);
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;

            Item.shoot = ProjectileType<AloeStriker>();
            Item.buffType = BuffType<AloeStaffBuff>();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position, limited by the gameplay range
            position = Main.MouseWorld;
            player.LimitPointToPlayerReachableArea(ref position);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            player.AddBuff(Item.buffType, 2);

            return true; // The minion projectile will be spawned by the game since we return true.
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<DesertAloe>(), 1)
                .AddIngredient(ItemID.Gel, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    class AloeStaffBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ProjectileType<AloeStriker>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    class AloeStriker : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetDefaults()
        {
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
        }

        // Here you can decide if your minion breaks things like grass or pots
        public override bool? CanCutTiles()
        {
            return false;
        }

        // This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
        public override bool MinionContactDamage()
        {
            return false;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<AloeStaffBuff>());

                return false;
            }

            if (owner.HasBuff(BuffType<AloeStaffBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override void AI()
        {
            // Generic, important variables

            Player owner = Main.player[Projectile.owner];

            CheckActive(owner);

            int index = 1;
            int ownedProjectiles = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    ownedProjectiles++;
                    if (i < Projectile.whoAmI)
                    {
                        index++;
                    }
                }
            }

            Vector2 IdealPos = owner.Center - new Vector2(((34 * index) + 10 / ownedProjectiles) * owner.direction, 51);
            Vector2 IdealVector = IdealPos - Projectile.Center;

            if (IdealVector.Length() > 900)
            {
                Projectile.position = IdealPos - (Projectile.Size / 2);
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            // Control movement and behavior

            Movement(IdealVector, IdealVector.Length());

            Animation(1, 1);

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            // AI[2] will be the timer variable for firing out attacks.

            if (foundTarget)
            {
                if (Projectile.ai[2] < 0) Projectile.ai[2] = 0;

                Projectile.spriteDirection = targetCenter.X > Projectile.Center.X ? 1 : -1;
                Projectile.ai[2]++;

                if (Projectile.ai[2] >= 100)
                {
                    Projectile.ai[2] = 0;
                    stretchWidth = 1.2f;
                    stretchHeight = .8f;

                    SoundEngine.PlaySound(SoundID.Item64, Projectile.Center);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(targetCenter).RotatedByRandom(MathHelper.ToRadians(5)) * 12, ProjectileType<AloeLeaf>(), Projectile.damage, 2, Projectile.owner);
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.spriteDirection = Projectile.Center.X < owner.Center.X ? 1 : -1;
                if (Projectile.ai[2] > 0) Projectile.ai[2]--;
            }
        }

        // From Tmodloader's example minion
        private void Movement(Vector2 vectorToIdlePosition, float distanceToIdlePosition)
        {
            float speed = 8f;
            float inertia = 30f;

            if (distanceToIdlePosition >= 600f) // Speed up the minion if it's away from the player
            {
                speed = 10f;
                inertia = 20f;
            }
            else if (distanceToIdlePosition <= 120f) // Slow down the minion if closer to the player
            {
                speed = 6f;
                inertia = 40f;
            }

            if (distanceToIdlePosition > 20f)
            {
                // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
            }
            else Projectile.velocity *= 0.9f;
        }
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            // Starting search distance
            distanceFromTarget = 650f;
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

        public float stretchWidth = 1;
        public float stretchHeight = 1;

        private void Animation(float width, float height)
        {
            if (Projectile.ai[1]++ % 5 == 1)
            {
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.05f, -0.33f, 0.33f);

            stretchWidth = MathHelper.SmoothStep(stretchWidth, width, .2f);
            stretchHeight = MathHelper.SmoothStep(stretchHeight, height, .2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameWidth = texture.Width;
            int frameY = frameHeight * Projectile.frame;

            float SpriteWidth = Projectile.scale * stretchWidth;
            float SpriteHeight = Projectile.scale * stretchHeight;

            Rectangle sourceRectangle = new Rectangle(0, frameY, frameWidth, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;

            if (Projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            else effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor, Projectile.rotation, origin, new Vector2(SpriteWidth, SpriteHeight), effects, 0);

            return false;
        }
    }

    class AloeLeaf : ModProjectile
    {
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

        public int GravityDelayTimer
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public float StickTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public sealed override void SetDefaults()
        {
            Projectile.Size = new Vector2(8);
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
        }

        public override void AI()
        {
            opacity = MathHelper.Lerp(opacity, 1, 0.2f);

            if (IsStickingToTarget)
            {
                Projectile.ignoreWater = true;
                Projectile.tileCollide = false;
                StickTimer += 1f;

                int npcTarget = TargetWhoAmI;
                if (StickTimer >= 200 || npcTarget < 0 || npcTarget >= 200)
                { // If the index is past its limits, kill it
                    Projectile.Kill();
                }
                else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
                {
                    // If the target is active and can take damage
                    // Set the projectile's position relative to the target's center
                    Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
                }
                else Projectile.Kill(); // Otherwise, kill the projectile
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity.Y += 0.01f;
            }
        }

        private const int MaxStickingJavelin = 5; // This is the max amount of javelins able to be attached to a single NPC
        private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin]; // The point array holding for sticking javelins
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            IsStickingToTarget = true; // we are sticking to a target
            TargetWhoAmI = target.whoAmI; // Set the target whoAmI
            Projectile.velocity = (target.Center - Projectile.Center) * 0.75f;
            Projectile.friendly = false;
            Projectile.netUpdate = true;

            target.AddBuff(BuffType<AloeLeafDebuff>(), 200);

            Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, stickingJavelins);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // By shrinking target hitboxes by a small amount, this projectile only hits if it more directly hits the target.
            // This helps the javelin stick in a visually appealing place within the target sprite.
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            // Return if the hitboxes intersects, which means the javelin collides or not
            return projHitbox.Intersects(targetHitbox);
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

        public float opacity = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            SpriteEffects effects = new SpriteEffects();
            if (Projectile.spriteDirection == 1) effects = SpriteEffects.FlipVertically;


            if (!IsStickingToTarget)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, lightColor * (1 - i / (float)Projectile.oldPos.Length) * 0.99f * opacity, Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * 0.99f, effects, 0);
                }
            }
            else
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            }

            return false;
        }
    }

    public class AloeLeafDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // NPCs will automatically be immune to this buff if they are immune to BoneJavelin. SkeletronHead and SkeletronPrime are immune to BoneJavelin.
            BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.BoneJavelin);
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AloeLeafDebuffNPC>().AloeDebuff = true;
        }
    }
}
