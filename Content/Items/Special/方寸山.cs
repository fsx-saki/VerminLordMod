using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 方寸山
    /// 小人族拥有的微型山峰，可移动，蕴含律道仙蛊“小”，能施展大而化小杀招。既是重宝也是居所。
    /// </summary>
    public class 方寸山 : ModItem
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
