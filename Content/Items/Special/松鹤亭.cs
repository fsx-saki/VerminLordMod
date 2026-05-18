using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 松鹤亭
    /// 中洲仙鹤门的仙蛊屋，被方源埋伏吞噬，后由龙公送出战场。
    /// </summary>
    public class 松鹤亭 : ModItem
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
