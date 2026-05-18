using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 酒类 — 猴儿酒
    /// 猴儿酒类属果酒，酒香清醇香甜，入口绵柔，一杯元石计价。
    /// </summary>
    public class 猴儿酒 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 30;
            Item.value = 10000;

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
