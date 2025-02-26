namespace TerrariaDesertExpansion.Content.NPCs.CactusSlime
{
    class CactusSlimeSpike : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(22);
            Projectile.tileCollide = true;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 400;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.25f;
            Projectile.velocity *= 0.995f;

            if (Projectile.timeLeft <= 10)
            {
                Projectile.alpha += 25;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, lightColor * ((1 - i / (float)Projectile.oldPos.Length) * 0.99f), Projectile.rotation, drawOrigin, Projectile.scale * (1f - i / Projectile.oldPos.Length) * 0.99f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Vector2 usePos = Projectile.position;

            Vector2 rotationVector = (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2();
            usePos += rotationVector * 16f;

            // Spawn some dusts upon javelin death
            for (int i = 0; i < 7; i++)
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
    }

    class CactusSlimeShockwave : ModProjectile
    {
        public override string Texture => "DesertExpansion/Content/ExtraAssets/Glow_1";

        public override void SetDefaults()
        {
            AIType = -1;
            Projectile.Size = new Vector2(10);

            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.hide = true;
            Projectile.penetrate = -1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return target.velocity.Y == 0;
        }

        public override void AI()
        {
            float num = 30f;
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 9f)
            {
                Projectile.Kill();
                return;
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.position = Projectile.Center;
            Projectile.Size = new Vector2(10) * MathHelper.Lerp(5f, num, Utils.GetLerpValue(0f, 9f, Projectile.ai[0]));
            Projectile.Center = Projectile.position;
            Point point = Projectile.TopLeft.ToTileCoordinates();
            Point point2 = Projectile.BottomRight.ToTileCoordinates();
            int num2 = point.X / 2 + point2.X / 2;
            int num3 = Projectile.width / 2;

            if ((int)Projectile.ai[0] % 3 != 0)
            {
                return;
            }

            int num4 = (int)Projectile.ai[0] / 3;

            for (int i = point.X; i <= point2.X; i++)
            {
                for (int j = point.Y; j <= point2.Y; j++)
                {
                    if (Vector2.Distance(Projectile.Center, new Vector2(i * 16, j * 16)) > num3)
                    {
                        continue;
                    }
                    Tile tileSafely = Framing.GetTileSafely(i, j);

                    if (!tileSafely.HasUnactuatedTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType])
                    {
                        continue;
                    }
                    Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                    if (tileSafely2.HasUnactuatedTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType])
                    {
                        continue;
                    }
                    int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
                    for (int k = 0; k < num5; k++)
                    {
                        Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj.velocity.Y -= 3f + num4 * 1.5f;
                        obj.velocity.Y *= Main.rand.NextFloat();
                        obj.velocity.Y *= 0.75f;
                        obj.scale += num4 * 0.03f;
                    }
                    if (num4 >= 2)
                    {
                        for (int m = 0; m < num5 - 1; m++)
                        {
                            Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                            obj2.velocity.Y -= 1f + num4;
                            obj2.velocity.Y *= Main.rand.NextFloat();
                            obj2.velocity.Y *= 0.75f;
                        }
                    }
                    if (num5 <= 0 || Main.rand.NextBool(3))
                    {
                        continue;
                    }
                    float num7 = Math.Abs(num2 - i) / (num / 2f);

                    Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, 61 + Main.rand.Next(3), 1f - num4 * 0.15f + num7 * 0.5f);
                    gore.velocity.Y -= 0.1f + num4 * 0.5f + num7 * num4 * 1f;
                    gore.velocity.Y *= Main.rand.NextFloat();
                    gore.position = new Vector2(i * 16, j * 16 + 20);

                }
            }
        }
    }
}
