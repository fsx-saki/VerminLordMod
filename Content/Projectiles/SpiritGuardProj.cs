using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 灵盾蛊弹幕 — 一转金道
	/// 特性：大体积+爆炸范围伤害(ExplosionKill) + 金系拖尾
	/// 弹道：慢速直线(AimBehavior)，体积较大(1.2倍缩放)
	/// 视觉：金道拖尾（金碎片+火花+光环+金属尘埃）
	/// 命中效果：60像素半径爆炸，造成50%溅射伤害
	/// 金道特性：坚不可摧，以质量换取伤害
	/// </summary>
	public class SpiritGuardProj : BaseBullet
	{
		private const float FlySpeed = 7f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.8f, 0.6f, 0.2f),
			});

			Behaviors.Add(new GoldTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(255, 215, 80, 180),
				ShardColor = new Color(255, 220, 100, 220),
				SparkColor = new Color(255, 240, 160, 240),
				ShardStretch = 2.5f,
			});

			Behaviors.Add(new ExplosionKillBehavior
			{
				ExplosionRadius = 60f,
				ExplosionDamage = 0.5f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.scale = 1.2f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 200;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 12; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(2f, 5f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0,
					new Color(255, 220, 80, 200), Main.rand.NextFloat(0.5f, 1.0f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}