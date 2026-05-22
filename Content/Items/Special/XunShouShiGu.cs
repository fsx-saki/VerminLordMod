using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 驯兽师蛊
    /// 凡蛊（推测，与匠人蛊等并列）
    /// </summary>
    public class XunShouShiGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 500;
        }
    }
}
