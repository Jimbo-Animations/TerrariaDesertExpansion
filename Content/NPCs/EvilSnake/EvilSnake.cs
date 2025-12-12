using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using TerrariaDesertExpansion.Content.Items.Materials;

namespace TerrariaDesertExpansion.Content.NPCs.EvilSnake
{
    [AutoloadBanner]
    class EvilSnake : ModNPC
    {
        private int attackCooldown
        {
            get => attackCooldown = (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private int soundCooldown
        {
            get => soundCooldown = (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("Giant, lazy snake that attempts to bite passerbys that it dislikes. Relies on its eyes more than its other senses, and regularly consumes anti-Terrarian propaganda.")
            });
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.UsesNewTargetting[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 52;
            NPC.height = 42;
            NPC.defense = 4;
            NPC.damage = 20;
            NPC.lifeMax = 50;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.HitSound = SoundID.NPCHit23;
            NPC.DeathSound = SoundID.NPCDeath26;
            NPC.value = Item.buyPrice(0, 0, 0, 5);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert && Main.dayTime && spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water) return 0.5f;
            else return 0f;
        }

        int animState;
        bool shouldBite;
        public int contactDamage;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 2;
            NPC.frameCounter++;

            NPC.frame.X = animState == 1 ? NPC.frame.Width : 0;

            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;

                if (animState == 1 && NPC.frame.Y == NPC.frame.Height * 3)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(30 * NPC.direction, -3), new Vector2(4 * NPC.direction, 0), ProjectileType<EvilSnakeBite>(), 10, 2f, Main.myPlayer);

                    SoundEngine.PlaySound(SoundID.NPCHit2, NPC.Center);
                }

                if (animState == 1 && NPC.frame.Y == NPC.frame.Height * 2)
                {
                    NPC.damage = contactDamage;
                    NPC.width = 62;
                    NPC.position.X -= 5;
                }
                if (animState == 1 && NPC.frame.Y == NPC.frame.Height * 4)
                {
                    NPC.damage = 0;
                    NPC.width = 52;
                    NPC.position.X += 5;
                }

                if (animState == 0 && attackCooldown == 0 && shouldBite)
                {
                    NPC.frame.Y = 0;
                    animState = 1;
                    attackCooldown = 150;
                }
                else NPC.frame.Y += frameHeight;
            }

            if (NPC.frame.Y >= frameHeight * 8 && animState == 0 || NPC.frame.Y >= frameHeight * 6 && animState == 1)
            {
                NPC.frame.Y = 0;
                if (animState == 1) animState = 0;
            }
        }

        public override void AI()
        {
            if (contactDamage == 0) contactDamage = Main.expertMode ? Main.masterMode ? Main.getGoodWorld ? 80 : 60 : 40 : 20;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (target.Distance(NPC.Center) < 300) NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

            if (attackCooldown > 0) attackCooldown--;
            if (soundCooldown > 0) soundCooldown--;

            shouldBite = attackCooldown <= 0 && NPC.HasValidTarget && BasicUtils.CloseTo(NPC.Center.X, target.Center.X, 100) && BasicUtils.CloseTo(NPC.Center.Y, target.Center.Y, 40);

            if (soundCooldown <= 0 && Main.rand.NextBool(150))
            {
                SoundEngine.PlaySound(SoundID.Item151, NPC.Center);
                soundCooldown = 300;
                NPC.netUpdate = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int numDusts = 3;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, newColor: Color.White, Scale: 1);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
            }

            NPC.direction = -hit.HitDirection;

            if (NPC.life <= 0)
            {
                int gore1 = Mod.Find<ModGore>("EvilSnakeGore1").Type;
                int gore2 = Mod.Find<ModGore>("EvilSnakeGore2").Type;
                int gore3 = Mod.Find<ModGore>("EvilSnakeGore3").Type;
                int gore4 = Mod.Find<ModGore>("EvilSnakeGore4").Type;
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Top + new Vector2(12 * NPC.direction, 0), new Vector2(6 * -NPC.direction, - 6).RotatedByRandom(MathHelper.ToRadians(10)), gore1);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Bottom - new Vector2(12 * NPC.direction, 0), new Vector2(4 * -NPC.direction, 0).RotatedByRandom(MathHelper.ToRadians(10)), gore2);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, new Vector2(1 * NPC.direction, -1).RotatedByRandom(MathHelper.ToRadians(10)), gore3);
                Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, gore4);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<SerpentFang>(), 3, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemID.SnakesIHateSnakes, 100));
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Venom, 200);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height);
            Vector2 drawPos = NPC.Bottom - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            spriteBatch.Draw(mask, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }

    class EvilSnakeBite : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(20);
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 30;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.95f;

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

            if (Projectile.timeLeft <= 20) opacity *= 0.92f;

            if (Projectile.timeLeft <= 10) Projectile.hostile = false;
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

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Venom, 60);
        }
    }

}
