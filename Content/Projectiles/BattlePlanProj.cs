using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 战策蛊弹幕 — 一转战术道
	/// 特性：前方分裂弹(4枚) + 灼烧Debuff + 金系拖尾
	/// 弹道：中速直线(AimBehavior)，命中后向前方扇形散射4枚子母弹
	/// 视觉：金系拖尾（金碎片+火花+光环）
	/// 命中效果：附加着火(OnFire) + 前方分裂(SplashMode.Forward)
	/// 战术道特性：精准计算，一击多效
	/// </summary>
	public class BattlePlanProj : BaseBullet
	{
		private const float FlySpeed = 12f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.5f, 0.5f, 0.1f),
			});

			Behaviors.Add(new GoldTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(180, 170, 60, 160),
				ShardColor = new Color(200, 190, 80, 200),
				SparkColor = new Color(220, 210, 100, 220),
			});

			Behaviors.Add(new SplashBehavior(SplashMode.Forward)
			{
				Count = 4,
				SpeedMin = 4f,
				SpeedMax = 8f,
				SpreadRadius = 2f,
				ConeAngle = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 150;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 120);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, vel, 0,
					new Color(200, 190, 80, 160), Main.rand.NextFloat(0.4f, 0.7f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}