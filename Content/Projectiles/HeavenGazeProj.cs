using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 天眼蛊弹幕 — 二转天道蛊虫
	/// 特性：极高速穿透 + 命中后方散射 + 高暴击
	/// 弹道：极高速直线飞行(AimBehavior)，穿透多个敌人
	/// 视觉：天道拖尾（光束+祥云+天光）
	/// 命中效果：前方扇形散射小型光束
	/// 天道特性：天威难测，迅捷凌厉的审判之光
	/// </summary>
	public class HeavenGazeProj : BaseBullet
	{
		private const float FlySpeed = 16f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.8f, 0.9f, 1.0f),
			});

			Behaviors.Add(new SkyTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(200, 220, 255, 180),
				BeamColor = new Color(220, 240, 255, 240),
				CloudColor = new Color(180, 200, 240, 160),
				SkyLightColor = new Color(240, 250, 255, 200),
			});

			Behaviors.Add(new SplashBehavior(SplashMode.Forward)
			{
				Count = 5,
				SpeedMin = 6f,
				SpeedMax = 12f,
				SpreadRadius = 2f,
				ConeAngle = 0.25f,
				SpawnExtraDust = true,
				ExtraDustCount = 8,
				DustType = DustID.YellowStarDust,
				DustColorStart = new Color(220, 240, 255, 200),
				DustColorEnd = new Color(180, 200, 255, 0),
				DustScaleMin = 0.3f,
				DustScaleMax = 0.6f,
				DustNoGravity = true,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 100;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust, vel, 0,
					new Color(220, 240, 255, 180), Main.rand.NextFloat(0.4f, 0.7f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}