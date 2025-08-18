using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;
using TerrariaDesertExpansion.Content.Tiles;
using Terraria.DataStructures;

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

            bool tombType;

            while (success < urnGenCount)
            {
                attempts++;
                tombType = Main.rand.NextBool(2, 3);

                if (attempts > 100000) break;

                x = WorldGen.genRand.Next(GenVars.desertHiveLeft, GenVars.desertHiveRight);
                y = WorldGen.genRand.Next(GenVars.desertHiveHigh, GenVars.desertHiveLow + 100);

                Point16 genPoint16 = new Point16(x, y);

                if ((Main.tile[x, y].TileType == TileID.Sandstone || Main.tile[x, y].TileType == TileID.HardenedSand) && WorldGen.InWorld(x, y, 8))
                {
                    if (UrnPoint.Count >= 1)
                    {
                        for (int i = 0; i < UrnPoint.Count; i++)
                        {
                            if (i >= UrnPoint.Count)
                            {
                                break;
                            }

                            if (Math.Sqrt(Math.Pow(x - UrnPoint[i].X, 2) + Math.Pow(y - UrnPoint[i].Y, 2)) >= 22 && GenVars.structures.CanPlace(new Rectangle(x - 6, y - 6, 13, 8), 4)) CreateDesertShrine(tombType, Main.rand.NextBool(2));
                            else break;
                        }
                    }
                    else CreateDesertShrine(Main.rand.NextBool(), Main.rand.NextBool());

                    GenVars.structures.AddProtectedStructure(new Rectangle(x - 6, y - 6, 13, 8), 4);
                }

                if (Main.tile[x, y].TileType == TileType<HauntedUrn>())
                {
                    success++;
                    rotateUrnType++;
                    UrnPoint.Add(genPoint16);

                    if (rotateUrnType > 2) rotateUrnType = 0;
                }
            }

            void CreateDesertShrine(bool tombType, bool faceWhichDir)
            {
                if (tombType) // GENERATE ROUND TOMB
                {
                    // Tomb dome

                    WorldUtils.Gen(new Point(x, y), new Shapes.HalfCircle(6), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x, y), new Shapes.HalfCircle(6), Actions.Chain(new Actions.RemoveWall(), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x, y), new Shapes.HalfCircle(6), Actions.Chain(new Actions.PlaceWall(WallID.GoldBrick), new Actions.SetFrames(frameNeighbors: true)));                   

                    // Tomb floor

                    WorldUtils.Gen(new Point(x - 6, y + 1), new Shapes.Rectangle(13, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x - 4, y + 2), new Shapes.Rectangle(9, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));

                    // Hollow out the inside

                    WorldUtils.Gen(new Point(x, y), new Shapes.HalfCircle(4), Actions.Chain(new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true)));

                    // Add blue banners

                    WorldGen.PlaceTile(x + 3, y - 4, TileID.Banners, true, style: 2);

                    WorldGen.PlaceTile(x - 3, y - 4, TileID.Banners, true, style: 2);

                    // Make entrance 

                    WorldUtils.Gen(new Point(faceWhichDir ? x - 6 : x + 4, y - 2), new Shapes.Rectangle(3, 3), Actions.Chain(new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(faceWhichDir ? x - 6 : x + 4, y - 2), new Shapes.Rectangle(3, 3), Actions.Chain(new Actions.RemoveWall(), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(faceWhichDir ? x - 6 : x + 4, y - 2), new Shapes.Rectangle(3, 3), Actions.Chain(new Actions.PlaceWall(WallID.GoldBrick), new Actions.SetFrames(frameNeighbors: true)));

                    WorldUtils.Gen(new Point(faceWhichDir ? x - 6 : x + 6, y - 2), new Shapes.Rectangle(1, 3), Actions.Chain(new Actions.SetTile(TileID.SandstoneColumn), new Actions.SetFrames(frameNeighbors: true)));

                    // Add sand

                    WorldUtils.Gen(new Point(faceWhichDir ? x - 5 : x + 3, y), new Shapes.Rectangle(3, 1), Actions.Chain(new Modifiers.Dither(.3), new Actions.SetTile(TileID.Sand), new Actions.SetHalfTile(Main.rand.NextBool()), new Actions.SetFrames(frameNeighbors: true)));

                    // Place urn

                    WorldGen.PlaceTile(faceWhichDir ? x + 1 : x, y, TileType<HauntedUrn>(), true, style: rotateUrnType);
                }
                else // GENERATE OPEN TOMB
                {
                    // Tomb floor

                    WorldUtils.Gen(new Point(x - 6, y + 1), new Shapes.Rectangle(12, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x - 4, y + 2), new Shapes.Rectangle(8, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));

                    // Hollow out the inside and add walls

                    WorldUtils.Gen(new Point(x - 6, y - 3), new Shapes.Rectangle(12, 4), Actions.Chain(new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x - 5, y - 3), new Shapes.Rectangle(10, 4), Actions.Chain(new Actions.RemoveWall(), new Actions.SetFrames(frameNeighbors: true)));

                    WorldUtils.Gen(new Point(x - 5, y - 3), new Shapes.Rectangle(10, 4), Actions.Chain(new Actions.PlaceWall(WallID.GoldBrick), new Actions.SetFrames(frameNeighbors: true)));

                    // Add roof

                    WorldUtils.Gen(new Point(x - 6, y - 4), new Shapes.Rectangle(12, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x - 4, y - 5), new Shapes.Rectangle(8, 1), Actions.Chain(new Actions.SetTile(TileID.SandStoneSlab), new Actions.SetFrames(frameNeighbors: true)));

                    // Add supporting pillars

                    WorldUtils.Gen(new Point(x - 5, y - 3), new Shapes.Rectangle(1, 4), Actions.Chain(new Actions.SetTile(TileID.SandstoneColumn), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x + 4, y - 3), new Shapes.Rectangle(1, 4), Actions.Chain(new Actions.SetTile(TileID.SandstoneColumn), new Actions.SetFrames(frameNeighbors: true)));

                    // Add sand

                    WorldUtils.Gen(new Point(x - 6, y), new Shapes.Rectangle(4, 1), Actions.Chain(new Modifiers.Dither(.2), new Actions.SetTile(TileID.Sand), new Actions.SetHalfTile(true), new Actions.SetFrames(frameNeighbors: true)));
                    WorldUtils.Gen(new Point(x + 3, y), new Shapes.Rectangle(4, 1), Actions.Chain(new Modifiers.Dither(.2), new Actions.SetTile(TileID.Sand), new Actions.SetHalfTile(true), new Actions.SetFrames(frameNeighbors: true)));

                    // Add blue banners

                    WorldGen.PlaceTile(x + 3, y - 3, TileID.Banners, true, style: 2);

                    WorldGen.PlaceTile(x - 4, y - 3, TileID.Banners, true, style: 2);

                    // Place urn

                    WorldGen.PlaceTile(x, y, TileType<HauntedUrn>(), true, style: rotateUrnType);
                }
            }            
        }
    }
}
