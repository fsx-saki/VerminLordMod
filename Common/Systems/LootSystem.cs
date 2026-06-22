using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Common.Systems
{
	/// <summary>
	/// 战利品系统（简化版）
	///
	/// 职责：
	/// - 暴露掉落计算（玩家死亡时 30% 物品散落）
	/// - 基地搜刮判定
	/// - 不再管理尸体/战利品 UI（已由 CorpseBag 替代）
	/// </summary>
	public class LootSystem : ModSystem
	{
		public static LootSystem Instance => ModContent.GetInstance<LootSystem>();

		public const float ExposedDropRate = 0.3f;

		// ============================================================
		// 暴露掉落（玩家死亡时部分背包物品散落）
		// ============================================================

		/// <summary>
		/// 计算暴露掉落：从玩家背包中随机选择 30% 物品散落。
		/// 媒介武器和空窍石不掉落。
		/// </summary>
		public List<Item> CalculateExposedDrops(Player player, float rate = ExposedDropRate)
		{
			var drops = new List<Item>();
			foreach (Item item in player.inventory)
			{
				if (item == null || item.IsAir) continue;
				if (IsEssentialItem(item)) continue;
				if (Main.rand.NextFloat() < rate)
				{
					drops.Add(item.Clone());
					item.TurnToAir();
				}
			}
			return drops;
		}

		/// <summary>
		/// 判断是否为关键物品（死亡不掉落）
		/// </summary>
		public bool IsEssentialItem(Item item)
		{
			return item.type == ModContent.ItemType<GuMediumWeapon>()
				|| item.type == ModContent.ItemType<KongQiaoStone>();
		}

		// ============================================================
		// 基地搜刮规则（D-15）
		// ============================================================

		/// <summary>
		/// 判断 NPC 是否可以攻击基地中的玩家。
		/// </summary>
		public bool CanNPCAttackBase(NPC npc, Player player)
		{
			if (!player.active) return false;

			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			bool hasHighValue = qiResource.QiOccupied > 30;

			return hasHighValue;
		}
	}
}
