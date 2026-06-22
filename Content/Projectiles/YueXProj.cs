using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
	/// 月芒弹 — 月芒蛊，银白月刃，附流血效果。
	/// </summary>
	public class YueXProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(0.8f, 0.8f, 0.9f)
			});

			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(200, 210, 255),
				GlowLayers = 3, GlowBaseScale = 1.2f, GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f,
			});

			Behaviors.Add(new DustTrailBehavior
			{
				DustType = DustID.Silver, SpawnChance = 3, DustScale = 1.0f,
				VelocityMultiplier = 0.1f, NoGravity = true, RandomSpeed = 1.5f
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				Buffs = new List<(int, int)> { (BuffID.Bleeding, 120) }
			});

			Behaviors.Add(new DustOnHitBehavior
			{
				DustType = DustID.Silver, DustCount = 8,
				SpeedMin = 1f, SpeedMax = 3f, ScaleMin = 0.8f, ScaleMax = 1.5f, Color = Color.Silver
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 10; Projectile.height = 10;
			Projectile.scale = 1f; Projectile.ignoreWater = false;
			Projectile.tileCollide = true; Projectile.penetrate = 1;
			Projectile.timeLeft = 300; Projectile.alpha = 20;
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
					color: new Color(180, 200, 255, 200),
					maxPositions: 16, widthScale: 1.5f, lengthScale: 1.8f, alpha: 0.7f, recordInterval: 2);
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(180, 200, 255));
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}
