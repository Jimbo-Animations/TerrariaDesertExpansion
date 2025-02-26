using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;
using Terraria.UI;
using Terraria.Graphics.Shaders;
using TerrariaDesertExpansion.Content.Items.Weapons;

namespace DesertExpansion.Systems.WorldGeneration
{
    public class DesertStructureSystem : ModSystem
    {      
        public static LocalizedText DesertExpansionWorldgenPassMessage1 { get; private set; }
        public static LocalizedText DesertExpansionWorldgenPassMessage2 { get; private set; }

        public override void SetStaticDefaults()
        {
            DesertExpansionWorldgenPassMessage1 = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DesertExpansionWorldgenPassMessage1)}"));
            DesertExpansionWorldgenPassMessage2 = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DesertExpansionWorldgenPassMessage2)}"));
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int PassIndex1 = tasks.FindIndex(genpass => genpass.Name.Equals("Full Desert"));
            int PassIndex2 = tasks.FindIndex(genpass => genpass.Name.Equals("Quick Cleanup"));

            if (PassIndex1 != -1)
            {
                tasks.Insert(PassIndex1 + 1, new DesertMoundPass("Generate Desert Mound", 100f));
            }


            if (PassIndex2 != -1)
            {
                tasks.Insert(PassIndex2 + 1, new DesertHousePass("Generate Desert House", 100f));
            }
        }
    }

    public class DesertMoundPass : GenPass
    {
        public DesertMoundPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {           
            progress.Message = DesertStructureSystem.DesertExpansionWorldgenPassMessage1.Value;

            bool success = false;
            int attempts = 0;

            int x = 1;
            int y = 1;         

            while (!success)
            {
                attempts++;

                x = WorldGen.genRand.Next(GenVars.desertHiveLeft, GenVars.desertHiveRight);

                for (int j = 1; j < Main.worldSurface; j++)
                {
                    if (WorldGen.SolidTile(x, j) && !Main.tile[x, j - 1].HasTile)
                    {
                        if (Main.tile[x, j].TileType == TileID.Sand)
                        {
                            y = j;
                            break;
                        }
                        else break;
                    }
                }

                Point point = new Point(x, y);
                Point stonePoint = new Point(x, y + 2);

                Point basePoint = new Point(x - 14, y + 1);
                Point basePoint2 = new Point(x, y + 8);

                Point genPoint = new Point(point.X - 14, point.Y);

                Dictionary<ushort, int> dictionary = new Dictionary<ushort, int>();
                WorldUtils.Gen(genPoint, new Shapes.Rectangle(28, 24), new Actions.TileScanner(TileID.Sand, TileID.HardenedSand).Output(dictionary));
                int sandCount = dictionary[TileID.Sand] + dictionary[TileID.HardenedSand];

                if (sandCount > 100 && WorldGen.InWorld(x, y, 28))
                {
                    //Do the worldgen here

                    ShapeData moundShapeData = new ShapeData();
                    ShapeData baseShapeData = new ShapeData();              

                    // Main mound
                    WorldUtils.Gen(point, new Shapes.Mound(14, 14), Actions.Chain(new Modifiers.Blotches(1, 0.4), new Actions.SetTile(TileID.HardenedSand), new Actions.SetFrames(frameNeighbors: true).Output(moundShapeData)));
                    WorldUtils.Gen(basePoint, new Shapes.Rectangle(28, 5), Actions.Chain(new Modifiers.Blotches(2, 0.6), new Modifiers.Dither(0.2), new Actions.SetTile(TileID.HardenedSand), new Actions.SetFrames(frameNeighbors: true).Output(baseShapeData)));
                    WorldUtils.Gen(basePoint2, new Shapes.Circle(12, 7), Actions.Chain(new Modifiers.Blotches(2, 0.8), new Modifiers.Dither(0.6), new Actions.SetTile(TileID.HardenedSand), new Actions.SetFrames(frameNeighbors: true).Output(baseShapeData)));

                    // Added sandstone for texture
                    WorldUtils.Gen(stonePoint, new Shapes.Circle(10, 10), Actions.Chain(new Modifiers.Blotches(1, 0.4), new Modifiers.Dither(0.4), new Actions.SetTile(TileID.Sandstone), new Actions.SetFrames(frameNeighbors: true).Output(moundShapeData)));
                    WorldUtils.Gen(stonePoint, new Shapes.Circle(10, 10), Actions.Chain(new Modifiers.Blotches(1, 0.4), new Actions.PlaceWall(WallID.Sandstone), new Actions.SetFrames(frameNeighbors: true).Output(moundShapeData)));     

                    GenVars.structures.AddProtectedStructure(new Rectangle(point.X - 14, point.Y, 28, 24), 4);
                    success = true;

                    DesertHousePass.MoundPoints.Add(point);

                    break;
                }
                else if (attempts > 100000) break;
            }
        }
    }

    public class DesertHousePass : GenPass
    {
        public static List<Point> MoundPoints = new List<Point>();

        public DesertHousePass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = DesertStructureSystem.DesertExpansionWorldgenPassMessage2.Value;           

            foreach (var point in MoundPoints)
            {
                ShapeData houseShapeData = new ShapeData();
                ShapeData hollowHouseShapeData = new ShapeData();

                int houseHeight = Main.rand.Next(30, 40);

                Vector2 houseGrowthPlacement1 = new(Main.rand.Next(-3, -1), Main.rand.Next(5, 8));
                Vector2 houseGrowthPlacement2 = new( Main.rand.Next(7, 9), Main.rand.Next(5, 8));

                Point housePoint = new Point(point.X - 3, point.Y - houseHeight);

                if (WorldGen.InWorld(housePoint.X, housePoint.Y, 12))
                {
                    // Cactus house base
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(6, houseHeight - 12), Actions.Chain(new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Mound(3, 3), Actions.Chain(new Modifiers.Offset(3, 0), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(6, houseHeight - 12), Actions.Chain(new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Mound(3, 3), Actions.Chain(new Modifiers.Offset(3, 0), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    // Nubs

                    switch (Main.rand.Next(3))
                    {
                        case 0:

                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement1.X, (int)houseGrowthPlacement1.Y), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement1.X, (int)houseGrowthPlacement1.Y), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                            break;

                        case 1:

                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement2.X, (int)houseGrowthPlacement2.Y), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement2.X, (int)houseGrowthPlacement2.Y), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                            break;

                        case 2:

                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement1.X, (int)houseGrowthPlacement1.Y), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement1.X, (int)houseGrowthPlacement1.Y), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement2.X, (int)houseGrowthPlacement2.Y), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                            WorldUtils.Gen(housePoint, new Shapes.Circle(3, 2), Actions.Chain(new Modifiers.Offset((int)houseGrowthPlacement2.X, (int)houseGrowthPlacement2.Y), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                            break;
                    }

                    WorldUtils.Gen(housePoint, new ModShapes.InnerOutline(hollowHouseShapeData, true), new Actions.SetTile(TileID.CactusBlock, true));

                    // Putting stuff in the house

                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(4, 1), Actions.Chain(new Modifiers.Offset(1, houseHeight - 20), new Actions.PlaceTile(TileID.Platforms, 25), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(1, 3), Actions.Chain(new Modifiers.Offset(0, houseHeight - 16), new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(1, 3), Actions.Chain(new Modifiers.Offset(5, houseHeight - 16), new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    WorldGen.PlaceTile(housePoint.X, housePoint.Y + houseHeight - 14, TileID.ClosedDoor, forced: true, style: 4);
                    WorldGen.PlaceTile(housePoint.X + 5, housePoint.Y + houseHeight - 14, TileID.ClosedDoor, forced: true, style: 4);

                    int chestIndex = WorldGen.PlaceChest(housePoint.X + 2, housePoint.Y + houseHeight - 14, style: 42);

                    if (chestIndex != -1)
                    {
                        Chest chest = Main.chest[chestIndex];
                        // itemsToAdd will hold type and stack data for each item we want to add to the chest
                        var itemsToAdd = new List<(int type, int stack)>();

                        itemsToAdd.Add((ItemID.CactusBreastplate, 1));
                        itemsToAdd.Add((ItemID.CactusHelmet, 1));
                        itemsToAdd.Add((ItemID.CactusLeggings, 1));
                        itemsToAdd.Add((ItemType<CactusStaff>(), 1));

                        // Finally, iterate through itemsToAdd and actually create the Item instances and add to the chest.item array
                        int chestItemIndex = 0;
                        foreach (var itemToAdd in itemsToAdd)
                        {
                            Item item = new Item();
                            item.SetDefaults(itemToAdd.type);
                            item.stack = itemToAdd.stack;
                            chest.item[chestItemIndex] = item;
                            chestItemIndex++;
                            if (chestItemIndex >= 40)
                                break; // Make sure not to exceed the capacity of the chest
                        }
                    }

                    GenVars.structures.AddProtectedStructure(new Rectangle(housePoint.X - 3, housePoint.Y - houseHeight - 3, 12, houseHeight - 3), 4);
                }           
            }    
        }
    }
}
