using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// PowerStructureSystem — 权力结构系统（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 管理各家族的职务继承顺位（硬编码）
    /// 2. 订阅 RoleVacancyEvent，自动补位
    /// 3. 维护当前各家族的职务在位状态
    /// 
    /// MVA 阶段：
    /// - 硬编码继承顺位表
    /// - 职务 NPC 死亡后自动补位（聊天栏提示）
    /// - 不开放玩家竞选（P2 再实现）
    /// - 无继任者时：家族功能停摆提示
    /// 
    /// 依赖：
    /// - EventBus（订阅 RoleVacancyEvent）
    /// - NpcDeathHandler（发布 RoleVacancyEvent）
    /// - GuWorldSystem（家族数据）
    /// </summary>
    public class PowerStructureSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static PowerStructureSystem Instance => ModContent.GetInstance<PowerStructureSystem>();

        // ===== 运行时数据 =====
        /// <summary> 当前各家族的职务在位状态 </summary>
        public Dictionary<FactionID, Dictionary<FactionRole, int>> CurrentOfficeHolders = new();

        /// <summary> 职务空缺记录（用于日志） </summary>
        public List<RoleVacancyLog> VacancyLogs = new();

        // ============================================================
        // 硬编码继承顺位表
        // ============================================================

        /// <summary>
        /// 硬编码继承顺位表。
        /// Key: 家族 ID
        /// Value: 职务 → 继任者 NPC Type 列表（按顺位排序）
        /// 
        /// MVA 阶段：只填充古月家族。
        /// P1 扩展：其他家族。
        /// </summary>
        public static readonly Dictionary<FactionID, Dictionary<FactionRole, List<int>>> SuccessionChains = new()
        {
            [FactionID.GuYue] = new Dictionary<FactionRole, List<int>>
            {
                // 药堂家老 → 药姬（弟子接替）
                [FactionRole.Healer] = new List<int>
                {
                    // NPCID.Count + 1, // 药姬（P1 注册具体 NPC Type）
                },
                // 学堂家老 → 弟子接替
                [FactionRole.Trainer] = new List<int>
                {
                    // NPCID.Count + 2, // 学堂弟子
                },
                // 巡逻队长 → 副队长接替
                [FactionRole.GuardCaptain] = new List<int>
                {
                    // NPCID.Count + 3, // 副队长
                },
                // 商队管事 → 商队弟子接替
                [FactionRole.Merchant] = new List<int>
                {
                    // NPCID.Count + 4, // 商队弟子
                },
                // 族长（不可继承，空缺时家族功能停摆）
                [FactionRole.ClanLeader] = new List<int>(),
            }
            // 其他家族 P1 再填充
        };

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            CurrentOfficeHolders.Clear();
            VacancyLogs.Clear();
            InitializeDefaultOfficeHolders();
        }

        /// <summary>
        /// 初始化默认职务在位者。
        /// </summary>
        private void InitializeDefaultOfficeHolders()
        {
            foreach (var (faction, roles) in SuccessionChains)
            {
                if (!CurrentOfficeHolders.ContainsKey(faction))
                    CurrentOfficeHolders[faction] = new Dictionary<FactionRole, int>();

                foreach (var (role, successors) in roles)
                {
                    // 默认：顺位第一的在位
                    if (successors.Count > 0)
                    {
                        CurrentOfficeHolders[faction][role] = successors[0];
                    }
                }
            }
        }

        // ============================================================
        // 事件订阅
        // ============================================================

        private bool _isSubscribed = false;

        public override void PostUpdateWorld()
        {
            if (!_isSubscribed)
            {
                EventBus.Subscribe<RoleVacancyEvent>(OnRoleVacancy);
                _isSubscribed = true;
            }
        }

        // ============================================================
        // 事件处理
        // ============================================================

        /// <summary>
        /// 处理职务空缺事件。
        /// 按硬编码继承顺位自动补位。
        /// </summary>
        public void OnRoleVacancy(RoleVacancyEvent evt)
        {
            // 记录空缺日志
            VacancyLogs.Add(new RoleVacancyLog
            {
                Faction = evt.Faction,
                Role = evt.VacatedRole,
                Tick = (int)Main.GameUpdateCount,
                DeceasedNPCType = evt.DeceasedNPCType
            });

            // 查找继承顺位
            int successorType = GetSuccessor(evt.Faction, evt.VacatedRole);

            if (successorType > 0)
            {
                // 更新在位者
                if (!CurrentOfficeHolders.ContainsKey(evt.Faction))
                    CurrentOfficeHolders[evt.Faction] = new Dictionary<FactionRole, int>();
                CurrentOfficeHolders[evt.Faction][evt.VacatedRole] = successorType;

                // 聊天栏提示
                string factionName = GuWorldSystem.GetFactionDisplayName(evt.Faction);
                string roleName = GetRoleDisplayName(evt.VacatedRole);
                string successorName = GetNPCTypeDisplayName(successorType);

                Main.NewText(
                    $"{factionName}的{roleName}已由{successorName}接任。",
                    Color.Yellow);
            }
            else
            {
                // 无继任者：家族功能停摆
                string factionName = GuWorldSystem.GetFactionDisplayName(evt.Faction);
                string roleName = GetRoleDisplayName(evt.VacatedRole);

                Main.NewText(
                    $"{factionName}的{roleName}空缺，相关功能暂时不可用。",
                    Color.Gray);

                // 标记职务为空
                if (CurrentOfficeHolders.ContainsKey(evt.Faction))
                {
                    CurrentOfficeHolders[evt.Faction][evt.VacatedRole] = -1;
                }
            }
        }

        // ============================================================
        // 继承顺位查询
        // ============================================================

        /// <summary>
        /// 获取指定职务的继任者 NPC Type。
        /// </summary>
        public int GetSuccessor(FactionID faction, FactionRole role)
        {
            if (!SuccessionChains.TryGetValue(faction, out var roles))
                return -1;

            if (!roles.TryGetValue(role, out var successors))
                return -1;

            if (successors.Count == 0)
                return -1;

            // 获取当前在位者
            int currentHolder = -1;
            if (CurrentOfficeHolders.TryGetValue(faction, out var holders))
            {
                holders.TryGetValue(role, out currentHolder);
            }

            // 查找下一个不在位的继任者
            foreach (int successor in successors)
            {
                if (successor != currentHolder)
                    return successor;
            }

            // 所有继任者都已使用 → 返回最后一个（循环继承）
            return successors[^1];
        }

        /// <summary>
        /// 获取当前职务在位者。
        /// </summary>
        public int GetCurrentOfficeHolder(FactionID faction, FactionRole role)
        {
            if (CurrentOfficeHolders.TryGetValue(faction, out var holders))
            {
                if (holders.TryGetValue(role, out int holder))
                    return holder;
            }
            return -1;
        }

        /// <summary>
        /// 检查职务是否空缺。
        /// </summary>
        public bool IsRoleVacant(FactionID faction, FactionRole role)
        {
            return GetCurrentOfficeHolder(faction, role) == -1;
        }

        // ============================================================
        // 工具方法
        // ============================================================

        /// <summary>
        /// 获取职务的中文显示名称。
        /// </summary>
        public static string GetRoleDisplayName(FactionRole role)
        {
            return role switch
            {
                FactionRole.None => "无",
                FactionRole.ClanLeader => "族长",
                FactionRole.Elder => "家老",
                FactionRole.GuardCaptain => "巡逻队长",
                FactionRole.Merchant => "商队管事",
                FactionRole.Scout => "斥候",
                FactionRole.Healer => "药堂家老",
                FactionRole.Trainer => "学堂家老",
                _ => "未知职务"
            };
        }

        /// <summary>
        /// 获取 NPC Type 的显示名称。
        /// </summary>
        private static string GetNPCTypeDisplayName(int npcType)
        {
            if (npcType > 0 && npcType < Terraria.ID.NPCID.Count)
            {
                return Lang.GetNPCNameValue(npcType);
            }
            return "继任者";
        }

        /// <summary>
        /// 获取某家族的所有在位职务摘要。
        /// </summary>
        public string GetFactionPowerSummary(FactionID faction)
        {
            if (!CurrentOfficeHolders.TryGetValue(faction, out var holders))
                return "无数据";

            var lines = new List<string>();
            foreach (var (role, holder) in holders)
            {
                string roleName = GetRoleDisplayName(role);
                string status = holder == -1 ? "（空缺）" : $"（在位）";
                lines.Add($"  {roleName}{status}");
            }

            return $"{GuWorldSystem.GetFactionDisplayName(faction)}权力结构：\n" + string.Join("\n", lines);
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var holderData = new List<TagCompound>();
            foreach (var (faction, roles) in CurrentOfficeHolders)
            {
                foreach (var (role, holder) in roles)
                {
                    holderData.Add(new TagCompound
                    {
                        ["faction"] = faction.ToString(),
                        ["role"] = (int)role,
                        ["holder"] = holder
                    });
                }
            }
            tag["officeHolders"] = holderData;

            var logData = new List<TagCompound>();
            foreach (var log in VacancyLogs)
            {
                logData.Add(new TagCompound
                {
                    ["faction"] = log.Faction.ToString(),
                    ["role"] = (int)log.Role,
                    ["tick"] = log.Tick,
                    ["deceased"] = log.DeceasedNPCType
                });
            }
            tag["vacancyLogs"] = logData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            CurrentOfficeHolders.Clear();
            VacancyLogs.Clear();

            if (tag.TryGet("officeHolders", out List<TagCompound> holderData))
            {
                foreach (var entry in holderData)
                {
                    if (System.Enum.TryParse<FactionID>(entry.GetString("faction"), out var faction))
                    {
                        if (!CurrentOfficeHolders.ContainsKey(faction))
                            CurrentOfficeHolders[faction] = new Dictionary<FactionRole, int>();

                        var role = (FactionRole)entry.GetInt("role");
                        var holder = entry.GetInt("holder");
                        CurrentOfficeHolders[faction][role] = holder;
                    }
                }
            }
            else
            {
                InitializeDefaultOfficeHolders();
            }

            if (tag.TryGet("vacancyLogs", out List<TagCompound> logData))
            {
                foreach (var entry in logData)
                {
                    if (System.Enum.TryParse<FactionID>(entry.GetString("faction"), out var faction))
                    {
                        VacancyLogs.Add(new RoleVacancyLog
                        {
                            Faction = faction,
                            Role = (FactionRole)entry.GetInt("role"),
                            Tick = entry.GetInt("tick"),
                            DeceasedNPCType = entry.GetInt("deceased")
                        });
                    }
                }
            }
        }
    }

    // ============================================================
    // 辅助数据结构
    // ============================================================

    /// <summary>
    /// 职务空缺日志条目。
    /// </summary>
    public class RoleVacancyLog
    {
        public FactionID Faction;
        public FactionRole Role;
        public int Tick;
        public int DeceasedNPCType;
    }
}
