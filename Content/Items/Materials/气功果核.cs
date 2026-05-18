using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 气功果核
    /// 天庭的气道造物，极为巨大，成为致命弱点。后被气海老祖炼化缩小，最终被方源用于炼成气道分身。
    /// </summary>
    public class 气功果核 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
            Item.value = 3000;
        }
    }
}
