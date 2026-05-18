using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 斑虎蜜蜂蜂翅
    /// 疑似斑虎蜜蜂的蜂翅碎片，太古荒兽残骸，金道道痕极多，发出道痕光晕，准九转仙材。
    /// </summary>
    public class 斑虎蜜蜂蜂翅 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 99;
            Item.value = 50000;
        }
    }
}
