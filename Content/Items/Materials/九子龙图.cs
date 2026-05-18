using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 九子龙图
    /// 商燕飞早年获得的仙缘，九张兽皮，每张描绘一种龙兽，有墨文注释，蕴含秘密。
    /// </summary>
    public class 九子龙图 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 10;
            Item.value = 20000;
        }
    }
}
