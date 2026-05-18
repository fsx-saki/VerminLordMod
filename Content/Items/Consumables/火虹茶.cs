using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 食材 — 火虹茶
    /// 琅琊地灵喜爱的茶水，红彤彤散光，内含游鱼般的虹光，需一口喝掉才能真正品味。
    /// </summary>
    public class 火虹茶 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 99;
            Item.value = 5000;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.DrinkOld;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
        }
    }
}
