using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Content.Trails;
using Terraria.GameContent;

namespace VerminLordMod.Common.Players
{
	/// <summary>
	/// 特效播放器 - 处理所有玩家身上的Buff视觉效果和战斗效果
	/// 
	/// 【迁移来源】从 QiPlayer 迁移：
	/// - OnHitByNPC（Buff反击效果）
	/// - CanUseItem（木魅蛊攻击 + 高阶返还真元）
	/// - MuMeiAttackDelay（木魅蛊攻击冷却）
	/// </summary>
	public class EffectsPlayer : ModPlayer
	{
		// ===== 视觉效果（原有） =====
		// 旋风特效相关
		private readonly TrailManager breezeTrailManager = new TrailManager();
		private bool hasBreezeBuff = false;
		private bool breezeTrailInitialized = false;

		// ===== 战斗效果（从 QiPlayer 迁移） =====
		/// <summary>木魅蛊攻击冷却计时器</summary>
		private int MuMeiAttackDelay = 0;

		public override void PostUpdate() {
			// 只为自己的玩家处理
			if (Player != Main.LocalPlayer) return;

			hasBreezeBuff = Player.HasBuff(ModContent.BuffType<BreezeWheelbuff>());

			// 旋风buff特效 - 使用TrailManager更新拖尾
			if (hasBreezeBuff) {
				// 首次激活时初始化拖尾
				if (!breezeTrailInitialized) {
					Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
					breezeTrailManager.AddGhostTrail(trailTex,
						color: new Color(70, 255, 160),
						maxPositions: 12,
						widthScale: 0.8f,
						lengthScale: 2.0f,
						alpha: 0.5f,
						recordInterval: 2);
					breezeTrailInitialized = true;
				}
				breezeTrailManager.Update(Player.Center, Player.velocity);
			}
			else {
				breezeTrailInitialized = false;
			}

			// 木魅蛊攻击冷却更新
			if (Player.HasBuff(ModContent.BuffType<MuMeibuff>())) {
				if (MuMeiAttackDelay != 0) {
					MuMeiAttackDelay--;
					MuMeiAttackDelay = Utils.Clamp(MuMeiAttackDelay, 0, 100);
				}
			}
		}

		/// <summary>
		/// 绘制特效（由PlayerEffectDrawSystem调用）
		/// </summary>
		public void DrawEffects(SpriteBatch sb) {
			if (!hasBreezeBuff || Player == null) return;
			DrawBreezeWheelEffect(sb, Player);
		}

		private void DrawBreezeWheelEffect(SpriteBatch sb, Player player) {
			// 获取玩家当前速度
			Vector2 velocity = player.velocity;
			float speed = velocity.Length();

			// 速度越大，特效越大（最小0.3，最大2.5）
			float speedScale = MathHelper.Lerp(0.3f, 2.5f, MathHelper.Clamp(speed / 10f, 0f, 1f));

			Texture2D tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/CycloneProj").Value;
			Color cycloneColor = new Color(70, 255, 160); // 绿色旋风

			// 旋转角度（随风旋转）
			float rotation = Main.GameUpdateCount * 0.05f;

			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Vector2 pos = player.Center - Main.screenPosition;
			Vector2 origin = tex.Size() * 0.5f;

			// 绘制多层旋风发光效果
			for (int i = 0; i < 4; i++) {
				float layerScale = speedScale * (0.8f + i * 0.4f);
				float layerAlpha = 0.6f - i * 0.12f;
				float rotOffset = i * MathHelper.PiOver4; // 每层偏转45度

				Color glowColor = cycloneColor * layerAlpha;
				Main.EntitySpriteDraw(tex, pos, null, glowColor,
					rotation + rotOffset, origin, layerScale, SpriteEffects.None, 0);
			}

			// 使用TrailManager绘制拖尾（已在Additive模式下）
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
			DrawBreezeTrail(sb, trailTex, speedScale);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawBreezeTrail(SpriteBatch sb, Texture2D trailTex, float scale) {
			// 从TrailManager获取GhostTrail数据绘制拖尾
			var ghostTrail = breezeTrailManager.Get<GhostTrail>();
			if (ghostTrail == null) return;

			var oldPosi = ghostTrail.GetPositions();
			if (oldPosi == null || oldPosi.Length < 2) return;

			for (int i = 1; i < oldPosi.Length; i++) {
				Vector2 start = oldPosi[i - 1] - Main.screenPosition;
				Vector2 end = oldPosi[i] - Main.screenPosition;
				if (start == Vector2.Zero || end == Vector2.Zero) continue;

				Vector2 diff = end - start;
				float segLength = diff.Length();
				if (segLength < 2f) continue;

				float rot = diff.ToRotation();
				float fadeAlpha = (1f - (float)i / oldPosi.Length) * 0.5f;
				Color trailColor = new Color(70, 255, 160) * fadeAlpha;

				float width = scale * 0.8f;
				float length = segLength * 0.5f;
				Vector2 drawPos = (start + end) / 2f;

				sb.Draw(trailTex, drawPos, null, trailColor, rot,
					trailTex.Size() * 0.5f, new Vector2(length, width), SpriteEffects.None, 0);
			}
		}

		// ============================================================
		// 以下方法从 QiPlayer 迁移
		// ============================================================

		/// <summary>
		/// 被NPC击中时的Buff反击效果。
		/// 从 QiPlayer.OnHitByNPC 迁移。
		/// </summary>
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
			if (Player.HasBuff(ModContent.BuffType<FireClothesbuff>()))
				npc.AddBuff(BuffID.OnFire, 300);
			if (Player.HasBuff(ModContent.BuffType<WaterShellbuff>())) {
				npc.AddBuff(ModContent.BuffType<Waterbuff>(), 300);
			}
			if (Player.HasBuff(ModContent.BuffType<YingShangbuff>())) {
				for (int i = 0; i < 100; i++) {
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame).velocity *= 2;
				}
				npc.life -= hurtInfo.Damage / 2;
				CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, 16, 16), Color.Red, hurtInfo.Damage / 10);
				npc.position += npc.position - Player.position;
			}
		}

		/// <summary>
		/// 物品使用前的特殊处理。
		/// 从 QiPlayer.CanUseItem 迁移：
		/// - 木魅蛊Buff下非木系武器触发自动攻击
		/// - 六转以上使用低阶蛊虫返还真元
		/// </summary>
		public override bool CanUseItem(Item item) {
			// 木魅蛊自动攻击
			if (Player.HasBuff(ModContent.BuffType<MuMeibuff>()) && !(item.ModItem is WoodWeapon)) {
				if (MuMeiAttackDelay != 0) {
					return false;
				}
				Vector2 v = Main.MouseWorld - Player.Center;

				if (v.Length() < 280) {
					v.Normalize();
					Projectile.NewProjectile(null, Player.Center, v * 2f, ModContent.ProjectileType<MuMeiAttackN>(), 75, 3);
					MuMeiAttackDelay = 25;
				}
				else {
					float r = (float)System.Math.Atan2(v.Y, v.X);
					for (int i = -1; i <= 1; i++) {
						float r2 = r + i * MathHelper.Pi / 8f;
						Vector2 shootVel = r2.ToRotationVector2() * 10;
						Projectile.NewProjectile(null, Player.Center, shootVel, ModContent.ProjectileType<PineNeedleProj>(), 40, 3);
						MuMeiAttackDelay = 40;
					}
				}
				return false;
			}
			else {
				// 六转以上使用低阶蛊虫返还真元
				if (item.ModItem is GuWeaponItem && Player.altFunctionUse != 2)
				{
					var qiRealm = Player.GetModPlayer<QiRealmPlayer>();
					if (qiRealm.GuLevel >= 6)
					{
						var gu = item.ModItem as GuWeaponItem;
						if (gu.GetGuLevel() < 6)
						{
							var qiResource = Player.GetModPlayer<QiResourcePlayer>();
							qiResource.RefundQi(gu.GetQiCost());
						}
					}
				}
				return base.CanUseItem(item);
			}
		}
	}
}
