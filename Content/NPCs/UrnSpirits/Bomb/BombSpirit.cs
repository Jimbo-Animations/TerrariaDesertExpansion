using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Bomb
{
    class BombSpirit : ModNPC
    {
        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {

            int associatedNPCType = NPCType<BombSpirit>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                new FlavorTextBestiaryInfoElement("Many spirits find themselves trapped within urns left beneath the sands of the Great Desert. Amongst these, the angry bombs are highly volatile, and can create new underground cavities with their self-destructive tendencies.")
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(38);

            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = .75f;
            NPC.value = 0;
            NPC.npcSlots = 5f;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontCountMe = true;
            NPC.immortal = true;

            NPC.HitSound = SoundID.NPCHit37 with { Volume = .5f, Pitch = .2f };
            NPC.DeathSound = SoundID.NPCDeath47 with { Volume = .5f, Pitch = .2f };
        }

        SlotId Fuse = SlotId.Invalid;
        bool turnOffSound;

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite, NPC.Center);

            NPC.velocity = new Vector2(0, -3).RotatedByRandom(MathHelper.PiOver2);

            Player player = Main.player[NPC.target];
            Fuse = SoundEngine.PlaySound(SoundID.DD2_KoboldIgniteLoop with { IsLooped = true }, NPC.Center);
        }

        private void UpdateSound()
        {
            if (SoundEngine.TryGetActiveSound(Fuse, out ActiveSound sound) && sound is not null && sound.IsPlaying)
            {
                sound.Position = NPC.Center;

                sound.Volume = MathHelper.Lerp(sound.Volume, 1, .1f);

                if (turnOffSound) sound.Stop();
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y == 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            NPC.velocity *= .99f;

            bool xCollision = Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height, true, true) != new Vector2(NPC.velocity.X, 0);
            bool yCollision = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, true, true) != new Vector2(0, NPC.velocity.Y);

            if (xCollision)
            {
                NPC.velocity.X *= -.75f;
            }
            if (yCollision)
            {
                NPC.velocity.Y *= -.75f;
            }

            NPC.rotation += NPC.velocity.X * .05f;

            if (NPC.localAI[0]++ >= 5)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 293, Scale: 1);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(-4, 0));

                NPC.localAI[0] = 0;
            }

            // Make this thing explode after a set time

            if (NPC.ai[0]++ > 360)
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ProjectileType<BombSpiritExplosion>(), 100, 2f, Main.myPlayer);
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);

                turnOffSound = true;

                NPC.active = false;
                NPC.netUpdate = true;
            }

            if (NPC.despawnEncouraged) turnOffSound = true;

            UpdateSound();
        }

        float timer;        
        float visibility;
        float visibility2;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            timer += 0.1f;

            if (visibility < 1) visibility += .02f;
            if (visibility2 < 1 && NPC.ai[0] >= 240) visibility2 = MathHelper.SmoothStep(visibility2, 1, .1f);

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, drawPos + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(255, 50, 0, 100) * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            if (NPC.ai[0] >= 240)
            {
                for (int i = 0; i < 4; i++)
                {
                    Main.EntitySpriteDraw(texture, drawPos + new Vector2(32 - (visibility2 * 32), 0).RotatedBy(-timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(200, 255, 50, 50) * visibility2, NPC.rotation, drawOrigin, NPC.scale * 1.1f, effects, 0);
                }
            }      
            
            return false;
        }
    }
}
