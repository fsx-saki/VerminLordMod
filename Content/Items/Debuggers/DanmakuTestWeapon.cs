using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI.DanmakuUI;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Items.Debuggers
{
	/// <summary>
	/// 弹幕测试武器 - 开发调试工具
	/// <para>左键：发射当前选中的弹幕投射物</para>
	/// <para>右键：打开UI选择模组中的所有弹幕投射物</para>
	/// </summary>
	public class DanmakuTestWeapon : ModItem
	{
		/// <summary>
		/// 当前选中的弹幕类型ID
		/// </summary>
		public static int SelectedProjectileType = -1;

		/// <summary>
		/// 当前选中的弹幕名称（用于显示）
		/// </summary>
		public static string SelectedProjectileName = "未选择";

		/// <summary>
		/// 发射速度倍率
		/// </summary>
		public float shootSpeedMultiplier = 1f;

		/// <summary>
		/// 伤害倍率
		/// </summary>
		public int damageMultiplier = 1;

		/// <summary>
		/// 弹幕数量（每次发射几个）
		/// </summary>
		public int projectileCount = 1;

		/// <summary>
		/// 散布角度（度）
		/// </summary>
		public float spreadAngle = 0f;

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Master;
			Item.maxStack = 1;
			Item.value = 100;

			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;

			Item.damage = 1;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 1f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shootSpeed = 10f;

			// 必须设置 shoot > 0 才能触发 Shoot() 回调
			Item.shoot = ProjectileID.WoodenArrowFriendly;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				// 右键：打开弹幕选择UI
				ModContent.GetInstance<DanmakuSelectionUISystem>().ToggleUI();
				return false;
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
				return false;

			// 如果没有选择弹幕，提示并返回
			if (SelectedProjectileType <= 0)
			{
				Main.NewText("[弹幕测试武器] 请右键打开UI选择要测试的弹幕！", Color.Yellow);
				return false;
			}

			// 检查选中的弹幕类型是否有效
			if (SelectedProjectileType >= ProjectileLoader.ProjectileCount)
			{
				Main.NewText("[弹幕测试武器] 选中的弹幕类型已失效，请重新选择！", Color.Red);
				SelectedProjectileType = -1;
				SelectedProjectileName = "未选择";
				return false;
			}

			// 计算朝向鼠标的方向
			Vector2 toMouse = Main.MouseWorld - player.Center;
			float baseAngle = (float)Math.Atan2(toMouse.Y, toMouse.X);

			// 发射多个弹幕（带散布）
			for (int i = 0; i < projectileCount; i++)
			{
				// 计算散布角度
				float spread = 0f;
				if (projectileCount > 1)
				{
					// 均匀散布在 spreadAngle 范围内
					spread = (i - (projectileCount - 1) / 2f) * (spreadAngle * MathHelper.Pi / 180f) / Math.Max(projectileCount - 1, 1);
				}

				float angle = baseAngle + spread + Main.rand.NextFloat(-0.02f, 0.02f);
				float speed = Item.shootSpeed * shootSpeedMultiplier * Main.rand.NextFloat(0.95f, 1.05f);
				Vector2 shootVel = angle.ToRotationVector2() * speed;

				// 发射选中的弹幕
				int proj = Projectile.NewProjectile(
					source,
					position,
					shootVel,
					SelectedProjectileType,
					Item.damage * damageMultiplier,
					Item.knockBack,
					player.whoAmI);

				// 如果弹幕有初始速度，可以在这里覆盖
				if (Main.projectile.IndexInRange(proj))
				{
					Main.projectile[proj].velocity = shootVel;
				}
			}

			return false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "DanmakuInfo",
				$"当前弹幕: [c/00ff00:{SelectedProjectileName}]"));
			tooltips.Add(new TooltipLine(Mod, "DanmakuControls",
				"[c/ffff00:左键] 发射弹幕  [c/ffff00:右键] 打开选择UI"));
			tooltips.Add(new TooltipLine(Mod, "DanmakuSettings",
				$"速度倍率: {shootSpeedMultiplier:F1}x  伤害倍率: {damageMultiplier}x  数量: {projectileCount}  散布: {spreadAngle}°"));
		}

		public override void AddRecipes()
		{
			// 无合成配方，通过 Cheat Sheet / Hero's Mod 获取
		}
	}
}
