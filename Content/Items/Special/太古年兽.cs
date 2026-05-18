using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 太古年兽
    /// 光阴长河中生存的荒兽级生物，种类多样，如太古年鸡、太古年猴等，可被奴役。
    /// </summary>
    public class 太古年兽 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
