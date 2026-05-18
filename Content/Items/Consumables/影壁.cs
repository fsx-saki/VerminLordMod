using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 酒类 — 影壁
    /// 山壁上形成的影像记录，记录了花酒行者和古月四代族长的恩怨，后异变显示花酒行者传承信息。
    /// </summary>
    public class 影壁 : ModItem
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
