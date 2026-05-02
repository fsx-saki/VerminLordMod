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
	class PineNeedleProj : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 60;
			Projectile.alpha = 40;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		private readonly TrailManager trailManager = new TrailManager();
		private Vector2 targetPos;

		public override void AI() {
			trailManager.Update(Projectile.Center, Projectile.velocity);
			var targetVel = Vector2.Normalize(targetPos - Projectile.Center) * 10f;
			Projectile.velocity = (targetVel + Projectile.velocity * 10) / 11f;
			Projectile.rotation = Projectile.velocity.ToRotation() + (float)(0.5 * MathHelper.Pi);
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);
			targetPos = Main.MouseWorld;
			
			// 初始化虚影拖尾
			Texture2D trailTex = TextureAssets.Projectile[Projectile.type].Value;
			trailManager.AddGhostTrail(trailTex,
				color: new Color(100, 200, 255),
				maxPositions: 16,
				widthScale: 0.5f,
				lengthScale: 1.0f,
				alpha: 0.6f,
				recordInterval: 2);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Chilled, 240);
		}
	}
}
