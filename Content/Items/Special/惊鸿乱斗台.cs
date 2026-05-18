using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 惊鸿乱斗台
    /// 一座由红莲蛊仙独创的仙蛊屋，能吸摄封印对手的攻势并反击回去。曾被用于镇压八转蛊仙武斗天王，最后埋藏于南疆无名山峰之下。
    /// </summary>
    public class 惊鸿乱斗台 : ModItem
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
