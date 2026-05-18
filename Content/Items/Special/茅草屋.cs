using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 茅草屋
    /// 东方部族的第一重宝，被方源等人无声无息收走，导致东方长凡恐慌。具体功能未明，但能支撑八转攻击片刻。
    /// </summary>
    public class 茅草屋 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
