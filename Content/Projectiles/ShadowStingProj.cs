using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 影刺蛊弹幕 — 一转影道
	/// 特性：穿透(2) + 暗影拖尾 + 命中标记敌人
	/// 弹道：中速直线(AimBehavior)，穿透2个敌人
	/// 视觉：影系拖尾（暗影残像+触须+暗影池+克隆体）
	/// 影道特性：暗杀型，穿透敌人并留下暗影标记
	/// </summary>
	public class ShadowStingProj : BaseBullet
	{
		private const float FlySpeed = 11f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.15f, 0.05f, 0.25f),
			});

			Behaviors.Add(new ShadowTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(80, 20, 120, 160),
				CloneColor = new Color(100, 30, 140, 200),
				CloneSpawnChance = 0.03f,
				CloneLife = 30,
			});

			Behaviors.Add(new DebuffOnHitBehavior(BuffID.ShadowFlame, 120));
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 180;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 0,
					new Color(90, 20, 130, 180), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}