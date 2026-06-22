	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Terraria;
	using Terraria.ModLoader;
	using VerminLordMod.Common.BulletBehaviors;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 焚身火焰代理弹幕 — 向上燃烧的火焰，拖出 FireBaseProj 同款 LiquidTrail
	/// ai[0] = 火焰高度进度 (0~1)
	/// ai[1] = 目标模式：0=跟随玩家，>0=NPC.whoAmI+1（用于怪物燃烧）
	/// </summary>
	public class FenShenFlameProj : BaseBullet
	{
		private const float FlameHeight = 48f;
		private const float FlameWidth = 14f;
		private const float RiseSpeed = 0.025f;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new LiquidTrailBehavior(
				texture: ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/Zero/FireBaseProj").Value)
			{
				MaxFragments = 30,
				FragmentLife = 18,
				SizeMultiplier = 1.4f,     // 更大拖尾
				SpawnInterval = 2,
				ColorStart = new Color(255, 220, 100, 255),
				ColorEnd = new Color(255, 30, 0, 0),
				Buoyancy = 0.04f,
				AirResistance = 0.93f,
				InertiaFactor = 0.3f,
				SplashFactor = 0.25f,
				SplashAngle = 0.5f,
				RandomSpread = 0.7f,
				AutoDraw = true,
				SuppressDefaultDraw = false
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.scale = 1.2f;  // 可见的火焰精灵
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
			Projectile.alpha = 30;     // 几乎不透明，显示火焰精灵
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.aiStyle = -1;
		}

		protected override void OnAI()
		{
			int targetNpcIdx = (int)Projectile.ai[1] - 1;

			if (targetNpcIdx >= 0 && targetNpcIdx < Main.maxNPCs)
			{
				// === 跟随指定 NPC（怪物燃烧） ===
				NPC npc = Main.npc[targetNpcIdx];
				if (!npc.active) { Projectile.Kill(); return; }

				Projectile.ai[0] += RiseSpeed;
				if (Projectile.ai[0] > 1f) Projectile.ai[0] = 0f;

				float h = Projectile.ai[0];
				float yOff = -FlameHeight * h * 0.6f;
				float xOff = (float)System.Math.Sin(h * MathHelper.TwoPi * 2.5f) * FlameWidth * 0.5f;

				Vector2 pos = npc.Center + new Vector2(xOff, yOff);
				Projectile.position = pos - new Vector2(Projectile.width / 2, Projectile.height / 2);
				Projectile.velocity = Vector2.Zero;
				Projectile.timeLeft = 10;
				return;
			}

			// === 跟随玩家（焚身Buff） ===
			Player player = Main.player[Projectile.owner];
			if (player == null || !player.active) { Projectile.Kill(); return; }

			// 火焰上升
			Projectile.ai[0] += RiseSpeed;
			if (Projectile.ai[0] > 1f) Projectile.ai[0] = 0f;

			float height = Projectile.ai[0];
			float yOffset = -FlameHeight * height; // 向上
			float xOffset = (float)System.Math.Sin(height * MathHelper.TwoPi * 2.5f) * FlameWidth;

			// 运动时火焰向后倾斜
			float tiltX = 0f, tiltY = 0f;
			if (player.velocity.Length() > 0.5f)
			{
				Vector2 wind = -player.velocity.SafeNormalize(Vector2.UnitY);
				tiltX = wind.X * height * 16f;
				tiltY = wind.Y * height * 10f;
			}

			Vector2 flamePos = player.Center + new Vector2(xOffset + tiltX, yOffset + tiltY - 4f);
			Projectile.position = flamePos - new Vector2(Projectile.width / 2, Projectile.height / 2);
			Projectile.velocity = (flamePos - Projectile.Center) * 0.5f;
			Projectile.timeLeft = 10;
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) => false;
	}
}
