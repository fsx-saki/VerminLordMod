using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 霜霖月弹 — 三转月道冰弹。
	/// 两层穿透，命中附加霜霖buff，冰蓝刀光拖尾。
	/// </summary>
	public class MoonlightProjC : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(0.5f, 0.8f, 1.2f)
			});

			// 月道蓝尾
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			// 冰系拖尾 — 星星 + 雪花，叠在月尾之上
			Behaviors.Add(new IceTrailBehavior
			{
				SuppressDefaultDraw = false,
				EnableGhostTrail = false,
				StarColor = new Color(180, 235, 255, 230),
				StarSize = 0.6f,
				SnowflakeColor = new Color(220, 245, 255, 200),
				SnowflakeSize = 0.3f,
				SnowflakeSpawnChance = 0.4f,
			});

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(160, 220, 255),
				GlowLayers = 3,
				GlowBaseScale = 1.5f,
				GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.6f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.35f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 12; Projectile.height = 12;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = true; Projectile.penetrate = 2;
			Projectile.timeLeft = 70; Projectile.alpha = 0;
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
					color: new Color(140, 210, 255, 200),
					maxPositions: 20,
					widthScale: 1.8f,
					lengthScale: 2.0f,
					alpha: 0.8f,
					recordInterval: 1);
			}
		}

		protected override void OnAI()
		{
			Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 1.0f);
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<ShuangLinbuff>(), 120);

			// 冰霜溅射粒子
			for (int i = 0; i < 6; i++)
			{
				Dust d = Dust.NewDustPerfect(
					target.Center + Main.rand.NextVector2Circular(8, 8),
					DustID.IceTorch,
					Main.rand.NextVector2Circular(3, 3), 30,
					default, Main.rand.NextFloat(0.6f, 1.0f));
				d.noGravity = true;
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnBurst(Projectile.Center, new Color(140, 210, 255));
		}
	}
}
