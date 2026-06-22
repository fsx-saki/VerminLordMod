using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 火龙蛊双绞线弹幕 — 两发弹幕交错螺旋前进，形成双绞火焰索
	/// ai[0] = 轨道相位（持续递增，两发初始差 π）
	/// ai[1] = 飞行方向角度（弧度）
	/// 使用 FireBaseProj 同款 LiquidTrail 拖尾，可穿墙，双绞线更细
	/// </summary>
	public class HuoLongTwistedProj : BaseBullet
	{
		private const float FlySpeed = 10f;
		private const float OrbitalRadius = 5f;   // 更细：14→5（约1/3）
		private const float OrbitalSpeed = 0.15f;

		protected override void RegisterBehaviors()
		{
			// FireBaseProj 同款液态火焰拖尾（黄→红渐变）
			Behaviors.Add(new LiquidTrailBehavior
			{
				MaxFragments = 50,
				FragmentLife = 15,
				SizeMultiplier = 0.6f,
				SpawnInterval = 1,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				Buoyancy = 0.05f,
				AirResistance = 0.96f,
				InertiaFactor = 0.4f,
				SplashFactor = 0.2f,
				SplashAngle = 0.5f,
				RandomSpread = 0.8f,
				AutoDraw = true,
				SuppressDefaultDraw = true
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.scale = 0.8f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;  // 穿墙
			Projectile.penetrate = 99;
			Projectile.timeLeft = 120;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnAI()
		{
			// 轨道相位递增
			Projectile.ai[0] += OrbitalSpeed;

			float phase = Projectile.ai[0];
			float angle = Projectile.ai[1];

			// 飞行方向向量
			Vector2 forward = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
			Vector2 up = new Vector2(-forward.Y, forward.X);

			// 中心线速度
			Projectile.velocity = forward * FlySpeed;

			// 更细的螺旋轨道偏移
			Vector2 orbitOffset = up * (float)System.Math.Cos(phase) * OrbitalRadius
			                    + forward * (float)System.Math.Sin(phase) * OrbitalRadius * 0.3f;

			Projectile.position += orbitOffset;
			Projectile.rotation = angle;

			// 少量额外火星点缀
			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.OrangeTorch, -forward * 0.5f);
				d.noGravity = true;
				d.scale = 0.7f;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false; // 不会撞墙
	}
}
