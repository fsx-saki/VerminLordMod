using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 仇恨蛊
    /// 八转
    /// </summary>
    public class ChouHenGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
        }
    }
}
