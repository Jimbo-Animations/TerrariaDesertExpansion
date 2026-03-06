using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Utilities;
using TerrariaDesertExpansion.Content.Items.Placeables.Trophies;

namespace TerrariaDesertExpansion.Content.NPCs.PharaohsCurse
{
    partial class PharaohsCurse : ModNPC
    {
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("")
            });
        }

        public override void BossHeadRotation(ref float rotation)
        {
            rotation = NPC.rotation;
        }

        public override void BossLoot(ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Common drops.
            npcLoot.Add(ItemDropRule.Common(ItemType<PharaohTrophy>(), 10));
        }

        public override void FindFrame(int frameHeight)
        {
            if (!die) NPC.frameCounter++;
            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            if (NPC.frame.Y >= frameHeight * 5)
            {
                NPC.frame.Y = 0;
            }
        }
        public ref float AITimer => ref NPC.ai[1];
        public ref float AITimer2 => ref NPC.ai[2];
        public ref float ArenaTimer => ref NPC.ai[3];
        private enum AttackPattern : byte
        {
            Intro = 0,
            Reposition = 1,
            ChargeAttack = 2,
            GroundPound = 3,
            FanBarrage = 4,
            TeleportAttack = 5,
            SwordRing = 6,
            SandnadoAttack = 7,
            ArenaSpawn = 8,
            PhaseTransition = 9,
            Phase3 = 10,
            DefeatAnim = 11
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return true;
        }

        // Keeping variables synced

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ) // Code that *should* send these variables for updating.
            {
                writer.Write(ArenaDir);
                writer.Write(ArenaDuration);

                for (int state = 2; state < aiWeights.Length; state++)
                {
                    writer.Write(aiWeights[state]);
                }

                writer.Write(useContactDamage);
                writer.Write(changeDirection);
                writer.Write(die);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient) // Receive extra AI variables to sync on multiplayer clients.
            {
                ArenaDir = reader.ReadInt32();
                ArenaDuration = reader.ReadInt32();

                for (int state = 2; state < aiWeights.Length; state++)
                {
                    aiWeights[state] = reader.ReadInt32();
                }

                useContactDamage = reader.ReadBoolean();
                changeDirection = reader.ReadBoolean();
                die = reader.ReadBoolean();
            }
        }

        // While creating an arena, take reduced damage from the player. Amount of damage reduced is based on difficulty.

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (NPC.ai[0] == 8) modifiers.FinalDamage *= Main.expertMode ? .5f : .75f;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (NPC.ai[0] == 8) modifiers.FinalDamage *= Main.expertMode ? .5f : .75f;
        }

        // Important for using the arena
        public Vector2 ArenaCenter;
        public float ArenaRadius = Main.expertMode ? 420 : 630;
        public int ArenaDuration = 1500;
        public float ArenaDir = 1;

        // Additional variables
        public Vector2 PositionPoint;
        public bool phase2;
        public bool phase3;
        public bool die;
        public bool eyeMode;
        public bool changeDirection = true;
        public bool useContactDamage;
        public float shakeFrequency = .05f;
        public float MusicPitch;

        public float[] aiWeights = new float[8];

        private void PickAttack() // Checks the RNG pool to see if the pool needs a reset, then picks a random attack.
        {
            WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
            int emptyCounter = 0;

            for (int state = 2; state < aiWeights.Length; state++)
            {
                if (aiWeights[state] == 0) emptyCounter++; // Checks how many attacks have been set to 0.

                aiStatePool.Add(state, Math.Pow(aiWeights[state], 1));
            }

            if (emptyCounter == 6) // If all attacks are expended, reset the pool and reposition again.
            {
                ResetAIStates();
                NPC.ai[0] = 1;
            }
            else if (ArenaTimer < ArenaDuration) // Pick a random attack and reduce its weight otherwise, assuming its not time to make a new arena or change phase.
            {
                NPC.ai[0] = aiStatePool;
                aiWeights[(int)NPC.ai[0]]--;
            }
            else NPC.ai[0] = 8; // Make a new arena if the timer is up.

            if ((NPC.life <= NPC.lifeMax * .6f && !phase2) || (NPC.life <= NPC.lifeMax * .2f && Main.expertMode && !phase3)) NPC.ai[0] = 9;
        }

        private void ResetAIStates()
        {
            aiWeights[0] = 0;
            for (int state = 2; state < aiWeights.Length; state++) // Enemy attack pool resets here. Attack pool is determined by phase.
            {
                if (phase3) aiWeights[state] = state == 2 ? 10 : 0;
                else if (phase2 && !phase3) aiWeights[state] = (state % 2 == 0 ? 3 : 2);
                else aiWeights[state] = (state >= 6 ? 1 : state >= 4 ? 2 : 3);
            }
        }
    }
}
