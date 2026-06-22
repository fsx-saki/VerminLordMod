using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 月影弹 — 月影蛊，暗影月刃，穿透无声。
	/// </summary>
	public class YueYingProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.3f, 0.2f, 0.5f) // 暗紫光
			});

			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(100, 80, 180, 150),
				GlowLayers = 2, GlowBaseScale = 1.0f, GlowScaleIncrement = 0.3f,
				GlowBaseAlpha = 0.4f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.25f,
			});

			Behaviors.Add(new DustTrailBehavior
			{
				DustType = DustID.Shadowflame, SpawnChance = 2, DustScale = 0.8f,
				VelocityMultiplier = 0.05f, NoGravity = true, RandomSpeed = 1.0f,
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				Buffs = new List<(int, int)> { (BuffID.MoonLeech, 120) }
			});

			Behaviors.Add(new DustOnHitBehavior
			{
				DustType = DustID.Shadowflame, DustCount = 10,
				SpeedMin = 2f, SpeedMax = 5f, ScaleMin = 0.6f, ScaleMax = 1.2f,
				Color = new Color(100, 80, 180)
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14; Projectile.height = 14;
			Projectile.scale = 1f; Projectile.ignoreWater = false;
			Projectile.tileCollide = true; Projectile.penetrate = 2; // 四转穿透2
			Projectile.timeLeft = 200; Projectile.alpha = 60; // 半透明
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
				tb.TrailManager.NewTrail(tex,
					color: new Color(100, 80, 180, 160),
					maxPositions: 16, widthScale: 1.2f, lengthScale: 1.5f, alpha: 0.5f, recordInterval: 2);
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(100, 80, 180));
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}
