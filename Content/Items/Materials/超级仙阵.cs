using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 超级仙阵
    /// 方源设计并改进的防御仙阵，由异人蛊仙操控，可对抗离歌等杀招。
    /// </summary>
    public class 超级仙阵 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
