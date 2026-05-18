using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 苍蓝龙鲸
    /// 太古传奇荒兽，被乐土仙尊点化，拥有仙窍可修行，体内有龙鲸乐土。
    /// </summary>
    public class 苍蓝龙鲸 : ModItem
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
