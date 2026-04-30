using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI.KongQiaoUI;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 空窍石 — 右键打开空窍面板，查看和管理已炼化的蛊虫。
    /// </summary>
    public class KongQiaoStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 1000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.consumable = false;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            ModContent.GetInstance<KongQiaoUISystem>().ToggleUI();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
