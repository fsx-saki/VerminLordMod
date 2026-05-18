using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 漂游炼巢
    /// 天地秘境雏形，状如蜂巢，漂浮炼海上，用于炼蛊。
    /// </summary>
    public class 漂游炼巢 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
