using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
	class InvitingMoonProjC : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1.5f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();
		private int frametime = 0;

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);
			NPC tar = Finder.FindCloestEnemy(Projectile.Center, 8000f, (n) => {
				return n.CanBeChasedBy() &&
				!n.dontTakeDamage && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
			});
			if (tar != null) {
				Vector2 targetPos = tar.Center;
				var targetVel = Vector2.Normalize(targetPos - Projectile.Center) * 10f;
				Projectile.velocity = (targetVel + Projectile.velocity * 10) / 11f;
			}
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
			frametime++;
		}

		public override void OnSpawn(IEntitySource source) {
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);

			// 紫色虚影拖尾 — 参照月光弹幕方式
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTailP").Value;
			var ghost = trailManager.AddGhostTrail(trailTex,
				color: new Color(205, 135, 255),
				maxPositions: 16,
				widthScale: 3f,
				lengthScale: 1f,
				alpha: 1f,
				recordInterval: 2,
				enableGlow: false);
			ghost.Offset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 7f;
		}

		public override bool PreDraw(ref Color lightColor) {
			// 先绘制拖尾（TrailManager统一管理Additive混合模式）
			trailManager.Draw(Main.spriteBatch);

			// 手动绘制发光层 + 本体（避免引擎默认绘制导致的重复）
			if (mainTexture != null)
			{
				Vector2 drawPos = Projectile.Center - Main.screenPosition;
				Vector2 origin = mainTexture.Size() * 0.5f;
				float scale = Projectile.scale;
				Color drawColor = Projectile.GetAlpha(lightColor);

				// 发光层（Additive混合，由TrailManager的Draw已开启）
				Color glowColor = new Color(205, 135, 255) * 0.3f;
				for (int i = 0; i < 3; i++)
				{
					float gs = scale * (1.2f + i * 0.4f);
					float ga = 0.5f - i * 0.15f;
					Main.spriteBatch.Draw(mainTexture, drawPos, null, glowColor * ga,
						Projectile.rotation, origin, gs, SpriteEffects.None, 0f);
				}

				// 结束Additive，切回正常混合绘制本体
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				// 绘制本体（正常混合，只绘制一次）
				Main.spriteBatch.Draw(mainTexture, drawPos, null, drawColor,
					Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
			}

			return false; // 返回false，阻止引擎默认绘制
		}
	}
}
