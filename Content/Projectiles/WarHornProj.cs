using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 战角蛊弹幕 — 二转战道蛊虫
	/// 特性：大体积+低速度+高伤害+命中后范围爆炸
	/// 弹道：慢速直线飞行(AimBehavior)，体积较大
	/// 视觉：战道拖尾（旌旗+号角+兵锋）
	/// 命中效果：大范围爆炸 + 破甲 + 强力击退
	/// 战道特性：势大力沉，以磅礴之力碾碎敌人
	/// </summary>
	public class WarHornProj : BaseBullet
	{
		private const float FlySpeed = 5f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.7f, 0.3f, 0.1f),
			});

			Behaviors.Add(new WarTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(200, 80, 30, 180),
			});

			Behaviors.Add(new ExplosionKillBehavior
			{
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.scale = 1.5f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.knockBack = 10f;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.BrokenArmor, 300);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 16; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(2f, 6f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0,
					new Color(240, 100, 30, 220), Main.rand.NextFloat(0.5f, 1.2f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}