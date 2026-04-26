using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.One;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	class FourFlavorWineBugWeapon : PractiseWeapon
	{
		protected override int qiCost => 10;
		protected override int _useTime => 200;
		protected override float unitConntrolRate => 5;
		protected override int controlQiCost => 5;
		protected override int _guLevel => 2;
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(1, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(2, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
		}

		//不会改变的属
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			Item.value = 50000;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			if (player.altFunctionUse == 2) {
				Item.useTime = 5;
				Item.useAnimation = 5;
				Item.useStyle = ItemUseStyleID.HoldUp;
				//Main.NewText($"qiCurrent{qiPlayer.qiCurrent}");
				//Main.NewText($"controlQiCost{controlQiCost}");
				if (hasBeenControlled) {
					Text.ShowTextRed(player, "您已经完全炼化该蛊虫");
					return false;
				}
				if (qiPlayer.qiCurrent < controlQiCost) {
					Text.ShowTextRed(player, "炼化失败 真元不足");
					return false;
				}
				qiPlayer.qiCurrent -= controlQiCost;
				controlRate += unitConntrolRate;
				Text.ShowTextGreen(player, $"炼化中......当前进度{controlRate}%");
				return true;
			}
			else {
				Item.useTime = _useTime;
				Item.useAnimation = _useTime;
				Item.useStyle = _useStyle;
			}
			if (!hasBeenControlled) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");
				return false;
			}
			if (_guLevel > qiPlayer.qiLevel) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiPlayer.qiLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
			}
			//
			if (qiPlayer.qiLevel != 2) {
				Text.ShowTextRed(player, $"您不是二转蛊师,该蛊不能为你提升真元恢复速度。");
				return false;
			}
			if (qiPlayer.hasFourWineBug == true) {
				Text.ShowTextRed(player, $"您已经使用过四味酒虫,该蛊不能为你提升真元恢复速度。");
				return false;
			}
			return qiPlayer.qiCurrent >= qiCost;
		}

		// Reduce resource on use
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				return false;
			}
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			//var random = new Random();

			qiPlayer.hasFourWineBug = true;
			qiPlayer.extraQiRegen += 4;
			qiPlayer.qiCurrent -= qiCost;
			//player.GetDamage<DamageClass.Melee>()+=0.01f
			Text.ShowTextRed(player, $"使用四味酒虫，真元恢复永久提升4点。");
			return true;



		}


		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<Consumables.WineBug>(), 1)
			.AddIngredient(ItemID.Lemonade, 1)
			.AddIngredient(ItemID.BottledHoney, 1)
			.AddIngredient(ItemID.LovePotion, 1)
			.AddIngredient(ItemID.Ale, 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 50)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}
	}
}
