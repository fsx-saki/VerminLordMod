using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 孪生冰心
    /// 蓝色心脏形状的珍贵仙材，分为普通冰心和孪生冰心（七转仙材），产自玉壶山。
    /// </summary>
    public class 孪生冰心 : ModItem
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
