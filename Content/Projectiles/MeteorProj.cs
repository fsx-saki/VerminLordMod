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

			// 初始化发光虚影拖尾
			trailManager.AddGhostTrail(trailTex,
				color: new Color(255, 237, 99),
				maxPositions: 16,
				widthScale: 1f,
				lengthScale: 1f,
				alpha: 1f,
				recordInterval: 2,
				enableGlow: true);
		}

		public override bool PreDraw(ref Color lightColor) {
			trailManager.Draw(Main.spriteBatch);
			return true;
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
