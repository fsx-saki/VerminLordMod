using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 裂天粉
    /// 雪胡老祖战斗后挥洒的洁白粉末，可将白天裂开，威力惊人。
    /// </summary>
    public class 裂天粉 : ModItem
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
