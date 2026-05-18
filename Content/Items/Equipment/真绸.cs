using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Equipment
{
    /// <summary>
    /// 装备 — 真绸
    /// 上等丝绸，用于包裹鞋子。
    /// </summary>
    public class 真绸 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 30000;
        }
    }
}
