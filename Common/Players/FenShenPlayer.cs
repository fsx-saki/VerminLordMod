using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Common.Players
{
	/// <summary>
	/// 焚身Player — 焚身Buff激活时，每次攻击额外发射追踪火弹
	/// - 追踪火弹本身不会再次触发追踪（防递归）
	/// - 内置 30 帧冷却
	/// - Buff 结束时清除火焰视觉弹幕
	/// </summary>
	public class FenShenPlayer : ModPlayer
	{
		public bool HasFenShen { get; set; }
		private bool _prevHasFenShen;

		private const int HomingCooldownMax = 30;
		private int _homingCooldown;

		public override void ResetEffects()
		{
			_prevHasFenShen = HasFenShen;
			HasFenShen = false;
		}

		public override void PostUpdate()
		{
			if (_homingCooldown > 0)
				_homingCooldown--;

			// 检测 Buff 结束（右击取消或自然结束），清除火焰弹幕
			if (_prevHasFenShen && !HasFenShen)
			{
				KillFlameProjs();
			}
		}

		private void KillFlameProjs()
		{
			int type = ModContent.ProjectileType<BurningEffectProj>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				// 只清除附着玩家的燃烧效果（ai[2]==1），不碰怪物身上的
				if (p.active && p.owner == Player.whoAmI && p.type == type && p.ai[2] == 1)
				{
					p.active = false;
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HasFenShen)
				SpawnHomingProj(target);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!HasFenShen)
				return;

			if (proj.type == ModContent.ProjectileType<FenShenHomingProj>())
				return;

			SpawnHomingProj(target);
		}

		private void SpawnHomingProj(NPC target)
		{
			if (target == null || !target.active)
				return;

			if (_homingCooldown > 0)
				return;
			_homingCooldown = HomingCooldownMax;

			Projectile.NewProjectile(
				Player.GetSource_OnHit(target),
				Player.Center,
				(target.Center - Player.Center).SafeNormalize(Vector2.Zero) * 10f,
				ModContent.ProjectileType<FenShenHomingProj>(),
				25,
				2f,
				Player.whoAmI
			);
		}
	}
}
