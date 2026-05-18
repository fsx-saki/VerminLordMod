using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 噬梦蛊弹幕 — 二转梦道
	/// 特性：穿透物块(tileCollide=false) + 追踪(Homing) + 困惑Debuff + 梦系拖尾
	/// 弹道：中速追踪(HomingBehavior)，无视物块飞行
	/// 视觉：梦道拖尾（梦幻残像+泡泡+涟漪+蝴蝶）
	/// 命中效果：附加 Confused 困惑Debuff
	/// 梦道特性：幻术型，无视物理障碍，以虚幻之力侵蚀敌人心智
	/// </summary>
	public class DreamBiteProj : BaseBullet
	{
		private const float FlySpeed = 9f;
		private const float TrackWeight = 1f / 20f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
			{
				Range = 500f,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.25f, 0.15f, 0.35f),
			});

			Behaviors.Add(new DreamTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(200, 150, 220, 140),
				BubbleColor = new Color(200, 160, 240, 200),
				RippleColor = new Color(180, 140, 220, 180),
				ButterflyColor = new Color(220, 180, 255, 220),
			});

			Behaviors.Add(new DebuffOnHitBehavior(BuffID.Confused, 180));
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
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
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(0.5f, 2f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
					new Color(200, 160, 240, 180), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}