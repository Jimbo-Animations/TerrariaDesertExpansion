using Terraria.GameContent;

namespace TerrariaDesertExpansion.Content.NPCs.TheSun
{
    partial class TheSun : ModNPC
    {
        public Player target
        {
            get => Main.player[NPC.target];
        }

        public override bool CheckActive()
        {
            return Main.player[NPC.target].dead;
        }

        int animState = 0;
        int ringFrame;
        int ringFrameTimer;
        float rotationTimer;
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 3;
            NPC.frameCounter++;
            ringFrameTimer++;

            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                ringFrameTimer++;
                NPC.frame.Y += frameHeight;
                ringFrame += frameHeight;
            }

            if (ringFrame >= frameHeight * 2) ringFrame = 0;

            if (animState == 0)
            {
                NPC.frame.X = 0;
                if (NPC.frame.Y >= frameHeight * 2) NPC.frame.Y = 0;
            }

            if (animState == 1)
            {
                NPC.frame.X = NPC.frame.Width;
                if (NPC.frame.Y >= frameHeight * 2) NPC.frame.Y = 0;
            }

            if (animState == 2)
            {
                NPC.frame.X = NPC.frame.Width * 2;
                NPC.frame.Y = 0;
            }
        }

        private enum AttackPattern : byte
        {
            Idle = 0,
            Swoop = 1,
            Volley = 2,
            ExpertMove = 3,
            Victory = 4,
            Defeat = 5
        }

        private AttackPattern AIstate
        {
            get => (AttackPattern)NPC.ai[0];
            set => NPC.localAI[0] = (float)value;
        }

        public override void PostAI()
        {
            float glowMult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.15f;

            Lighting.AddLight(NPC.Center, 2.5f * (1 + glowMult), 1.75f * (1 + glowMult), 1 * (1 + glowMult));
            if (AITimer1 % 20 == 1 && Main.rand.NextBool(4))
            {
                float rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float flareWidth = (float)Main.rand.NextFloat(0.2f, 0.3f);
                float flareHeight = Main.rand.NextFloat(6, 9);
                float flareOffset = (float)Main.rand.NextFloat(0, 3);

                for (int i = -6; i < 7; i++)
                {
                    var dust = Dust.NewDust(NPC.Center + new Vector2(0, 20).RotatedBy(rotation), 0, 0, DustID.DesertTorch, 0, 0, Scale: 2.5f);
                    Main.dust[dust].noGravity = true;
                    Vector2 trueVelocity =  new Vector2(0, flareHeight).RotatedBy(i * MathHelper.Pi/ 13);
                    trueVelocity.X *= flareWidth;
                    trueVelocity = trueVelocity.RotatedBy(rotation) - new Vector2(0, flareOffset).RotatedBy(rotation);
                    Main.dust[dust].velocity = NPC.velocity + trueVelocity;
                }
            }
        }
    }
}
