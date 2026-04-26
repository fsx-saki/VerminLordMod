using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	class LiquidFlameProjectile : ModProjectile
	{
		public override void SetStaticDefaults() { }

		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.scale = 1.0f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.aiStyle = -1;
		}

		private readonly TrailManager trailManager = new TrailManager();

		public override void OnSpawn(IEntitySource source) {
			Texture2D myTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/LiquidFlameProjectile").Value;

			// 使用新框架的液态拖尾
			var liquid = trailManager.AddLiquidTrail(myTex,
				colorStart: new Color(255, 80, 30, 255),
				colorEnd: new Color(60, 10, 20, 0),
				maxFragments: 40,
				fragmentLife: 30,
				sizeMultiplier: 1f,
				spawnInterval: 2);
			liquid.Buoyancy = 0.08f;
			liquid.AirResistance = 0.92f;
			liquid.InertiaFactor = 0.25f;
			liquid.SplashFactor = 0.3f;
			liquid.SplashAngle = 0.8f;
			liquid.RandomSpread = 1.2f;
		}

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft) {
			// 使用旧 LiquidTrailManager 的爆炸效果兼容层
			var explosionManager = new LiquidTrailManager {
				TrailTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/LiquidFlameProjectile").Value,
				ExplosionTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/LiquidFlameProjectile").Value,
				SizeMultiplier = 1.2f,
				ColorStart = new Color(255, 80, 30, 255),
				ColorEnd = new Color(60, 10, 20, 0),
				FragmentLife = 35
			};
			explosionManager.SpawnExplosion(Projectile.Center, count: 25, speed: 5f);
		}
	}
}
