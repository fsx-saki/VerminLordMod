using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 黑油
    /// 食道仙材，黑色油状，易于开采，产量大，可炼蛊、延缓仙僵死窍崩解，具有腐蚀性，常被采油蛊师采集。
    /// </summary>
    public class 黑油 : ModItem
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
