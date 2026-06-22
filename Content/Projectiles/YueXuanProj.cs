using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 月旋弹 — 月旋蛊，绿色月刃沿螺旋曲线追踪目标。
	/// </summary>
	public class YueXuanProj : BaseBullet
	{

		private const float Accel = 0.25f;
		private const float MaxSpd = 12f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(0.3f, 0.8f, 0.5f)
			});

			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(100, 220, 160),
				GlowLayers = 3, GlowBaseScale = 1.2f, GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f,
			});

			Behaviors.Add(new DustTrailBehavior
			{
				DustType = DustID.JungleSpore, SpawnChance = 3, DustScale = 1.0f,
				VelocityMultiplier = 0.1f, NoGravity = true, RandomSpeed = 1.5f
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				Buffs = new List<(int, int)> { (BuffID.MoonLeech, 120) }
			});

			Behaviors.Add(new DustOnHitBehavior
			{
				DustType = DustID.JungleSpore, DustCount = 8,
				SpeedMin = 1f, SpeedMax = 3f, ScaleMin = 0.8f, ScaleMax = 1.5f, Color = Color.PaleGreen
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14; Projectile.height = 14;
				Projectile.scale = 0.5f; Projectile.ignoreWater = false;
			Projectile.tileCollide = true; Projectile.penetrate = 1;
			Projectile.timeLeft = 240; Projectile.alpha = 20;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(100, 220, 160, 200),
					maxPositions: 20, widthScale: 1.2f, lengthScale: 1.6f, alpha: 0.7f, recordInterval: 2);
			}
		}

		protected override void OnAI()
		{
			// 有怪追怪，无怪追鼠标（风卷残云式约束转向）
			Vector2 targetPos = Main.MouseWorld;
			NPC target = Finder.FindCloestEnemy(Projectile.Center, 8000f, n =>
				n.CanBeChasedBy() && !n.dontTakeDamage);
			if (target != null)
				targetPos = target.Center;

			Vector2 toTarget = targetPos - Projectile.Center;
			float dist = toTarget.Length();
			Vector2 desiredDir = dist > 1f ? toTarget / dist : Vector2.UnitX;

			Vector2 currentDir = Projectile.velocity.Length() > 0.1f
				? Vector2.Normalize(Projectile.velocity) : desiredDir;

			// 限制每帧最大转角（ConeHalfAngle ≈ 36°）
			float maxTurn = MathHelper.Pi / 5f;
			float angleBetween = (float)Math.Acos(Math.Clamp(
				Vector2.Dot(currentDir, desiredDir), -1f, 1f));
			float turnAngle = Math.Min(angleBetween, maxTurn);
			float turnSign = Math.Sign(currentDir.X * desiredDir.Y - currentDir.Y * desiredDir.X);
			Vector2 newDir = currentDir.RotatedBy(turnAngle * turnSign);

			// 加速到最大速度
			float currentSpeed = Projectile.velocity.Length();
			float targetSpeed = Math.Min(currentSpeed + Accel * 2f, MaxSpd);
				Projectile.velocity = newDir * targetSpeed;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.Center, 0.2f, 0.5f, 0.3f);
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnBurst(Projectile.Center, new Color(100, 220, 160));
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}
