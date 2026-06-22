using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 邀月 A — 母体
	/// 缓慢飞行，不断生成子体追踪预测位置，死亡时生成 C 级追击体。
	/// </summary>
	public class InvitingMoonProjA : BaseBullet
	{
		private int _spawnTimer;
		private int _nextSpawnInterval;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(205, 135, 255),
				GlowLayers = 3,
				GlowBaseScale = 1.2f,
				GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.4f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.25f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = false; Projectile.penetrate = 99;
			Projectile.timeLeft = 80; Projectile.alpha = 100;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			_nextSpawnInterval = Main.rand.Next(1, 7);
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTailP").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(205, 135, 255),
					maxPositions: 16, widthScale: 1f, lengthScale: 1f, alpha: 0.8f, recordInterval: 2);
			}
		}

		protected override void OnAI()
		{
			Projectile.rotation += 0.3f; // 母体自旋

			_spawnTimer++;
			if (_spawnTimer >= _nextSpawnInterval && _spawnTimer <= 50)
			{
				_spawnTimer = 0;
				_nextSpawnInterval = Main.rand.Next(1, 7);

				var player = Main.player[Projectile.owner];
				float angle = Main.rand.Next(0, 9) * 2 * MathHelper.Pi / 9f;
				var b = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					player.Center,
					angle.ToRotationVector2() * 4f,
					ModContent.ProjectileType<InvitingMoonProjB>(),
					Projectile.damage, 0f, Projectile.owner);

				if (b.ModProjectile is InvitingMoonProjB bProj)
					bProj.targetPos = Projectile.Center + Projectile.velocity * 36;
			}

			// 紫色旋风粒子
			if (Main.rand.NextBool(2))
			{
				float a = (_spawnTimer * 0.3f + Main.rand.NextFloat(-0.3f, 0.3f));
				float r = 20f + Main.rand.NextFloat(-4f, 4f);
				Vector2 pos = Projectile.Center + a.ToRotationVector2() * r;
				Dust d = Dust.NewDustPerfect(pos, DustID.ApprenticeStorm,
					Vector2.Zero, 100, new Color(205, 135, 255), 0.75f + Main.rand.NextFloat() * 0.25f);
				d.noGravity = true;
				d.velocity = (Projectile.Center - pos).RotatedBy(1.57f) * 0.15f;
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			// 紫色旋风爆发
			for (int i = 0; i < 25; i++)
			{
				float angle = MathHelper.TwoPi / 25f * i;
				Vector2 dir = angle.ToRotationVector2();
				Dust d = Dust.NewDustPerfect(
					Projectile.Center + dir * 35f,
					DustID.ApprenticeStorm,
					dir.RotatedBy(MathHelper.PiOver2) * 6f,
					100, new Color(205, 135, 255), 1.5f);
				d.noGravity = true;
			}

			// 生成 C 级追击体
			Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.position, Projectile.velocity,
				ModContent.ProjectileType<InvitingMoonProjC>(),
				Projectile.damage, 10f, Projectile.owner);

			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(205, 135, 255));
		}
	}
}
