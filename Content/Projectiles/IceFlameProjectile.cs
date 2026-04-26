using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	class IceFlameProjectile : ModProjectile
	{
		public override void SetStaticDefaults() { }

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
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
			Texture2D myTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/IceFlameProjectile").Value;
			trailManager.AddGhostTrail(myTex,
				color: new Color(255, 255, 200),
				maxPositions: 50,
				widthScale: 0.5f,
				lengthScale: 1f,
				alpha: 0.8f,
				recordInterval: 1);
		}

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft) {
			// 拖尾自然消散即可
		}
	}
}
