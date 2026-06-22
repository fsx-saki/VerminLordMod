using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 爆燃蛊·极专属弹幕 — 基于炎道爆炸冲击波
	/// 金焰爆燃特效：双重收缩光圈 + 金色爆炸 + 炽白星火
	/// </summary>
	public class BaoX3ExplosionProj : BaseBullet
	{
		private const float FlySpeed = 10f;
		private const int SparkCount = 8;
		private const float SparkSpeed = 7f;
		private const int FlameWallCount = 4;

		/// <summary>光圈收缩起始半径（像素）</summary>
		private const float RingStartRadius = 100f;
		/// <summary>光圈收缩触发时的剩余时间</summary>
		private const int RingTriggerTime = 50;
		/// <summary>光圈收缩间隔（帧）</summary>
		private const int RingSpawnInterval = 4;

		private int _ringTimer;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: FlySpeed)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(2f, 1.5f, 0.2f) // 金色光芒
			});

			Behaviors.Add(new LiquidTrailBehavior
			{
				MaxFragments = 40,
				FragmentLife = 20,
				SizeMultiplier = 0.9f,
				SpawnInterval = 1,
				ColorStart = new Color(255, 240, 150, 255), // 金色拖尾
				ColorEnd = new Color(255, 200, 50, 0),
				Buoyancy = 0.03f,
				AirResistance = 0.97f,
				InertiaFactor = 0.3f,
				SplashFactor = 0.15f,
				SplashAngle = 0.4f,
				RandomSpread = 0.6f,
				AutoDraw = true,
				SuppressDefaultDraw = true
			});

			Behaviors.Add(new ExplosionKillBehavior
			{
				ExplodeOnKill = true,
				KillCount = 40,        // 更多碎片
				KillSpeed = 10f,
				KillSizeMultiplier = 2f,
				KillFragmentLife = 40,
				ExplodeOnTileCollide = true,
				TileCollideCount = 30,
				TileCollideSpeed = 8f,
				TileCollideSizeMultiplier = 1.8f,
				TileCollideFragmentLife = 35,
				DestroyOnTileCollideExplosion = true,
				ColorStart = new Color(255, 255, 200, 255), // 炽白→金
				ColorEnd = new Color(255, 200, 50, 0)
			});
		}

		protected override void OnAI()
		{
			if (Projectile.timeLeft <= RingTriggerTime && Projectile.timeLeft > 0)
			{
				_ringTimer++;
				if (_ringTimer % RingSpawnInterval == 0)
				{
					float progress = 1f - (float)Projectile.timeLeft / RingTriggerTime;
					float currentRadius = RingStartRadius * (1f - progress * 0.85f);

					// 主光圈 — 金色炎环
					SpawnDustRing(Projectile.Center, currentRadius, progress, DustID.GoldFlame, 20, 1.5f);

					// 副光圈 — 倾斜45°的炽白环，增强立体感
					SpawnDustRing(Projectile.Center + new Vector2(0, -8), currentRadius * 0.7f, progress, DustID.WhiteTorch, 12, 1.0f);
				}
			}
		}

		private void SpawnDustRing(Vector2 center, float radius, float progress, int dustType, int count, float baseScale)
		{
			for (int i = 0; i < count; i++)
			{
				float angle = MathHelper.TwoPi * i / count;
				Vector2 offset = angle.ToRotationVector2() * radius;
				Vector2 pos = center + offset;

				Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero);
				d.noGravity = true;
				d.scale = baseScale + progress * 1.0f;
				d.alpha = (int)(80 + progress * 120);
				d.velocity = -offset.SafeNormalize(Vector2.Zero) * (2f + progress * 2f);
			}
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 0.8f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 99;
			Projectile.timeLeft = 180;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnKilled(int timeLeft)
		{
			// 爆炸产生炽金星火弹向四面八方飞散
			int fireBaseType = ModContent.ProjectileType<FireBaseProj>();
			for (int i = 0; i < SparkCount; i++)
			{
				float angle = MathHelper.TwoPi * i / SparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
				float speed = SparkSpeed + Main.rand.NextFloat(-1f, 2f);
				Vector2 vel = angle.ToRotationVector2() * speed;

				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					vel,
					fireBaseType,
					(int)(Projectile.damage * 0.5f),
					Projectile.knockBack * 0.4f,
					Projectile.owner
				);
			}

			// 爆炸后在地面留下滞留火焰区域
			int flameWallType = ModContent.ProjectileType<FireFlameWallProj>();
			for (int i = 0; i < FlameWallCount; i++)
			{
				float angle = MathHelper.TwoPi * i / FlameWallCount;
				float distance = Main.rand.NextFloat(40f, 80f);
				Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * distance;

				Vector2 groundPos = FindGroundPosition(spawnPos);
				if (groundPos != Vector2.Zero)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						groundPos,
						Vector2.Zero,
						flameWallType,
						(int)(Projectile.damage * 0.3f),
						0f,
						Projectile.owner
					);
				}
			}
		}

		private Vector2 FindGroundPosition(Vector2 startPos)
		{
			int startX = (int)(startPos.X / 16f);
			int startY = (int)(startPos.Y / 16f);

			for (int dy = 0; dy < 30; dy++)
			{
				int y = startY + dy;
				if (y < 0 || y >= Main.maxTilesY) break;

				Tile tile = Main.tile[startX, y];
				if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
				{
					return new Vector2(startX * 16 + 8, (y - 1) * 16 + 8);
				}
			}

			return startPos;
		}
	}
}
