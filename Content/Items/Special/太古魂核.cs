using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 太古魂核
    /// 魂兽死后留下的核心，可用于魂道修行，房家曾向方源提供包括八转太古魂核在内的多种魂核。
    /// </summary>
    public class 太古魂核 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
