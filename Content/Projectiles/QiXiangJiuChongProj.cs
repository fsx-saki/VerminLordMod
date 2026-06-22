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
	public class QiXiangJiuChongProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(0.6f, 0.9f, 0.9f)
			});

			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(160, 220, 240),
				GlowLayers = 3, GlowBaseScale = 1.2f, GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f,
			});

			Behaviors.Add(new DustTrailBehavior
			{
				DustType = DustID.BlueFlare, SpawnChance = 3, DustScale = 1.0f,
				VelocityMultiplier = 0.1f, NoGravity = true, RandomSpeed = 1.5f
			});

			Behaviors.Add(new DebuffOnHitBehavior
			{
				Buffs = new List<(int, int)> { (BuffID.MoonLeech, 120) }
			});

			Behaviors.Add(new DustOnHitBehavior
			{
				DustType = DustID.BlueFlare, DustCount = 8,
				SpeedMin = 1f, SpeedMax = 3f, ScaleMin = 0.8f, ScaleMax = 1.5f,
				Color = Color.Lavender
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14; Projectile.height = 14;
			Projectile.scale = 1f; Projectile.ignoreWater = false;
			Projectile.tileCollide = true; Projectile.penetrate = 1;
			Projectile.timeLeft = 240; Projectile.alpha = 20;
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
					color: new Color(160, 220, 240, 200),
					maxPositions: 16, widthScale: 1.2f, lengthScale: 1.5f, alpha: 0.7f, recordInterval: 2);
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(160, 220, 240));
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}
