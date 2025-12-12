using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    class SandBarrier : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(58, 62);
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }

        Vector2 spawnCenter;
        bool unleash;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, 8);
            spawnCenter = Main.npc[(int)Projectile.ai[0]].Center;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] % 4 == 0)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 32, newColor: (Projectile.ai[2] > 0 ? Color.Red : Color.Tan), Scale: 1);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
                Main.dust[dust].noGravity = true;

                Projectile.frame++;

                if (Projectile.frame >= 8) Projectile.frame = 0;
            }

            switch (Projectile.ai[2])
            {
                case 0: // Arena mode. Circle an area, and launch outwards if a player leaves.

                    if (Projectile.timeLeft <= 1500)
                    {
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].Distance(spawnCenter) > Projectile.ai[1] && Main.player[i].Distance(spawnCenter) <= 1600 && Projectile.timeLeft <= 1480 && !unleash)
                            {
                                unleash = true;
                                Projectile.timeLeft = 400;
                            }
                        }

                        if (!unleash) Projectile.velocity = Projectile.DirectionTo(spawnCenter).RotatedBy(MathHelper.PiOver2) * 7 * Projectile.ai[0]; // Spin if the projectiles shouldn't be unleashed. Otherwise, fly outwards and accelerate.
                        else Projectile.velocity *= 1.0033f;

                        // Projectiles fade out and stop dealing damage at the end of their lifespan.
                        Projectile.hostile = Projectile.timeLeft <= 30 ? false : true;
                        opacity = Projectile.timeLeft <= 60 ? MathHelper.SmoothStep(opacity, 0, .2f) : 1;
                    }
                    else opacity = MathHelper.SmoothStep(opacity, .33f, .2f);

                    break;

                case 1: // Standard projectile mode. Tilt and move up slowly.

                    if (Projectile.timeLeft > 300) Projectile.timeLeft = 300;
                    Projectile.hostile = Projectile.timeLeft <= 30 ? false : true;

                    opacity = MathHelper.SmoothStep(opacity, Projectile.timeLeft <= 60 ? 0 : 1, .2f);
                    Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.Clamp(Projectile.velocity.X * .05f, -.75f, .75f), .2f);

                    Projectile.velocity.Y = MathHelper.Clamp((Projectile.velocity.Y - .01f) * 1.01f, -16, 16); // Moves projectile upwards slowly, and caps vertical speed.

                    break;
            }                 
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[2] > 0) 
            {
                if (target.velocity.Y > 0) target.velocity.Y = -10;
                else target.velocity.Y -= 10;
                
            }
        }

        public float opacity;
        float timer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i += 2)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, (Projectile.ai[2] > 0 ? Color.Red : lightColor) * (1 - i / (float)Projectile.oldPos.Length) * .9f * opacity, Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * .98f, SpriteEffects.None, 0);
            }

            if (Projectile.ai[2] > 0)
            {
                timer += 0.1f;

                if (timer >= MathHelper.Pi)
                {
                    timer = 0f;
                }

                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), frame, Color.Red * .75f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor * opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
