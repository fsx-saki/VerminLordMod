using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
	class InvitingMoonProjA : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 99;
			Projectile.timeLeft = 80;
			Projectile.alpha = 130;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private int frametime = 0;
		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();

		public override void OnSpawn(IEntitySource source) {
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);

			// 紫色虚影拖尾 — 参照月光弹幕方式
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTailP").Value;
			var ghost = trailManager.AddGhostTrail(trailTex,
				color: new Color(205, 135, 255),
				maxPositions: 16,
				widthScale: 1f,
				lengthScale: 1f,
				alpha: 1f,
				recordInterval: 2,
				enableGlow: false);
			// 沿速度反方向偏移，使拖尾完全位于弹幕后方
			ghost.Offset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 5f;
		}

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);

			Projectile.rotation += 20;
			Player player = Main.player[Projectile.owner];
			Random rand = new Random();
			if (frametime %  rand.Next(1,7)== 0&&frametime<=50) {
				
					Projectile projectile = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Center, ((float)(rand.Next(0,9)*2*Math.PI/9)).ToRotationVector2()*4, ModContent.ProjectileType<InvitingMoonProjB>(), 20, 0);
					var a = projectile.ModProjectile as InvitingMoonProjB;
					a.targetPos=Projectile.Center+Projectile.velocity*36;
				
			}
			frametime++;

			// 紫色旋风环绕粒子（参照CycloneProj的旋风特效）
			if (Main.rand.NextBool(2)) {
				float angle = frametime * 0.3f + Main.rand.NextFloat(-0.3f, 0.3f);
				float radius = 20f + Main.rand.NextFloat(-4f, 4f);
				Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
				Dust dust = Dust.NewDustPerfect(dustPos, DustID.ApprenticeStorm,
					Vector2.Zero, 100, new Color(205, 135, 255), 0.75f + Main.rand.NextFloat() * 0.25f);
				dust.noGravity = true;
				dust.velocity = (Projectile.Center - dustPos).RotatedBy(1.57f) * 0.15f;
			}
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

		public override void OnKill(int timeLeft) {
			// 紫色旋风粒子爆发（参照CycloneProj的OnKill旋风效果）
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
					new Color(205, 135, 255),
					1.5f
				);
				dust.noGravity = true;
				dust.velocity = outwardVel + spinVel;
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity, ModContent.ProjectileType<InvitingMoonProjC>(), 20, 10);
		}
	}
}
