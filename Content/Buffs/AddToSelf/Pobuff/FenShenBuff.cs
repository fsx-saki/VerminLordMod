using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
	/// <summary>
	/// 焚身Buff — 全身被火焰包裹（使用 FireBaseProj 同款液态火焰视觉）
	/// - 持续灼烧自身生命
	/// - 攻击时额外发射追踪火弹
	/// </summary>
	public class FenShenBuff : ModBuff
	{
		/// <summary>火焰代理弹幕数量</summary>
		private const int FlameProxyCount = 3;

		public override void Update(Player player, ref int buffIndex)
		{
			// 标记 ModPlayer 启用追踪火弹
			player.GetModPlayer<FenShenPlayer>().HasFenShen = true;

			// 每30帧扣1血
			if (Main.GameUpdateCount % 30 == 0 && player.statLife > 1)
			{
				player.statLife--;
				if (player.statLife <= 0)
					player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(player.name + "自焚而亡"), 1, 0);
			}

				// === 燃烧视觉效果（使用 BurningEffectProj） ===
				// 维持1个燃烧效果附着玩家，碎片向上飘升模拟真实火焰

				int burnType = ModContent.ProjectileType<BurningEffectProj>();
				bool hasBurn = false;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == player.whoAmI && p.type == burnType && p.ai[1] == player.whoAmI + 1)
					{ hasBurn = true; break; }
				}

				if (!hasBurn)
				{
					BurningEffectProj.SpawnOn(player, player.whoAmI);
				}

			// 额外火星点缀（少量，增强氛围）
			if (Main.rand.NextBool(4))
			{
				Vector2 pos = player.Center + Main.rand.NextVector2Circular(16f, 12f);
				Dust d = Dust.NewDustPerfect(pos, DustID.OrangeTorch,
					Main.rand.NextVector2Circular(0.5f, 0.5f));
				d.noGravity = true;
				d.scale = Main.rand.NextFloat(0.8f, 1.4f);
			}
		}
	}
}
