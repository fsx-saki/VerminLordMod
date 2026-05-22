using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 爱情仙蛊
    /// 九转
    /// </summary>
    public class AiQingXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 1;
            Item.value = 5000000;
        }
    }
}
