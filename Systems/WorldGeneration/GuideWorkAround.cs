using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace TerrariaDesertExpansion.Systems.WorldGeneration
{ /*
    class GuideWorkAround : ModSystem
    {
        public static LocalizedText GuideCheckMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            GuideCheckMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(GuideCheckMessage)}"));
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int PassIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));

            if (PassIndex != -1)
            {
                tasks.Insert(PassIndex + 1, new GuideCheckPass("Check for Guide", 100f));
            }
        }
    }

    public class GuideCheckPass : GenPass
    {
        public GuideCheckPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = GuideWorkAround.GuideCheckMessage.Value;

            if (!NPC.AnyNPCs(NPCID.Guide))
            {
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), Main.spawnTileX, Main.spawnTileY, NPCID.Guide);
            }
        }
    } */
}
