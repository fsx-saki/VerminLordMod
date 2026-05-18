using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 天露
    /// 八转仙材，透明水滴状，由黑白水每天面对苍穹昼夜轮转而凝聚，是天露绿洲的核心产物。
    /// </summary>
    public class 天露 : ModItem
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
