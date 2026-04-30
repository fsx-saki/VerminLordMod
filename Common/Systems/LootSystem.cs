using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 搜尸/掠夺系统（LootSystem）
    /// 
    /// 实现「暴露掉落 + 深度搜尸」的分层设计：
    /// 1. 暴露掉落：击杀瞬间 30% 基础物品散落，可立即拾取
    /// 2. 深度搜尸：玩家对尸体长按交互 3 秒，获得额外战利品，期间可被感知
    /// 3. 基地搜刮规则：NPC 只攻击在场玩家，不翻箱子
    /// 
    /// 支持两种尸体：
    /// - 怪物尸体（NpcCorpse.Type == Monster/Boss）：由 GlobalNPCCorpseHandler 生成
    /// - 玩家尸体（NpcCorpse.Type == Player）：由 NpcDeathHandler 生成
    /// 
    /// 依赖：
    /// - NpcCorpse（通用尸体实体）
    /// - NpcDeathHandler（死亡处理）
    /// - EventBus（事件发布）
    /// 
    /// MVA 简化：
    /// - 深度搜尸 3 秒（180 帧），有进度条 UI
    /// - 搜尸中断条件：移动或攻击
    /// - 成功获得最多 2 件物品
    /// </summary>
    public class LootSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static LootSystem Instance => ModContent.GetInstance<LootSystem>();

        // ===== 配置常量 =====

        /// <summary> 暴露掉落比例 </summary>
        public const float ExposedDropRate = 0.3f;

        /// <summary> 深度搜尸持续时间（帧） </summary>
        public const int DeepLootDuration = 180; // 3 秒

        /// <summary> 深度搜尸获得的最大物品数 </summary>
        public const int MaxDeepLootItems = 2;

        /// <summary> 深度搜尸交互范围 </summary>
        public const float DeepLootRange = 60f;

        /// <summary> 基地搜刮判定阈值（真元占据 > 此值视为高价值） </summary>
        public const int BaseHighValueThreshold = 30;

        // ===== 深度搜尸状态 =====
        private readonly Dictionary<int, DeepLootState> _activeDeepLoots = new(); // Key = Player.whoAmI

        // ============================================================
        // 暴露掉落（击杀瞬间）
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
                if (IsEssentialItem(item)) continue; // 媒介/空窍石不掉落
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
        // 深度搜尸（持续交互）
        // ============================================================

        /// <summary>
        /// 玩家开始深度搜尸。
        /// 由玩家在尸体附近按交互键触发。
        /// 支持 NpcCorpse（通用尸体，包括怪物和玩家尸体）。
        /// </summary>
        public void StartDeepLoot(Player player, NpcCorpse corpse)
        {
            if (_activeDeepLoots.ContainsKey(player.whoAmI)) return;
            if (!corpse.HasRemainingItems())
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("尸体已被搜刮干净。", Color.Gray);
                return;
            }

            int duration = DeepLootDuration;
            _activeDeepLoots[player.whoAmI] = new DeepLootState
            {
                PlayerID = player.whoAmI,
                Corpse = corpse,
                Duration = duration,
                Elapsed = 0,
                IsInterrupted = false
            };

            // 标记尸体正在被深度搜尸
            corpse.IsBeingDeepLooted = true;
            corpse.DeepLootingPlayerID = player.whoAmI;

            // 发布事件：NPC 可感知
            EventBus.Publish(new DeepLootingStartedEvent
            {
                PlayerID = player.whoAmI,
                CorpsePosition = corpse.Projectile.Center,
                CorpseOwnerPlayerID = corpse.OwnerPlayerID,
                DurationTicks = duration
            });

            if (player.whoAmI == Main.myPlayer)
                Main.NewText("开始搜尸...", Color.Yellow);
        }

        /// <summary>
        /// 每帧更新深度搜尸状态。
        /// 由 PostUpdateWorld 调用。
        /// </summary>
        public void UpdateDeepLoot(Player player)
        {
            if (!_activeDeepLoots.TryGetValue(player.whoAmI, out var state)) return;

            // 检查尸体是否仍然有效
            if (!state.Corpse.Projectile.active || !state.Corpse.HasRemainingItems())
            {
                EndDeepLoot(player, state, true);
                return;
            }

            // 检查中断条件：移动或攻击
            if (player.velocity.Length() > 0.5f || player.itemAnimation > 0)
            {
                state.IsInterrupted = true;
                EndDeepLoot(player, state, false);
                return;
            }

            // 检查是否仍在尸体附近
            if (!state.Corpse.IsPlayerNearby(player, DeepLootRange))
            {
                state.IsInterrupted = true;
                EndDeepLoot(player, state, false);
                return;
            }

            state.Elapsed++;

            if (state.Elapsed >= state.Duration)
            {
                EndDeepLoot(player, state, false);
            }
        }

        /// <summary>
        /// 结束深度搜尸。
        /// </summary>
        private void EndDeepLoot(Player player, DeepLootState state, bool forceInterrupt)
        {
            _activeDeepLoots.Remove(player.whoAmI);

            // 清除尸体标记
            if (state.Corpse.Projectile.active)
            {
                state.Corpse.IsBeingDeepLooted = false;
                state.Corpse.DeepLootingPlayerID = -1;
            }

            bool wasInterrupted = state.IsInterrupted || forceInterrupt;

            if (wasInterrupted)
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("搜尸被中断！", Color.Red);

                EventBus.Publish(new DeepLootingCompletedEvent
                {
                    PlayerID = player.whoAmI,
                    CorpsePosition = state.Corpse.Projectile.Center,
                    LootedItemTypes = new List<int>(),
                    WasInterrupted = true
                });
                return;
            }

            // 成功完成：获得物品
            var looted = new List<Item>();
            int rareCount = System.Math.Min(MaxDeepLootItems, state.Corpse.RemainingItems.Count);
            for (int i = 0; i < rareCount && state.Corpse.RemainingItems.Count > 0; i++)
            {
                int idx = Main.rand.Next(state.Corpse.RemainingItems.Count);
                var item = state.Corpse.RemainingItems[idx];
                state.Corpse.RemainingItems.RemoveAt(idx);
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), item);
                looted.Add(item);
            }

            if (player.whoAmI == Main.myPlayer)
            {
                string names = looted.Count > 0 ? string.Join(", ", looted.Select(i => i.Name)) : "无";
                Main.NewText($"搜尸完成！获得：{names}", Color.Green);
            }

            EventBus.Publish(new DeepLootingCompletedEvent
            {
                PlayerID = player.whoAmI,
                CorpsePosition = state.Corpse.Projectile.Center,
                LootedItemTypes = looted.Select(i => i.type).ToList(),
                WasInterrupted = false
            });
        }

        /// <summary>
        /// 检查玩家是否正在深度搜尸
        /// </summary>
        public bool IsPlayerDeepLooting(int playerID)
        {
            return _activeDeepLoots.ContainsKey(playerID);
        }

        /// <summary>
        /// 获取玩家当前的深度搜尸状态
        /// </summary>
        public DeepLootState GetDeepLootState(int playerID)
        {
            _activeDeepLoots.TryGetValue(playerID, out var state);
            return state;
        }

        // ============================================================
        // 玩家交互检测
        // ============================================================

        /// <summary>
        /// 检测玩家是否在尸体附近并按下了交互键。
        /// 由 ModPlayer.PostUpdate 或 GlobalNPC 调用。
        /// </summary>
        public bool TryInteractWithCorpse(Player player)
        {
            // 已经在搜尸中
            if (_activeDeepLoots.ContainsKey(player.whoAmI)) return false;

            // 检查玩家是否按下了交互键（默认为 Open）
            if (!player.controlUseItem) return false;

            // 查找附近的尸体
            NpcCorpse corpse = FindCorpseNearPlayer(player);
            if (corpse == null) return false;

            // 检查尸体是否有剩余物品
            if (!corpse.HasRemainingItems())
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("尸体已被搜刮干净。", Color.Gray);
                return false;
            }

            // 开始深度搜尸
            StartDeepLoot(player, corpse);
            return true;
        }

        /// <summary>
        /// 查找玩家附近的尸体（包括怪物和玩家尸体）
        /// </summary>
        private static NpcCorpse FindCorpseNearPlayer(Player player, float radius = DeepLootRange)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse corpse)
                {
                    if (Vector2.Distance(player.Center, corpse.Projectile.Center) < radius)
                        return corpse;
                }
            }
            return null;
        }

        // ============================================================
        // 基地搜刮规则（D-15）
        // ============================================================

        /// <summary>
        /// 判断 NPC 是否可以攻击基地中的玩家。
        /// NPC 只攻击在场且携带高价值物品的玩家，不翻箱子。
        /// </summary>
        public bool CanNPCAttackBase(NPC npc, Player player)
        {
            if (!player.active) return false; // 玩家不在场

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            // "高价值" = 携带元石 > 50 或启用高阶蛊虫
            bool hasHighValue = qiResource.QiOccupied > BaseHighValueThreshold;

            return hasHighValue;
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 每帧更新所有进行中的深度搜尸
            foreach (Player player in Main.player)
            {
                if (player.active && _activeDeepLoots.ContainsKey(player.whoAmI))
                    UpdateDeepLoot(player);
            }

            // 检测玩家交互（仅在客户端）
            if (Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer >= 0)
            {
                Player localPlayer = Main.LocalPlayer;
                if (localPlayer != null && localPlayer.active && localPlayer.whoAmI == Main.myPlayer)
                {
                    TryInteractWithCorpse(localPlayer);
                }
            }
        }
    }

    /// <summary>
    /// 深度搜尸状态
    /// </summary>
    public class DeepLootState
    {
        /// <summary> 搜尸玩家 ID </summary>
        public int PlayerID;

        /// <summary> 被搜的尸体 </summary>
        public NpcCorpse Corpse;

        /// <summary> 总持续时间（帧） </summary>
        public int Duration;

        /// <summary> 已过去时间（帧） </summary>
        public int Elapsed;

        /// <summary> 是否被中断 </summary>
        public bool IsInterrupted;

        /// <summary> 进度百分比 [0, 100] </summary>
        public float Progress => Duration > 0 ? (float)Elapsed / Duration * 100f : 0f;
    }
}
