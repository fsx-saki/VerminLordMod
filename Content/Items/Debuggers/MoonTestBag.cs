using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Items.Weapons.Four;
using VerminLordMod.Content.Items.Weapons.Five;
using VerminLordMod.Content.Items.Weapons.Zero;

namespace VerminLordMod.Content.Items.Debuggers
{
	/// <summary>
	/// 月道测试宝藏袋 — 包含已实装的月道蛊虫。
	/// </summary>
	public class MoonTestBag : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
			Item.value = 0;
			Item.rare = ItemRarityID.Quest;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.UseSound = SoundID.Item7;
		}

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			// === 零转（月道技术储备）===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<MoonBaseGu>());

			// === 二转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<InvitingMoonGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YinLinGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YueHenGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YueX2Gu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YueXXGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YueXuanGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YaoYueGu>());

			// === 三转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<GoldMoon>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<MoonHandKnife>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<QiXiangJiuChongGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<ShuangLinMoonGu>());

			// === 四转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YueYingGu>());

			// === 五转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<BaoYueGuangWangGu>());

			if (player.whoAmI == Main.myPlayer)
				Main.NewText("月道测试物品已发放！", new Color(180, 210, 255));
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Consumables.YuanS>(), 1)
				.Register();
		}
	}
}
