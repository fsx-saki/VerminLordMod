using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 劫运坛
    /// 八转仙蛊屋，巨阳仙尊所创，被冰塞川操纵，曾附着无极魔尊的杀招。
    /// </summary>
    public class 劫运坛 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
