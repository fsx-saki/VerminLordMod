using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 疾风蛊弹幕 — 一转飞道
	/// 特性：极高速度(14) + 波浪轨迹 + 风系拖尾
	/// 弹道：高速直线(AimBehavior)配合正弦波(WaveBehavior)左右摆动
	/// 视觉：风系拖尾（风纹+涡流+薄雾）
	/// 飞道特性：轻快灵动，以速度和不可预测的轨迹迷惑敌人
	/// </summary>
	public class SwiftWindProj : BaseBullet
	{
		private const float FlySpeed = 14f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.2f, 0.6f, 0.4f),
			});

			Behaviors.Add(new WindTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(80, 200, 120, 140),
				StreakColor = new Color(100, 220, 140, 200),
				VortexColor = new Color(120, 240, 160, 180),
				MistColor = new Color(80, 200, 140, 100),
			});

			Behaviors.Add(new WaveBehavior(amplitude: 0.03f, frequency: 0.15f));
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 120;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 5; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
					DustID.Cloud, vel, 0, new Color(100, 220, 140, 180),
					Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, vel, 0,
					new Color(80, 200, 120, 160), Main.rand.NextFloat(0.4f, 0.7f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}