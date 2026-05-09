using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 情报网络系统（D-43）。
    ///
    /// 职责：
    /// 1. 管理情报片段（IntelFragment）的获取、存储、验证
    /// 2. 提供多种情报获取途径：对话试探、观察、交易、搜尸
    /// 3. 情报可信度系统：未验证的情报可能不准确
    /// 4. 情报共享：玩家可将情报分享给 NPC
    /// </summary>
    public class IntelligenceNetwork : ModSystem
    {
        // ===== 单例 =====
        public static IntelligenceNetwork Instance => ModContent.GetInstance<IntelligenceNetwork>();

        // ===== 情报类型 =====
        public enum IntelType
        {
            NPCLocation,        // NPC 位置
            NPCAttitude,        // NPC 态度
            FactionRelation,    // 势力关系
            BountyInfo,         // 悬赏信息
            ResourceLocation,   // 资源位置
            PowerVacancy,       // 职位空缺
        }

        // ===== 情报片段 =====
        public class IntelFragment
        {
            public IntelType Type;
            public string Title;
            public string Content;
            public int Reliability;         // 可信度 0-100
            public int ObtainedDay;         // 获取游戏日
            public bool IsVerified;         // 是否已验证

            /// <summary>
            /// 获取情报的文本描述。
            /// </summary>
            public string GetSummary()
            {
                string reliabilityStr = Reliability switch
                {
                    >= 80 => "高度可信",
                    >= 50 => "基本可信",
                    >= 20 => "存疑",
                    _ => "不可靠"
                };

                string verifiedStr = IsVerified ? "[已验证]" : "[未验证]";
                return $"[{Type}] {Title}\n{Content}\n可信度: {Reliability}/100 {reliabilityStr} {verifiedStr}";
            }
        }

        // ===== 玩家情报存储 =====
        /// <summary> player.whoAmI → 情报列表 </summary>
        public Dictionary<int, List<IntelFragment>> PlayerIntel = new();

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            PlayerIntel.Clear();
        }

        public override void OnWorldUnload()
        {
            PlayerIntel.Clear();
        }

        // ============================================================
        // 情报获取
        // ============================================================

        /// <summary>
        /// 获取情报。
        /// 根据情报类型和来源 NPC 生成情报片段。
        /// </summary>
        public IntelFragment GatherIntel(Player player, IntelType type, NPC source)
        {
            if (!PlayerIntel.ContainsKey(player.whoAmI))
                PlayerIntel[player.whoAmI] = new List<IntelFragment>();

            var intel = GenerateIntel(type, source);
            if (intel != null)
            {
                PlayerIntel[player.whoAmI].Add(intel);
            }
            return intel;
        }

        /// <summary>
        /// 根据情报类型和来源生成情报片段。
        /// </summary>
        private IntelFragment GenerateIntel(IntelType type, NPC source)
        {
            int currentDay = (int)(Main.time / 36000);

            switch (type)
            {
                case IntelType.NPCLocation:
                    return new IntelFragment
                    {
                        Type = type,
                        Title = $"{source.GivenOrTypeName} 的位置",
                        Content = $"{source.GivenOrTypeName} 最后一次出现在 ({source.position.X / 16:F0}, {source.position.Y / 16:F0})",
                        Reliability = 70,
                        ObtainedDay = currentDay,
                        IsVerified = false,
                    };

                case IntelType.NPCAttitude:
                    if (source.ModNPC is Content.NPCs.GuMasters.GuMasterBase guMaster)
                    {
                        var belief = guMaster.GetBelief(Main.LocalPlayer.name);
                        return new IntelFragment
                        {
                            Type = type,
                            Title = $"{source.GivenOrTypeName} 的态度",
                            Content = $"风险阈值: {belief.RiskThreshold:P0}, 置信度: {belief.ConfidenceLevel:P0}, 实力估计: {belief.EstimatedPower:P0}",
                            Reliability = 60,
                            ObtainedDay = currentDay,
                            IsVerified = false,
                        };
                    }
                    return null;

                case IntelType.FactionRelation:
                    if (source.ModNPC is Content.NPCs.GuMasters.GuMasterBase factionMaster)
                    {
                        var faction = factionMaster.GetFaction();
                        var state = WorldStateMachine.GetFactionState(faction);
                        string relationInfo = "";
                        foreach (var rel in state.Relations)
                        {
                            relationInfo += $"{WorldStateMachine.GetFactionDisplayName(rel.Key)}: {rel.Value}\n";
                        }
                        return new IntelFragment
                        {
                            Type = type,
                            Title = $"{WorldStateMachine.GetFactionDisplayName(faction)} 势力状态",
                            Content = $"领地: {state.TerritorySubworld}\n关系:\n{relationInfo}",
                            Reliability = 50,
                            ObtainedDay = currentDay,
                            IsVerified = false,
                        };
                    }
                    return null;

                case IntelType.BountyInfo:
                    var bountySystem = ModContent.GetInstance<BountySystem>();
                    var bounties = bountySystem.GetFactionBounties(FactionID.GuYue);
                    if (bounties.Count > 0)
                    {
                        var bounty = bounties[0];
                        return new IntelFragment
                        {
                            Type = type,
                            Title = $"悬赏信息",
                            Content = bounty.GetSummary(),
                            Reliability = 80,
                            ObtainedDay = currentDay,
                            IsVerified = false,
                        };
                    }
                    return new IntelFragment
                    {
                        Type = type,
                        Title = "悬赏信息",
                        Content = "目前没有活跃的悬赏。",
                        Reliability = 90,
                        ObtainedDay = currentDay,
                        IsVerified = true,
                    };

                case IntelType.ResourceLocation:
                    var resourceSystem = ModContent.GetInstance<ResourceNodeSystem>();
                    var nearest = resourceSystem.FindNearestNode(Main.LocalPlayer);
                    if (nearest != null)
                    {
                        return new IntelFragment
                        {
                            Type = type,
                            Title = $"资源位置",
                            Content = $"发现 {nearest.Type} 资源节点，位置 ({nearest.Position.X / 16:F0}, {nearest.Position.Y / 16:F0})，剩余 {nearest.CurrentAmount}/{nearest.MaxAmount}",
                            Reliability = 65,
                            ObtainedDay = currentDay,
                            IsVerified = false,
                        };
                    }
                    return null;

                case IntelType.PowerVacancy:
                    var powerSystem = ModContent.GetInstance<PowerStructureSystem>();
                    string vacancies = "";
                    foreach (FactionID faction in System.Enum.GetValues<FactionID>())
                    {
                        foreach (Events.FactionRole role in System.Enum.GetValues<Events.FactionRole>())
                        {
                            if (powerSystem.IsRoleVacant(faction, role))
                            {
                                vacancies += $"{WorldStateMachine.GetFactionDisplayName(faction)} - {PowerStructureSystem.GetRoleDisplayName(role)}\n";
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(vacancies))
                        vacancies = "目前没有职位空缺。";
                    return new IntelFragment
                    {
                        Type = type,
                        Title = "职位空缺",
                        Content = vacancies,
                        Reliability = 85,
                        ObtainedDay = currentDay,
                        IsVerified = true,
                    };

                default:
                    return null;
            }
        }

        // ============================================================
        // 情报验证
        // ============================================================

        /// <summary>
        /// 验证情报。
        /// 验证后可信度提升。
        /// </summary>
        public bool VerifyIntel(Player player, IntelFragment intel)
        {
            if (intel.IsVerified) return true;

            // 验证逻辑：根据情报类型检查是否与实际情况一致
            bool verified = false;

            switch (intel.Type)
            {
                case IntelType.NPCLocation:
                    // 检查 NPC 是否在指定位置附近
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        var npc = Main.npc[i];
                        if (npc.active && npc.GivenOrTypeName == intel.Title.Replace(" 的位置", ""))
                        {
                            verified = true;
                            break;
                        }
                    }
                    break;

                case IntelType.BountyInfo:
                    // 悬赏信息默认可验证
                    verified = true;
                    break;

                case IntelType.ResourceLocation:
                    // 检查资源节点是否存在
                    var resourceSystem = ModContent.GetInstance<ResourceNodeSystem>();
                    var nearest = resourceSystem.FindNearestNode(Main.LocalPlayer);
                    verified = nearest != null;
                    break;

                default:
                    // 其他类型情报需要玩家自行判断
                    verified = false;
                    break;
            }

            if (verified)
            {
                intel.IsVerified = true;
                intel.Reliability = System.Math.Min(100, intel.Reliability + 20);
            }

            return verified;
        }

        // ============================================================
        // 情报共享
        // ============================================================

        /// <summary>
        /// 将情报分享给指定 NPC。
        /// 可能影响 NPC 对玩家的态度。
        /// </summary>
        public void ShareIntel(Player player, NPC target, IntelFragment intel)
        {
            if (target.ModNPC is Content.NPCs.GuMasters.GuMasterBase guMaster)
            {
                var belief = guMaster.GetBelief(player.name);

                // 分享有价值的情报可提升好感
                if (intel.Reliability >= 50)
                {
                    belief.ConfidenceLevel = System.Math.Min(1f, belief.ConfidenceLevel + 0.1f);
                    belief.RiskThreshold = System.Math.Max(0f, belief.RiskThreshold - 0.05f);
                    Main.NewText($"{target.GivenOrTypeName} 对你的信任略有提升", Microsoft.Xna.Framework.Color.LightGreen);
                }
            }
        }

        // ============================================================
        // 玩家情报查询
        // ============================================================

        /// <summary>
        /// 获取玩家拥有的指定类型情报。
        /// </summary>
        public List<IntelFragment> GetPlayerIntel(Player player, IntelType? type = null)
        {
            if (!PlayerIntel.TryGetValue(player.whoAmI, out var intelList))
                return new List<IntelFragment>();

            if (type.HasValue)
            {
                return intelList.FindAll(i => i.Type == type.Value);
            }
            return intelList;
        }

        /// <summary>
        /// 获取玩家拥有的已验证情报。
        /// </summary>
        public List<IntelFragment> GetVerifiedIntel(Player player)
        {
            if (!PlayerIntel.TryGetValue(player.whoAmI, out var intelList))
                return new List<IntelFragment>();

            return intelList.FindAll(i => i.IsVerified);
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var playerData = new List<TagCompound>();

            foreach (var kvp in PlayerIntel)
            {
                var intelList = new List<TagCompound>();
                foreach (var intel in kvp.Value)
                {
                    intelList.Add(new TagCompound
                    {
                        ["type"] = (int)intel.Type,
                        ["title"] = intel.Title,
                        ["content"] = intel.Content,
                        ["reliability"] = intel.Reliability,
                        ["day"] = intel.ObtainedDay,
                        ["verified"] = intel.IsVerified,
                    });
                }

                playerData.Add(new TagCompound
                {
                    ["playerID"] = kvp.Key,
                    ["intel"] = intelList,
                });
            }

            tag["intelNetwork"] = playerData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            PlayerIntel.Clear();

            if (tag.TryGet("intelNetwork", out List<TagCompound> playerData))
            {
                foreach (var entry in playerData)
                {
                    int pid = entry.GetInt("playerID");
                    var intelList = new List<IntelFragment>();

                    if (entry.TryGet("intel", out List<TagCompound> intelEntries))
                    {
                        foreach (var ie in intelEntries)
                        {
                            intelList.Add(new IntelFragment
                            {
                                Type = (IntelType)ie.GetInt("type"),
                                Title = ie.GetString("title"),
                                Content = ie.GetString("content"),
                                Reliability = ie.GetInt("reliability"),
                                ObtainedDay = ie.GetInt("day"),
                                IsVerified = ie.GetBool("verified"),
                            });
                        }
                    }

                    PlayerIntel[pid] = intelList;
                }
            }
        }
    }
}
