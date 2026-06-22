using System.Collections.Generic;
using TerrariaDesertExpansion.Content.Items.Materials;
using TerrariaDesertExpansion.Systems.GlobalNPCs;

namespace TerrariaDesertExpansion.Content.Projectiles.Ammo
{
    class AboreArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(14, 32);
            Item.damage = 10; 
            Item.DamageType = DamageClass.Ranged;

            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(copper: 30);
            Item.shoot = ProjectileType<AboreArrowProjectile>(); // The projectile that weapons fire when using this item as ammunition.
            Item.shootSpeed = 5f; // The speed of the projectile.
            Item.ammo = AmmoID.Arrow; // The ammo class this ammo belongs to.
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Type, 50);
            recipe.AddIngredient(ItemType<DesertAloe>(), 1)
            .AddIngredient(ItemID.Wood, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }

    class AboreArrowProjectile : ModProjectile
    {
        public bool IsStickingToTarget
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        // Index of the current target
        public int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int GravityDelayTimer
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public float StickTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public float GenTimer
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(10);

            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (IsStickingToTarget)
            {
                Projectile.ignoreWater = true;
                Projectile.tileCollide = false;
                StickTimer++;

                Projectile.timeLeft = 2;

                int npcTarget = TargetWhoAmI;
                if (StickTimer >= 200 || npcTarget < 0 || npcTarget >= 200)
                { // If the index is past its limits, kill it
                    Projectile.Kill();
                }
                else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
                {
                    // If the target is active and can take damage
                    // Set the projectile's position relative to the target's center
                    Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
                }
                else Projectile.Kill(); // Otherwise, kill the projectile
            }
            else
            {
                // Apply gravity after a quarter of a second
                GenTimer++;
                if (GenTimer++ >= 20f) Projectile.velocity.Y += 0.1f;

                // The projectile is rotated to face the direction of travel
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                // Cap downward velocity
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }          
        }

        private const int MaxStickingJavelin = 5; // This is the max amount of javelins able to be attached to a single NPC
        private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin]; // The point array holding for sticking javelins
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            IsStickingToTarget = true; // we are sticking to a target
            TargetWhoAmI = target.whoAmI; // Set the target whoAmI
            Projectile.velocity = (target.Center - Projectile.Center) * 0.75f;
            Projectile.friendly = false;
            Projectile.netUpdate = true;

            target.AddBuff(BuffType<AboreArrowDebuff>(), 200);

            Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, stickingJavelins);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            // Return if the hitboxes intersects, which means the javelin collides or not
            return projHitbox.Intersects(targetHitbox);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            // For going through platforms and such, javelins use a tad smaller size
            width = height = 10; // notice we set the width to the height, the height to 10. so both are 10
            return true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            // If attached to an NPC, draw behind tiles (and the npc) if that NPC is behind tiles, otherwise just behind the NPC.
            if (IsStickingToTarget)
            {
                int npcIndex = TargetWhoAmI;
                if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
                {
                    if (Main.npc[npcIndex].behindTiles)
                    {
                        behindNPCsAndTiles.Add(index);
                    }
                    else
                    {
                        behindNPCs.Add(index);
                    }

                    return;
                }
            }
            // Since we aren't attached, add to this list
            behindProjectiles.Add(index);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position); 
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 291);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }
    }

    public class AboreArrowDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // NPCs will automatically be immune to this buff if they are immune to BoneJavelin. SkeletronHead and SkeletronPrime are immune to BoneJavelin.
            BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.BoneJavelin);
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AboreArrowDebuffNPC>().AboreDebuff = true;
        }
    }
}
