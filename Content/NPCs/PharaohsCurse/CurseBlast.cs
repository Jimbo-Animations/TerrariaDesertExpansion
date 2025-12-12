using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    internal class CurseBlast : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;
        private Vector2 goalPosition;
        float opacity;

        public override void OnSpawn(IEntitySource source)
        {
            startPosition = Projectile.Center;
            startVelocity = Projectile.velocity;
        }


        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 15;
        }

        public override void AI()
        { // This makes the projectile undulate. ai[0] controls wave intensity, ai[1] controls which direction the undulation starts in, and ai[2] controls velocity.
            goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * Projectile.ai[2] * Projectile.localAI[0] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * Projectile.ai[0] * (float)Math.Sin(Projectile.ai[1] + Projectile.localAI[0] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / Projectile.ai[0]);
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] % 4 == 0)
            {
                Projectile.frame++;

                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptTorch, Scale: 1);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;

                if (Projectile.frame >= 7) Projectile.frame = 0;
            }

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.rotation = Projectile.velocity.ToRotation();
            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft <= 60 ? 0 : 1, .2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, Color.LightPink * .5f * opacity, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 1.25f - (i / (float)Projectile.oldPos.Length), Projectile.scale * .75f - (i / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);

            }
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.LightPink * opacity, Projectile.rotation, glow.Size() / 2 - new Vector2(glow.Width * .2f, 0), Projectile.scale * .3f, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * opacity, Projectile.rotation, drawOrigin + new Vector2(frame.Width * .15f, 0), Projectile.scale, SpriteEffects.FlipHorizontally, 0);        

            return false;
        }
    }

    internal class CurseBlastBig : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        private Vector2 startPosition;
        private Vector2 startVelocity;
        private Vector2 goalPosition;
        float opacity;

        public override void OnSpawn(IEntitySource source)
        {
            startPosition = Projectile.Center;
            startVelocity = Projectile.velocity;
        }


        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.Distance(target.Center) <= 27;
        }

        public override void AI()
        { // This makes the projectile undulate. ai[0] controls wave intensity, ai[1] controls which direction the undulation starts in, and ai[2] controls velocity.
            goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * Projectile.ai[2] * Projectile.localAI[0] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * Projectile.ai[0] * (float)Math.Sin(Projectile.ai[1] + Projectile.localAI[0] / 5f) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / Projectile.ai[0]);
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] % 4 == 0)
            {
                Projectile.frame++;

                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptTorch, Scale: 1);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;

                if (Projectile.frame >= 7) Projectile.frame = 0;
            }

            Projectile.velocity = goalPosition - Projectile.Center;
            Projectile.rotation = Projectile.velocity.ToRotation();
            opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft <= 60 ? 0 : 1, .2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Texture2D trail = Request<Texture2D>("GloryMod/CoolEffects/Textures/SemiStar").Value;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(trail, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, null, Color.LightPink * .5f * opacity, Projectile.oldRot[i] + MathHelper.PiOver2, trail.Size() / 2, new Vector2(Projectile.scale * 1.67f - (i / (float)Projectile.oldPos.Length), Projectile.scale * 1f - (i / (float)Projectile.oldPos.Length)), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.LightPink * opacity, Projectile.rotation, glow.Size() / 2 - new Vector2(glow.Width * .15f, 0), Projectile.scale * .5f, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * opacity, Projectile.rotation, drawOrigin + new Vector2(frame.Width * .15f, 0), Projectile.scale, SpriteEffects.FlipHorizontally, 0);

            return false;
        }
    }
}
