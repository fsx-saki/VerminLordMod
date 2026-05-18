using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 幽火龙蟒
    /// 资源，在火炭湖中繁衍，次佳环境是火炭石湖，最佳是碧潭福地的布置。
    /// </summary>
    public class 幽火龙蟒 : ModItem
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
