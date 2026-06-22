using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.GlobalNPCs
{
	/// <summary>
	/// 全局 NPC 尸体处理器（简化版）
	/// 
	/// NPC 死亡时掉落 CorpseBag（尸囊袋），内含模组专属战利品（元石、蛊虫）。
	/// 原版物品仍然通过原版掉落表正常掉落。
	/// 
	/// 机制：
	/// 1. NPC 死亡时，根据 NPC 类型和强度生成一个 CorpseBag
	/// 2. 袋中包含 GuDropRegistry 定义的蛊虫 + 元石
	/// 3. 玩家拾取后右键打开获得物品
	/// 4. 不再生成 NpcCorpse 弹幕实体
	/// </summary>
	public class GlobalNPCCorpseHandler : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			// 跳过城镇 NPC
			if (npc.townNPC) return;
			// 跳过友方非城镇 NPC
			if (npc.friendly && !npc.townNPC) return;

			// 生成 CorpseBag
			var bagItem = new Item(ModContent.ItemType<CorpseBag>());
			var bag = bagItem.ModItem as CorpseBag;
			if (bag == null) return;

			bag.SourceNPCName = npc.GivenOrTypeName;

			// 填充战利品
			PopulateBagContents(npc, bag);

			// 掉落在地上
			if (bag.StoredItems.Count > 0)
			{
				Item.NewItem(npc.GetSource_Death(), npc.Center, Vector2.Zero, bagItem);
			}
		}

		/// <summary>
		/// 根据 NPC 类型和强度填充 CorpseBag 内容。
		/// </summary>
		private void PopulateBagContents(NPC npc, CorpseBag bag)
		{
			// 1. 元石（基于 NPC 强度）
			int npcPower = npc.lifeMax + npc.damage * 10;
			int yuanSAmount = System.Math.Max(1, npcPower / 500);
			yuanSAmount += Main.rand.Next(0, System.Math.Max(1, npcPower / 200));

			if (yuanSAmount > 0)
			{
				var yuanS = new Item(ModContent.ItemType<Content.Items.Consumables.YuanS>());
				yuanS.stack = yuanSAmount;
				bag.StoredItems.Add(yuanS);
			}

			// 2. Boss 专属蛊虫
			if (GuDropRegistry.TryGetBossDrops(npc.type, out var bossDrops))
			{
				foreach (var drop in bossDrops)
				{
					if (Main.rand.Next(drop.ChanceDenominator) == 0)
					{
						var item = new Item(drop.ItemType);
						item.stack = Main.rand.Next(drop.MinStack, drop.MaxStack + 1);
						bag.StoredItems.Add(item);
					}
				}
				return; // Boss 不继续添加通用掉落
			}

			// 3. NPC 组掉落（按 GuDropRegistry 分组）
			if (GuDropRegistry.TryGetDrops(npc.type, out var groupDrops))
			{
				foreach (var drop in groupDrops)
				{
					if (Main.rand.Next(drop.ChanceDenominator) == 0)
					{
						var item = new Item(drop.ItemType);
						item.stack = Main.rand.Next(drop.MinStack, drop.MaxStack + 1);
						bag.StoredItems.Add(item);
					}
				}
			}
			else
			{
				// 4. 无专属掉落的 NPC：通用蛊虫（低概率）
				foreach (var drop in GuDropRegistry.UniversalDrops)
				{
					if (Main.rand.Next(drop.ChanceDenominator) == 0)
					{
						var item = new Item(drop.ItemType);
						item.stack = Main.rand.Next(drop.MinStack, drop.MaxStack + 1);
						bag.StoredItems.Add(item);
					}
				}

				// 5. Boss 通用掉落（对所有 Boss 有效的通用蛊虫）
				if (npc.boss)
				{
					foreach (var drop in GuDropRegistry.BossCommonDrops)
					{
						if (Main.rand.Next(drop.ChanceDenominator) == 0)
						{
							var item = new Item(drop.ItemType);
							item.stack = Main.rand.Next(drop.MinStack, drop.MaxStack + 1);
							bag.StoredItems.Add(item);
						}
					}
				}
			}
		}
	}
}
