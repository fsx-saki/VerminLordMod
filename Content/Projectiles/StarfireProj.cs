using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	class StarfireProj : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.penetrate = 99;
			Projectile.timeLeft = 120;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private readonly TrailManager trailManager = new TrailManager();
		private float gravity = 0.12f;
		private int bounceCount = 0;
		private const int maxBounces = 2;
		private const float bounceFactor = 0.4f;

		public override void OnSpawn(IEntitySource source) {
			Texture2D myTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value;

			// 使用新框架的液态拖尾
			var liquid = trailManager.AddLiquidTrail(myTex,
				colorStart: new Color(255, 220, 100, 255),
				colorEnd: new Color(255, 30, 0, 0),
				maxFragments: 50,
				fragmentLife: 15,
				sizeMultiplier: 0.6f,
				spawnInterval: 1);
			liquid.Buoyancy = 0.05f;
			liquid.AirResistance = 0.96f;
			liquid.InertiaFactor = 0.4f;
			liquid.SplashFactor = 0.2f;
			liquid.SplashAngle = 0.5f;
			liquid.RandomSpread = 0.8f;
		}

		public override void AI() {
			Projectile.velocity.Y += gravity;
			trailManager.Update(Projectile.Center, Projectile.velocity);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			bounceCount++;

			if (bounceCount <= maxBounces) {
				if (oldVelocity.X != Projectile.velocity.X) {
					Projectile.velocity.X = -oldVelocity.X * bounceFactor;
				}
				if (oldVelocity.Y != Projectile.velocity.Y) {
					Projectile.velocity.Y = -oldVelocity.Y * bounceFactor;
				}
				if (Math.Abs(Projectile.velocity.X) < 1f && Math.Abs(Projectile.velocity.Y) < 1f) {
					Projectile.velocity *= 0f;
					Projectile.timeLeft = Math.Min(Projectile.timeLeft, 30);
				}
				return false;
			}

			// 爆炸效果
			var explosionManager = new LiquidTrailManager {
				TrailTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				ExplosionTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				SizeMultiplier = 0.8f,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				FragmentLife = 20
			};
			explosionManager.SpawnExplosion(Projectile.Center, count: 12, speed: 3f);
			return true;
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft) {
			var explosionManager = new LiquidTrailManager {
				TrailTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				ExplosionTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				SizeMultiplier = 1f,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				FragmentLife = 25
			};
			explosionManager.SpawnExplosion(Projectile.Center, count: 15, speed: 4f);
		}
	}
}
