using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 十绝大阵
    /// 影宗布置的仙阵，用于炼制某只神秘仙蛊，以幽魂魔尊的魂道底蕴和万劫力量为养料。
    /// </summary>
    public class 十绝大阵 : ModItem
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
