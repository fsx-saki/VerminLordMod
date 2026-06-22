using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 月旋转弹 — MoonSpinning，追踪鼠标位置的月弧弹。
	/// </summary>
	public class MoonlightProjS : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(120, 200, 255), GlowLayers = 3,
				GlowBaseScale = 1.2f, GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = false; Projectile.penetrate = 5;
			Projectile.timeLeft = 180; Projectile.alpha = 0;
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
				tb.TrailManager.NewTrail(tex, color: new Color(120, 200, 255),
					maxPositions: 20, widthScale: 1.5f, lengthScale: 1.8f, alpha: 0.7f, recordInterval: 2);
			}
		}

		protected override void OnAI()
		{
			Vector2 targetVel = Vector2.Normalize(Main.MouseWorld - Projectile.Center) * 12f;
			Projectile.velocity = (targetVel + Projectile.velocity * 10) / 11f;
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center);
		}
	}
}
