using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Content;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Players
{
	/// <summary>
	/// 春秋蝉玩家状态管理器
	/// <para>在玩家受到即死伤害时拦截，触发死亡回溯：旋转+滑行回记录点</para>
	/// </summary>
	public class ChunQiuChanPlayer : ModPlayer
	{
		/// <summary>
		/// 标记本 tick 是否已触发回溯，防止递归
		/// </summary>
		private bool rebirthTriggeredThisTick = false;

		/// <summary>
		/// 标记玩家是否正处于回溯保护状态（跨帧持续保护）
		/// </summary>
		private bool rebirthProtected = false;

		/// <summary>
		/// 回溯保护剩余时间（刻）
		/// </summary>
		private int rebirthProtectionTime = 0;

		/// <summary>
		/// 标记本 tick 是否已经在 ConsumableDodge 中显示过哀鸣文本
		/// 防止 PreKill 重复显示
		/// </summary>
		private bool lamentAlreadyShown = false;

		public override void Initialize()
		{
			rebirthTriggeredThisTick = false;
			rebirthProtected = false;
			rebirthProtectionTime = 0;
			lamentAlreadyShown = false;
		}

		public override void ResetEffects()
		{
			rebirthTriggeredThisTick = false;
			lamentAlreadyShown = false;
		}

		/// <summary>
		/// 每帧后处理：确保回溯保护期间玩家保持存活
		/// </summary>
		public override void PostUpdate()
		{
			if (rebirthProtected && Player.whoAmI == Main.myPlayer)
			{
				if (rebirthProtectionTime > 0)
				{
					rebirthProtectionTime--;

					// 强制保持存活
					if (Player.statLife <= 0)
					{
						Player.statLife = Player.statLifeMax2;
					}
					Player.dead = false;

					// 保护真元不被死亡清空
					var qiResource = Player.GetModPlayer<QiResourcePlayer>();
					if (qiResource.QiCurrent < 0)
					{
						qiResource.QiCurrent = 0;
					}

					// 保持无敌
					Player.immune = true;
					Player.immuneTime = Math.Max(Player.immuneTime, 2);
				}
				else
				{
					rebirthProtected = false;
				}
			}
		}

		/// <summary>
		/// 在伤害应用前修改伤害值。作为第三道防线：
		/// 回溯保护状态下，将所有伤害归零。
		/// </summary>
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (Player.whoAmI != Main.myPlayer)
				return;

			// 保护状态下，所有伤害归零
			if (rebirthTriggeredThisTick || rebirthProtected)
			{
				modifiers.FinalDamage *= 0f;
			}
		}

		/// <summary>
		/// 消耗性闪避：返回 true 可完全闪避此次伤害。
		/// 用于拦截即死伤害，触发春秋蝉回溯。
		/// </summary>
		public override bool ConsumableDodge(Player.HurtInfo info)
		{
			if (rebirthTriggeredThisTick)
				return false;

			if (Player.whoAmI != Main.myPlayer)
				return false;

			// 检查此次伤害是否致命：伤害 >= 当前生命值（即会致死）
			if (Player.statLife > info.Damage && Player.statLife > 0)
				return false;

			// 查找春秋蝉
			ChunQiuChan chunQiuChan = FindChunQiuChanInInventory();
			if (chunQiuChan == null)
				return false;

			if (!chunQiuChan.hasBeenControlled)
			{
				Text.ShowTextRed(Player, "春秋蝉尚未炼化，无法触发回溯！");
				return false;
			}

			// 检查时光能量是否足够（至少需要5%能量才能触发回溯）
			if (chunQiuChan.rebirthEnergy < 5f)
			{
				// 能量不足：在消息栏显示春秋蝉的哀鸣文本（10选1），然后让玩家正常死亡
				string deathMessage = ChunQiuChan.EnergyDepletedMessages[Main.rand.Next(ChunQiuChan.EnergyDepletedMessages.Length)];
				Text.ShowMessageGold(deathMessage);
				lamentAlreadyShown = true;
				return false;
			}

			// 能量充足：在消息栏显示金色死亡模拟消息（10选1）
			string rebirthMessage = ChunQiuChan.RebirthDeathMessages[Main.rand.Next(ChunQiuChan.RebirthDeathMessages.Length)];
			Text.ShowMessageGold(rebirthMessage);

			// 标记已触发
			rebirthTriggeredThisTick = true;
			rebirthProtected = true;
			rebirthProtectionTime = 330; // 5.5秒保护（5秒滑行 + 0.5秒到达后无敌）

			// ConsumableDodge 在伤害应用前触发，statLife 仍为受伤前的值
			// 设置 statLife = 1 确保存活，TriggerRebirth 会按能量百分比补充剩余生命
			if (Player.statLife <= 0)
			{
				Player.statLife = 1;
			}
			Player.dead = false;

			// 给予无敌帧
			Player.immune = true;
			Player.immuneTime = 120;
			Player.SetImmuneTimeForAllTypes(120);

			// 执行回溯逻辑（TriggerRebirth 会根据能量百分比计算生命和真元恢复量）
			chunQiuChan.TriggerRebirth(Player);

			// 春秋蝉重生后：清除所有蛊师NPC的敌对状态
			ClearAllGuMasterAggro();

			return true;
		}

		/// <summary>
		/// 在玩家死亡前触发。作为第二道防线。
		/// </summary>
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
		{
			if (rebirthTriggeredThisTick || rebirthProtected)
			{
				Player.statLife = Player.statLifeMax2;
				Player.dead = false;
				Player.immune = true;
				Player.immuneTime = 120;
				Player.SetImmuneTimeForAllTypes(120);
				return false;
			}

			// 尝试在 PreKill 中触发回溯（处理 ConsumableDodge 未覆盖的死亡情况）
			if (Player.whoAmI != Main.myPlayer)
				return true;

			ChunQiuChan chunQiuChan = FindChunQiuChanInInventory();
			if (chunQiuChan == null)
				return true;

			if (!chunQiuChan.hasBeenControlled)
				return true;

			// 检查时光能量是否足够（至少需要5%能量才能触发回溯）
			if (chunQiuChan.rebirthEnergy < 5f)
			{
				// 能量不足：如果 ConsumableDodge 还没显示过哀鸣，则在此显示
				if (!lamentAlreadyShown)
				{
					string deathMessage = ChunQiuChan.EnergyDepletedMessages[Main.rand.Next(ChunQiuChan.EnergyDepletedMessages.Length)];
					Text.ShowMessageGold(deathMessage);
				}
				return true;
			}

			// 能量充足：在消息栏显示金色死亡模拟消息（10选1）
			string rebirthMessage = ChunQiuChan.RebirthDeathMessages[Main.rand.Next(ChunQiuChan.RebirthDeathMessages.Length)];
			Text.ShowMessageGold(rebirthMessage);

			// 在 PreKill 中触发回溯
			rebirthTriggeredThisTick = true;
			rebirthProtected = true;
			rebirthProtectionTime = 330; // 5.5秒保护（5秒滑行 + 0.5秒到达后无敌）

			// PreKill 在伤害应用后触发，statLife 可能为负数
			// 设置 statLife = 1 确保存活，TriggerRebirth 会按能量百分比补充剩余生命
			Player.statLife = 1;
			Player.dead = false;
			Player.immune = true;
			Player.immuneTime = 120;
			Player.SetImmuneTimeForAllTypes(120);

			chunQiuChan.TriggerRebirth(Player);

			// 春秋蝉重生后：清除所有蛊师NPC的敌对状态
			ClearAllGuMasterAggro();

			return false;
		}

		/// <summary>
		/// 春秋蝉重生后：清除所有蛊师NPC的敌对状态
		/// 遍历所有NPC，将 GuMasterBase 类型的 NPC 的 HasBeenHitByPlayer 和 AggroTimer 重置
		/// </summary>
		private void ClearAllGuMasterAggro()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.ModNPC is GuMasterBase guMaster)
				{
					guMaster.HasBeenHitByPlayer = false;
					guMaster.AggroTimer = 0;
				}
			}
		}

		/// <summary>
		/// 在玩家背包和装备栏中查找春秋蝉
		/// </summary>
		private ChunQiuChan FindChunQiuChanInInventory()
		{
			for (int i = 0; i < 58; i++)
			{
				Item item = Player.inventory[i];
				if (item != null && !item.IsAir && item.ModItem is ChunQiuChan chan)
				{
					return chan;
				}
			}

			return null;
		}
	}
}
