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
	class StarArrowProj : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1.1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();
		private bool stuck = false;
		private bool stuckOnNPC = false;
		private int stuckNPCIndex = -1;
		private int stuckTimer = 0;
		private Vector2 stuckOffset;

		public override void AI() {
			if (stuck && stuckOnNPC) {
				stuckTimer++;
				
				if (!Main.npc[stuckNPCIndex].active) {
					Projectile.Kill();
					return;
				}
				
				Projectile.Center = Main.npc[stuckNPCIndex].Center + stuckOffset;
				
				if (Randommer.Roll(3))
					Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust).velocity *= 0.3f;
				
				if (stuckTimer % 15 == 0) {
					NPC target = Main.npc[stuckNPCIndex];
					target.StrikeNPC(new NPC.HitInfo {
						Damage = Projectile.damage / 4,
						HitDirection = (target.Center.X > Projectile.Center.X) ? 1 : -1,
						Knockback = 1f
					});
				}
				
				return;
			}
			
			if (stuck) {
				stuckTimer++;
				
				if (Randommer.Roll(5))
					Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust).velocity *= 0.3f;
				
				float damageRadius = 50f;
				for (int i = 0; i < Main.npc.Length; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && !npc.friendly && npc.Hitbox.Distance(Projectile.Center) < damageRadius) {
						if (stuckTimer % 20 == 0) {
							npc.StrikeNPC(new NPC.HitInfo {
								Damage = Projectile.damage / 3,
								HitDirection = (npc.Center.X > Projectile.Center.X) ? 1 : -1,
								Knockback = 2f
							});
						}
					}
				}
				
				return;
			}
			
			// 飞行时更新拖尾
			trailManager.Update(Projectile.Center, Projectile.velocity);
			
			Projectile.velocity.Y += 0.133f;
			if (Projectile.velocity.Y > 15f)
				Projectile.velocity.Y = 15f;
			
			if (Randommer.Roll(10))
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust).velocity *= 0.3f;
			
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
		}
		
		public override bool OnTileCollide(Vector2 oldVelocity) {
			stuck = true;
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.netUpdate = true;
			return false;
		}
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			stuck = true;
			stuckOnNPC = true;
			stuckNPCIndex = target.whoAmI;
			stuckOffset = Projectile.Center - target.Center;
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.netUpdate = true;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/WhiteTail").Value;

			// 初始化虚影拖尾（带偏移量修正贴图宽度导致的视觉偏移）
			var ghost = trailManager.AddGhostTrail(trailTex,
				color: new Color(255, 237, 99),
				maxPositions: 45,
				widthScale: 0.6f,
				lengthScale: 0.6f,
				alpha: 1f,
				recordInterval: 2);
			ghost.Offset = new Vector2(4f, 0f);
		}

		public override bool PreDraw(ref Color lightColor) {
			// 插住时阻止默认绘制，飞行时允许默认绘制并绘制拖尾
			if (stuck) {
				return false;
			}
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void PostDraw(Color lightColor) {
			if (stuck) {
				// 插住时手动绘制带缩放抖动的效果
				float scale = Projectile.scale * (1f + (float)Math.Sin(stuckTimer * 0.3f) * 0.05f);
				Main.spriteBatch.Draw(mainTexture, Projectile.Center - Main.screenPosition, null, new Color(255, 237, 99),
					Projectile.rotation, mainTexture.Size() * 0.5f, scale, SpriteEffects.None, 0);
			}
		}

		public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                int dustId = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.YellowStarDust,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100, default, 1.5f
                );
                Main.dust[dustId].noGravity = true;
            }
        }
	}
}
