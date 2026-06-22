using TerrariaDesertExpansion.Content.Items.Weapons;
using TerrariaDesertExpansion.Content.Projectiles.Ammo;

namespace TerrariaDesertExpansion.Systems.GlobalNPCs
{
    public class AloeLeafDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool AloeDebuff;

        public override void ResetEffects(NPC npc)
        {
            AloeDebuff = false;
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[projectile.owner];

            if (AloeDebuff && ProjectileID.Sets.IsAWhip[projectile.type])
            {
                int EmbedCount = 0;
                foreach (var p in Main.ActiveProjectiles)
                {
                    if (p.type == ProjectileType<AloeLeaf>() && p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        EmbedCount++;
                        p.active = false;
                    }
                }

                int numDusts = 12;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(npc.Center, 0, 0, 102, Scale: 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }

                SoundEngine.PlaySound(SoundID.Item112, projectile.Center);

                owner.Heal(EmbedCount);
                npc.SimpleStrikeNPC(2 + (EmbedCount * 2),hit.HitDirection, false, 2 + EmbedCount, DamageClass.Summon, false);

                npc.DelBuff(npc.FindBuffIndex(BuffType<AloeLeafDebuff>()));
                AloeDebuff = false;
            }
        }
    }

    public class AboreArrowDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool AboreDebuff;

        public override void ResetEffects(NPC npc)
        {
            AboreDebuff = false;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (AboreDebuff)
            {
                int EmbedCount = 0;
                foreach (var p in Main.ActiveProjectiles)
                {
                    if (p.type == ProjectileType<AboreArrowProjectile>() && p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        EmbedCount++;
                        p.localAI[0] = 0;
                    }
                }

                modifiers.FlatBonusDamage += EmbedCount;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (AboreDebuff && !projectile.IsMinionOrSentryRelated)
            {
                int exampleJavelinCount = 0;
                foreach (var p in Main.ActiveProjectiles)
                {
                    if (p.type == ProjectileType<AboreArrowProjectile>() && p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        exampleJavelinCount++;
                        p.localAI[0] = 0;
                    }
                }

                modifiers.FlatBonusDamage += exampleJavelinCount;
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ProjectileType<AboreArrowProjectile>())
            {
                int exampleJavelinCount = 0;
                foreach (var p in Main.ActiveProjectiles)
                {
                    if (p.type == ProjectileType<AboreArrowProjectile>() && p.ai[0] == 1f && p.ai[1] == npc.whoAmI)
                    {
                        exampleJavelinCount++;
                        p.localAI[0] = 0;
                    }
                }

                CombatText.NewText(npc.getRect(), Color.LightSeaGreen, "+" + exampleJavelinCount + " damage!", dot: true);
            }
        }
    }
}
