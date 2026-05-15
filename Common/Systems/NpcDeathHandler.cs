using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// NPC 死亡处理者（NpcDeathHandler）
    /// 
    /// 职责：
    /// 1. 玩家死亡处理：真元清空、蛊虫休眠、暴露掉落计算、尸体生成、事件发布
    /// 2. NPC 死亡处理：死亡事件发布、悬赏发布、职务空缺处理
    /// 3. NPC 搜尸逻辑：掠夺型 NPC 发现尸体后搜刮剩余物品
    /// 
    /// 依赖：
    /// - NpcCorpse（通用尸体实体）
    /// - EventBus（事件发布）
    /// - KongQiaoPlayer（蛊虫休眠）
    /// - QiResourcePlayer（真元清空）
    /// - GuWorldPlayer（声望/恶名）
    /// 
    /// MVA 简化：
    /// - 暴露掉落固定 30%
    /// - 悬赏奖励 = NPC 生命值 / 10
    /// - 继承顺位硬编码
    /// - 无多人同步
    /// </summary>
    public class NpcDeathHandler : ModSystem
    {
        // ===== 单例访问 =====
        public static NpcDeathHandler Instance => ModContent.GetInstance<NpcDeathHandler>();

        // ===== 配置常量 =====

        /// <summary> 暴露掉落比例（击杀瞬间散落） </summary>
        private const float ExposedDropRate = 0.3f;

        /// <summary> 元石暴露掉落比例 </summary>
        private const float YuanStoneDropRate = 0.5f;

        /// <summary> NPC 搜尸比例（搜走剩余物品的百分比） </summary>
        private const float NpcLootRatio = 0.5f;

        /// <summary> 尸体感知范围 </summary>
        public const float CorpseDetectionRange = 300f;

        /// <summary> 同屏最大尸体数量 </summary>
        public const int MaxCorpsesPerScreen = 5;

        // ============================================================
        // 玩家死亡处理
        // ============================================================

        /// <summary>
        /// 玩家死亡时调用。由 Player.Kill 事件触发。
        /// 已在 KongQiaoPlayer.OnPlayerDeath 和 QiResourcePlayer.OnDeathClearQi 中处理了蛊虫休眠和真元清空。
        /// 此方法处理暴露掉落和尸体生成。
        /// </summary>
        public void OnPlayerKilled(Player player, int? killerNPCType = null)
        {
            // 1. 真元清空（已由 QiResourcePlayer.OnDeathClearQi 处理）
            // 2. 蛊虫休眠 + 忠诚度判定（已由 KongQiaoPlayer.OnPlayerDeath 处理）

            // 3. 计算暴露掉落（30% 基础物品散落）
            var exposedDrops = CalculateExposedDrops(player);
            foreach (var item in exposedDrops)
            {
                if (item != null && !item.IsAir)
                    Item.NewItem(player.GetSource_Death(), player.Center, item);
            }

            // 4. 检查同屏尸体数量限制
            int corpseCount = CountActiveCorpses();
            if (corpseCount >= MaxCorpsesPerScreen)
            {
                // 超出限制：剩余物品直接散落，不生成尸体
                var remaining = GetRemainingItems(player, exposedDrops);
                foreach (var item in remaining)
                {
                    if (item != null && !item.IsAir)
                        Item.NewItem(player.GetSource_Death(), player.Center, item);
                }

                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("尸体太多了，你的物品直接散落在地上。", Color.Gray);
                return;
            }

            // 5. 创建尸体实体
            var remainingItems = GetRemainingItems(player, exposedDrops);
            int corpseIndex = Projectile.NewProjectile(
                player.GetSource_Death(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<NpcCorpse>(),
                0, 0, player.whoAmI);

            if (corpseIndex >= 0 && corpseIndex < Main.maxProjectiles)
            {
                var corpseProj = Main.projectile[corpseIndex];
                if (corpseProj.ModProjectile is NpcCorpse corpse)
                {
                    corpse.CorpseType = CorpseType.Player;
                    corpse.OwnerPlayerID = player.whoAmI;
                    corpse.OwnerName = player.name;
                    corpse.RemainingItems = remainingItems;
                }
            }

            // 6. 发布事件
            EventBus.Publish(new PlayerDeathEvent
            {
                PlayerID = player.whoAmI,
                Position = player.Center,
                DroppedItemTypes = exposedDrops.Select(i => i.type).ToList(),
                KillerNPCID = killerNPCType ?? -1,
                IsBackstab = false  // MVA 简化，P1 再检测
            });

            // 7. 死亡日志
            if (killerNPCType.HasValue && killerNPCType.Value > 0)
            {
                string npcName = Lang.GetNPCNameValue(killerNPCType.Value);
                if (string.IsNullOrEmpty(npcName))
                    npcName = "未知敌人";
                Main.NewText($"你被 {npcName} 击杀。尸体留在原地。", Color.Red);
            }
            else
            {
                Main.NewText("你死了。尸体留在原地。", Color.Red);
            }
        }

        // ============================================================
        // NPC 死亡处理
        // ============================================================

        /// <summary>
        /// NPC 死亡时调用。由 GuMasterBase.OnKill 触发。
        /// </summary>
        public void OnNPCKilled(NPC npc, Player killer)
        {
            // 1. 发布死亡事件
            var deathEvent = new NPCDeathEvent
            {
                NPCType = npc.type,
                NPCWhoAmI = npc.whoAmI,
                KillerPlayerID = killer?.whoAmI ?? -1,
                Position = npc.Center,
                Faction = (npc.ModNPC as GuMasterBase)?.GetFaction() ?? FactionID.Scattered,
                VacatedRole = FactionRole.None  // MVA 简化：暂不实现职务系统
            };
            EventBus.Publish(deathEvent);

            // 2. 发布悬赏事件（MVA 简化：只有家族 NPC 死亡才发布）
            if (deathEvent.Faction != FactionID.Scattered && killer != null && killer.active)
            {
                int bountyReward = CalculateBountyReward(npc);
                if (bountyReward > 0)
                {
                    EventBus.Publish(new BountyPostedEvent
                    {
                        PostingFaction = deathEvent.Faction,
                        TargetPlayerID = killer.whoAmI,
                        RewardAmount = bountyReward,
                        Reason = BountyReason.Revenge,
                        BountyID = GenerateBountyID()
                    });

                    Main.NewText(
                        $"{WorldStateMachine.GetFactionDisplayName(deathEvent.Faction)} 对 {killer.name} 发布了悬赏！赏金 {bountyReward} 元石",
                        Color.Orange);
                }
            }

            // 3. 职务空缺处理（P2：使用 PowerStructureSystem 查询继承顺位）
            if (deathEvent.VacatedRole != FactionRole.None)
            {
                int successor = -1;
                var powerSystem = ModContent.GetInstance<PowerStructureSystem>();
                if (powerSystem != null)
                {
                    successor = powerSystem.GetSuccessor(deathEvent.Faction, deathEvent.VacatedRole);
                }
                if (successor <= 0)
                {
                    successor = GetHardcodedSuccessor(deathEvent.Faction, deathEvent.VacatedRole);
                }

                EventBus.Publish(new RoleVacancyEvent
                {
                    Faction = deathEvent.Faction,
                    VacatedRole = deathEvent.VacatedRole,
                    DeceasedNPCType = npc.type,
                    SuccessorNPCType = successor
                });
            }

            // 4. 战术触发：击杀触发（D-29）
            if (killer != null && killer.active)
            {
                TacticalTriggerSystem.OnNPCKilled(killer, npc);
            }
        }

        // ============================================================
        // NPC 搜尸逻辑
        // ============================================================

        /// <summary>
        /// NPC 发现玩家尸体后搜尸。
        /// 由 GuYuePatrolGuMaster 的 AI 触发。
        /// </summary>
        public void NPCLootCorpse(NPC npc, NpcCorpse corpse)
        {
            if (corpse.IsLootedByNPC) return;  // 已被搜过
            if (!corpse.HasRemainingItems()) return;

            // 掠夺型 NPC 搜走 50% 剩余物品
            int lootCount = System.Math.Max(1, corpse.RemainingItems.Count / 2);
            var looted = corpse.RemoveRandomItems(lootCount);

            if (looted.Count == 0) return;

            corpse.IsLootedByNPC = true;
            corpse.LootingNPCType = npc.type;
            corpse.LootingNPCName = npc.GivenOrTypeName;

            // 被搜走的物品掉落到地上（NPC 不直接持有）
            foreach (var item in looted)
            {
                if (item != null && !item.IsAir)
                    Item.NewItem(npc.GetSource_DropAsItem(), corpse.Projectile.Center, item);
            }

            // 发布事件
            EventBus.Publish(new NPCLootedPlayerEvent
            {
                NPCType = npc.type,
                TargetPlayerID = corpse.OwnerPlayerID,
                LootPosition = corpse.Projectile.Center,
                LootedItemTypes = looted.Select(i => i.type).ToList()
            });

            // 死亡日志（发送给尸体原主人）
            if (corpse.CorpseType == CorpseType.Player)
            {
                Player owner = Main.player[corpse.OwnerPlayerID];
                if (owner.active && owner.whoAmI == Main.myPlayer)
                {
                    string itemNames = string.Join(", ", looted.Select(i => i.Name));
                    Main.NewText($"[死亡日志] 你的尸体被 {corpse.LootingNPCName} 搜索过，失去了：{itemNames}", Color.Orange);
                }
            }
        }

        // ============================================================
        // 工具方法 - 暴露掉落计算
        // ============================================================

        /// <summary>
        /// 计算暴露掉落：委托给 LootSystem。
        /// </summary>
        private List<Item> CalculateExposedDrops(Player player)
        {
            return LootSystem.Instance.CalculateExposedDrops(player, ExposedDropRate);
        }

        /// <summary>
        /// 获取剩余物品（未被暴露掉落的）
        /// </summary>
        private List<Item> GetRemainingItems(Player player, List<Item> exposed)
        {
            var remaining = new List<Item>();
            foreach (Item item in player.inventory)
            {
                if (item != null && !item.IsAir)
                {
                    remaining.Add(item.Clone());
                }
            }
            return remaining;
        }

        // ============================================================
        // 工具方法 - 悬赏
        // ============================================================

        /// <summary>
        /// 计算悬赏奖励。
        /// MVA 简化：奖励 = NPC 生命值 / 10
        /// </summary>
        private int CalculateBountyReward(NPC npc)
        {
            return System.Math.Max(1, npc.lifeMax / 10);
        }

        /// <summary>
        /// 生成悬赏唯一 ID
        /// </summary>
        private int GenerateBountyID()
        {
            return (int)(Main.GameUpdateCount + Main.rand.Next(1000));
        }

        /// <summary>
        /// 获取硬编码的继承顺位。
        /// MVA 占位，P2 再实现具体 NPC。
        /// </summary>
        private int GetHardcodedSuccessor(FactionID faction, FactionRole role)
        {
            // MVA 硬编码继承顺位
            // 例如：古月药堂家老死亡 → 古月药姬（弟子）接替
            return -1; // 占位，P2 再实现具体 NPC
        }

        // ============================================================
        // 工具方法 - 尸体检测
        // ============================================================

        /// <summary>
        /// 查找 NPC 附近的尸体
        /// </summary>
        public static NpcCorpse FindNearbyCorpse(NPC npc, float radius = CorpseDetectionRange)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse corpse)
                {
                    if (Vector2.Distance(npc.Center, corpse.Projectile.Center) < radius)
                        return corpse;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找指定玩家附近的尸体
        /// </summary>
        public static NpcCorpse FindCorpseNearPlayer(Player player, float radius = 60f)
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

        /// <summary>
        /// 统计当前活跃的尸体数量
        /// </summary>
        private static int CountActiveCorpses()
        {
            int count = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse)
                    count++;
            }
            return count;
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 每帧更新：检查尸体腐烂状态（由 Projectile.timeLeft 自动处理）
            // 此处预留扩展点：P1 可添加尸体腐烂特效
        }
    }
}
