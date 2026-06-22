using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles.Zero;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Weapons.Zero
{
	/// <summary>
	/// 月道基础蛊 — 月刃蛊
	/// 月道技术储备库的武器端实现。
	/// 聚焦月系特性：月刃、月光爆、月追、月汐、月华斩、月爆。
	/// 手持时按 R 键切换当前攻击模式。
	///
	/// 攻击模式（6种不同的月道效果）：
	///   模式0 - 月刃：月刃直线飞行，拖尾月华
	///   模式1 - 月光：直线飞行，命中后三层爆散
	///   模式2 - 月追：追踪鼠标位置的月弧弹
	///   模式3 - 月汐：波浪轨迹飞行，拖尾潮汐涟漪
	///   模式4 - 月华斩：高速穿透月刃，短距离快斩
	///   模式5 - 月爆：在目标位置召唤月光爆裂
	/// </summary>
	public class MoonBaseGu : MoonWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 8;
		protected override int _useTime => 20;
		protected override int _guLevel => 1;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;

		// ===== 攻击方式切换系统 =====

		/// <summary>当前攻击模式索引</summary>
		public int attackMode = 0;

		/// <summary>所有攻击模式的名称</summary>
		private static readonly string[] AttackModeNames = new[]
		{
			"月刃",         // 模式0：直线月刃
			"月光",         // 模式1：月光爆散
			"月追",         // 模式2：追踪鼠标
			"月汐",         // 模式3：波浪月汐
			"月华斩",       // 模式4：高速穿透
			"月爆",         // 模式5：范围月爆
		};

		[CloneByReference] private int[] _modeProjectileTypes;

		private readonly float[] _modeShootSpeeds = new[]
		{
			12f,  // 月刃
			10f,  // 月光
			8f,   // 月追
			9f,   // 月汐
			20f,  // 月华斩（高速）
			6f,   // 月爆
		};

		private readonly float[] _modeDamageMultipliers = new[]
		{
			1.0f,  // 月刃
			1.2f,  // 月光
			0.9f,  // 月追
			0.8f,  // 月汐
			1.5f,  // 月华斩
			1.8f,  // 月爆
		};

		private readonly int[] _modeUseTimes = new[]
		{
			20,  // 月刃
			22,  // 月光
			18,  // 月追
			26,  // 月汐
			30,  // 月华斩
			35,  // 月爆
		};

		// ===== IOnHitEffectProvider =====
		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.MoonMark };
		public float DoTDuration => 3f;
		public float DoTDamage => 4f;
		public float SlowPercent => 0.3f;
		public int SlowDuration => 120;
		public float ArmorShredAmount => 5f;
		public int ArmorShredDuration => 180;
		public float WeakenPercent => 0.15f;
		public float LifeStealPercent => 0.1f;
		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 16;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 3f;
			Item.crit = 4;
			Item.noMelee = true;

			_modeProjectileTypes = new[]
			{
				ModContent.ProjectileType<MoonBladeProj>(),    // 模式0：月刃
				ModContent.ProjectileType<MoonBurstProj>(),    // 模式1：月光爆散
				ModContent.ProjectileType<MoonChaseProj>(),    // 模式2：月追
				ModContent.ProjectileType<MoonTideProj>(),     // 模式3：月汐
				ModContent.ProjectileType<MoonCutProj>(),      // 模式4：月华斩
				ModContent.ProjectileType<MoonNovaProj>(),     // 模式5：月爆
			};

			Item.shoot = _modeProjectileTypes[0];
			Item.shootSpeed = _modeShootSpeeds[0];
		}

		// ===== 切换攻击方式（R 键）=====

		private bool _lastRKeyState = false;

		public override void UpdateInventory(Player player)
		{
			base.UpdateInventory(player);

			if (player.HeldItem.type == Item.type)
			{
				bool currentRState = Main.keyState.IsKeyDown(Keys.R);
				if (currentRState && !_lastRKeyState)
				{
					SwitchAttackMode(player);
				}
				_lastRKeyState = currentRState;
			}
			else
			{
				_lastRKeyState = false;
			}
		}

		private void SwitchAttackMode(Player player)
		{
			attackMode = (attackMode + 1) % _modeProjectileTypes.Length;

			Item.shoot = _modeProjectileTypes[attackMode];
			Item.shootSpeed = _modeShootSpeeds[attackMode];
			Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);

			string modeName = AttackModeNames[attackMode];
			CombatText.NewText(player.getRect(), new Color(180, 210, 255), $"月道切换：{modeName}");
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);

			string modeName = AttackModeNames[attackMode];
			tooltips.Add(new TooltipLine(Mod, "AttackMode", $"当前月道：[c:99CCFF:{modeName}]"));
			tooltips.Add(new TooltipLine(Mod, "SwitchHint", "手持时按 [c:99CCFF:R] 键切换攻击方式"));
		}

		public override bool CanUseItem(Player player)
		{
			if (attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
			{
				Item.shoot = _modeProjectileTypes[attackMode];
				Item.shootSpeed = _modeShootSpeeds[attackMode];
			}
			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (attackMode == 5)
			{
				// 模式5：月爆 — 在鼠标位置生成爆炸
				Vector2 mousePos = Main.MouseWorld;
				Projectile.NewProjectile(source, mousePos, Vector2.Zero, type, damage, 0f, player.whoAmI);
				return false;
			}

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		// ===== 持久化 =====

		public override void SaveData(TagCompound tag)
		{
			base.SaveData(tag);
			tag["attackMode"] = attackMode;
		}

		public override void LoadData(TagCompound tag)
		{
			base.LoadData(tag);
			attackMode = tag.GetInt("attackMode");

			if (_modeProjectileTypes != null && attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
			{
				Item.shoot = _modeProjectileTypes[attackMode];
				Item.shootSpeed = _modeShootSpeeds[attackMode];
				Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);
			}
		}
	}
}
