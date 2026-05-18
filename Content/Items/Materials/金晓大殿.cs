using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 金晓大殿
    /// 正道一方的基地悬空大殿，恢弘雄壮。
    /// </summary>
    public class 金晓大殿 : ModItem
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
