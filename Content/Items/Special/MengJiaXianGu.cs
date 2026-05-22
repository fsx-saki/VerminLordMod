using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 梦甲仙蛊
    /// 疑似九转（梦道稀有）
    /// </summary>
    public class MengJiaXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 1;
            Item.value = 5000000;
        }
    }
}
