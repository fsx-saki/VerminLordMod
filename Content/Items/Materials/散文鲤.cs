using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 散文鲤
    /// 一种稀有奇鱼，与真武鲤并称文武双鲤。受惊时会化为文气，极难捕捉。东海罕见。
    /// </summary>
    public class 散文鲤 : ModItem
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
