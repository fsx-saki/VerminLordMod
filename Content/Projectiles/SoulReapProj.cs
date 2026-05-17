using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 割魂蛊弹幕 — 二转魂道蛊虫
	/// 特性：追踪+穿透物块+命中吸取生命
	/// 弹道：慢速追踪(HomingBehavior)，穿透物块
	/// 视觉：魂道拖尾（灵焰+锁链+鬼火）
	/// 命中效果：吸血回复玩家生命 + 灵魂爆散
	/// 魂道特性：无视物理，以魂火灼烧生灵并汲取生命力
	/// </summary>
	public class SoulReapProj : BaseBullet
	{
		private const float FlySpeed = 7f;
		private const float TrackWeight = 1f / 22f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
			{
				Range = 700f,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
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
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 30;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 灵魂收割：命中回复生命
			if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
			{
				Player player = Main.player[Projectile.owner];
				if (player != null && player.active && !player.dead)
				{
					int healAmount = System.Math.Max(1, damageDone / 8);
					player.Heal(healAmount);

					for (int i = 0; i < 8; i++)
					{
						Vector2 vel = (player.Center - target.Center).SafeNormalize(Vector2.Zero) *
							Main.rand.NextFloat(2f, 5f);
						Vector2 spawnPos = target.Center + Main.rand.NextVector2Circular(15f, 15f);
						Dust d = Dust.NewDustPerfect(spawnPos, DustID.SpectreStaff, vel, 0,
							new Color(140, 200, 255, 200), Main.rand.NextFloat(0.5f, 0.9f));
						d.noGravity = true;
					}
				}
			}

			target.AddBuff(BuffID.Confused, 120);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(0.5f, 2.5f);
				Vector2 vel = angle.ToRotationVector2() * speed;
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SpectreStaff, vel, 0,
					new Color(120, 180, 255, 180), Main.rand.NextFloat(0.4f, 0.8f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}