using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 通用燃烧效果弹幕 — 附着在目标身上，LiquidTrail 碎片向上飘升模拟真实火焰
	/// 静止时火焰向上，运动时碎片被拖在后面形成倾斜
	/// 
	/// ai[0] = unused
	/// ai[1] = target.whoAmI + 1
	/// ai[2] = 0=附着NPC, 1=附着玩家
	/// </summary>
	public class BurningEffectProj : BaseBullet
	{
		/// <summary>拖尾尺寸倍率</summary>
		public float SizeMultiplier = 1.2f;
		/// <summary>碎片起始颜色</summary>
		public Color ColorStart = new Color(255, 220, 100, 255);
		/// <summary>碎片结束颜色</summary>
		public Color ColorEnd = new Color(255, 30, 0, 0);
		/// <summary>每帧伤害 (0=不造成伤害)</summary>
		public int DamagePerTick = 0;

		private int _damageTimer;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new LiquidTrailBehavior(
				texture: ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/Zero/FireBaseProj").Value)
			{
				MaxFragments = 35,
				FragmentLife = 22,
				SizeMultiplier = SizeMultiplier,
				SpawnInterval = 2,
				ColorStart = ColorStart,
				ColorEnd = ColorEnd,
				Buoyancy = 0.15f,       // 高浮力 → 碎片向上飘 → 模拟火焰上升
				AirResistance = 0.92f,
				InertiaFactor = 0.3f,
				SplashFactor = 0.2f,
				SplashAngle = 0.5f,
				RandomSpread = 0.7f,
				AutoDraw = true,
				SuppressDefaultDraw = false
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.scale = 0f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
			Projectile.alpha = 255;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.aiStyle = -1;
		}

		protected override void OnAI()
		{
			int targetIdx = (int)Projectile.ai[1] - 1;
			if (targetIdx < 0)
			{
				Projectile.Kill();
				return;
			}

			Entity target;
			if (Projectile.ai[2] == 1)
			{
				if (targetIdx >= Main.maxPlayers) { Projectile.Kill(); return; }
				target = Main.player[targetIdx];
			}
			else
			{
				if (targetIdx >= Main.maxNPCs) { Projectile.Kill(); return; }
				target = Main.npc[targetIdx];
			}

			if (target == null || !target.active)
			{
				Projectile.Kill();
				return;
			}

			// 跟随目标位置（略微向上偏移使火焰看起来从身上燃起）
			Vector2 targetPos = target.Center + new Vector2(0, -6f);
			Projectile.position = targetPos - new Vector2(Projectile.width / 2, Projectile.height / 2);
			Projectile.velocity = (targetPos - Projectile.Center) * 0.3f;

			// 续命
			Projectile.timeLeft = 10;

			// 持续伤害
			if (DamagePerTick > 0)
			{
				_damageTimer++;
				if (_damageTimer >= 30)
				{
					_damageTimer = 0;
					if (Projectile.ai[2] == 1)
					{
						// 对玩家伤害
						Player p = (Player)target;
						if (p.statLife > DamagePerTick)
							p.statLife -= DamagePerTick;
					}
					else
					{
						// 对 NPC 伤害
						NPC n = (NPC)target;
						if (!n.friendly && !n.dontTakeDamage)
							n.SimpleStrikeNPC(DamagePerTick, 0);
					}
				}
			}
		}

		/// <summary>
		/// 在目标身上生成燃烧效果
		/// </summary>
		public static int SpawnOn(Entity target, int owner, float size = 1.2f,
			Color? colorStart = null, Color? colorEnd = null, int damagePerTick = 0)
		{
			int type = ModContent.ProjectileType<BurningEffectProj>();
			int targetType = (target is Player) ? 1 : 0;

			return Projectile.NewProjectile(
				target.GetSource_FromThis(),
				target.Center,
				Vector2.Zero,
				type,
				0, 0, owner,
				size,
				target.whoAmI + 1,
				targetType
			);
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}
