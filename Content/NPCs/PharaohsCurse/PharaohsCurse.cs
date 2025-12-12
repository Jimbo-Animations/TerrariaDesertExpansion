namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    [AutoloadBossHead]
    partial class PharaohsCurse : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            ContentSamples.NpcBestiaryRarityStars[Type] = 3;

            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Midas] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(68, 94);

            NPC.damage = 60;
            NPC.defense = 12;
            NPC.lifeMax = Main.masterMode ? 17340 / 3 : Main.expertMode ? 13600 / 2: 8500;
            NPC.knockBackResist = 0f;
            NPC.scale = 1;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.boss = true;
            NPC.npcSlots = 50f;

            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2 with { Volume = 2, Pitch = -.2f };
            NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 2, Pitch = -.2f };
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (changeDirection) NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? -1 : 1;

            NPC.damage = useContactDamage ? NPC.GetAttackDamage_ScaledByStrength(60) : 0;

            AITimer++;
            ArenaTimer++;

            if (target.Distance(ArenaCenter) > ArenaRadius && target.Distance(ArenaCenter) <= 2000) ArenaTimer = 1500; // Automatically readies the arena attack to reoccur if the players leave the arena.

            switch (AIstate)
            {
                case AttackPattern.Intro: // Starts the fight

                    NPC.ai[0] = 1;
                    ResetAIStates();

                    break;

                case AttackPattern.Reposition: // Standard behavior. Follow player and choose an attack after a set timer.

                    Reposition();

                    break;

                case AttackPattern.ChargeAttack:

                    ChargeAttack();

                    break;

                case AttackPattern.GroundPound:

                    GroundPound();

                    break;

                case AttackPattern.FanBarrage:

                    FanBarrage();

                    break;

                case AttackPattern.TeleportAttack:

                    TeleportAttack();

                    break;

                case AttackPattern.SandnadoAttack:

                    Main.NewText("Sandnadoes");
                    NPC.ai[0] = 1;

                    break;

                case AttackPattern.ChaseAndShoot:

                    Main.NewText("Chase");
                    NPC.ai[0] = 1;

                    break;

                case AttackPattern.ArenaSpawn:

                    ArenaSpawn();

                    break;

                case AttackPattern.DefeatAnim:

                    break;
            }
        }

        float timer;
        float outlineVis;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos; ;

            float time = Main.GameUpdateCount * 0.05f;
            float glowbrightness = (float)MathF.Sin(NPC.whoAmI - time);

            SpriteEffects effects = new SpriteEffects();

            if (eyeMode) effects = NPC.spriteDirection > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            else effects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            timer += 0.1f;

            if (timer >= MathHelper.Pi)
            {
                timer = 0f;
            }

            outlineVis = MathHelper.Lerp(outlineVis, 1, .15f);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, drawPos + new Vector2(2, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, new Color(180, 50, 220, 100) * outlineVis, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

            return false;
        }
    }
}
