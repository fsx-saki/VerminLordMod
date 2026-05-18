using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 天目蛊弹幕 — 二转天道
	/// 特性：慢速追踪(Homing) + 穿透(3) + 灵道拖尾
	/// 弹道：慢速追踪(HomingBehavior)，穿透3个敌人
	/// 视觉：灵道拖尾（残像+火焰+锁链+光灵）
	/// 天道特性：洞察一切，以天眼之力锁定敌人要害
	/// </summary>
	public class HeavenGazeProj : BaseBullet
	{
		private const float FlySpeed = 7f;
		private const float TrackWeight = 1f / 30f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
			{
				Range = 700f,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.3f, 0.5f, 0.8f),
			});

			Behaviors.Add(new SoulTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(120, 180, 255, 160),
				FlameColor = new Color(140, 200, 255, 230),
				ChainColor = new Color(100, 160, 240, 200),
				WispColor = new Color(180, 220, 255, 240),
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(1f, 3f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, vel, 0,
					new Color(120, 180, 255, 180), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}