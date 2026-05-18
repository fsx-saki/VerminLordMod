using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 铁壳花
    /// 狐仙福地西部移植的植物，形成花海，供养花粉兔。
    /// </summary>
    public class 铁壳花 : ModItem
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
