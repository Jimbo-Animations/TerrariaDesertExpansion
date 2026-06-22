using Terraria.GameContent.ItemDropRules;
using TerrariaDesertExpansion.Content.Items.Equips;
using TerrariaDesertExpansion.Content.Items.Equips.Armor;
using TerrariaDesertExpansion.Content.Items.Materials;
using TerrariaDesertExpansion.Content.Items.Tools;
using TerrariaDesertExpansion.Content.NPCs.CactusSlime;

namespace TerrariaDesertExpansion.Content.Items.Misc
{
    internal class MegaCactusSlimeTreasureBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            // This set is one that every boss bag should have.
            // It will create a glowing effect around the item when dropped in the world.
            // It will also let our boss bag drop dev armor..
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;

            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true; // This makes sure that "Expert" displays in the tooltip and the item name color changes
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {    
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<DesertAloe>(), 1, 2, 2));
            itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemType<CactusSlime_Mask>(), 7));
            itemLoot.Add(ItemDropRule.Common(ItemType<CactusLamp>(), 1));
            itemLoot.Add(ItemDropRule.Common(ItemType<BotanicBomb>(), 1));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(NPCType<MegaCactusSlime>()));

        }
    }
}
