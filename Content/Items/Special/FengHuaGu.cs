using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 风花蛊
    /// 未明确，但属凡蛊（可能四转）。
    /// </summary>
    public class FengHuaGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 20000;
        }
    }
}
