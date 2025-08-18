namespace TerrariaDesertExpansion.Content.Items.Equips
{
    class CactusLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ProjectileType<CactusLampPet>();
            Item.width = 16;
            Item.height = 30;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Green;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 1, 50);
            Item.buffType = BuffType<CactusLampBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }
    }

    class CactusLampBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.projPet[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ProjectileType<CactusLampPet>());
        }
    }

    class CactusLampPet : ModProjectile
    {
        public override string Texture => "TerrariaDesertExpansion/Content/Items/Equips/CactusLamp";

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;

            // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
            // * It uses fluent API syntax, just like Recipe
            // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
            // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
            // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
            // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 0)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.scale = 1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        private void CheckActive(Player player)
        {
            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff
            if (!player.dead && player.HasBuff(ModContent.BuffType<CactusLampBuff>()))
            {
                Projectile.timeLeft = 2;
            }
        }

        bool flicker;
        bool chase;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Vector2 IdealPos = owner.Center + new Vector2(34 * owner.direction, -24);

            if (flicker)
            {
                if (Projectile.ai[1] > 0) Projectile.ai[1]--;
                else flicker = false;
            }

            if (Main.rand.NextBool(600) && !flicker) 
            {
                flicker = true;
                Projectile.ai[1] = Main.rand.Next(5, 11);
            }

            CheckActive(owner);

            if (Projectile.Distance(IdealPos) > 160) chase = true;
            if (Projectile.Distance(IdealPos) < 80) chase = false;

            // Decides whether the pet should move slow or fast

            if (chase)
            {
                Projectile.velocity += Projectile.DirectionTo(IdealPos) * 0.5f;
                Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.velocity.ToRotation() + MathHelper.PiOver2, .2f);
                Projectile.velocity *= 0.99f;

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 31, Scale: .8f);
                    dust.noGravity = false;
                    dust.alpha = 140;
                    dust.fadeIn = 1.2f;
                }
            }
            else
            {
                if (Projectile.Distance(IdealPos) > 4) Projectile.velocity += Projectile.DirectionTo(IdealPos) * 0.2f;
                Projectile.velocity *= 0.95f;

                Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.Clamp(Projectile.velocity.X * 0.1f, -0.75f, 0.75f), 0.2f);                
            }

            // Teleports the pet if the player is too far away

            if (Projectile.Distance(IdealPos) > 800)
            {
                Projectile.position = IdealPos - (Projectile.Size / 2);
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            float glowMult = (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 30;

            if (flicker) 
            {
                if (!Main.dedServ) Lighting.AddLight(Projectile.Center, new Vector3(Projectile.Opacity * .35f, Projectile.Opacity * .2f, Projectile.Opacity * .1f));
            }
            else if (!Main.dedServ) Lighting.AddLight(Projectile.Center, new Vector3(.75f + Math.Abs(glowMult), .5f + Math.Abs(glowMult), .3f + Math.Abs(glowMult)));

            Projectile.ai[0]++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;
            Texture2D mask = Request<Texture2D>(Texture + "Mask").Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects effects = SpriteEffects.None;

            if (Projectile.spriteDirection < 0) effects = SpriteEffects.FlipHorizontally;
            else effects = SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, drawOrigin, 1, effects, 0);
            if (!flicker) Main.EntitySpriteDraw(mask, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, 1, effects, 0);

            return false;
        }
    }
}
