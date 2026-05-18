using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 食材 — 气泡鱼
    /// 特殊鱼类，能吞食虫子，在体内培育蛊虫，增加虫群产蛊几率。在五域大战时期价值极高。
    /// </summary>
    public class 气泡鱼 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
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
