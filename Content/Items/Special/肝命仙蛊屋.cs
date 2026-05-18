using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 肝命仙蛊屋
    /// 原为肝命大阵，后被方源改良为仙蛊屋，具有治疗和恢复功效。
    /// </summary>
    public class 肝命仙蛊屋 : ModItem
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
