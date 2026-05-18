using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 万里丝廊
    /// 萧家拥有的仙蛊屋，能铺设成巨大丝网，加速货物运输，依赖蜘蛛凡蛊维持运转。
    /// </summary>
    public class 万里丝廊 : ModItem
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
