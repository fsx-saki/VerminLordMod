using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 星宿棋盘
    /// 仙蛊屋，紫薇仙子所有，可用于推算和支撑星投杀招，若得智慧蛊可升九转。
    /// </summary>
    public class 星宿棋盘 : ModItem
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
