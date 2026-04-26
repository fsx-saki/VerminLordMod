using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class YanTongGu : FireWeapon
	{
		public static Color OverrideColor = new(122, 173, 255);

		protected override int qiCost => 100;
		protected override int _useTime => 27;
		protected override int _guLevel => 4;

		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.CloneDefaults(ItemID.LastPrism);
			Item.mana = 0;
			Item.damage = 10;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.shoot = ModContent.ProjectileType<YanTongHoldout>();
			Item.shootSpeed = 30f;
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
					//Main.NewText("您已经完全炼化该蛊虫");
					return true;
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
				Text.ShowTextRed(player,"您正在强行调动高转蛊虫！！！");
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiPlayer.qiLevel) * Main.LocalPlayer.statLifeMax2 / 10, 0);
			}
			return qiPlayer.qiCurrent >= qiCost&& player.ownedProjectileCounts[ModContent.ProjectileType<YanTongHoldout>()] <= 0;
			;
		}

	}
}
