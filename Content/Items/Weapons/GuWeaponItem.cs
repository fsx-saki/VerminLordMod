using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons
{
	/// <summary>
	/// 需要重写的字段：
	/// <para>使用时消耗的真元qiCost = 10</para>
	/// <para>炼化时消耗的真元controlQiCos = 10</para>
	/// <para>一次炼化增加的炼化进度unitConntrolRate = 10</para>
	/// <para>蛊虫脱离炼化的速度uncontrolRate = 0.01f</para>
	/// <para>左键使用速度_useTime = 20</para>
	/// <para>左键使用动画_useStyle = ItemUseStyleID.Guitar</para>
	/// <para>转数_guLevel = 1</para>
	/// </summary>
	public class GuWeaponItem :ModItem, IWeaponCanReforge,IGu
	{
		/// <summary>
		/// 是否需要炼化
		/// </summary>
		protected virtual bool needCtrl => true;

		/// <summary>
		/// 使用时消耗的真元
		/// </summary>
		protected virtual int qiCost => 7;
		/// <summary>
		/// 炼化时消耗的真元
		/// </summary>
		protected virtual int controlQiCost => 10;
		/// <summary>
		/// 一次炼化增加的炼化进度
		/// </summary>
		protected virtual float unitConntrolRate => 10;
		/// <summary>
		/// 是否已经被炼化
		/// </summary>
		public bool hasBeenControlled=false;
		/// <summary>
		/// 当前炼化进度
		/// </summary>
		public float controlRate=0f;
		/// <summary>
		/// 蛊虫脱离炼化的速度
		/// </summary>
		protected virtual float uncontrolRate => 0.01f;
		/// <summary>
		/// 左键使用速度
		/// </summary>
		protected virtual int _useTime => 20;
		/// <summary>
		/// 左键使用动画
		/// </summary>
		protected virtual int _useStyle => ItemUseStyleID.Guitar;
		/// <summary>
		/// 转数
		/// </summary>
		protected virtual int _guLevel => 1;
		public int GetGuLevel() {
			return _guLevel;
		}
		public int GetQiCost() {
			return qiCost;
		}
		public override bool? UseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			qiPlayer.qiCurrent -= qiCost;

			return true;
		}
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				if (needCtrl) tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				if(needCtrl) tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
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
					//Text.ShowTextRed(player, "您已经完全炼化该蛊虫");
					return true;
				}
				if (qiPlayer.qiCurrent < controlQiCost) {
					Text.ShowTextRed(player, "炼化失败 真元不足");	

					return false;
				}
				qiPlayer.qiCurrent -= controlQiCost;
				controlRate += unitConntrolRate;
				Text.ShowTextGreen(player, $"炼化中......当前进度{controlRate}%");
				return false;
			}
			else {
				Item.useTime = _useTime;
				Item.useAnimation = _useTime;
				Item.useStyle = _useStyle;
			}
			if (!hasBeenControlled&&needCtrl) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");

				return false;
			}
			if (_guLevel > qiPlayer.qiLevel) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel-qiPlayer.qiLevel)*Main.LocalPlayer.statLifeMax2 / 10, 0);
			}
			return qiPlayer.qiCurrent >= qiCost;
		}
		public override void UpdateInventory(Player player) {
			if (player.HeldItem.type != Item.type) {
				hasShownGhost = false; // 切换到其他武器时重置标志
			}
			if (controlRate == 100) {
				hasBeenControlled = true;
			}
			if (hasBeenControlled)
				return;
			controlRate -= uncontrolRate;
			controlRate = Utils.Clamp(controlRate, 0, 100);
		}
		public override bool AltFunctionUse(Player player) {
			
			return true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			return true;
		}
		protected bool hasShownGhost = false;
		/// <summary>
		/// 虚影类型
		/// </summary>
		protected virtual int moddustType => -1;
		public override void HoldItem(Player player) {
			if (!hasShownGhost && moddustType != -1 && player.inventory[58].type!=Item.type) {
				// 生成虚影
				Vector2 position = player.Center + new Vector2(0, -player.height / 2); // 在玩家头上生成
				Dust dust = Dust.NewDustPerfect(position, moddustType);
				dust.velocity = new Vector2(0, -2); // 上浮
				dust.noGravity = true;
				dust.fadeIn = 0.5f;
				dust.scale = 0.5f; // 调整大小
				dust.position += new Vector2(-dust.frame.Width / 2 * dust.scale, -dust.frame.Height / 2 * dust.scale);

				hasShownGhost = true;
			}
		}
		public override void SaveData(TagCompound tag) {
			tag["controlRate"] = controlRate;
			tag["hasBeenControlled"] = hasBeenControlled;
		}
		public override void LoadData(TagCompound tag) {
			controlRate = tag.GetFloat("controlRate");
			hasBeenControlled = tag.GetBool("hasBeenControlled");
		}
	}
}
