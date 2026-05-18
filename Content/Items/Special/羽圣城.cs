using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 羽圣城
    /// 七转仙蛊屋，外形辉煌圣城，由薄青操纵，曾与方源的惊鸿乱斗台碰撞。
    /// </summary>
    public class 羽圣城 : ModItem
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
