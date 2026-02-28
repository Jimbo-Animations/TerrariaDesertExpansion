using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    internal class DesertBlade : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(50);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI() // Spin up and move out.
        {
            spinSpeed = MathHelper.SmoothStep(spinSpeed, .4f, .1f);
            Projectile.rotation += spinSpeed;
            Projectile.velocity *= .99f;

            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft <= 30 ? 0 : 1, .175f);
            Projectile.hostile = Projectile.timeLeft <= 330 && Projectile.timeLeft > 30;

            if (Projectile.timeLeft <= 330 && Projectile.timeLeft > 240) Projectile.velocity += new Vector2(1, 0).RotatedBy(Projectile.ai[0]) * .3f;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 25;
        }

        public float opacity;
        float spinSpeed;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(mask, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Color.DeepSkyBlue * (.8f - i / (float)Projectile.oldPos.Length) * opacity, Projectile.oldRot[i], drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * .98f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(mask, Projectile.Center - Main.screenPosition, frame, Color.DeepSkyBlue * opacity * .8f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
