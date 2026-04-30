using Microsoft.Xna.Framework;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using VerminLordMod.Common.Configs;
using VerminLordMod.Content;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Debuggers;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Content.Tiles.Furniture;

namespace VerminLordMod.Common.Players
{
	public enum ZiZhi
	{
		GUA=10,
		RO = 0,
		RJIA = 8,
		RYI = 6,
		RBING = 4,
		RDING = 2,
	}
	
	public class QiPlayer : ModPlayer
	{
		public const int UNITKONGQIAO = 100;

		// ===== 修为系统 =====
		public int qiLevel = 0;
		public int levelStage = 0;
		public float levelStageUpRate = 0f;
		public ZiZhi PlayerZiZhi;
		public int kongQiaoMax = 0;//空窍大小
		public int qiMax=0;//元海大小
		public float baseQiRegenRate=0;//基本真元恢复速度
		public bool qiEnabled = false;//是否开启空窍

		// ===== 真元系统 =====
		public int qiMax2 = 1;//当前可用元海大小
		public float qiRegenRate;//当前真元回复速度
		public int qiCurrent;//当前真元值
		internal int qiRegenTimer = 0; // 计时器

		// ===== 真元恢复加成（由装备/蛊虫提供） =====
		public int extraQiRegen = 0;

		// ===== 临时状态（每帧重置） =====
		public int ThreeStepGrassCounter = 0;
		public static readonly Color HealQi = new(187, 91, 201); //真元颜色
		int MuMeiAttackDelay = 0;

		// ===== 兼容层：旧字段映射到 GuPerkSystem =====
		// 这些属性保持向后兼容，实际数据存储在 GuPerkSystem 中
		[Obsolete("请使用 GuPerkSystem.whitePigPower")]
		public int whitePigs
		{
			get => Player.GetModPlayer<GuPerkSystem>().whitePigPower;
			set { /* 通过 GuPerkSystem.TryAddWhitePigPower 修改 */ }
		}
		[Obsolete("请使用 GuPerkSystem.blackPigPower")]
		public int blackPigs
		{
			get => Player.GetModPlayer<GuPerkSystem>().blackPigPower;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.jinLiPower / junLiPower")]
		public int otherPigs
		{
			get => Player.GetModPlayer<GuPerkSystem>().jinLiPower + Player.GetModPlayer<GuPerkSystem>().junLiPower;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.extraAges")]
		public int extraAges
		{
			get => Player.GetModPlayer<GuPerkSystem>().extraAges;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.extraSpeed / extraAccel")]
		public float extraV
		{
			get => Player.GetModPlayer<GuPerkSystem>().extraSpeed;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.extraAccel")]
		public float extraAcc
		{
			get => Player.GetModPlayer<GuPerkSystem>().extraAccel;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.hasOneMinion")]
		public bool hasOneMinion
		{
			get => Player.GetModPlayer<GuPerkSystem>().hasOneMinion;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.wineBugLevel")]
		public bool hasWineBug
		{
			get => Player.GetModPlayer<GuPerkSystem>().wineBugLevel >= GuPerkSystem.WineBugLevel.Basic;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.wineBugLevel")]
		public bool hasFourWineBug
		{
			get => Player.GetModPlayer<GuPerkSystem>().wineBugLevel >= GuPerkSystem.WineBugLevel.FourFlavor;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.wineBugLevel")]
		public bool hasSevenWineBug
		{
			get => Player.GetModPlayer<GuPerkSystem>().wineBugLevel >= GuPerkSystem.WineBugLevel.SevenScent;
			set { }
		}
		[Obsolete("请使用 GuPerkSystem.wineBugLevel")]
		public bool hasNineWineBug
		{
			get => Player.GetModPlayer<GuPerkSystem>().wineBugLevel >= GuPerkSystem.WineBugLevel.NineEye;
			set { }
		}

		public override void Initialize() {
			qiRegenRate = baseQiRegenRate;
			qiMax2 = qiMax;
		}

		public override void ResetEffects() {
			ResetVariables();
		}

		public override void UpdateDead() {
			ResetVariables();
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			qiCurrent /= 2;
			base.Kill(damage, hitDirection, pvp, damageSource);
		}

		// 每帧重置的临时变量
		private void ResetVariables() {
			qiRegenRate = baseQiRegenRate;
			qiMax2 = qiMax;
		}

		public override void PostUpdateMiscEffects() {
			UpdateResource();
		}

		public override void PostUpdate() {
			//CapResourceGodMode();
		}

		// 真元恢复 + 酒虫效果 + 破境处理
		private void UpdateResource() {
			// 每帧的真元恢复处理
			qiRegenTimer++;
			if (qiRegenRate == 0 || !qiEnabled) {
				qiRegenTimer = 0;
				return;
			}

			// 基础真元恢复
			int regenAmount = 0;
			if (qiRegenRate <= 60 && qiRegenTimer > 60 / qiRegenRate) {
				regenAmount = 1;
				qiRegenTimer = 0;
			} else if (qiRegenRate > 60) {
				regenAmount = (int)qiRegenRate / 60;
				qiRegenTimer = 0;
			}

			// 酒虫精炼真元效果
			var guPerk = Player.GetModPlayer<GuPerkSystem>();
			regenAmount += guPerk.GetWineBugRegenBonus();

			// 黄罗天牛Buff额外真元恢复
			if (Player.HasBuff(ModContent.BuffType<HuangLuoLongicornbuff>())) {
				if (Randommer.Roll(2)) {
					regenAmount += 1;
				}
			}

			qiCurrent += regenAmount;
			qiCurrent = Utils.Clamp(qiCurrent, 0, qiMax2);

			// 每帧的破境处理（已迁移到 QiRealmPlayer）
			if (levelStageUpRate <= 100) {
				levelStageUpRate -= 0.01f;
			}
			else {
				levelStageUpRate = 0;
				var qiRealm = Player.GetModPlayer<QiRealmPlayer>();
				qiRealm.StageUp();
			}

			levelStageUpRate=Utils.Clamp(levelStageUpRate, 0, 100);

			// 木魅蛊攻击冷却处理
			if (Player.HasBuff(ModContent.BuffType<MuMeibuff>())) {
				if (MuMeiAttackDelay != 0) {
					MuMeiAttackDelay--;
					MuMeiAttackDelay=Utils.Clamp(MuMeiAttackDelay, 0, 100);
				}
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (item.damage > 0 && (item.DamageType == DamageClass.Melee||item.DamageType == DamageClass.MeleeNoSpeed)) {
				var guPerk = Player.GetModPlayer<GuPerkSystem>();
				bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
				damage += guPerk.GetPowerDamageBonus(qiLevel, limited);
			}

			// 炎心蛊加强火系
			if(item.ModItem as FireWeapon != null && Player.HasBuff(ModContent.BuffType<FireHeartbuff>())) {
				item.damage = (int)(item.OriginalDamage * 1.2f);
			}else
			// 背水一战蛊加强水系
			if(item.ModItem as WaterWeapon != null && Player.HasBuff(ModContent.BuffType<ReWaterbuff>())) {
				item.damage = (int)(item.OriginalDamage * 3f);
			}else item.damage = item.OriginalDamage;
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			base.ModifyMaxStats(out health, out mana);
			var guPerk = Player.GetModPlayer<GuPerkSystem>();
			bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
			health.Flat += guPerk.GetAgeHealthBonus(qiLevel, limited);
		}

		public override void PostUpdateRunSpeeds() {
			var guPerk = Player.GetModPlayer<GuPerkSystem>();
			Player.maxRunSpeed += guPerk.extraSpeed;
			Player.runAcceleration += guPerk.extraAccel;
		}

		public override void PostUpdateEquips() {
			var guPerk = Player.GetModPlayer<GuPerkSystem>();
			if (guPerk.hasOneMinion)
				Player.maxMinions += 1;
			if (Player.HasBuff(ModContent.BuffType<LittleSoulbuff>())) {
				Player.maxMinions += 1;
			}
			if (Player.HasBuff(ModContent.BuffType<LittleSoulbuff>())) {
				Player.maxMinions += 2;
			}
			if (Player.HasBuff(ModContent.BuffType<GiantSpiritHeartbuff>())) {
				Player.maxMinions *= 3;
			}
		}

		public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) {
			if (item.damage > 0 && item.DamageType == DamageClass.Melee) {
				var guPerk = Player.GetModPlayer<GuPerkSystem>();
				bool limited = ModContent.GetInstance<VerminLordModConfig>().LimitSth;
				knockback += guPerk.GetPowerDamageBonus(qiLevel, limited);
			}
		}

		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
			if (Player.HasBuff(ModContent.BuffType<FireClothesbuff>())) 
				npc.AddBuff(BuffID.OnFire, 300);
			if (Player.HasBuff(ModContent.BuffType<WaterShellbuff>())) {
				npc.AddBuff(ModContent.BuffType<Waterbuff>(), 300);
			}if (Player.HasBuff(ModContent.BuffType<YingShangbuff>())) {
				for (int i = 0; i < 100; i++) {
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame).velocity *= 2;
				}
				npc.life -= hurtInfo.Damage / 2;
				CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, 16, 16), Color.Red, hurtInfo.Damage / 10);
				npc.position += npc.position - Player.position;
			}
		}

		public override bool CanUseItem(Item item) {
			if (Player.HasBuff(ModContent.BuffType<MuMeibuff>())&&!(item.ModItem is WoodWeapon)) {
				if (MuMeiAttackDelay != 0) {
					return false;
				}
				Vector2 v = Main.MouseWorld-Player.Center;

				if (v.Length() < 280) {
					v.Normalize();
					Projectile.NewProjectile(null, Player.Center, v * 2f, ModContent.ProjectileType<MuMeiAttackN>(), 75, 3);
					MuMeiAttackDelay = 25;
				}
				else {
					float r = (float)Math.Atan2(v.Y, v.X);
					for (int i = -1; i <= 1; i++) {
						float r2 = r + i * MathHelper.Pi / 8f;
						Vector2 shootVel = r2.ToRotationVector2() * 10;
						Projectile.NewProjectile(null, Player.Center, shootVel, ModContent.ProjectileType<PineNeedleProj>(), 40, 3);
						MuMeiAttackDelay = 40;
					}
				}
				return false;
			}
			else {
				// 六转以上使用低阶蛊虫返还真元（已迁移到新系统引用）
				if (item.ModItem is GuWeaponItem && Player.altFunctionUse != 2)
				{
					var qiRealm = Player.GetModPlayer<QiRealmPlayer>();
					if (qiRealm.GuLevel >= 6)
					{
						var gu = item.ModItem as GuWeaponItem;
						if (gu.GetGuLevel() < 6)
						{
							var qiResource = Player.GetModPlayer<QiResourcePlayer>();
							qiResource.RefundQi(gu.GetQiCost());
						}
					}
				}
				return base.CanUseItem(item);
			}
		}

		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			return new[]
			{
				new Item(ModContent.ItemType<Hopeness>(), 1),
				new Item(ModContent.ItemType<Info>(), 1),
			};
		}

		public override void SaveData(TagCompound tag) {
			tag["qiEnabled"] = qiEnabled;
			tag["qiMax"] = qiMax;
			tag["qiCurrent"] = qiCurrent;
			tag["baseQiRegenRate"] = baseQiRegenRate;
			tag["qiLevel"] = qiLevel;
			tag["levelStage"] = levelStage;
			tag["kongQiaoMax"] = kongQiaoMax;
			tag["ZiZhi"] = (int)PlayerZiZhi;
		}

		public override void LoadData(TagCompound tag) {
			qiEnabled = tag.GetBool("qiEnabled");
			qiCurrent = tag.GetInt("qiCurrent");
			qiMax = tag.GetInt("qiMax");
			baseQiRegenRate = tag.GetFloat("baseQiRegenRate");
			qiLevel = tag.GetInt("qiLevel");
			levelStage = tag.GetInt("levelStage");
			kongQiaoMax = tag.GetInt("kongQiaoMax");
			PlayerZiZhi = (ZiZhi)tag.GetInt("ZiZhi");
		}

		public static void resetAll(QiPlayer qiPlayer) {
			qiPlayer.PlayerZiZhi = 0;
			qiPlayer.qiEnabled = false;
			qiPlayer.qiCurrent = 0;
			qiPlayer.qiMax = 0;
			qiPlayer.baseQiRegenRate = 0;
			qiPlayer.qiLevel = 0;
			qiPlayer.levelStage = 0;
			qiPlayer.levelStageUpRate = 0;
			qiPlayer.kongQiaoMax = qiPlayer.qiLevel * QiPlayer.UNITKONGQIAO;

			// 同时重置 GuPerkSystem
			qiPlayer.Player.GetModPlayer<GuPerkSystem>().ResetAll();
		}

		public static void LevelUp(QiPlayer qiPlayer) {
			qiPlayer.qiLevel += 1;
			qiPlayer.levelStage = 0;
			QiPlayer.SetQis(qiPlayer);
		}

		public static void StageUp(QiPlayer qiPlayer) {
			if(qiPlayer.levelStage>=3)return;
			qiPlayer.levelStage++;
			string str = "";
			switch (qiPlayer.levelStage) {
				case 0:
					str = "初期";
					break;
				case 1:
					str = "中期";
					break;
				case 2:
					str = "后期";
					break;
				case 3:
					str = "巅峰";
					break;
			}

			Text.ShowTextGreen(qiPlayer.Player,$"破境成功！当前境界为{qiPlayer.qiLevel}转{str}");
			SetQis(qiPlayer);
			qiPlayer.qiCurrent = qiPlayer.qiMax;
		}

		public static void SetQis(QiPlayer qiPlayer) {
			int times=(int)Math.Pow(10,qiPlayer.qiLevel-1)*(int)Math.Pow(2,qiPlayer.levelStage);
			qiPlayer.baseQiRegenRate = qiPlayer.qiLevel * (int)qiPlayer.PlayerZiZhi / 2;
			qiPlayer.kongQiaoMax = times * QiPlayer.UNITKONGQIAO;
			qiPlayer.qiMax = qiPlayer.kongQiaoMax * (int)qiPlayer.PlayerZiZhi / 10;
		}

		public static void showInfo(QiPlayer qiPlayer) {
			var guPerk = qiPlayer.Player.GetModPlayer<GuPerkSystem>();
			Main.NewText($"-----蛊师信息-----\n" +
				$"空窍：{qiPlayer.qiEnabled}#  #" +
				$"空窍大小：{qiPlayer.kongQiaoMax}#  #" +
				$"转数：{qiPlayer.qiLevel}#  #" +
				$"阶段：{qiPlayer.levelStage}#  #" +
				$"元海大小：{qiPlayer.qiMax}#  #" +
				$"基本真元恢复速率：{qiPlayer.baseQiRegenRate} \n" +
				$"当前真元：{qiPlayer.qiCurrent}#  #" +
				$"当前最大真元：{qiPlayer.qiMax2}#  #" +
				$"当前真元恢复速率：{qiPlayer.qiRegenRate} \n" +

				$"白豕蛊之力：{guPerk.whitePigPower}/{GuPerkSystem.MAX_WHITE_PIG_POWER}#  #" +
				$"黑豕蛊之力：{guPerk.blackPigPower}/{GuPerkSystem.MAX_BLACK_PIG_POWER}#  #" +
				$"斤力蛊之力：{guPerk.jinLiPower}#  #" +
				$"钧力蛊之力：{guPerk.junLiPower}#  #" +
				$"额外寿命：{guPerk.extraAges}\n" +
				$"一胎蛊：{guPerk.hasOneMinion}#  #" +
				$"酒虫等级：{guPerk.wineBugLevel}\n" +
				$"------------------");
		}
	}
}
