using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 规蛊
    /// 概念级（无具体转数）
    /// </summary>
    public class GuiGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 1;
            Item.value = 10000000;
        }
    }
}
