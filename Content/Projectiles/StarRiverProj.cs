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
	class StarRiverProj : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 0.45f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 20;
			Projectile.timeLeft = 1000;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}
		private int state = 0; // 0: orbit, 1: chase, 2: return
		private float orbitAngle = 0f;
		private float orbitSpeed = 0.025f;
		private const float baseRadius = 50f;
		private const float radiusWaveRange = 25f;
		private const int chaseRange = 260;
		private const float chaseSpeed = 12f;
		private const float returnDist = 450f;
		private float chaseTimer = 0f;
		private float returnPhase = 0f;
		private const float transitionSpeed = 0.08f;
		private float currentRadius = baseRadius;
		private Vector2 targetPos;
		private NPC chaseTarget;
		private float chaosPhase1 = 0f;
		private float chaosPhase2 = 0f;
		private float chaosPhase3 = 0f;
		private Texture2D mainTexture;
		private readonly TrailManager trailManager = new TrailManager();

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			
			chaosPhase1 += 0.017f;
			chaosPhase2 += 0.013f;
			chaosPhase3 += 0.009f;
			
			float irregularRadius = baseRadius 
				+ (float)Math.Sin(chaosPhase1) * radiusWaveRange * 0.5f
				+ (float)Math.Sin(chaosPhase2 * 1.7f) * radiusWaveRange * 0.3f
				+ (float)Math.Sin(chaosPhase3 * 2.3f) * radiusWaveRange * 0.2f;
			
			NPC nearestEnemy = null;
			float nearestDist = chaseRange;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && npc.CanBeChasedBy() && !npc.friendly) {
					float dist = Projectile.Distance(npc.Center);
					if (dist < nearestDist) {
						nearestDist = dist;
						nearestEnemy = npc;
					}
				}
			}
			
			float distToOwner = Projectile.Distance(owner.Center);
			
			switch (state) {
				case 0:
					if (nearestEnemy != null) {
						state = 1;
						chaseTarget = nearestEnemy;
						chaseTimer = 0f;
						returnPhase = 0f;
					} else {
						orbitAngle += orbitSpeed;
						float targetX = owner.Center.X + (float)Math.Cos(orbitAngle) * irregularRadius;
						float targetY = owner.Center.Y + (float)Math.Sin(orbitAngle) * irregularRadius * 0.6f;
						targetPos = new Vector2(targetX, targetY);
						
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, 
							(targetPos - Projectile.Center) * 0.15f, transitionSpeed);
						
						currentRadius = MathHelper.Lerp(currentRadius, irregularRadius, 0.05f);
					}
					break;
					
				case 1:
					if (chaseTarget == null || !chaseTarget.active) {
						state = 2;
						break;
					}
					
					chaseTimer++;
					float chaseProgress = chaseTimer / 60f;
					
					if (returnPhase < 1f) {
						float arcAngle = chaseTimer * 0.08f;
						Vector2 toEnemy = chaseTarget.Center - Projectile.Center;
						float distToEnemy = toEnemy.Length();
						
						float spiralFactor = MathHelper.Lerp(0.3f, 1f, MathHelper.Clamp(chaseProgress, 0f, 1f));
						Vector2 perpendicular = new Vector2(-toEnemy.Y, toEnemy.X).SafeNormalize(Vector2.Zero);
						Vector2 arcOffset = perpendicular * (float)Math.Sin(arcAngle) * 60f * spiralFactor;
						
						Vector2 arcTarget = chaseTarget.Center + arcOffset;
						
						Projectile.velocity = Vector2.Lerp(Projectile.velocity,
							(arcTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * chaseSpeed,
							transitionSpeed * 1.5f);
						
						if (distToEnemy < 40f) {
							returnPhase = 1f;
							chaseTimer = 0f;
						}
					} else {
						float returnProgress = chaseTimer / 45f;
						
						float returnAngle = orbitAngle + MathHelper.Pi * 0.5f;
						float returnRadius = baseRadius + (float)Math.Sin(chaosPhase1) * radiusWaveRange * 0.5f;
						Vector2 orbitTarget = owner.Center + new Vector2(
							(float)Math.Cos(returnAngle) * returnRadius,
							(float)Math.Sin(returnAngle) * returnRadius * 0.6f
						);
						
						Projectile.velocity = Vector2.Lerp(Projectile.velocity,
							(orbitTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * chaseSpeed * 0.8f,
							transitionSpeed);
						
						if (Projectile.Distance(owner.Center) < baseRadius + radiusWaveRange + 20f) {
							state = 0;
							orbitAngle = (Projectile.Center - owner.Center).ToRotation();
						}
					}
					break;
					
				case 2:
					orbitAngle += orbitSpeed * 1.5f;
					float returnTargetRadius = irregularRadius;
					Vector2 returnTarget = owner.Center + new Vector2(
						(float)Math.Cos(orbitAngle) * returnTargetRadius,
						(float)Math.Sin(orbitAngle) * returnTargetRadius * 0.6f
					);
					
					Projectile.velocity = Vector2.Lerp(Projectile.velocity,
						(returnTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * chaseSpeed,
						transitionSpeed);
					
					if (distToOwner < baseRadius + radiusWaveRange + 30f) {
						state = 0;
					}
					break;
			}
			
			if (distToOwner > returnDist && state != 2) {
				state = 2;
			}
			
			Projectile.Center += Projectile.velocity;
			
			if(Randommer.Roll(1))
				Dust.NewDustDirect(Projectile.position, 16, 16, DustID.YellowStarDust).velocity *= 0.2f;

			trailManager.Update(Projectile.Center, Projectile.velocity);
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			mainTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/WhiteTail").Value;

			// 初始化虚影拖尾
			trailManager.AddGhostTrail(trailTex,
				color: new Color(255, 237, 99),
				maxPositions: 16,
				widthScale: 0.2f,
				lengthScale: 0.2f,
				alpha: 1f,
				recordInterval: 2);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}
	}
}
