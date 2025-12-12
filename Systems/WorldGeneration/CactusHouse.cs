using System.Collections.Generic;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using TerrariaDesertExpansion.Content.Items.Weapons;

namespace TerrariaDesertExpansion.Systems.WorldGeneration
{
    public class DesertStructureSystem : ModSystem
    {
        public static LocalizedText DesertHouseMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            DesertHouseMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DesertHouseMessage)}"));
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int PassIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Quick Cleanup"));

            if (PassIndex != -1)
            {
                tasks.Insert(PassIndex + 1, new DesertHousePass("Generate Desert House", 100f));
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
            progress.Message = DesertStructureSystem.DesertHouseMessage.Value;

            int x = 1;
            int y = 1;

            bool success = false;
            int attempts = 0;

            while (!success)
            {
                attempts++;

                // Find location to spawn Cactus House

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

                // Set up variables and points for the Cactus House

                int houseHeight = Main.rand.Next(25, 33);

                Vector2 houseGrowthPlacement1 = new(Main.rand.Next(-3, -1), Main.rand.Next(5, 8));
                Vector2 houseGrowthPlacement2 = new(Main.rand.Next(7, 9), Main.rand.Next(5, 8));

                Point point = new Point(x, y);
                Point genPoint = new Point(point.X - 14, point.Y);
                Point housePoint = new Point(point.X, point.Y - houseHeight);

                Dictionary<ushort, int> dictionary = new Dictionary<ushort, int>();
                WorldUtils.Gen(genPoint, new Shapes.Rectangle(28, 24), new Actions.TileScanner(TileID.Sand, TileID.HardenedSand).Output(dictionary));
                int sandCount = dictionary[TileID.Sand] + dictionary[TileID.HardenedSand];

                ShapeData houseShapeData = new ShapeData();
                ShapeData hollowHouseShapeData = new ShapeData();

                // Generate the house

                if (sandCount > 100 && WorldGen.InWorld(housePoint.X, housePoint.Y, 12) && GenVars.structures.CanPlace(new Rectangle(housePoint.X - 3, housePoint.Y - houseHeight - 3, 12, houseHeight - 3), 4))
                {
                    // Cactus house base
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(6, houseHeight), Actions.Chain(new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Mound(3, 3), Actions.Chain(new Modifiers.Offset(3, 0), new Actions.Blank(), new Actions.SetFrames(frameNeighbors: true).Output(hollowHouseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(6, houseHeight), Actions.Chain(new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Mound(3, 3), Actions.Chain(new Modifiers.Offset(3, 0), new Actions.PlaceWall(WallID.Cactus), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    //Generate sand underneath house

                    WorldUtils.Gen(point, new Shapes.Rectangle(8, 2), Actions.Chain(new Modifiers.Offset(0, 0), new Actions.SetTile(TileID.Sand, true), new Actions.SetFrames(frameNeighbors: true)));

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

                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(4, 1), Actions.Chain(new Modifiers.Offset(1, houseHeight - 8), new Actions.PlaceTile(TileID.Platforms, 25), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(1, 3), Actions.Chain(new Modifiers.Offset(0, houseHeight - 4), new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));
                    WorldUtils.Gen(housePoint, new Shapes.Rectangle(1, 3), Actions.Chain(new Modifiers.Offset(5, houseHeight - 4), new Actions.ClearTile(), new Actions.SetFrames(frameNeighbors: true).Output(houseShapeData)));

                    WorldGen.PlaceTile(housePoint.X, housePoint.Y + houseHeight - 2, TileID.ClosedDoor, forced: true, style: 4);
                    WorldGen.PlaceTile(housePoint.X + 5, housePoint.Y + houseHeight - 2, TileID.ClosedDoor, forced: true, style: 4);

                    int chestIndex = WorldGen.PlaceChest(housePoint.X + 2, housePoint.Y + houseHeight - 2, style: 42);

                    if (chestIndex != -1)
                    {
                        Chest chest = Main.chest[chestIndex];

                        List<Tuple<int, double>> LootTable = new List<Tuple<int, double>>()
                        {
                            Tuple.Create((int)ItemID.ThornsPotion, 5.0),
                            Tuple.Create((int)ItemID.SwiftnessPotion, 5.0),
                            Tuple.Create((int)ItemID.LeadBar, 4.0),
                            Tuple.Create((int)ItemID.PalmWood, 4.0),
                            Tuple.Create((int)ItemID.CopperCoin, 4.0),
                            Tuple.Create((int)ItemID.PinkPricklyPear, 3.0),
                            Tuple.Create((int)ItemID.SilverCoin, 3.0),
                            Tuple.Create((int)ItemID.RestorationPotion, 2.0),
                            Tuple.Create((int)ItemID.Waterleaf, 2.0),
                            Tuple.Create((int)ItemID.BananaSplit, 1.0),
                            Tuple.Create((int)ItemID.None, 1.0),
                            Tuple.Create((int)ItemID.GenderChangePotion, 0.1)
                        };

                        int chestItemIndex = 0;

                        Item ChestItem = new Item();
                        ChestItem.SetDefaults(ItemType<CactusStaff>());
                        ChestItem.stack = 1;
                        chest.item[chestItemIndex] = ChestItem;

                        chestItemIndex++;

                        for (int i = 0; i < 6; i++)
                        {
                            WeightedRandom<int> FunnyLoot = new WeightedRandom<int>();
                            foreach (var item in LootTable)
                            {
                                FunnyLoot.Add(item.Item1, item.Item2);
                            }

                            int selectedItem = FunnyLoot.Get();

                            Item RandomItem = new Item();

                            if (selectedItem != ItemID.None)
                            {
                                RandomItem.SetDefaults(selectedItem);
                                RandomItem.stack = (selectedItem == ItemID.BananaSplit || selectedItem == ItemID.GenderChangePotion) ? 1 : (selectedItem == ItemID.LeadBar || selectedItem == ItemID.PalmWood || selectedItem == ItemID.CopperCoin || selectedItem == ItemID.SilverCoin) ? Main.rand.Next(8, 17) : Main.rand.Next(1, 4);
                                chest.item[chestItemIndex] = RandomItem;

                                chestItemIndex++;
                            }
                            LootTable.RemoveAll(item => item.Item1 == selectedItem);
                        }
                    }

                    GenVars.structures.AddProtectedStructure(new Rectangle(housePoint.X - 3, housePoint.Y - 3, 12, houseHeight - 3), 4);

                    success = true;
                }
                else if (attempts > 100000) break;
            }               
        }
    }
}
