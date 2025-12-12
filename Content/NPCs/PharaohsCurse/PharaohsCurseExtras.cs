using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Utilities;

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

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
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
            SandnadoAttack = 6,
            ChaseAndShoot = 7,
            ArenaSpawn = 8,      
            Phase2 = 9,
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

        // Important for using the arena
        public Vector2 ArenaCenter;
        public float ArenaRadius = 420;
        public int ArenaDuration = 1500;
        public float ArenaDir = 1;

        // Additional variables
        public Vector2 PositionPoint;
        public bool phase2;
        public bool eyeMode;
        public bool changeDirection = true;
        public bool useContactDamage;
        public float shakeFrequency = .05f;

        public override void OnSpawn(IEntitySource source)
        {
            ArenaCenter = NPC.Center;
            ArenaTimer = ArenaDuration;
        }

        private float[] aiWeights = new float[8];

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
            else // Pick a random attack and reduce its weight otherwise.
            {
                NPC.ai[0] = aiStatePool;
                aiWeights[(int)NPC.ai[0]]--;
            }           

            if (ArenaTimer >= ArenaDuration) NPC.ai[0] = 8; // If it's time to make a new arena, perform it instead of any other attacks.
        }

        private void ResetAIStates()
        {
            aiWeights[0] = 0;
            for (int state = 2; state < aiWeights.Length; state++)
            {
                aiWeights[state] = (state >= 6 ? 1 : state >= 4 ? 2 : 3);
            }

            Main.NewText("Reset");
        }
    }
}
