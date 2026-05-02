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
	class MeteorProj : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1.1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);

			Projectile.rotation += 0.55f;
			Dust.NewDustDirect(Projectile.position, 16, 16, DustID.YellowStarDust).velocity*=0.2f;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/WhiteTail").Value;

			// 初始化虚影拖尾（发光效果在PreDraw中手动控制）
			trailManager.AddGhostTrail(trailTex,
				color: new Color(255, 237, 99),
				maxPositions: 16,
				widthScale: 1f,
				lengthScale: 1f,
				alpha: 1f,
				recordInterval: 2,
				enableGlow: false);
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
				
				// 发光层（Additive混合，由TrailManager的Draw已开启）
				Color glowColor = new Color(255, 237, 99) * 0.3f;
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
				Color drawColor = Projectile.GetAlpha(lightColor);
				Main.spriteBatch.Draw(mainTexture, drawPos, null, drawColor,
					Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
			}
			
			return false; // 阻止引擎默认绘制
		}

		public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
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

	}
}
