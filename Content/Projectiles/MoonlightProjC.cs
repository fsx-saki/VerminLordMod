using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	class MoonlightProjC : ModProjectile
	{
		public override void SetStaticDefaults() {
			
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 70;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;

			tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjC").Value;
		}

		private Texture2D tex;
		private readonly TrailManager trailManager = new TrailManager();

		public override void OnSpawn(IEntitySource source) {
			Projectile.rotation = (float)Math.Atan2(Projectile.velocity.X, -Projectile.velocity.Y);

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
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<ShuangLinbuff>(), 120);
		}
	}
}
