using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    public enum TerritoryType
    {
        Village,            // 村落 — 凡人聚居，低级资源
        Town,               // 城镇 — 商业中心，中级资源
        GuMasterSettlement, // 蛊师聚落 — 蛊师聚集地
        ResourceZone,       // 资源区 — 矿脉/药园/灵泉
        Wilderness,         // 荒野 — 无主之地，危险
        ForbiddenZone,      // 禁区 — 高危区域，稀有资源
        InheritanceGround,  // 传承地 — 秘境入口
    }

    public enum TerritoryStatus
    {
        Unclaimed,          // 无主
        Claimed,            // 已占领
        Contested,          // 争夺中
        UnderAttack,        // 遭受攻击
        Occupied,           // 被敌对势力占领
        Abandoned,          // 废弃
    }

    public class TerritoryNode
    {
        public string TerritoryID;
        public string DisplayName;
        public TerritoryType Type;
        public TerritoryStatus Status;
        public FactionID OwnerFaction;
        public FactionID AttackerFaction;
        public Rectangle Bounds;
        public Vector2 Center;
        public int DefenseLevel;
        public int MaxDefense;
        public float DefensePercent => MaxDefense > 0 ? (float)DefenseLevel / MaxDefense : 0f;
        public List<string> AdjacentTerritories = new();
        public Dictionary<NodeResourceType, int> Resources = new();
        public int ContestTimer;
        public int AttackCooldown;
    }

    public class TerritoryControlSystem : ModSystem
    {
        public static TerritoryControlSystem Instance => ModContent.GetInstance<TerritoryControlSystem>();

        public Dictionary<string, TerritoryNode> Territories = new();
        public Dictionary<FactionID, List<string>> FactionTerritories = new();

        public override void OnWorldLoad()
        {
            Territories.Clear();
            FactionTerritories.Clear();
            InitializeDefaultTerritories();
        }

        private void InitializeDefaultTerritories()
        {
            // TODO: 根据世界生成结果动态创建领地节点
            // MVA阶段：硬编码古月族地及周边区域

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "guyue_core",
                DisplayName = "古月族地",
                Type = TerritoryType.GuMasterSettlement,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.GuYue,
                DefenseLevel = 100,
                MaxDefense = 100,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "guyue_village",
                DisplayName = "古月外村",
                Type = TerritoryType.Village,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.GuYue,
                DefenseLevel = 50,
                MaxDefense = 50,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "qingmao_forest",
                DisplayName = "青茅山",
                Type = TerritoryType.ResourceZone,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.GuYue,
                DefenseLevel = 30,
                MaxDefense = 30,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "wilderness_north",
                DisplayName = "北荒",
                Type = TerritoryType.Wilderness,
                Status = TerritoryStatus.Unclaimed,
                OwnerFaction = FactionID.None,
                DefenseLevel = 0,
                MaxDefense = 0,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "bai_territory",
                DisplayName = "白家领地",
                Type = TerritoryType.GuMasterSettlement,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.Bai,
                DefenseLevel = 80,
                MaxDefense = 80,
            });

            // 设置邻接关系
            SetAdjacent("guyue_core", "guyue_village");
            SetAdjacent("guyue_village", "qingmao_forest");
            SetAdjacent("qingmao_forest", "wilderness_north");
            SetAdjacent("wilderness_north", "bai_territory");
        }

        private void RegisterTerritory(TerritoryNode node)
        {
            Territories[node.TerritoryID] = node;
            if (!FactionTerritories.ContainsKey(node.OwnerFaction))
                FactionTerritories[node.OwnerFaction] = new List<string>();
            FactionTerritories[node.OwnerFaction].Add(node.TerritoryID);
        }

        private void SetAdjacent(string a, string b)
        {
            if (Territories.TryGetValue(a, out var nodeA))
                nodeA.AdjacentTerritories.Add(b);
            if (Territories.TryGetValue(b, out var nodeB))
                nodeB.AdjacentTerritories.Add(a);
        }

        public TerritoryNode GetTerritory(string id)
        {
            Territories.TryGetValue(id, out var node);
            return node;
        }

        public TerritoryNode GetTerritoryAt(Vector2 worldPos)
        {
            foreach (var kvp in Territories)
            {
                if (kvp.Value.Bounds.Contains((int)worldPos.X, (int)worldPos.Y))
                    return kvp.Value;
            }
            return null;
        }

        public IReadOnlyList<TerritoryNode> GetFactionTerritoriesList(FactionID faction)
        {
            var result = new List<TerritoryNode>();
            if (FactionTerritories.TryGetValue(faction, out var ids))
            {
                foreach (var id in ids)
                {
                    if (Territories.TryGetValue(id, out var node))
                        result.Add(node);
                }
            }
            return result;
        }

        public void StartContest(string territoryID, FactionID attacker)
        {
            if (!Territories.TryGetValue(territoryID, out var territory)) return;
            if (territory.Status != TerritoryStatus.Claimed) return;
            if (territory.OwnerFaction == attacker) return;

            territory.Status = TerritoryStatus.Contested;
            territory.AttackerFaction = attacker;
            territory.ContestTimer = 0;

            EventBus.Publish(new TerritoryContestedEvent
            {
                TerritoryID = territoryID,
                AttackerFaction = attacker,
                DefenderFaction = territory.OwnerFaction,
            });
        }

        public void CaptureTerritory(string territoryID, FactionID newOwner)
        {
            if (!Territories.TryGetValue(territoryID, out var territory)) return;

            var oldOwner = territory.OwnerFaction;
            if (FactionTerritories.TryGetValue(oldOwner, out var oldList))
                oldList.Remove(territoryID);

            territory.OwnerFaction = newOwner;
            territory.Status = TerritoryStatus.Claimed;
            territory.AttackerFaction = FactionID.None;
            territory.DefenseLevel = territory.MaxDefense / 2;

            if (!FactionTerritories.ContainsKey(newOwner))
                FactionTerritories[newOwner] = new List<string>();
            FactionTerritories[newOwner].Add(territoryID);

            EventBus.Publish(new TerritoryCapturedEvent
            {
                TerritoryID = territoryID,
                NewOwnerFaction = newOwner,
                OldOwnerFaction = oldOwner,
            });
        }

        public override void PostUpdateWorld()
        {
            // TODO: 领地争夺进度推进
            // TODO: 防御恢复
            // TODO: 资源产出
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存领地数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载领地数据
        }
    }

    public class TerritoryContestedEvent : GuWorldEvent
    {
        public string TerritoryID;
        public FactionID AttackerFaction;
        public FactionID DefenderFaction;
    }

    public class TerritoryCapturedEvent : GuWorldEvent
    {
        public string TerritoryID;
        public FactionID NewOwnerFaction;
        public FactionID OldOwnerFaction;
    }
}
