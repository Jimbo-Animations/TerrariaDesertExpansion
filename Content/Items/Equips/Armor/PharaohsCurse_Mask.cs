namespace TerrariaDesertExpansion.Content.Items.Equips.Armor
{
    [AutoloadEquip(EquipType.Head)]
    internal class PharaohsCurse_Mask : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;

            // Common values for every boss mask
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 75);
            Item.vanity = true;
            Item.maxStack = 1;
        }
    }
}
