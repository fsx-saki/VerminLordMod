using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 年华池
    /// 至尊仙窍内的特殊建筑，沟通光阴支流，用于拘禁和驯化太古年兽。
    /// </summary>
    public class 年华池 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
