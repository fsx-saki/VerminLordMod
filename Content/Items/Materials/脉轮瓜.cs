using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 脉轮瓜
    /// 七转仙材，蕴藏炼道道痕，可用于炼蛊，瓜熟地洞大量豢养，占宝黄天六成市场。
    /// </summary>
    public class 脉轮瓜 : ModItem
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
