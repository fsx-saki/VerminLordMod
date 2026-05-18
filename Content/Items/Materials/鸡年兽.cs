using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 鸡年兽
    /// 上古年兽的一种形态，体型如山，双翼巨大，羽毛鲜艳，鸡爪锋利。可被年兽召来杀招召唤，喜欢吃年蛊，战力强但不受完全掌控。
    /// </summary>
    public class 鸡年兽 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 20000;
        }
    }
}
