using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// NPC 社交网络（D-20）。
    ///
    /// 职责：
    /// 1. 管理 NPC 之间的关系图（盟友/敌对）
    /// 2. 信念传播：一个 NPC 对玩家的信念可传播给盟友
    /// 3. 警报传播：扩展 AlertNearbyAllies 为全局社交网络
    /// 4. 信念聚合：查询 NPC 类型对玩家的综合信念
    /// </summary>
    public class NPCSocialNetwork : ModSystem
    {
        // ===== 单例 =====
        public static NPCSocialNetwork Instance => ModContent.GetInstance<NPCSocialNetwork>();

        // ===== 关系图 =====
        /// <summary> NPC type → 盟友 NPC types </summary>
        public Dictionary<int, HashSet<int>> AllyGraph = new();

        /// <summary> NPC type → 敌对 NPC types </summary>
        public Dictionary<int, HashSet<int>> RivalGraph = new();

        // ===== 默认关系配置 =====
        /// <summary>
        /// 注册默认的 NPC 关系。
        /// 同一势力的蛊师互为盟友。
        /// </summary>
        public void RegisterDefaultRelations()
        {
            // 清空现有关系
            AllyGraph.Clear();
            RivalGraph.Clear();

            // 遍历所有加载的 NPC 类型，按势力分组
            for (int type = 1; type < NPCLoader.NPCCount; type++)
            {
                if (NPCLoader.GetNPC(type) is GuMasterBase guMaster)
                {
                    var faction = guMaster.GetFaction();
                    var allies = new HashSet<int>();

                    // 同势力 NPC 互为盟友
                    for (int otherType = 1; otherType < NPCLoader.NPCCount; otherType++)
                    {
                        if (otherType == type) continue;
                        if (NPCLoader.GetNPC(otherType) is GuMasterBase otherMaster)
                        {
                            if (otherMaster.GetFaction() == faction)
                            {
                                allies.Add(otherType);
                            }
                        }
                    }

                    if (allies.Count > 0)
                        AllyGraph[type] = allies;
                }
            }
        }

        /// <summary>
        /// 注册两个 NPC 类型为盟友。
        /// </summary>
        public void RegisterAlly(int typeA, int typeB)
        {
            if (!AllyGraph.ContainsKey(typeA))
                AllyGraph[typeA] = new HashSet<int>();
            if (!AllyGraph.ContainsKey(typeB))
                AllyGraph[typeB] = new HashSet<int>();

            AllyGraph[typeA].Add(typeB);
            AllyGraph[typeB].Add(typeA);
        }

        /// <summary>
        /// 注册两个 NPC 类型为敌对。
        /// </summary>
        public void RegisterRival(int typeA, int typeB)
        {
            if (!RivalGraph.ContainsKey(typeA))
                RivalGraph[typeA] = new HashSet<int>();
            if (!RivalGraph.ContainsKey(typeB))
                RivalGraph[typeB] = new HashSet<int>();

            RivalGraph[typeA].Add(typeB);
            RivalGraph[typeB].Add(typeA);
        }

        /// <summary>
        /// 检查两个 NPC 类型是否为盟友。
        /// </summary>
        public bool AreAllies(int typeA, int typeB)
        {
            return AllyGraph.TryGetValue(typeA, out var allies) && allies.Contains(typeB);
        }

        /// <summary>
        /// 检查两个 NPC 类型是否为敌对。
        /// </summary>
        public bool AreRivals(int typeA, int typeB)
        {
            return RivalGraph.TryGetValue(typeA, out var rivals) && rivals.Contains(typeB);
        }

        // ============================================================
        // 信念传播
        // ============================================================

        /// <summary>
        /// 将源 NPC 对玩家的信念传播给其盟友。
        /// 传播时信念值会有衰减（乘以传播系数）。
        /// </summary>
        public void SpreadBelief(int sourceNPCOriginalType, string playerName, BeliefState sourceBelief, float spreadRange = 500f)
        {
            if (!AllyGraph.TryGetValue(sourceNPCOriginalType, out var allyTypes))
                return;

            Vector2 sourcePos = Vector2.Zero;
            bool foundSource = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (npc.active && npc.type == sourceNPCOriginalType)
                {
                    sourcePos = npc.Center;
                    foundSource = true;
                    break;
                }
            }
            if (!foundSource) return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (!npc.active || npc.ModNPC is not GuMasterBase ally)
                    continue;

                if (allyTypes.Contains(npc.type) &&
                    Vector2.Distance(npc.Center, sourcePos) < spreadRange)
                {
                    var targetBelief = ally.GetBelief(playerName);

                    // 信念传播：置信度取最大值，风险阈值取最小值（更激进）
                    if (sourceBelief.ConfidenceLevel > targetBelief.ConfidenceLevel)
                    {
                        targetBelief.ConfidenceLevel = sourceBelief.ConfidenceLevel * 0.8f; // 衰减 20%
                    }
                    if (sourceBelief.RiskThreshold < targetBelief.RiskThreshold)
                    {
                        targetBelief.RiskThreshold = sourceBelief.RiskThreshold * 1.1f; // 衰减（向谨慎方向）
                    }
                    if (sourceBelief.WasDefeated)
                    {
                        targetBelief.WasDefeated = true;
                    }
                    if (sourceBelief.EstimatedPower > targetBelief.EstimatedPower)
                    {
                        targetBelief.EstimatedPower = sourceBelief.EstimatedPower * 0.85f; // 衰减 15%
                    }
                }
            }
        }

        /// <summary>
        /// 传播警报：通知所有盟友 NPC 玩家有威胁。
        /// 扩展 AlertNearbyAllies 为基于社交网络的传播。
        /// </summary>
        public void SpreadAlert(int sourceNPCType, string playerName, float range)
        {
            if (!AllyGraph.TryGetValue(sourceNPCType, out var allyTypes))
                return;

            Vector2 sourcePos = Vector2.Zero;
            bool foundSource = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (npc.active && npc.type == sourceNPCType)
                {
                    sourcePos = npc.Center;
                    foundSource = true;
                    break;
                }
            }
            if (!foundSource) return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (!npc.active || npc.ModNPC is not GuMasterBase ally)
                    continue;

                if (allyTypes.Contains(npc.type) &&
                    Vector2.Distance(npc.Center, sourcePos) < range)
                {
                    ally.HasBeenHitByPlayer = true;
                    ally.AggroTimer = 1800;
                    ally.ProjectileProtectionEnabled = false;
                }
            }
        }

        // ============================================================
        // 信念聚合
        // ============================================================

        /// <summary>
        /// 获取指定 NPC 类型对玩家的聚合信念。
        /// 聚合所有盟友的信念，取平均值。
        /// </summary>
        public BeliefState AggregateBelief(int npcType, string playerName)
        {
            if (!AllyGraph.TryGetValue(npcType, out var allyTypes))
                return null;

            float totalConfidence = 0f;
            float totalRiskThreshold = 0f;
            float totalEstimatedPower = 0f;
            int count = 0;
            bool anyDefeated = false;
            bool anyHasFought = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (!npc.active || npc.ModNPC is not GuMasterBase ally)
                    continue;

                if (allyTypes.Contains(npc.type))
                {
                    var belief = ally.GetBelief(playerName);
                    totalConfidence += belief.ConfidenceLevel;
                    totalRiskThreshold += belief.RiskThreshold;
                    totalEstimatedPower += belief.EstimatedPower;
                    count++;
                    if (belief.WasDefeated) anyDefeated = true;
                    if (belief.HasFought) anyHasFought = true;
                }
            }

            if (count == 0) return null;

            return new BeliefState
            {
                PlayerName = playerName,
                ConfidenceLevel = totalConfidence / count,
                RiskThreshold = totalRiskThreshold / count,
                EstimatedPower = totalEstimatedPower / count,
                WasDefeated = anyDefeated,
                HasFought = anyHasFought,
                ObservationCount = count,
            };
        }

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            AllyGraph.Clear();
            RivalGraph.Clear();
            RegisterDefaultRelations();
        }

        public override void OnWorldUnload()
        {
            AllyGraph.Clear();
            RivalGraph.Clear();
        }
    }
}
