using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 功德方尖碑
    /// 八转仙蛊屋，位于龙鲸乐土，发布任务并给予功德奖励，可兑换仙材、名号等。
    /// </summary>
    public class 功德方尖碑 : ModItem
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
