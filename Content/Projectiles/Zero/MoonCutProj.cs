using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 月道模式4 — 月华斩
	/// 高速穿透月刃，短距离快斩。
	/// 极短生存时间、高速度、穿透明亮。
	/// </summary>
	public class MoonCutProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(180, 220, 255), GlowLayers = 2,
				GlowBaseScale = 1.5f, GlowScaleIncrement = 0.5f,
				GlowBaseAlpha = 0.6f, GlowAlphaDecay = 0.2f, GlowAlphaMultiplier = 0.4f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 20; Projectile.height = 20;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = false; Projectile.penetrate = 10;
			Projectile.timeLeft = 20; Projectile.alpha = 60;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
				tb.TrailManager.NewTrail(tex, color: new Color(160, 210, 255),
					maxPositions: 10, widthScale: 0.6f, lengthScale: 2f, alpha: 0.8f, recordInterval: 1);
			}
		}

		protected override void OnAI()
		{
			Lighting.AddLight(Projectile.Center, 0.3f, 0.4f, 0.6f);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
			{
				var d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10),
					DustID.AncientLight, Main.rand.NextVector2Circular(8, 8), 50,
					new Color(160, 210, 255), Main.rand.NextFloat(0.8f, 1.5f));
				d.noGravity = true;
			}
		}
	}
}
