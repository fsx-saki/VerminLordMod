using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SubworldLibrary;
using VerminLordMod.Common.SubWorlds;

namespace VerminLordMod.Content.Items.Placeable
{
    /// <summary>
    /// 古月族地传送门物品
    /// 使用后进入 GuYueTerritory 小世界
    /// </summary>
    public class SubworldPortalGuYue : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer && !SubworldSystem.AnyActive())
            {
                SubworldSystem.Enter<GuYueTerritory>();
                return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
