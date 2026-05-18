using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 心境
    /// 天地秘境，星宿仙尊持有，灯笼形态，可抽取情绪和智道道痕。
    /// </summary>
    public class 心境 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
