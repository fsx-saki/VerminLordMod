using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs;
using Terraria.ID;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
	class StarDartProj : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 5;
			Projectile.height = 5;
			Projectile.scale = 0.6f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 10;
			Projectile.timeLeft = 180;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();
		private bool gravityAffected = false;

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
			
			// 一直受重力影响
			Projectile.velocity.Y += 0.3f;
			if (Projectile.velocity.Y > 12f)
				Projectile.velocity.Y = 12f;
		}
		
		public override bool OnTileCollide(Vector2 oldVelocity) {
			int bounceCount = (int)Projectile.ai[0];
			
			if (bounceCount >= 6) {
				Projectile.tileCollide = false;
				return true;
			}
			
			if (oldVelocity.X != Projectile.velocity.X) {
				Projectile.velocity.X = -Projectile.velocity.X;
			}
			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.Y = -Projectile.velocity.Y;
			}
			
			gravityAffected = true;
			Projectile.ai[0]++;
			Projectile.netUpdate = true;
			
			return false;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/WhiteTail").Value;

			// 初始化虚影拖尾（带偏移量修正贴图宽度导致的视觉偏移）
			var ghost = trailManager.AddGhostTrail(trailTex,
				color: new Color(255, 237, 99),
				maxPositions: 16,
				widthScale: 0.2f,
				lengthScale: 0.2f,
				alpha: 1f,
				recordInterval: 2);
			ghost.Offset = new Vector2(8f, 0f);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int dustId = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.YellowStarDust,
                    Main.rand.NextFloat(-5f, 5f),
                    Main.rand.NextFloat(-5f, 5f),
                    100, default, 1.5f
                );
                Main.dust[dustId].noGravity = true;
            }
        }
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			float randomAngle = Main.rand.NextFloat(-1.8f, 1.8f);
			float currentSpeed = Projectile.velocity.Length();
			float newAngle = Projectile.velocity.ToRotation() + randomAngle;
			Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * currentSpeed;
			Projectile.netUpdate = true;
		}

	}
}
