using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 金月弹幕 — 月道+金道融合。
	/// 金色月刃，带刀光拖尾，死亡时金色爆散。
	/// </summary>
	public class MoonlightProjG : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(1.5f, 1.2f, 0.4f) // 金色光
			});

			// 金色拖尾
			var trail = new TrailBehavior
			{
				AutoDraw = true,
				SuppressDefaultDraw = false
			};
			Behaviors.Add(trail);

			// 金色发光
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(255, 220, 120),
				GlowLayers = 3,
				GlowBaseScale = 1.3f,
				GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.6f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.35f,
			});
		}

		public override void SetDefaults()
		{
		Projectile.width = 10;
		Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 99;
			Projectile.timeLeft = 60;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(255, 220, 100, 200),
					maxPositions: 20,
					widthScale: 2.0f,
					lengthScale: 2.0f,
					alpha: 0.8f,
					recordInterval: 1);
			}
		}

		protected override void OnAI()
		{
			Lighting.AddLight(Projectile.Center, 0.6f, 0.5f, 0.2f);
		}

		protected override void OnKilled(int timeLeft)
		{
			// 金色爆散
			MoonBurstHelper.SpawnBurst(Projectile.Center, new Color(255, 220, 120), 1.2f);
			// 额外金粉
			for (int i = 0; i < 15; i++)
			{
				Dust d = Dust.NewDustPerfect(
					Projectile.Center + Main.rand.NextVector2Circular(10, 10),
					DustID.GoldFlame,
					Main.rand.NextVector2Circular(4, 4), 30,
					default, Main.rand.NextFloat(0.8f, 1.5f));
				d.noGravity = true;
			}
		}
	}
}
