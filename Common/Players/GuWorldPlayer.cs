using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Players
{
    // ============================================================
    // GuWorldPlayer - 玩家在世界模拟中的状态数据
    // 每个玩家独立拥有：声望、通缉状态、关系网络
    // ============================================================

    /// <summary> 玩家与某个家族的关系快照 </summary>
    public class FactionRelation
    {
        public FactionID Faction;
        public int ReputationPoints;        // 声望点数 [-1000, 1000]
        public bool HasBounty;              // 是否被该家族悬赏
        public int BountyValue;             // 悬赏金额（元石）
        public bool IsAllied;               // 是否结盟
        public int LastInteractionDay;      // 最后交互的游戏日

        /// <summary> 根据声望点数计算等级 </summary>
        public RepLevel GetLevel()
        {
            if (ReputationPoints <= -200) return RepLevel.Hostile;
            if (ReputationPoints < 0) return RepLevel.Unfriendly;
            if (ReputationPoints == 0) return RepLevel.Neutral;
            if (ReputationPoints < 200) return RepLevel.Friendly;
            return RepLevel.Allied;
        }
    }

    /// <summary> 玩家的一次背刺记录 </summary>
    public class BetrayalRecord
    {
        public FactionID TargetFaction;
        public FactionID BeneficiaryFaction; // 受益方
        public int GameDay;
        public bool IsExposed;               // 是否暴露
    }

    /// <summary> 玩家当前的通缉状态 </summary>
    public class BountyStatus
    {
        public FactionID Issuer;
        public int Amount;
        public int RemainingDuration;        // 持续游戏天数
        public bool IsActive => RemainingDuration > 0;
    }

    public class GuWorldPlayer : ModPlayer
    {
        // ===== 声望数据 =====
        public Dictionary<FactionID, FactionRelation> FactionRelations = new();

        // ===== 通缉数据 =====
        public List<BountyStatus> ActiveBounties = new();

        // ===== 背刺记录 =====
        public List<BetrayalRecord> BetrayalHistory = new();

        // ===== 当前状态 =====
        public FactionID CurrentAlly = FactionID.None;     // 当前结盟的家族
        public int InfamyPoints;                            // 恶名值（影响所有家族初始态度）
        public int FamePoints;                              // 声望值（影响中立NPC态度）

        // ===== 个体级信念辅助（MVA-Mini） =====
        // 信念数据实际存储在 NPC 实例的 PlayerBeliefs 字典中
        // 这里只提供辅助方法，方便从玩家侧查询

        // ===== 初始化 =====
        public override void OnEnterWorld()
        {
            // 确保所有已知家族都有记录
            foreach (FactionID fid in System.Enum.GetValues<FactionID>())
            {
                if (fid == FactionID.None || fid == FactionID.Scattered) continue;
                if (!FactionRelations.ContainsKey(fid))
                {
                    // 恶名影响新家族首次接触时的初始声望
                    int initialRep = -InfamyPoints / 2;
                    FactionRelations[fid] = new FactionRelation
                    {
                        Faction = fid,
                        ReputationPoints = initialRep,
                        HasBounty = false,
                        IsAllied = false,
                        LastInteractionDay = (int)Main.time
                    };
                }
            }
        }

        // ===== 声望操作 =====

        /// <summary> 获取对某家族的态度等级 </summary>
        public RepLevel GetRepLevel(FactionID faction)
        {
            if (FactionRelations.TryGetValue(faction, out var rel))
                return rel.GetLevel();
            return RepLevel.Neutral;
        }

        /// <summary> 获取对某家族的声望点数 </summary>
        public int GetRepPoints(FactionID faction)
        {
            return FactionRelations.TryGetValue(faction, out var rel) ? rel.ReputationPoints : 0;
        }

        /// <summary> 增加声望 </summary>
        public void AddReputation(FactionID faction, int points, string reason = "")
        {
            if (!FactionRelations.ContainsKey(faction))
                FactionRelations[faction] = new FactionRelation { Faction = faction };

            var rel = FactionRelations[faction];
            rel.ReputationPoints = (int)MathHelper.Clamp(rel.ReputationPoints + points, -1000, 1000);
            rel.LastInteractionDay = (int)Main.time;

            // 声望变化通知
            string direction = points >= 0 ? $"+{points}" : $"{points}";
            string msg = $"{GuWorldSystem.GetFactionDisplayName(faction)}声望 {direction}";
            if (!string.IsNullOrEmpty(reason)) msg += $" ({reason})";
            Main.NewText(msg, points >= 0 ? Color.Green : Color.Red);

            // 连锁反应：影响相关家族
            ApplyChainReaction(faction, points);
        }

        /// <summary> 减少声望 </summary>
        public void RemoveReputation(FactionID faction, int points, string reason = "")
        {
            AddReputation(faction, -points, reason);
        }

        /// <summary> 连锁反应：对某家族的行为影响其盟友/敌对家族 </summary>
        private void ApplyChainReaction(FactionID faction, int points)
        {
            foreach (var (otherId, otherRel) in FactionRelations)
            {
                if (otherId == faction) continue;
                int relationBetween = GuWorldSystem.GetRelation(faction, otherId);

                // 友方家族：声望变化减半传递
                if (relationBetween > 30)
                    otherRel.ReputationPoints = (int)MathHelper.Clamp(otherRel.ReputationPoints + points / 2, -1000, 1000);
                // 敌对家族：反向传递
                else if (relationBetween < -20)
                    otherRel.ReputationPoints = (int)MathHelper.Clamp(otherRel.ReputationPoints - points / 2, -1000, 1000);
            }
        }

        // ===== 通缉系统 =====

        /// <summary> 被某家族悬赏 </summary>
        public void SetBounty(FactionID issuer, int amount)
        {
            ActiveBounties.Add(new BountyStatus
            {
                Issuer = issuer,
                Amount = amount,
                RemainingDuration = 30  // 持续30天
            });

            if (FactionRelations.TryGetValue(issuer, out var rel))
                rel.HasBounty = true;

            Main.NewText($"你被{GuWorldSystem.GetFactionDisplayName(issuer)}悬赏了！赏金 {amount} 元石",
                Color.OrangeRed);
        }

        /// <summary> 检查是否被某家族通缉 </summary>
        public bool IsBountyActive(FactionID faction)
        {
            return ActiveBounties.Exists(b => b.Issuer == faction && b.IsActive);
        }

        // ===== 结盟系统 =====

        /// <summary> 与某家族结盟 </summary>
        public bool FormAlliance(FactionID faction)
        {
            if (CurrentAlly != FactionID.None)
            {
                Main.NewText("你已经与其他家族结盟了！", Color.Yellow);
                return false;
            }

            if (GetRepLevel(faction) < RepLevel.Friendly)
            {
                Main.NewText($"声望不够，{GuWorldSystem.GetFactionDisplayName(faction)}拒绝结盟",
                    Color.Yellow);
                return false;
            }

            CurrentAlly = faction;
            if (FactionRelations.TryGetValue(faction, out var rel))
                rel.IsAllied = true;

            Main.NewText($"你与{GuWorldSystem.GetFactionDisplayName(faction)}结盟了！",
                Color.Green);
            return true;
        }

        /// <summary> 背刺当前盟友 </summary>
        public bool BetrayAlly(FactionID beneficiary)
        {
            if (CurrentAlly == FactionID.None) return false;

            var betrayed = CurrentAlly;
            BetrayalHistory.Add(new BetrayalRecord
            {
                TargetFaction = betrayed,
                BeneficiaryFaction = beneficiary,
                GameDay = (int)Main.time,
                IsExposed = false
            });

            // 背刺的声望惩罚
            RemoveReputation(betrayed, 300, "背刺");
            AddReputation(beneficiary, 100, "背刺受益");

            CurrentAlly = FactionID.None;
            if (FactionRelations.TryGetValue(betrayed, out var rel))
                rel.IsAllied = false;

            Main.NewText($"你背叛了{GuWorldSystem.GetFactionDisplayName(betrayed)}！",
                Color.Purple);
            return true;
        }

        // ===== 恶名/声望 =====

        /// <summary> 增加恶名（击杀NPC等行为） </summary>
        /// <summary>
        /// 添加恶名值
        /// 恶名影响新家族首次接触时的初始声望，但不直接修改已有家族的声望值。
        /// 已有家族的声望变化应通过 RemoveReputation（有目击者时）单独处理。
        /// </summary>
        public void AddInfamy(int points)
        {
            InfamyPoints += points;
            // 恶名不再自动扣除所有家族声望
            // 恶名值会在 OnEnterWorld 初始化新家族时影响初始声望
        }

        // ============================================================
        // 个体级信念辅助方法（MVA-Mini）
        // ============================================================

        /// <summary>
        /// 获取某个NPC对当前玩家的信念状态
        /// 信念数据存储在NPC实例中，这里通过NPC的IGuMasterAI接口查询
        /// </summary>
        public static BeliefState GetNPCBelief(NPC npc, Player player)
        {
            if (npc.ModNPC is IGuMasterAI guAI)
            {
                return guAI.GetBelief(player.name);
            }
            return null;
        }

        /// <summary>
        /// 检查某个NPC是否对当前玩家有敌意（基于信念）
        /// </summary>
        public static bool IsNPCHostileBasedOnBelief(NPC npc, Player player)
        {
            var belief = GetNPCBelief(npc, player);
            if (belief == null) return false;
            return belief.RiskThreshold < 0.3f;
        }

        /// <summary>
        /// 获取某个NPC对当前玩家的风险阈值（调试用）
        /// </summary>
        public static float GetNPCThreatLevel(NPC npc, Player player)
        {
            var belief = GetNPCBelief(npc, player);
            if (belief == null) return 0.5f;
            return belief.RiskThreshold;
        }

        // ===== 持久化 =====

        public override void SaveData(TagCompound tag)
        {
            // 保存声望（使用 List<TagCompound> 替代 Dictionary）
            var repData = new List<TagCompound>();
            foreach (var (fid, rel) in FactionRelations)
            {
                repData.Add(new TagCompound
                {
                    ["faction"] = fid.ToString(),
                    ["points"] = rel.ReputationPoints
                });
            }
            tag["factionRep"] = repData;

            // 保存通缉
            var bountyData = new List<TagCompound>();
            foreach (var b in ActiveBounties)
            {
                bountyData.Add(new TagCompound
                {
                    ["issuer"] = b.Issuer.ToString(),
                    ["amount"] = b.Amount,
                    ["duration"] = b.RemainingDuration
                });
            }
            tag["bounties"] = bountyData;

            tag["currentAlly"] = CurrentAlly.ToString();
            tag["infamy"] = InfamyPoints;
            tag["fame"] = FamePoints;
        }

        public override void LoadData(TagCompound tag)
        {
            // 加载声望
            if (tag.TryGet("factionRep", out List<TagCompound> repData))
            {
                foreach (var entry in repData)
                {
                    var fidStr = entry.GetString("faction");
                    var points = entry.GetInt("points");
                    if (System.Enum.TryParse<FactionID>(fidStr, out var fid))
                    {
                        FactionRelations[fid] = new FactionRelation
                        {
                            Faction = fid,
                            ReputationPoints = points
                        };
                    }
                }
            }

            // 加载通缉
            if (tag.TryGet("bounties", out List<TagCompound> bountyData))
            {
                foreach (var b in bountyData)
                {
                    if (System.Enum.TryParse<FactionID>(b.GetString("issuer"), out var issuer))
                    {
                        ActiveBounties.Add(new BountyStatus
                        {
                            Issuer = issuer,
                            Amount = b.GetInt("amount"),
                            RemainingDuration = b.GetInt("duration")
                        });
                    }
                }
            }

            if (tag.TryGet("currentAlly", out string allyStr))
                System.Enum.TryParse<FactionID>(allyStr, out CurrentAlly);

            InfamyPoints = tag.GetInt("infamy");
            FamePoints = tag.GetInt("fame");
        }

        // ===== 每日更新 =====

        public override void PreUpdate()
        {
            // 每天更新通缉状态
            if (Main.time == 0 && Main.dayTime)
            {
                for (int i = ActiveBounties.Count - 1; i >= 0; i--)
                {
                    ActiveBounties[i].RemainingDuration--;
                    if (!ActiveBounties[i].IsActive)
                    {
                        var issuer = ActiveBounties[i].Issuer;
                        if (FactionRelations.TryGetValue(issuer, out var rel))
                            rel.HasBounty = false;
                        ActiveBounties.RemoveAt(i);
                    }
                }
            }

            // 按 P 键打开/关闭声望 UI
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P) &&
                !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
            {
                var uiSystem = ModContent.GetInstance<Common.UI.ReputationUI.ReputationUISystem>();
                uiSystem?.ToggleUI();
            }
        }
    }
}
