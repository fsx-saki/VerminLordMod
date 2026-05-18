using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 任务榜
    /// 至尊仙窍中的三大榜单之一，列出各种任务供蛊仙接取以获取贡献。
    /// </summary>
    public class 任务榜 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
            Item.value = 3000;
        }
    }
}
