using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 虚裂蛊弹幕 — 二转虚道蛊虫
	/// 特性：穿透物块 + 命中附加击退 + 大范围爆炸
	/// 弹道：低速直线飞行(AimBehavior)，无视物块
	/// 视觉：虚道拖尾（折叠线+曲点+镜碎）
	/// 命中效果：径向虚爆 + 强力击退
	/// 虚道特性：虚无穿行，命中时撕裂空间造成范围伤害
	/// </summary>
	public class VoidRipProj : BaseBullet
	{
		private const float FlySpeed = 6f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.4f, 0.2f, 0.6f),
			});

			Behaviors.Add(new VoidTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(160, 120, 220, 160),
			});

			Behaviors.Add(new SplashBehavior(SplashMode.Radial)
			{
				Count = 10,
				SpeedMin = 1f,
				SpeedMax = 4f,
				SpreadRadius = 6f,
				SpawnExtraDust = true,
				ExtraDustCount = 12,
				DustType = DustID.PurpleTorch,
				DustColorStart = new Color(160, 120, 220, 200),
				DustColorEnd = new Color(60, 40, 120, 0),
				DustScaleMin = 0.3f,
				DustScaleMax = 0.8f,
				DustNoGravity = true,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.knockBack = 8f;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 12; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 4f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
					new Color(160, 120, 220, 200), Main.rand.NextFloat(0.4f, 0.9f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}