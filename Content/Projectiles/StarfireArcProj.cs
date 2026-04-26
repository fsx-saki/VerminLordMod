using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	class StarfireArcProj : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.scale = 0.8f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 90;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.extraUpdates = 0;
		}

		private readonly TrailManager trailManager = new TrailManager();

		public override void OnSpawn(IEntitySource source) {
			Texture2D myTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value;

			// 使用新框架的液态拖尾
			var liquid = trailManager.AddLiquidTrail(myTex,
				colorStart: new Color(255, 180, 50, 255),
				colorEnd: new Color(200, 50, 0, 0),
				maxFragments: 25,
				fragmentLife: 12,
				sizeMultiplier: 0.5f,
				spawnInterval: 1);
			liquid.Buoyancy = -0.05f;
			liquid.AirResistance = 0.98f;
			liquid.InertiaFactor = 0.3f;
			liquid.SplashFactor = 0.15f;
			liquid.SplashAngle = 0.4f;
			liquid.RandomSpread = 0.5f;
		}

		public override void AI() {
			Projectile.velocity.Y += 0.25f;
			trailManager.Update(Projectile.Center, Projectile.velocity);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft) {
			var explosionManager = new LiquidTrailManager {
				TrailTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				ExplosionTexture = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
				SizeMultiplier = 0.8f,
				ColorStart = new Color(255, 180, 50, 255),
				ColorEnd = new Color(200, 50, 0, 0),
				FragmentLife = 20
			};
			explosionManager.SpawnExplosion(Projectile.Center, count: 10, speed: 3f);
		}
	}
}
