using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 火光烛天蛊弹幕 — 火道
	/// 范围提示光圈 + 烈焰法阵，爆炸后点燃范围内所有怪物（焚身同款 LiquidTrail 燃烧）
	/// </summary>
	public class HuoGuangZhuTianProj : BaseBullet
	{
		private const float BlastRadius = 300f;
		private const int BurstInterval = 12;
		private const float BurstChance = 0.35f;
		private const int RingDustInterval = 3;

		private int _timer;

		protected override void RegisterBehaviors()
		{
			// 法阵扩大（从 0.3→4 倍）
			Behaviors.Add(new ScaleOverLifeBehavior(0.3f, 4f, animateAlpha: true, startAlpha: 120, endAlpha: 0)
			{
				EnableLight = true,
				LightColor = new Vector3(2f, 1.2f, 0.3f)
			});

			// 粒子法阵
			Behaviors.Add(new ParticleBodyBehavior(particleCount: 60, bodyRadius: 50f)
			{
				ParticleSize = 0.8f,
				ColorStart = new Color(255, 220, 80, 220),
				ColorEnd = new Color(255, 100, 20, 100),
				SwirlSpeed = 0.08f,
				ReturnForce = 0.7f,
				JitterStrength = 0.3f,
				ShrinkOverLife = false,
				StretchOnMove = false,
				StretchFactor = 0f,
				EnableLight = false
			});

			// 扩散边缘液态火焰
			Behaviors.Add(new LiquidTrailBehavior
			{
				MaxFragments = 30,
				FragmentLife = 12,
				SizeMultiplier = 1.2f,
				SpawnInterval = 1,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				Buoyancy = 0.03f,
				AirResistance = 0.95f,
				InertiaFactor = 0.3f,
				SplashFactor = 0.2f,
				SplashAngle = 0.5f,
				RandomSpread = 0.6f,
				AutoDraw = true,
				SuppressDefaultDraw = false
			});

			Behaviors.Add(new SuppressDrawBehavior());

			// 大范围爆破伤害 + 灼烧 Buff
			Behaviors.Add(new OnKillAoEBehavior
			{
				Radius = BlastRadius,
				DamageMultiplier = 1.5f,
				Knockback = 10f,
				Buffs = new List<(int, int)>
				{
					(BuffID.OnFire, 600),
					(BuffID.CursedInferno, 300)
				}
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 55;
			Projectile.alpha = 255;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnAI()
		{
			_timer++;

			float scale = 0.3f + (1f - (float)Projectile.timeLeft / 55f) * 3.7f;

			// 1. 范围提示光圈
			if (_timer % RingDustInterval == 0)
			{
				int ringCount = 24;
				float ringRadius = BlastRadius * (scale / 4f);
				for (int i = 0; i < ringCount; i++)
				{
					float angle = MathHelper.TwoPi * i / ringCount;
					Vector2 pos = Projectile.Center + angle.ToRotationVector2() * ringRadius;

					Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, Vector2.Zero);
					d.noGravity = true;
					d.scale = 0.8f + scale * 0.3f;
					d.alpha = (int)(200 - scale * 30);

					if (i % 2 == 0)
					{
						Vector2 innerPos = Projectile.Center + angle.ToRotationVector2() * ringRadius * 0.85f;
						Dust d2 = Dust.NewDustPerfect(innerPos, DustID.OrangeTorch, Vector2.Zero);
						d2.noGravity = true;
						d2.scale = 0.6f;
					}
				}
			}

			// 2. 随机爆燃
			if (_timer % BurstInterval == 0 && Main.rand.NextFloat() < BurstChance)
			{
				int burstCount = Main.rand.Next(3, 6);
				for (int i = 0; i < burstCount; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
						vel,
						ModContent.ProjectileType<FireBaseProj>(),
						(int)(Projectile.damage * 0.3f),
						2f,
						Projectile.owner
					);
				}
			}

			// 3. 少量向外火星
			if (Main.rand.NextBool(2))
			{
				float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
				float dist = Main.rand.NextFloat(10f, 10f * scale);
				Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
				Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
					angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f));
				d.noGravity = true;
				d.scale = Main.rand.NextFloat(0.6f, 1.2f);
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			// 遍历 BlastRadius 内的所有 NPC，附加燃烧效果
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.dontTakeDamage)
					continue;

				if (Vector2.Distance(npc.Center, Projectile.Center) <= BlastRadius)
				{
					BurningEffectProj.SpawnOn(npc, Projectile.owner, size: 1.5f, damagePerTick: 5);
				}
			}
		}
	}
}
