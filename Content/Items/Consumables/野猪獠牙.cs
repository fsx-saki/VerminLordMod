using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 食材 — 野猪獠牙
    /// 野猪的獠牙，可作为蛊虫食物或炼制材料，也是年中考核的成绩凭证。
    /// </summary>
    public class 野猪獠牙 : ModItem
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
