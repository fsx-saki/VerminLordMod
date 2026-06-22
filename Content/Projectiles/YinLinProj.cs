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
	/// 隐鳞弹 — 隐鳞蛊，近乎透明的隐蔽月刃。
	/// 弹幕本身几乎不可见，命中时微光一闪。
	/// </summary>
	public class YinLinProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = false, // 隐形无光
			});

			// 极淡拖尾
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			Behaviors.Add(new DebuffOnHitBehavior
			{
				Buffs = new List<(int, int)> { (BuffID.MoonLeech, 120) }
			});

			// 命中时微光闪烁
			Behaviors.Add(new DustOnHitBehavior
			{
				DustType = DustID.SpectreStaff, DustCount = 6,
				SpeedMin = 1f, SpeedMax = 3f, ScaleMin = 0.4f, ScaleMax = 0.8f,
				Color = Color.Transparent
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 12; Projectile.height = 12;
			Projectile.scale = 0.5f; Projectile.ignoreWater = false;
			Projectile.tileCollide = true; Projectile.penetrate = 1;
			Projectile.timeLeft = 240; Projectile.alpha = 200; // 高度透明
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
					color: new Color(200, 200, 220, 80),
					maxPositions: 8, widthScale: 0.4f, lengthScale: 1.0f, alpha: 0.2f, recordInterval: 3);
			}
		}

		protected override void OnAI()
		{
			// 闪烁：周期性地几乎完全消失
			if (Main.rand.NextBool(4))
				Projectile.alpha = 210 + Main.rand.Next(0, 45);
		}

		protected override void OnKilled(int timeLeft)
		{
			// 几乎无爆散，保持隐蔽
			for (int i = 0; i < 4; i++)
			{
				Dust d = Dust.NewDustPerfect(
					Projectile.Center + Main.rand.NextVector2Circular(4, 4),
					DustID.SpectreStaff, Main.rand.NextVector2Circular(1, 1), 0,
					new Color(180, 180, 200, 50), Main.rand.NextFloat(0.3f, 0.6f));
				d.noGravity = true;
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => true;
	}
}
