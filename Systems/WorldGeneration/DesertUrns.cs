using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;
using TerrariaDesertExpansion.Content.Tiles;
using Terraria.DataStructures;
/*
namespace TerrariaDesertExpansion.Systems.WorldGeneration
{
    public class DesertUrn : ModSystem
    {
        public static LocalizedText DesertUrnMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            DesertUrnMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DesertUrnMessage)}"));
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int PassIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Pots"));

            if (PassIndex != -1)
            {
                tasks.Insert(PassIndex + 1, new DesertUrnPass("Cremating the dead", 100f));
            }
        }
    }

    public class DesertUrnPass : GenPass
    {
        public static List<Point16> UrnPoint = new List<Point16>();

        public DesertUrnPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = DesertUrn.DesertUrnMessage.Value;

            int success = 0;
            int attempts = 0;

            int urnGenCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.0000012);

            int x = 1;
            int y = 1;
            int rotateUrnType = 0;

            while (success < urnGenCount)
            {
                attempts++;

                if (attempts > 100000) break;

                x = WorldGen.genRand.Next(GenVars.desertHiveLeft, GenVars.desertHiveRight);
                y = WorldGen.genRand.Next(GenVars.desertHiveHigh, GenVars.desertHiveLow + 100);

                Point16 genPoint16 = new Point16(x, y);

                if ((Main.tile[x, y + 1].TileType == TileID.Sandstone || Main.tile[x, y + 1].TileType == TileID.HardenedSand) && WorldGen.InWorld(x, y, 8))
                {
                    if (UrnPoint.Count >= 1)
                    {
                        for (int i = 0; i < UrnPoint.Count; i++)
                        {
                            if (i >= UrnPoint.Count)
                            {
                                break;
                            }

                            if (Math.Sqrt(Math.Pow(x - UrnPoint[i].X, 2) + Math.Pow(y - UrnPoint[i].Y, 2)) >= 20) WorldGen.PlaceTile(x, y, TileType<HauntedUrn>(), true, style: rotateUrnType);
                            else break;
                        }
                    }
                    else WorldGen.PlaceTile(x, y, TileType<HauntedUrn>(), true, style: rotateUrnType);
                }

                if (Main.tile[x, y].TileType == TileType<HauntedUrn>())
                {
                    success++;
                    rotateUrnType++;
                    UrnPoint.Add(genPoint16);

                    if (rotateUrnType > 2) rotateUrnType = 0;
                }
            }
        }
    }
}
*/