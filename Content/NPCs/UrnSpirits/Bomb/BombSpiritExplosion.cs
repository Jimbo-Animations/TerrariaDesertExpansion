using Terraria.DataStructures;
using Terraria.Map;

namespace TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Bomb
{
    class BombSpiritExplosion : ModProjectile
    {
        private bool IsChild
        {
            get => Projectile.localAI[0] == 1;
            set => Projectile.localAI[0] = value.ToInt();
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);

            int numDusts = 30;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, 6, Scale: 3f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = true;
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
            }

            int explosionRadius = 5; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
            int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            // Ensure that all tile coordinates are within the world bounds
            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

            // Does not destroy wall tiles as to keep the Desert Biome intact.
            Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, false);
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= 4)
            {
                Projectile.frame++;
                Projectile.ai[1] = 0;

                if (Projectile.frame == 3 && !IsChild)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        // Importantly, IsChild is set to true here. This is checked in OnTileCollide to prevent bouncing and here in OnKill to prevent an infinite chain of splitting projectiles.
                        Projectile child = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(120, 0).RotatedBy(MathHelper.TwoPi * i / 9), Vector2.Zero, Projectile.type, Projectile.damage, Projectile.knockBack, Main.myPlayer, 0, 1);
                        (child.ModProjectile as BombSpiritExplosion).IsChild = true;
                    }
                }
            }

            if (Projectile.ai[0] >= 25)
            {
                Projectile.friendly = false;
                Projectile.hostile = false;
            }

            visibility = MathHelper.Lerp(visibility, Projectile.timeLeft <= 20 ? 0 : 1, .15f);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Rectangle result = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
            int num = (int)Utils.Remap(Projectile.ai[0] * 2, 0, 200, 10, 40);
            result.Inflate(num, num);
            hitbox = result;
        }

        float visibility = 1;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D glow = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_1").Value;
            Rectangle frame = new Rectangle(0, (texture.Height / Main.projFrames[Projectile.type]) * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * visibility, 0, drawOrigin, Projectile.scale * ((Projectile.ai[0] * 0.03f) + 1), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, new Color(200, 150, 50, 50) * visibility, 0, glow.Size() / 2, Projectile.scale * ((Projectile.ai[0] * 0.07f) + 0.5f), SpriteEffects.None, 0);

            return false;
        }
    }
}
