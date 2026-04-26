using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
	class CycloneProj:ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 90;
			Projectile.alpha = 100;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private readonly TrailManager trailManager = new TrailManager();

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);

			Projectile.rotation += 0.5f;

			Vector2 targetPos = Main.MouseWorld;
			var targetVel = Vector2.Normalize(targetPos - Projectile.Center) * 10f;
			Projectile.velocity = (targetVel + Projectile.velocity * 10) / 11f;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);

			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;

			// 初始化虚影拖尾（带发光效果，兼容原5层发光）
			trailManager.AddGhostTrail(trailTex,
				color: new Color(70, 255, 160),
				maxPositions: 20,
				widthScale: 1.6f,
				lengthScale: 1.8f,
				alpha: 0.6f,
				recordInterval: 2,
				enableGlow: true);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 30; i++) {
				float angle = MathHelper.TwoPi / 30f * i;
				Vector2 direction = angle.ToRotationVector2();
				
				Vector2 outwardVel = direction * 0f;
				Vector2 spinVel = direction.RotatedBy(MathHelper.PiOver2) * 6f;

				Dust dust = Dust.NewDustPerfect(
					Projectile.Center + direction * 40f, 
					DustID.ApprenticeStorm, 
					outwardVel + spinVel,
					100, 
					default(Color), 
					1.5f
				);
				dust.noGravity = true;
				dust.velocity = outwardVel + spinVel;
			}
		}
	}
}
