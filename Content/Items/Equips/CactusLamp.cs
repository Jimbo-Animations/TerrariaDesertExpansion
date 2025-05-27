using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace TerrariaDesertExpansion.Content.Items.Equips
{
    class CactusLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileType<CactusLampPet>();
            Item.width = 16;
            Item.height = 30;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Green;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 1, 50);
            Item.buffType = BuffType<CactusLampBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }
    }

    class CactusLampBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.projPet[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ProjectileType<CactusLampPet>());
        }
    }

    class CactusLampPet : ModProjectile
    {
        public override string Texture => "TerrariaDesertExpansion/Content/Items/Equips/CactusLamp";
        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.scale = 1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        bool flicker;
        bool chase;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Vector2 IdealPos = owner.Center + new Vector2(34 * owner.direction, -24);

            if (flicker)

            if (flicker && Main.rand.NextBool(6)) flicker = false;

            if (Main.rand.NextBool(600) && !flicker) flicker = true;

            if (!owner.active)
            {
                Projectile.active = false;
                return;
            }

            if (!owner.dead && owner.HasBuff(BuffType<CactusLampBuff>())) Projectile.timeLeft = 2;

            if (Projectile.Distance(IdealPos) > 160) chase = true;
            if (Projectile.Distance(IdealPos) < 80) chase = false;

            if (chase)
            {
                Projectile.velocity += Projectile.DirectionTo(IdealPos) * 0.5f;
                Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.velocity.ToRotation() + MathHelper.PiOver2, .2f);
                Projectile.velocity *= 0.99f;

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 31, Scale: .8f);
                    dust.noGravity = false;
                    dust.alpha = 140;
                    dust.fadeIn = 1.2f;
                }
            }
            else
            {
                if (Projectile.Distance(IdealPos) > 4) Projectile.velocity += Projectile.DirectionTo(IdealPos) * 0.2f;
                Projectile.velocity *= 0.95f;

                Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.Clamp(Projectile.velocity.X * 0.1f, -0.75f, 0.75f), 0.2f);
            }


            float glowMult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 30;

            if (flicker) if (!Main.dedServ) Lighting.AddLight(Projectile.Center, new Vector3(.3f, .2f, .1f));
            else if(!Main.dedServ) Lighting.AddLight(Projectile.Center, new Vector3(.55f + glowMult, .45f + glowMult, .225f + glowMult));

            Projectile.ai[0]++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects effects = SpriteEffects.None;

            if (Projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            else effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, drawOrigin, 1, effects, 0);
            if (!flicker) Main.EntitySpriteDraw(mask, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, 1, effects, 0);

            return false;
        }
    }
}
