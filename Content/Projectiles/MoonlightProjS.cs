using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	class MoonlightProjS:ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1.5f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;

			tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjS").Value;
		}

		private Texture2D tex;
		private readonly TrailManager trailManager = new TrailManager();

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);

			// 初始化虚影拖尾
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
			trailManager.AddGhostTrail(trailTex,
				color: new Color(150, 220, 255),
				maxPositions: 16,
				widthScale: 0.4f,
				lengthScale: 2f,
				alpha: 0.8f,
				recordInterval: 2);
		}

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
			Vector2 targetPos = Main.MouseWorld;
			var targetVel = Vector2.Normalize(targetPos - Projectile.Center) * 10f;
			Projectile.velocity = (targetVel + Projectile.velocity * 10) / 11f;
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}
	}
}
