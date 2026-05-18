using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 角连营
    /// 中洲仙蛊屋，由许多蛊虫组成，包括爱意仙蛊。在逆流河中解体。
    /// </summary>
    public class 角连营 : ModItem
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
