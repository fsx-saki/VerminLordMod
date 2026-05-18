using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 事实浮冰
    /// 无极魔尊用衍化仙蛊和天外混沌推演出的物品，蕴藏衍化仙蛊和天机蛊仙蛊方。
    /// </summary>
    public class 事实浮冰 : ModItem
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
