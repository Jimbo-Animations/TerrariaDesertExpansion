global using Terraria.ModLoader;
global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ID;
global using static Terraria.ModLoader.ModContent;
global using Terraria.Audio;
global using TerrariaDesertExpansion.Systems.Utilities;
using System.Collections.Generic;

namespace TerrariaDesertExpansion
{
	public class TerrariaDesertExpansion : Mod
	{
        private static readonly HashSet<string> AutoloadedContent = [];

        public static void AutoloadModBannersAndCritters(Mod mod)
        {
            if (!AutoloadedContent.Contains(mod.Name))
            {
                AutoloadedContentLoader.Load(mod);
                AutoloadedContent.Add(mod.Name);
            }
        }

        public static void UnloadMod(Mod mod)
        {
            AutoloadedContent.Remove(mod.Name);

            if (AutoloadedContent.Count == 0)
            {
                AutoloadedContentLoader.Unload();
            }
        }

        public override void Load()
        {
            AutoloadModBannersAndCritters(this);
        }

        public override void Unload()
        {
            UnloadMod(this);
        }
    }
}
