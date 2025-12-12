using Terraria.DataStructures;

namespace TerrariaDesertExpansion.Content.NPCs.DustDevil
{
    class HauntedSandBall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(20);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
        }

        float rotateAt;
        public override void OnSpawn(IEntitySource source)
        {
            rotateAt = Main.rand.NextFloat(-.4f, .41f);
        }

        public override void AI()
        {
            Projectile.rotation += rotateAt;
            Projectile.velocity.Y += .1f;
            Projectile.ai[0]++;

            if (Projectile.ai[0] > 1) Projectile.tileCollide = true;

            if (Projectile.ai[0] % 3 == 0)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 32, newColor: Color.Tan, Scale: 1);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item127, Projectile.Center);

            NPC.NewNPC(NPC.GetSource_None(), (int)Projectile.Center.X + (int)Projectile.velocity.X, (int)Projectile.Bottom.Y + (int)Projectile.velocity.Y, NPCType<HauntedSandGlob>(), 1, Projectile.rotation);
            Projectile.netUpdate = true;

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            Rectangle frame = new Rectangle(0, texture.Height / Main.projFrames[Projectile.type] * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Projectile.type]);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(mask, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, new Color(180, 50, 220, 100) * (1 - i / (float)Projectile.oldPos.Length) * 0.95f, Projectile.rotation, drawOrigin, Projectile.scale * (1.25f - i / Projectile.oldPos.Length) * 0.98f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    class HauntedSandGlob : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(11);
            NPC.defense = 0;
            NPC.damage = 10;
            NPC.lifeMax = 30;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.HitSound = SoundID.NPCHit11;
            NPC.DeathSound = SoundID.NPCDeath15;
            NPC.value = 0;
        }

        public void SlimeSquish(float width, float height)
        {
            stretchWidth = MathHelper.SmoothStep(stretchWidth, width, .2f);
            stretchHeight = MathHelper.SmoothStep(stretchHeight, height, .2f);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int numDusts = 3;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 2);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
            }

            if (NPC.life <= 0)
            {
                numDusts = 10;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 32, newColor: Color.Tan, Scale: 2);
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.67f, 1.68f), Main.rand.NextFloat(-1.67f, 1.68f));
                }
            }
        }

        public override void AI()
        {
            SlimeSquish(1 + (float)Math.Sin(NPC.ai[1] / 4) * 0.15f, 1 - (float)Math.Sin(NPC.ai[1] / 4) * 0.15f);

            NPC.rotation = NPC.ai[0];
            NPC.ai[1]++;
        }

        float timer;
        public float stretchWidth = .5f;
        public float stretchHeight = .5f;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;

            float SpriteWidth = NPC.scale * stretchWidth;
            float SpriteHeight = NPC.scale * stretchHeight;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(mask, drawPos + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(180, 50, 220, 100), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            }

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, new Vector2(SpriteWidth, SpriteHeight), SpriteEffects.None, 0f);

            return false;
        }
    }
}
