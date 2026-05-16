using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    public enum FactionRole
    {
        None = 0,
        Chief,              // 族长 — 最高权力
        Elder,              // 家老 — 各脉领袖
        Deacon,             // 执事 — 管理具体事务
        PatrolLeader,       // 巡逻队长 — 安全事务
        Alchemist,          // 炼丹师 — 丹药管理
        Instructor,         // 教习 — 训练新人
        Quartermaster,      // 库管 — 物资管理
        Diplomat,           // 外交使 — 势力交涉
        Spy,                // 密探 — 情报收集
        Servant,            // 仆从 — 基础服务
    }

    public enum FactionGovernmentType
    {
        Patriarchal,        // 族长制 — 古月/白家，族长独裁
        Council,            // 长老制 — 熊家/铁家，长老合议
        Meritocracy,        // 能者制 — 百家，凭实力上位
        Commercial,         // 商会制 — 贾家，财权决定权力
        Shadow,             // 暗主制 — 赵家，幕后操控
        Military,           // 军管制 — 汪家，武力至上
    }

    public class FactionRoleData
    {
        public FactionRole Role;
        public string DisplayName;
        public int AuthorityLevel;
        public bool CanIssueQuest;
        public bool CanDeclareWar;
        public bool CanTrade;
        public bool CanDiplomacy;
        public bool CanPromote;
        public int SalaryPerDay;
        public List<FactionRole> Subordinates = new();
    }

    public class FactionPowerStructure
    {
        public FactionID Faction;
        public FactionGovernmentType GovernmentType;
        public Dictionary<FactionRole, FactionRoleData> Roles = new();
        public Dictionary<FactionRole, int> CurrentHolders = new();
        public Dictionary<FactionRole, List<int>> SuccessionLine = new();
        public int StabilityScore;
        public int InternalConflictLevel;
    }

    public class FactionPowerSystem : ModSystem
    {
        public static FactionPowerSystem Instance => ModContent.GetInstance<FactionPowerSystem>();

        public Dictionary<FactionID, FactionPowerStructure> PowerStructures = new();

        public override void OnWorldLoad()
        {
            PowerStructures.Clear();
            InitializePowerStructures();
        }

        private void InitializePowerStructures()
        {
            InitializeGuYue();
            InitializeBai();
            InitializeXiong();
            InitializeTie();
            InitializeBai2();
            InitializeJia();
            InitializeWang();
            InitializeZhao();
        }

        private void InitializeGuYue()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.GuYue,
                GovernmentType = FactionGovernmentType.Patriarchal,
                StabilityScore = 70,
                InternalConflictLevel = 10,
            };

            structure.Roles[FactionRole.Chief] = new FactionRoleData
            {
                Role = FactionRole.Chief,
                DisplayName = "族长",
                AuthorityLevel = 100,
                CanIssueQuest = true,
                CanDeclareWar = true,
                CanTrade = true,
                CanDiplomacy = true,
                CanPromote = true,
                SalaryPerDay = 50,
            };

            structure.Roles[FactionRole.Elder] = new FactionRoleData
            {
                Role = FactionRole.Elder,
                DisplayName = "家老",
                AuthorityLevel = 70,
                CanIssueQuest = true,
                CanDeclareWar = false,
                CanTrade = true,
                CanDiplomacy = true,
                CanPromote = false,
                SalaryPerDay = 30,
            };

            structure.Roles[FactionRole.Deacon] = new FactionRoleData
            {
                Role = FactionRole.Deacon,
                DisplayName = "执事",
                AuthorityLevel = 40,
                CanIssueQuest = true,
                CanDeclareWar = false,
                CanTrade = true,
                CanDiplomacy = false,
                CanPromote = false,
                SalaryPerDay = 15,
            };

            structure.Roles[FactionRole.Instructor] = new FactionRoleData
            {
                Role = FactionRole.Instructor,
                DisplayName = "教习",
                AuthorityLevel = 35,
                CanIssueQuest = true,
                CanDeclareWar = false,
                CanTrade = false,
                CanDiplomacy = false,
                CanPromote = false,
                SalaryPerDay = 10,
            };

            PowerStructures[FactionID.GuYue] = structure;
        }

        private void InitializeBai()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Bai,
                GovernmentType = FactionGovernmentType.Patriarchal,
                StabilityScore = 80,
                InternalConflictLevel = 5,
            };
            PowerStructures[FactionID.Bai] = structure;
        }

        private void InitializeXiong()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Xiong,
                GovernmentType = FactionGovernmentType.Council,
                StabilityScore = 60,
                InternalConflictLevel = 20,
            };
            PowerStructures[FactionID.Xiong] = structure;
        }

        private void InitializeTie()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Tie,
                GovernmentType = FactionGovernmentType.Council,
                StabilityScore = 65,
                InternalConflictLevel = 15,
            };
            PowerStructures[FactionID.Tie] = structure;
        }

        private void InitializeBai2()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Bai2,
                GovernmentType = FactionGovernmentType.Meritocracy,
                StabilityScore = 50,
                InternalConflictLevel = 25,
            };
            PowerStructures[FactionID.Bai2] = structure;
        }

        private void InitializeJia()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Jia,
                GovernmentType = FactionGovernmentType.Commercial,
                StabilityScore = 75,
                InternalConflictLevel = 10,
            };
            PowerStructures[FactionID.Jia] = structure;
        }

        private void InitializeWang()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Wang,
                GovernmentType = FactionGovernmentType.Military,
                StabilityScore = 55,
                InternalConflictLevel = 30,
            };
            PowerStructures[FactionID.Wang] = structure;
        }

        private void InitializeZhao()
        {
            var structure = new FactionPowerStructure
            {
                Faction = FactionID.Zhao,
                GovernmentType = FactionGovernmentType.Shadow,
                StabilityScore = 40,
                InternalConflictLevel = 35,
            };
            PowerStructures[FactionID.Zhao] = structure;
        }

        public FactionPowerStructure GetPowerStructure(FactionID faction)
        {
            PowerStructures.TryGetValue(faction, out var structure);
            return structure;
        }

        public FactionRoleData GetRoleData(FactionID faction, FactionRole role)
        {
            var structure = GetPowerStructure(faction);
            if (structure == null) return null;
            structure.Roles.TryGetValue(role, out var data);
            return data;
        }

        public bool CanNPCIssueQuest(int npcType, FactionRole role)
        {
            foreach (var kvp in PowerStructures)
            {
                if (kvp.Value.Roles.TryGetValue(role, out var data))
                    return data.CanIssueQuest;
            }
            return false;
        }

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            foreach (var kvp in PowerStructures)
            {
                var structure = kvp.Value;

                int stabilityChange = 0;
                if (structure.InternalConflictLevel > 30)
                    stabilityChange -= Main.rand.Next(1, 4);
                else if (structure.InternalConflictLevel < 10)
                    stabilityChange += Main.rand.Next(0, 2);

                structure.StabilityScore = System.Math.Clamp(
                    structure.StabilityScore + stabilityChange, 0, 100);

                if (Main.rand.NextFloat() < 0.05f)
                {
                    structure.InternalConflictLevel = System.Math.Clamp(
                        structure.InternalConflictLevel + Main.rand.Next(-5, 6), 0, 100);
                }

                if (structure.InternalConflictLevel > 50 && Main.rand.NextFloat() < 0.03f)
                {
                    TriggerInternalConflict(structure);
                }
            }
        }

        private void TriggerInternalConflict(FactionPowerStructure structure)
        {
            structure.StabilityScore = System.Math.Max(0, structure.StabilityScore - 10);
            structure.InternalConflictLevel = System.Math.Min(100, structure.InternalConflictLevel + 5);

            if (Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                string factionName = structure.Faction.ToString();
                Main.NewText($"【{factionName}】内部发生权力斗争！稳定性下降...", Microsoft.Xna.Framework.Color.Orange);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var kvp in PowerStructures)
            {
                list.Add(new TagCompound
                {
                    ["faction"] = (int)kvp.Key,
                    ["stability"] = kvp.Value.StabilityScore,
                    ["conflict"] = kvp.Value.InternalConflictLevel,
                });
            }
            tag["powerStructures"] = list;
            tag["powerDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            PowerStructures.Clear();
            InitializePowerStructures();

            var list = tag.GetList<TagCompound>("powerStructures");
            if (list == null) return;

            foreach (var t in list)
            {
                var faction = (FactionID)t.GetInt("faction");
                if (PowerStructures.TryGetValue(faction, out var structure))
                {
                    structure.StabilityScore = t.GetInt("stability");
                    structure.InternalConflictLevel = t.GetInt("conflict");
                }
            }

            _lastDay = tag.GetInt("powerDayCounter");
        }
    }
}
