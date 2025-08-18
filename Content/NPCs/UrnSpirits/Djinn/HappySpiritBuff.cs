namespace TerrariaDesertExpansion.Content.NPCs.UrnSpirits.Djinn
{
    internal class HappySpiritBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.luck += 0.5f;
            player.lifeRegen++;
            player.aggro -= 500;
            player.GetKnockback(DamageClass.Generic) += 0.1f;
        }
    }
}
