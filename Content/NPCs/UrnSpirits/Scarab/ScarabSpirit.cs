using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Scarab
{
    class ScarabSpirit : ModNPC
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

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {

            int associatedNPCType = NPCType<ScarabSpirit>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                new FlavorTextBestiaryInfoElement("Many spirits find themselves trapped within urns left beneath the sands of the Great Desert. Amongst these, the swarms of giant, restless scarabs may be the most aggressive of all spirits.")
            });
        }


        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(6);

            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = Main.getGoodWorld ? 80 : Main.masterMode ? 65 : 50;
            NPC.knockBackResist = .05f;
            NPC.value = Item.buyPrice(0, 0, 0, 6);
            NPC.npcSlots = .02f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontCountMe = true;
            NPC.dontTakeDamage = true;

            NPC.HitSound = SoundID.NPCHit37 with { Volume = .5f, Pitch = .2f };
            NPC.DeathSound = SoundID.NPCDeath47 with { Volume = .5f, Pitch = .2f };
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 0)
            {
                for (int i = 1; i < (Main.expertMode ? 29 : 19); i++)
                {
                    NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)NPC.Center.X + Main.rand.Next(-1, 2), (int)NPC.Center.Y - i, NPC.type, ai0: 1);
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 3)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y == 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                int numDusts = 3;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, newColor: Color.White, Scale: 2);
                    Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
                }
            }
        }

        private void SwarmSeparation(Vector2 goalPosition)
        {
            //boids
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC otherNPC = Main.npc[i];

                if (i != NPC.whoAmI && otherNPC.type == NPC.type && otherNPC.active && (NPC.Center - otherNPC.Center).Length() < 128)
                {
                    count++;

                    //separation component
                    separation += 10f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

                    //alignment component
                    alignment += 1 / 10f * (otherNPC.velocity - NPC.velocity);

                    //cohesion component
                    cohesion += 1 / 20f * (otherNPC.Center - NPC.Center);
                }

            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            Vector2 goalVelocity = NPC.ai[1] > 120 ? NPC.velocity + separation + alignment + cohesion + (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) : NPC.velocity + separation + alignment + cohesion;
            if (goalVelocity.Length() > 5)
            {
                goalVelocity.Normalize();
                goalVelocity *= Main.expertMode ? (count < 15 ? 5.5f : 4.5f) : (count < 10 ? 5.5f : 4.5f);
            }
            NPC.velocity += (goalVelocity - NPC.velocity) / 10;
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            SwarmSeparation(target.Center);

            // Controls movement and when to slow down.

            if (NPC.ai[1]++ > 360) NPC.ai[1] = 60;
            if (NPC.ai[1] <= 120) NPC.velocity *= .98f;

            if (NPC.ai[1] == 60 && NPC.dontTakeDamage) 
            {
                NPC.dontTakeDamage = false;
                NPC.damage = Main.getGoodWorld ? 56 : Main.masterMode ? 42 : Main.expertMode ? 28 : 14;
            }

            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
            NPC.rotation = NPC.velocity.X / 10f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Rabies, 600);
        }

        float timer;
        float visibility;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * 0.5f, NPC.frame.Height);
            Vector2 drawPos = NPC.Bottom - screenPos;

            SpriteEffects effects = new SpriteEffects();
            if (NPC.spriteDirection == 1) effects = SpriteEffects.FlipHorizontally;

            timer += 0.1f;

            if (visibility < 1) visibility += .02f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, drawPos + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(0, 255, 0, 100) * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0);             
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White * visibility, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
