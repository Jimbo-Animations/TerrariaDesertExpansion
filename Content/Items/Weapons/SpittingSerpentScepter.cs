using System.Collections.Generic;
using Terraria.Enums;
using TerrariaDesertExpansion.Content.Items.Materials;

namespace TerrariaDesertExpansion.Content.Items.Weapons
{
    class SpittingSerpentScepter : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(42);
            Item.UseSound = SoundID.Item151 with { Pitch = .3f };
            Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(silver: 50));

            Item.SetWeaponValues(12, 4f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.shoot = ProjectileType<ScepterSpit>();
            Item.shootSpeed = 10;

            Item.useAnimation = 20;
            Item.useTime = 5;
            Item.reuseDelay = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.mana = 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = new TooltipLine(Mod, "Tooltip#0", "\"It has some toxic things to say!\"")
            {
                OverrideColor = new Color(120, 230, 10)
            };
            tooltips.Add(line);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {        
            velocity = velocity.RotatedBy(MathHelper.ToRadians(10) * -player.direction).RotatedByRandom(MathHelper.ToRadians(10)) * Main.rand.NextFloat(.8f, 1.3f);

            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 42f;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SerpentFang>(8)
                .AddIngredient(ItemID.Wood, 2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    /* class ScepterBite : ModProjectile
    {
        public override string Texture => "TerrariaDesertExpansion/Content/NPCs/EvilSnake/EvilSnakeBite";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;

        }
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(20);
            Projectile.scale = 1.5f;

            Projectile.friendly = true;
            Projectile.timeLeft = 30;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.96f;

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) + Projectile.velocity * Projectile.ai[0]++;

            if (Main.rand.NextBool(25))
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.PoisonStaff, Scale: 1f);
                Main.dust[dust].velocity = new Vector2(1, 0).RotatedBy(Projectile.rotation);
            }

            if (Projectile.timeLeft <= 20) opacity *= 0.92f;

            if (Projectile.timeLeft <= 10) Projectile.friendly = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + new Vector2(42, 0).RotatedBy(Projectile.rotation), new Vector2(10, 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(10) * Projectile.spriteDirection).RotatedByRandom(MathHelper.ToRadians(5)), ProjectileType<ScepterSpit>(), 1, 2, Projectile.owner);
            Projectile.netUpdate = true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.MountedCenter + new Vector2(42, 0).RotatedBy(Projectile.rotation), new Vector2(10, 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(10) * Projectile.spriteDirection).RotatedByRandom(MathHelper.ToRadians(5)), ProjectileType<ScepterSpit>(), 1, 2, Projectile.owner);
            Projectile.netUpdate = true;
        }

        public float opacity = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            SpriteEffects effects = new SpriteEffects();
            if (Projectile.spriteDirection == 1) effects = SpriteEffects.FlipVertically;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Color.White * (1 - i / (float)Projectile.oldPos.Length) * 0.99f * opacity, Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * 0.99f, effects, 0);
            }

            return false;
        }
    } */

    class ScepterSpit : ModProjectile
    {
        public override string Texture => "TerrariaDesertExpansion/Content/ExtraAssets/NoSprite";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(20);

            Projectile.friendly = true;
            Projectile.timeLeft = 60;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;

            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.2f;
            Projectile.velocity *= 0.998f;

            int dust = Dust.NewDust(Projectile.Center, 0, 0, 273, Scale: 2f);
            Main.dust[dust].velocity = new Vector2(2, 0).RotatedBy(Projectile.rotation);
            Main.dust[dust].noGravity = true;

            if (Main.rand.NextBool(15))
            {
                dust = Dust.NewDust(Projectile.Center, 0, 0, 273, Scale: 1f);
                Main.dust[dust].velocity = new Vector2(1, 0).RotatedBy(Projectile.rotation);
                Main.dust[dust].noGravity = false;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool()) target.AddBuff(BuffID.Poisoned, 600);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool()) target.AddBuff(BuffID.Poisoned, 600);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);

            for (int i = 0; i < 6; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 273, Scale: 1f);
                Main.dust[dust].velocity = new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi);
            }

        }
    }
}
