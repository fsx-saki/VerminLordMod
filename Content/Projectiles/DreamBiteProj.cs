using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 噬梦蛊弹幕 — 二转梦道蛊虫
	/// 特性：慢速追踪 + 穿透物块 + 命中附加困惑
	/// 弹道：追踪弹(HomingBehavior)，以小角度追踪敌人
	/// 视觉：梦系拖尾（梦境泡泡+幻影残像+迷雾）
	/// 命中效果：困惑(Confused) + 恐惧(Fear)
	/// 梦道特性：无视物理障碍，缓慢但确定地追踪目标
	/// </summary>
	public class DreamBiteProj : BaseBullet
	{
		private const float FlySpeed = 9f;
		private const float TrackWeight = 1f / 30f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
			{
				Range = 800f,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
			});

			Behaviors.Add(new DreamTrailBehavior
			{
				SuppressDefaultDraw = true,
				EnableGhostTrail = true,
				GhostColor = new Color(180, 120, 255, 140),
				BubbleColor = new Color(200, 140, 255, 200),
				PhantomColor = new Color(160, 100, 240, 180),
				MistColor = new Color(140, 80, 220, 140),
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				BuffType = BuffID.Confused,
				BuffDuration = 180,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 20;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Confused, 180);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(0.5f, 2f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
					new Color(180, 120, 255, 160), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}