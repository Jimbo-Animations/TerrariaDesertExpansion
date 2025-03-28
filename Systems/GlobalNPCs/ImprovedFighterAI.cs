namespace TerrariaDesertExpansion.Systems.GlobalNPCs
{
    public class ImprovedFighterAI : GlobalNPC /// Code taken from Terrorborn, made by Penumbral/Impaxim
    {
        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        public static void CustomizableFighterAI(NPC NPC, Player target, float maxSpeed, float accelleration, float decelleration, float jumpSpeed, bool faceDirection = true, int jumpCooldown = 0, int stillTimeUntilTurnaround = 120, int wanderTime = 90, int fighter_JumpCooldown = 0, int fighter_StillTime = 0, int fighter_TargetPlayerCounter = 0)
        {
            if (Math.Abs(NPC.velocity.X) < maxSpeed - accelleration)
            {
                fighter_StillTime++;
                if (fighter_StillTime > stillTimeUntilTurnaround)
                {
                    fighter_TargetPlayerCounter = wanderTime;
                    NPC.direction *= -1;
                    fighter_StillTime = 0;
                }
            }
            else
            {
                fighter_StillTime = 0;
            }

            if (NPC.direction == 1 && NPC.velocity.X < maxSpeed)
            {
                NPC.velocity.X += accelleration;
            }

            if (NPC.direction == -1 && NPC.velocity.X > -maxSpeed)
            {
                NPC.velocity.X -= accelleration;
            }

            if (NPC.velocity.Y == 0)
            {
                if (fighter_JumpCooldown > 0)
                {
                    fighter_JumpCooldown--;
                }
                else if (!Collision.SolidCollision(NPC.position + new Vector2(NPC.width * NPC.direction, NPC.height), NPC.width, 17) || Collision.SolidCollision(NPC.position + new Vector2(NPC.width * NPC.direction, 0), NPC.width, NPC.height) || MathHelper.Distance(target.Center.X, NPC.Center.X) < NPC.width && target.position.Y + target.width < NPC.position.Y)
                {
                    if (target.Center.Y < NPC.position.Y + NPC.height)
                    {
                        NPC.velocity.Y -= jumpSpeed;
                        fighter_JumpCooldown = jumpCooldown;
                    }
                }
            }

            if (faceDirection)
            {
                NPC.spriteDirection = NPC.direction;
            }
        }

    }
}
