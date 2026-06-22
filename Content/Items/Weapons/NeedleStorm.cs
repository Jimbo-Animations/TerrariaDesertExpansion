using Terraria.DataStructures;
using Terraria.Enums;
using TerrariaDesertExpansion.Content.Items.Materials;

namespace TerrariaDesertExpansion.Content.Items.Weapons
{
    class NeedleStorm : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(42);
            Item.UseSound = SoundID.Item156;
            Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(silver: 50));

            Item.DefaultToMagicWeapon(ProjectileType<NeedleStormProjectile>(), 30, 20, true);

            Item.SetWeaponValues(10, 1);
            Item.mana = 8;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 target = Main.screenPosition + new Vector2(Main.mouseX + Main.rand.NextFloat(-75, 76), Main.mouseY + Main.rand.NextFloat(-100, 51));
            float ceilingLimit = target.Y;
            if (ceilingLimit > player.Center.Y - 200f)
            {
                ceilingLimit = player.Center.Y - 200f;
            }
            // Loop these functions 3 times.
            for (int i = 0; i < 5; i++)
            {
                position = player.Center - new Vector2(Main.rand.NextFloat(451) * player.direction, 600f);
                position.Y -= 100 * i;
                Vector2 heading = target - position;

                if (heading.Y < 0f)
                {
                    heading.Y *= -1f;
                }

                if (heading.Y < 20f)
                {
                    heading.Y = 20f;
                }

                heading.Normalize();
                heading *= velocity.Length();
                Projectile.NewProjectile(source, position, heading, type, damage, knockback, player.whoAmI, ceilingLimit);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<DesertAloe>(), 1)
                .AddIngredient(ItemID.Gel, 5)
                .AddIngredient(ItemID.Cactus, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    class NeedleStormProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(11);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 360;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.ArmorPenetration = 10;
        }

        public override void AI()
        {
            opacity = MathHelper.Lerp(opacity, 1, 0.2f);

            if (Projectile.Center.Y < Projectile.ai[0] && Projectile.tileCollide == false) Projectile.tileCollide = true;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.1f;
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

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, lightColor * (1 - i / (float)Projectile.oldPos.Length) * 0.99f * opacity, Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * 0.99f, effects, 0);
            }

            return false;
        }
    }
}
