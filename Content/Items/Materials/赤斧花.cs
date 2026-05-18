using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 赤斧花
    /// 地灾后新生的野花，花瓣如斧头，白色但绽放赤红光芒，可炼制三转凡蛊。
    /// </summary>
    public class 赤斧花 : ModItem
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
