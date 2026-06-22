using Terraria;
using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.Items.Equips
{
    class FunkyFruit : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileType<MiniCactusSlime>();
            Item.width = 16;
            Item.height = 30;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 5);
            Item.buffType = BuffType<MiniCactusSlimeBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            return false;
        }
    }

    class MiniCactusSlimeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ProjectileType<MiniCactusSlime>());
        }
    }

    class MiniCactusSlime : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 6;
          
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 1, 1, false)
                .WithOffset(-2, 2)
                .WithSpriteDirection(1)
                .WhenSelected(0, 4, 5, false)
                .WithCode(DelegateMethods.CharacterPreview.SlimePet);
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 38;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.scale = 1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.netImportant = true;
        }

        Vector2 goalPos;
        bool chase;
        bool grounded;
        bool animfinished;

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.Y >= 0)
            {
                Projectile.velocity.Y = 0;
                grounded = true;
            }

            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            Player owner = Main.player[Projectile.owner];

            fallThrough = owner.Top.Y > Projectile.Bottom.Y ? true : false;

            return true;
        }

        private void CheckActive(Player player)
        {
            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff
            if (!player.dead && player.HasBuff(BuffType<MiniCactusSlimeBuff>()))
            {
                Projectile.timeLeft = 2;
            }
        }

        public override void AI() // Rough replica of Prince Slime AI.
        {
            Player owner = Main.player[Projectile.owner];

            goalPos = owner.Center - new Vector2(60 * owner.direction, 0);

            CheckActive(owner);

            switch (Projectile.ai[0])
            {
                case 0: // Passive following. Start hopping towards the player 

                    Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y += 0.4f, -6, 8);

                    if (Projectile.Distance(owner.Center) > 100) chase = true;
                    else if (Projectile.Distance(goalPos) < 12 && chase) chase = false;

                    if (chase)
                    {
                        Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

                        Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X += 0.15f * (Projectile.Center.X < goalPos.X ? 1 : -1), -20, 20);
                        if (Projectile.Center.X.CloseTo(goalPos.X, 20)) Projectile.velocity.X *= .94f;
                        else Projectile.velocity.X *= .98f;
                    }
                    else Projectile.spriteDirection = owner.Center.X < Projectile.Center.X ? -1 : 1;

                    if (grounded) // Controls animations and movement while on the ground.
                    {
                        if (chase)
                        {
                            Projectile.frame = 0;

                            Projectile.velocity.Y -= 6;
                            grounded = false;
                            animfinished = false;
                        }
                        else
                        {
                            Projectile.velocity.X *= 0.8f;

                            if (!animfinished) Projectile.frameCounter++;

                            if (Projectile.frameCounter >= 5)
                            {
                                Projectile.frameCounter = 0;
                                if (Projectile.frame < 6) Projectile.frame++;

                                if (Projectile.frame >= 6) 
                                {
                                    Projectile.frame = 0;
                                    animfinished = true;
                                }
                            }

                        }
                    }
                    else // Jump animation.
                    {
                        animfinished = false;
                        Projectile.frameCounter++;

                        if (Projectile.frameCounter >= 5)
                        {
                            Projectile.frameCounter = 0;
                            if (Projectile.frame < 3) Projectile.frame++;
                        }
                    }

                    if (Projectile.Distance(owner.Center) > 500 || owner.Center.Y + 200 < Projectile.Center.Y) // start flying.
                    {
                        Projectile.ai[0] = 1;
                        Projectile.tileCollide = false;
                    }

                    break;

                case 1: // Flying

                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= 5)
                    {
                        Projectile.frameCounter = 0;
                        if (Projectile.frame < 6) Projectile.frame++;

                        if (Projectile.frame >= 6) Projectile.frame = 0;
                    }

                    Projectile.velocity += Projectile.DirectionTo(goalPos + owner.velocity) * 0.5f;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
                    Projectile.velocity *= 0.98f;

                    if (Main.rand.NextBool(5))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 40, Scale: .5f);
                        dust.noGravity = false;
                        dust.alpha = 140;
                        dust.fadeIn = 1.2f;
                    }

                    if (Projectile.Distance(goalPos) < 60)
                    {
                        Projectile.ai[0] = 0;
                        Projectile.tileCollide =  true;
                        Projectile.rotation = 0;
                    }

                    break;
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameWidth = texture.Width / 2;
            int frameY = frameHeight * Projectile.frame;
            int frameX = Projectile.ai[0] == 0 ? 0 : texture.Width / 2;

            Rectangle sourceRectangle = new Rectangle(frameX, frameY, frameWidth, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;

            if (Projectile.spriteDirection < 0) effects = SpriteEffects.None;
            else effects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, lightColor, Projectile.rotation, origin, 1, effects, 0);

            return false;
        }
    }
}
