using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 第十三座鹰巢
    /// 黑凡留下的一系列考验和宝藏，包括天晶鹰巢、上极天鹰蛋、血道杀招以及黑凡洞天中的本命真传。
    /// </summary>
    public class 第十三座鹰巢 : ModItem
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
