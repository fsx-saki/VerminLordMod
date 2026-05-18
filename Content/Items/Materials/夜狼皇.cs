using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 夜狼皇
    /// 狼群中的皇者，夜狼的兽皇，媲美五转蛊师战力。方源通过它收编大量夜狼群。
    /// </summary>
    public class 夜狼皇 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 20000;
        }
    }
}
