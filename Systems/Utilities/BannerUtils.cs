using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ObjectData;

//Code made by Gabehaswon

namespace DesertExpansion.Systems.Utilities
{
    /// <summary>
    /// Apply this to a ModNPC's class to autoload their items as NPCNameBanner and NPCNameBannerItem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoloadBannerAttribute : Attribute
    {
    }

    public class AutoloadedContentLoader
    {
        static Hook setDefaultsDetour;

        internal static void Load(Mod mod)
        {
            var types = AssemblyManager.GetLoadableTypes(mod.Code).Where(x => !x.IsAbstract && typeof(ModNPC).IsAssignableFrom(x));
            string banners = "";
            string critters = "";

            foreach (var type in types)
            {
                if (Attribute.IsDefined(type, typeof(AutoloadBannerAttribute)))
                {
                    var banner = type.GetCustomAttribute(typeof(AutoloadBannerAttribute));
                    var npc = mod.Find<ModNPC>(type.Name);

                    mod.AddContent(new BaseBannerTile(npc.Type, npc.Name + "Banner", npc.Texture + "Banner"));
                    mod.AddContent(new BaseBannerItem(npc.Name + "BannerItem", mod.Find<ModTile>(npc.Name + "Banner").Type, npc.Type, npc.Texture + "BannerItem"));
                    banners += $"{mod.Name}.{npc.Name}Banner, ";
                }
            }

            if (banners.Length > 2)
                mod.Logger.Debug($"[NPCUtils] AutoloadBanner: Autoloaded banners: {banners[..^2]}");

            if (critters.Length > 2)
                mod.Logger.Debug($"[NPCUtils] CritterItem: Autoloaded critters: {critters[..^2]}.");

            setDefaultsDetour = new Hook(typeof(NPCLoader).GetMethod("SetDefaults", BindingFlags.Static | BindingFlags.NonPublic), SetAutoloadedValues, true);
        }

        internal static void Unload()
        {
            setDefaultsDetour.Undo();
            setDefaultsDetour.Dispose();
            setDefaultsDetour = null;
        }

        private static void SetAutoloadedValues(Action<NPC, bool> orig, NPC self, bool createModNPC)
        {
            orig(self, createModNPC);

            if (self.ModNPC is null)
                return;

            if (Attribute.IsDefined(self.ModNPC.GetType(), typeof(AutoloadBannerAttribute)))
            {
                self.ModNPC.Banner = self.type;
                self.ModNPC.BannerItem = self.ModNPC.Mod.Find<ModItem>(self.ModNPC.Name + "BannerItem").Type;
            }
        }
    }

    public class BaseBannerTile : ModTile
    {
        /// <summary>
        /// The NPC this banner is associated with.
        /// </summary>
        public readonly int NPCType;

        /// <summary>
        /// The internal name of this banner, which is NPCNameBanner.
        /// </summary>
        public readonly string InternalName;

        /// <summary>
        /// The internal name of this banner, which is NPCNameBanner.
        /// </summary>
        public readonly string InternalTexture;

        /// <summary>
        /// Overrides the internal name to use the <see cref="InternalName"/>.
        /// </summary>
        public sealed override string Name => InternalName;

        /// <summary>
        /// Overrides the texture to use the desired texture in <see cref="InternalTexture"/>.
        /// </summary>
        public override string Texture => InternalTexture;

        /// <summary>
        /// Creates a banner tile with no name and no internal name.
        /// </summary>
        public BaseBannerTile()
        {
            NPCType = -1;
            InternalName = "";
            InternalTexture = "";
        }

        /// <summary>
        /// Creates a banner tile with the given name and internal name.
        /// </summary>
        /// <param name="npcType">The associated NPC ID.</param>
        /// <param name="internalName">The internal name of the banner.</param>
        /// <param name="texture">The internal texture path of the banner.</param>
        public BaseBannerTile(int npcType, string internalName, string texture)
        {
            InternalTexture = texture;
            NPCType = npcType;
            InternalName = internalName;
        }

        /// <summary>
        /// Disables loading if NPCType is invalid.
        /// </summary>
        /// <param name="mod">The mod loading the banner.</param>
        /// <returns></returns>
        public override bool IsLoadingEnabled(Mod mod) => NPCType != -1;

        /// <summary>
        /// Sets defaults to a normal banner.
        /// </summary>
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileID.Sets.ReplaceTileBreakDown[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.MultiTileSway[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top); // Default style
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);

            // Platform-placed style
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.DrawYOffset = -8;
            TileObjectData.addAlternate(0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(13, 88, 130));

            DustType = -1;
        }

        /// <summary>
        /// Sets banner buff and hasBanner flags.
        /// </summary>
        /// <param name="i"><inheritdoc/></param>x
        /// <param name="j"><inheritdoc/></param>
        /// <param name="closer"><inheritdoc/></param>
        public override void NearbyEffects(int i, int j, bool closer)
        {
            int itemType = Mod.Find<ModItem>(Name + "Item").Type;

            if (ItemID.Sets.BannerStrength.IndexInRange(itemType) && ItemID.Sets.BannerStrength[itemType].Enabled)
            {
                Main.SceneMetrics.NPCBannerBuff[NPCType] = true;
                Main.SceneMetrics.hasBanner = true;
            }
        }

        /// <summary>
        /// Draws tile sway.
        /// </summary>
        /// <param name="i">X position.</param>
        /// <param name="j">Y position.</param>
        /// <param name="spriteBatch">spriteBatch to use.</param>
        /// <returns>Whether the tile draws.</returns>
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileObjectData.IsTopLeft(Main.tile[i, j]))
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);

            return false;
        }
    }

    public class BaseBannerItem : ModItem
    {
        string InternalName;
        string InternalTexture;
        int PlaceID;
        int NPCType;

        /// <summary>
        /// New banners must be cloned.
        /// </summary>
        protected sealed override bool CloneNewInstances => true;

        /// <summary>
        /// New banners must use the internal name.
        /// </summary>
        public sealed override string Name => InternalName;

        /// <summary>
        /// New banners must also use the internal texture.
        /// </summary>
        public sealed override string Texture => InternalTexture;

        /// <summary>
        /// New banners use only the BannerBonus tooltip line.
        /// </summary>
        public override LocalizedText Tooltip => Language.GetText("CommonItemTooltip.BannerBonus");

        /// <summary>
        /// Creates a banner item with default values.
        /// </summary>
        public BaseBannerItem()
        {
            InternalName = "";
            InternalTexture = "";
            PlaceID = TileID.Dirt;
            NPCType = NPCID.None;
        }

        /// <summary>
        /// Creates a banner item with the given values.
        /// </summary>
        /// <param name="internalName">The given internal name.</param>
        /// <param name="placeID">The banner ID to place.</param>
        /// <param name="npcType">The NPC type associated with the banner. Used for the tooltip.</param>
        /// <param name="texture">The given texture.</param>
        public BaseBannerItem(string internalName, int placeID, int npcType, string texture)
        {
            InternalName = internalName;
            InternalTexture = texture;
            PlaceID = placeID;
            NPCType = npcType;
        }

        /// <summary>
        /// Clones the current <see cref="BaseBannerItem"/>.
        /// </summary>
        /// <param name="newEntity">The new entity this is being cloned to.</param>
        /// <returns>The clone.</returns>
        public override ModItem Clone(Item newEntity)
        {
            var clone = base.Clone(newEntity) as BaseBannerItem;
            clone.InternalName = InternalName;
            clone.InternalTexture = InternalTexture;
            clone.PlaceID = PlaceID;
            clone.NPCType = NPCType;
            return clone;
        }

        /// <summary>
        /// Adds the name of the associated NPC to the BannerBuff tooltip.
        /// </summary>
        /// <param name="tooltips"></param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltip = tooltips.First(x => x.Name == "Tooltip0");
            tooltip.Text += Lang.GetNPCName(NPCType);
        }

        /// <summary>
        /// Disables loading if this item places only dirt.
        /// </summary>
        public override bool IsLoadingEnabled(Mod mod) => PlaceID != TileID.Dirt;

        /// <summary>
        /// Sets the defaults of the banner item to be a 12x30 tile placing item with a Blue rarity.
        /// </summary>
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(PlaceID);
            Item.Size = new Vector2(12, 30);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
