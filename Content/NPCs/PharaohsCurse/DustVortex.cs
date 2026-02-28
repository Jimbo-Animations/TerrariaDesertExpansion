
namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    internal class DustVortex : ModProjectile
    {
        public override string Texture => "TerrariaDesertExpansion/Content/ExtraAssets/NoSprite";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(192);
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.rotation += .33f * opacity;
            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft > 90 ? .25f : (Projectile.timeLeft >= 30 ? 1 : 0), .175f);

            Projectile.hostile = Projectile.timeLeft <= 75 && Projectile.timeLeft >= 20;

            if (Projectile.timeLeft % 4 == 0) 
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 32, newColor: Color.Tan, Scale: 1.5f * opacity);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(10, 20), 0).RotatedByRandom(MathHelper.TwoPi);
                Main.dust[dust].noGravity = true;
            }           
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 96;
        }

        float opacity = 0;
        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D vortex = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_4").Value;
            Texture2D backglow = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_1").Value;

            Rectangle vortexFrame = new Rectangle(0, 0, vortex.Width, vortex.Height);
            Rectangle glowFrame = new Rectangle(0, 0, backglow.Width, backglow.Height);
            Vector2 vortexOrigin = new Vector2(vortex.Width * 0.5f, vortex.Height * 0.5f);
            Vector2 glowOrigin = new Vector2(backglow.Width * 0.5f, backglow.Height * 0.5f);

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            // Glowing vortex texture to create a janky outline

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vortex, Projectile.Center - Main.screenPosition + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), vortexFrame, Color.Red * .75f * opacity, Projectile.rotation * (MathHelper.PiOver4 * i), vortexOrigin, Projectile.scale * opacity, SpriteEffects.None, 0);
            }

            // Repeat the main texture at different scaling, rotations, and colors.

            Main.EntitySpriteDraw(vortex, Projectile.Center - Main.screenPosition, vortexFrame, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, Color.SandyBrown) * opacity, Projectile.rotation * .95f - MathHelper.PiOver4, vortexOrigin, Projectile.scale * opacity, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(vortex, Projectile.Center - Main.screenPosition, vortexFrame, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, Color.SandyBrown) * opacity, Projectile.rotation * .95f + MathHelper.PiOver4, vortexOrigin, Projectile.scale * opacity, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(vortex, Projectile.Center - Main.screenPosition, vortexFrame, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, new Color (170, 93, 62)) * opacity, Projectile.rotation + MathHelper.PiOver2, vortexOrigin, Projectile.scale * .75f * opacity, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(vortex, Projectile.Center - Main.screenPosition, vortexFrame, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, new Color(195, 121, 76)) * opacity, Projectile.rotation, vortexOrigin, Projectile.scale * .75f * opacity, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(backglow, Projectile.Center - Main.screenPosition, glowFrame, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, new Color(160, 83, 52)) * opacity, 0, glowOrigin, Projectile.scale * .33f, SpriteEffects.None, 0);
            return false;
        }
    }
}
