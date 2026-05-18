using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 青铜面具
    /// 方源给夏琳的面具，色彩斑斓，用鱼鳞、鸟羽编织而成。
    /// </summary>
    public class 青铜面具 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 999;
            Item.value = 2000;
        }
    }
}
