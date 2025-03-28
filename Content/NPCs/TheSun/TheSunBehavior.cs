namespace TerrariaDesertExpansion.Content.NPCs.TheSun
{
    partial class TheSun : ModNPC
    {
        public ref float AITimer1 => ref NPC.ai[1];
        public ref float AITimer2 => ref NPC.ai[2];
        public ref float AITimer3 => ref NPC.ai[3];

        Vector2 SunIdolPos;
        public override void AI()
        {
            AITimer1++;

            int goalDirection = target.Center.X < NPC.Center.X ? -1 : 1;

            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
                NPC.TargetClosest();

            switch (AIstate)
            {
                case AttackPattern.Idle:

                    if (AITimer2 == 0) SunIdolPos = new Vector2(SunIdolPos.X == 300 ? -300 : 300, -300);

                    AITimer2++;

                    NPC.rotation = NPC.rotation.AngleTowards(0f + NPC.velocity.X * .05f, .1f);
                    MathHelper.Clamp(NPC.rotation, -1, 1);


                    if (NPC.Distance(target.Center + SunIdolPos) > 15) NPC.velocity += NPC.DirectionTo(target.Center + SunIdolPos) * .6f;
                    NPC.velocity *= NPC.Distance(target.Center + SunIdolPos) > 120 ? .98f : .92f;

                    if (NPC.Distance(target.Center + SunIdolPos) > 100 && Main.rand.NextBool(5))
                    {
                        var dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.DesertTorch, NPC.velocity.X, NPC.velocity.Y, Scale: 2f);
                        Main.dust[dust].noGravity = true;
                    }

                    if (AITimer2 > 300) AITimer2 = 0;

                    break;
            }
        }
    }
}
