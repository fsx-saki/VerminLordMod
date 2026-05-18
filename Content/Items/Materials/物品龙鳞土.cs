using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 物品:龙鳞土
    /// 凡道蛊材，颗颗结粒，土质坚硬，形似龙鳞，能激发龙鱼繁衍。
    /// </summary>
    public class 物品龙鳞土 : ModItem
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
