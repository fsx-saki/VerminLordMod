using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 仙蛊屋黑牢
    /// 仙蛊屋，外形如黑色流星，能撞击、干扰敌人，由黑城操纵。
    /// </summary>
    public class 仙蛊屋黑牢 : ModItem
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
