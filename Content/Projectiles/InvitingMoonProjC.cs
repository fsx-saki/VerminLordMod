using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 邀月 C — 追击体
	/// 有怪追怪，无怪追鼠标（WindStorm 风格平滑转向）。
	/// </summary>
	public class InvitingMoonProjC : BaseBullet
	{
		private const float Accel = 0.35f;
		private const float MaxSpd = 14f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(0.6f, 0.3f, 0.8f) });
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(205, 135, 255),
				GlowLayers = 3,
				GlowBaseScale = 1.5f,
				GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1.2f; Projectile.ignoreWater = true;
			Projectile.tileCollide = false; Projectile.penetrate = 3;
			Projectile.timeLeft = 300; Projectile.alpha = 0;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTailP").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(205, 135, 255),
					maxPositions: 16, widthScale: 2.0f, lengthScale: 1.5f, alpha: 0.9f, recordInterval: 2);
			}
		}

		protected override void OnAI()
		{
			// 有怪追怪，无怪追鼠标
			Vector2 targetPos = Main.MouseWorld;
			NPC target = Finder.FindCloestEnemy(Projectile.Center, 8000f, n =>
				n.CanBeChasedBy() && !n.dontTakeDamage &&
				Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1));
			if (target != null)
				targetPos = target.Center;

			// == 风卷残云式约束转向 ==
			Vector2 toTarget = targetPos - Projectile.Center;
			float dist = toTarget.Length();
			Vector2 desiredDir = dist > 1f ? toTarget / dist : Vector2.UnitX;

			// 当前飞行方向
			Vector2 currentDir = Projectile.velocity.Length() > 0.1f
				? Vector2.Normalize(Projectile.velocity)
				: desiredDir;

			// 计算转向角，限制每帧最大转角（ConeHalfAngle ≈ 36°）
			float maxTurnAngle = MathHelper.Pi / 5f;
			float angleBetween = (float)Math.Acos(Math.Clamp(
				Vector2.Dot(currentDir, desiredDir), -1f, 1f));
			float turnAngle = Math.Min(angleBetween, maxTurnAngle);

			// 朝目标方向旋转
			float cross = currentDir.X * desiredDir.Y - currentDir.Y * desiredDir.X;
			float turnDir = Math.Sign(cross);
			Vector2 newDir = currentDir.RotatedBy(turnAngle * turnDir);

			// 加速到最大速度
			float currentSpeed = Projectile.velocity.Length();
			float targetSpeed = Math.Min(currentSpeed + Accel * 2f, MaxSpd);
			Projectile.velocity = newDir * targetSpeed;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnBurst(Projectile.Center, new Color(205, 135, 255));
		}
	}
}
