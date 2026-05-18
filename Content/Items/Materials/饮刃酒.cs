using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 饮刃酒
    /// 七转仙材，由蒙屠足下血迹渗透地底结合刀道道痕形成，可助刀道悟道。方源取走少量。
    /// </summary>
    public class 饮刃酒 : ModItem
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
