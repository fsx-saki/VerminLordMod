using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 龙宫
    /// 八转仙蛊屋，内含神秘梦道仙蛊，外形如宫殿，拥有攻伐防护威能，行驶在东海高空，引发八转争夺。
    /// </summary>
    public class 龙宫 : ModItem
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
