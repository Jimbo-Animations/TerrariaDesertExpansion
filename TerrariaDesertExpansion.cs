global using System;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.Audio;
global using Terraria.ID;
global using Terraria.ModLoader;
global using TerrariaDesertExpansion.Systems.Utilities;
global using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;

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

            // Audio for mummy boss
            Request<SoundEffect>("TerrariaDesertExpansion/Content/Music/MummyGroan1", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("TerrariaDesertExpansion/Content/Music/MummyGroan2", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("TerrariaDesertExpansion/Content/Music/MummyGroan3", AssetRequestMode.ImmediateLoad);
            Request<SoundEffect>("TerrariaDesertExpansion/Content/Music/MummyGroan4", AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            UnloadMod(this);
        }

        // ADD PACKETS!!! 
    }
}
