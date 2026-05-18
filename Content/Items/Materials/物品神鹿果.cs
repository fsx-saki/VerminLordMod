using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 物品:神鹿果
    /// 六转仙材，形如婴儿拳头，生长于荒兽挂果麋鹿的鹿角上，用于喂养金刚念仙蛊。
    /// </summary>
    public class 物品神鹿果 : ModItem
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
