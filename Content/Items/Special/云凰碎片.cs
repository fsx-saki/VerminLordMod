using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 云凰碎片
    /// 太古荒兽云凰的身体碎片，似云似雾，蕴含大量云道道痕，可化为凤凰状白云。
    /// </summary>
    public class 云凰碎片 : ModItem
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
