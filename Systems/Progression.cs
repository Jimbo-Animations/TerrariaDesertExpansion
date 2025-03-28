using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace TerrariaDesertExpansion.Systems
{
    internal class Progression : ModSystem
    {
        public static bool DownedCactusSlime;

        public override void OnWorldLoad()
        {
            DownedCactusSlime = false;
        }

        public override void OnWorldUnload()
        {
            DownedCactusSlime = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var downed = new List<string>();

            if (DownedCactusSlime) downed.Add("CactusSlime");

            tag.Add("downed", downed);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            var downed = tag.GetList<string>("downed");

            DownedCactusSlime = downed.Contains("CactusSlime");
        }

        public override void NetSend(BinaryWriter writer)
        {
            // Order of operations is important and has to match that of NetReceive
            var flags = new BitsByte();
            flags[0] = DownedCactusSlime;
            writer.Write(flags);
        }


        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            DownedCactusSlime = flags[0];
        }
    }
}
