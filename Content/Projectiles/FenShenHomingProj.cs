using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 焚身追踪火弹 — 弧形穿墙追踪，FireBaseProj 同款液态火焰拖尾
	/// 命中后在目标身上留下焚身液态火焰
	/// </summary>
	public class FenShenHomingProj : BaseBullet
	{
		private const float FlySpeed = 12f;
		private const float HomingRange = 600f;

		private int _hitNpcIndex = -1;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new HomingBehavior(speed: FlySpeed)
			{
				TrackingWeight = 1f / 22f,
				Range = HomingRange,
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
			});

			Behaviors.Add(new LiquidTrailBehavior
			{
				MaxFragments = 40,
				FragmentLife = 18,
				SizeMultiplier = 0.7f,
				SpawnInterval = 1,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				Buoyancy = 0.05f,
				AirResistance = 0.96f,
				InertiaFactor = 0.4f,
				SplashFactor = 0.2f,
				SplashAngle = 0.5f,
				RandomSpread = 0.8f,
				AutoDraw = true,
				SuppressDefaultDraw = true
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.scale = 0.9f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
		{
			_hitNpcIndex = target.whoAmI;
		}

		protected override void OnKilled(int timeLeft)
		{
			// 命中目标后，在目标身上留下燃烧效果
			if (_hitNpcIndex >= 0 && _hitNpcIndex < Main.maxNPCs)
			{
				NPC npc = Main.npc[_hitNpcIndex];
				if (npc.active)
				{
					BurningEffectProj.SpawnOn(npc, Projectile.owner, size: 1.0f, damagePerTick: 3);
				}
			}
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}
