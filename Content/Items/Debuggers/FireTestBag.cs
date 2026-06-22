using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons.Five;
using VerminLordMod.Content.Items.Weapons.Four;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Items.Weapons.Zero;
using VerminLordMod.Content.Items.Special;

namespace VerminLordMod.Content.Items.Debuggers
{
	/// <summary>
	/// 炎道测试宝藏袋 — 包含已实装的炎道（火道）蛊虫。
	/// </summary>
	public class FireTestBag : ModItem
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
			// === 零转（炎道技术储备）===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<FireBaseGu>());

			// === 一转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<ChuiYanGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<FireClothesGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<StoveGu>());

			// === 二转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HongYanMangGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<ShuangQiaoHuoLuGu>());

			// === 三转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<BaoRanGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<BaoX3Gu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<DanHuoGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<FenShenGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<FireHeartGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HuoZhuaGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<RenShouXShengGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<SanQiaoHuoWuGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YanZhouGu>());

			// === 四转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HuoLongGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HuoSheGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<ShuiBiGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<SiQiaoHuoLouGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<TunHuoGu>());

			// === 五转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<BaiGuCheLunGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HuoGuangZhuTianGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<HuoYanPiFengGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<WuXHuoTaGu>());

			// === 特殊（三转~五转）===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<旱魃炎>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<ShuoShiDaoFei>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<火雷真传>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<YanHuaXianGu>());

			// === 六转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<JiuZhuanHuoGu>());
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<CanZhu>());

			// === 八转 ===
			player.QuickSpawnItem(player.GetSource_OpenItem(Type), ModContent.ItemType<XiaGu>());

			if (player.whoAmI == Main.myPlayer)
				Main.NewText("炎道测试物品已发放！", new Color(255, 120, 50));
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Consumables.YuanS>(), 1)
				.Register();
		}
	}
}
