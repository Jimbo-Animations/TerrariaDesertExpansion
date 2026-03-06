using System.Collections.Generic;
using Terraria.Graphics.CameraModifiers;

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

            NPCID.Sets.TrailCacheLength[NPC.type] = 15;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Midas] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(68);

            NPC.damage = 60;
            NPC.defense = 12;
            NPC.lifeMax = Main.masterMode ? 18360 / 3 : Main.expertMode ? 14400 / 2: 9000;
            NPC.knockBackResist = 0f;
            NPC.scale = 1;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.boss = true;
            NPC.npcSlots = 50f;

            NPC.dontTakeDamage = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2 with { Volume = 2, Pitch = -.2f };
            NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 2, Pitch = -.2f };

            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/Undying_Fury");
            //MusicLoader.GetMusic(Mod, "Content/Music/Undying_Fury").SetVariable("Pitch", MusicPitch);

        }

        public override void AI()
        {
            // Normal checks and timers for controlling the fight.

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            if (changeDirection) NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? -1 : 1;

            NPC.damage = useContactDamage ? NPC.GetAttackDamage_ScaledByStrength(60) : 0;

            AITimer++;
            ArenaTimer++;

            if (target.Distance(ArenaCenter) > ArenaRadius + 16 && target.Distance(ArenaCenter) <= 2000) ArenaTimer = 1500; // Automatically readies the arena attack to reoccur if the players leave the arena.

            switch (AIstate)
            {
                case AttackPattern.Intro: // Starts the fight

                    Intro();

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

                case AttackPattern.SwordRing:

                    SwordRing();

                    break;

                case AttackPattern.SandnadoAttack:

                    Sandnadoes();

                    break;

                case AttackPattern.ArenaSpawn:

                    ArenaSpawn();

                    break;

                case AttackPattern.PhaseTransition:

                    PhaseTransition();

                    break;

                case AttackPattern.DefeatAnim:

                    DefeatAnim();

                    break;
            }
        }

        public override void HitEffect(NPC.HitInfo hit) // Set effects when hit.
        {
            int numDusts = 3;
            for (int i = 0; i < numDusts; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, newColor: Color.White, Scale: 1.5f);
                Main.dust[dust].velocity = new Vector2(Main.rand.NextFloat(-1.0f, 1.1f), Main.rand.NextFloat(-1.0f, 1.1f));
            }

            if (NPC.life <= 0 && !die) // Final blow.
            {
                die = true;
                NPC.dontTakeDamage = true;
                NPC.life = 1;

                AITimer = 0;
                AITimer2 = 0;
                eyeMode = false;
                changeDirection = true;
                useContactDamage = false;
                usemeleeVis = false;
                useruneVis = false;

                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(hit.HitDirection, 0f), 10f, 10f, 20, 1000f, "PharaohCurse");
                Main.instance.CameraModifiers.Add(modifier);

                SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);

                NPC.ai[0] = 11;
                NPC.netUpdate = true;
            }
        }

        bool usemeleeVis;
        bool useruneVis;
        float timer;
        float opacity;
        float outlineVis;
        float meleeVis;
        float runeVis;
        List<Tuple<Vector2, float, float, float>> shockwave = new List<Tuple<Vector2, float, float, float>>();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value; // Normal stuff for drawing the boss
            Texture2D melee = Request<Texture2D>(Texture + "Melee").Value;
            Vector2 drawOrigin = new Vector2(NPC.frame.Width * .5f, NPC.frame.Height * .5f);
            Vector2 drawPos = NPC.Center - screenPos;

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

            // Controls visibility of effects.

            meleeVis = MathHelper.Lerp(meleeVis, usemeleeVis ? 1 : 0, .05f);
            runeVis = MathHelper.Lerp(runeVis, useruneVis ? 1 : 0, .1f);

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                spriteBatch.Draw(texture, NPC.oldPos[i] - NPC.position + drawPos, NPC.frame, (usemeleeVis ? Color.Red : Color.Purple) * outlineVis * .3f * (.8f - i / (float)NPC.oldPos.Length), NPC.oldRot[i], drawOrigin, NPC.scale, effects, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(texture, drawPos + new Vector2(3, 0).RotatedBy(timer + i * MathHelper.TwoPi / 4), NPC.frame, (usemeleeVis ? Color.Red : Color.Purple) * outlineVis * .3f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            }

            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor * opacity, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            spriteBatch.Draw(melee, drawPos, NPC.frame, Color.White * opacity * meleeVis, NPC.rotation, drawOrigin, NPC.scale, effects, 0);

            if (shockwave.Count > 0) // Controls shockwave visuals from the boss.
            {
                Texture2D shockwavetex = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/Glow_5").Value; // Texture for the effect.

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                for (int i = 0; i < shockwave.Count; i++)
                {
                    if (i >= shockwave.Count)
                    {
                        break;
                    }

                    spriteBatch.Draw(shockwavetex, shockwave[i].Item1 - Main.screenPosition, null, Color.LightPink * shockwave[i].Item4, 0, shockwavetex.Size() / 2, shockwave[i].Item2 / shockwavetex.Width, SpriteEffects.None, 0);

                    shockwave[i] = new Tuple<Vector2, float, float, float>(shockwave[i].Item1, shockwave[i].Item2 + shockwave[i].Item3, shockwave[i].Item3, shockwave[i].Item4);
                    if (shockwave[i].Item2 >= target.Distance(shockwave[i].Item1) + Main.screenWidth * 3)
                    {
                        shockwave.RemoveAt(i);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }

            // Creates a blue rune circle for the blades attack.

            Texture2D runetex = Request<Texture2D>("TerrariaDesertExpansion/Content/ExtraAssets/rune2").Value; // Texture for the effect.

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(runetex, drawPos, null, Color.DeepSkyBlue * runeVis * .8f, -timer, runetex.Size() / 2, NPC.scale * .5f * runeVis, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);


            return false;
        }
    }
}
