using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 邀月 B — 子体光流
	/// 无实体贴图，仅由拖尾构成的光流，朝母体预测位置飞行。
	/// </summary>
	public class InvitingMoonProjB : BaseBullet
	{
		public Vector2 targetPos;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
			// 仅拖尾 + 发光，不绘制本体
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = true });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(205, 135, 255),
				GlowLayers = 2,
				GlowBaseScale = 0.8f,
				GlowScaleIncrement = 0.3f,
				GlowBaseAlpha = 0.5f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 4; Projectile.height = 4;
			Projectile.scale = 0.1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = true; Projectile.penetrate = 99;
			Projectile.timeLeft = 3600; Projectile.alpha = 255;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTailP").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(205, 135, 255),
					maxPositions: 20, widthScale: 1.2f, lengthScale: 2.0f, alpha: 0.9f, recordInterval: 1);
			}
		}

		protected override void OnAI()
		{
			var targetVel = Vector2.Normalize(targetPos - Projectile.Center) * 12f;
			Projectile.velocity = (targetVel + Projectile.velocity * 20) / 21f;
			Projectile.alpha = Math.Max(0, Projectile.alpha - 8);

			if (Vector2.Distance(Projectile.Center, targetPos) <= 16f)
				Projectile.Kill();
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(205, 135, 255), 0.5f);
		}
	}
}
