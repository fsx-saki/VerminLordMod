using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 寻心蛊弹幕 — 一转情道
	/// 特性：追踪弹(Homing) + 魅惑Debuff(Lovestruck) + 心形拖尾
	/// 弹道：慢速追踪(HomingBehavior)，追踪范围600像素
	/// 视觉：情道拖尾（丝线+心瓣+红晕辉光）
	/// 命中效果：附加 Lovestruck 魅惑2秒
	/// 情道特性：温柔而致命，以情感之力俘获敌人心智
	/// </summary>
	public class HeartSeekerProj : BaseBullet
	{
		private const float FlySpeed = 8f;
		private const float TrackWeight = 1f / 25f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
			{
				Range = 600f,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
			});

			Behaviors.Add(new LoveTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(255, 140, 180, 160),
				SilkThreadColor = new Color(255, 160, 200, 220),
				HeartPetalColor = new Color(255, 120, 160, 200),
				BlushGlowColor = new Color(255, 180, 210, 180),
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				BuffType = BuffID.Lovestruck,
				BuffDuration = 120,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(0.5f, 2f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, vel, 0,
					new Color(255, 150, 190, 180), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}