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

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "xiong_territory",
                DisplayName = "熊家领地",
                Type = TerritoryType.GuMasterSettlement,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.Xiong,
                DefenseLevel = 90,
                MaxDefense = 90,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "tie_territory",
                DisplayName = "铁家领地",
                Type = TerritoryType.GuMasterSettlement,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.Tie,
                DefenseLevel = 85,
                MaxDefense = 85,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "jia_caravan_route",
                DisplayName = "贾家商路",
                Type = TerritoryType.Town,
                Status = TerritoryStatus.Claimed,
                OwnerFaction = FactionID.Jia,
                DefenseLevel = 40,
                MaxDefense = 40,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "wilderness_east",
                DisplayName = "东荒",
                Type = TerritoryType.Wilderness,
                Status = TerritoryStatus.Unclaimed,
                OwnerFaction = FactionID.None,
                DefenseLevel = 0,
                MaxDefense = 0,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "forbidden_valley",
                DisplayName = "禁断谷",
                Type = TerritoryType.ForbiddenZone,
                Status = TerritoryStatus.Unclaimed,
                OwnerFaction = FactionID.None,
                DefenseLevel = 0,
                MaxDefense = 0,
            });

            RegisterTerritory(new TerritoryNode
            {
                TerritoryID = "inheritance_peak",
                DisplayName = "传承峰",
                Type = TerritoryType.InheritanceGround,
                Status = TerritoryStatus.Unclaimed,
                OwnerFaction = FactionID.None,
                DefenseLevel = 0,
                MaxDefense = 0,
            });

            SetAdjacent("guyue_core", "guyue_village");
            SetAdjacent("guyue_village", "qingmao_forest");
            SetAdjacent("qingmao_forest", "wilderness_north");
            SetAdjacent("wilderness_north", "bai_territory");
            SetAdjacent("bai_territory", "xiong_territory");
            SetAdjacent("xiong_territory", "tie_territory");
            SetAdjacent("tie_territory", "wilderness_east");
            SetAdjacent("wilderness_east", "jia_caravan_route");
            SetAdjacent("jia_caravan_route", "guyue_village");
            SetAdjacent("wilderness_north", "forbidden_valley");
            SetAdjacent("forbidden_valley", "inheritance_peak");
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

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            foreach (var kvp in Territories)
            {
                var territory = kvp.Value;

                if (territory.Status == TerritoryStatus.Contested)
                {
                    territory.ContestTimer++;
                    if (territory.ContestTimer >= 3)
                    {
                        if (Main.rand.NextFloat() < 0.4f)
                        {
                            CaptureTerritory(territory.TerritoryID, territory.AttackerFaction);
                        }
                        else
                        {
                            territory.Status = TerritoryStatus.Claimed;
                            territory.AttackerFaction = FactionID.None;
                            territory.ContestTimer = 0;
                        }
                    }
                }

                if (territory.Status == TerritoryStatus.Claimed && territory.DefenseLevel < territory.MaxDefense)
                {
                    territory.DefenseLevel = System.Math.Min(territory.MaxDefense,
                        territory.DefenseLevel + 5);
                }

                if (territory.Status == TerritoryStatus.Claimed && territory.OwnerFaction != FactionID.None)
                {
                    ProduceResources(territory);
                }
            }
        }

        private void ProduceResources(TerritoryNode territory)
        {
            switch (territory.Type)
            {
                case TerritoryType.ResourceZone:
                    if (!territory.Resources.ContainsKey(NodeResourceType.YuanSpring))
                        territory.Resources[NodeResourceType.YuanSpring] = 0;
                    territory.Resources[NodeResourceType.YuanSpring] += Main.rand.Next(1, 5);
                    break;
                case TerritoryType.Village:
                    if (!territory.Resources.ContainsKey(NodeResourceType.HerbGarden))
                        territory.Resources[NodeResourceType.HerbGarden] = 0;
                    territory.Resources[NodeResourceType.HerbGarden] += Main.rand.Next(2, 8);
                    break;
                case TerritoryType.GuMasterSettlement:
                    if (!territory.Resources.ContainsKey(NodeResourceType.YuanSpring))
                        territory.Resources[NodeResourceType.YuanSpring] = 0;
                    territory.Resources[NodeResourceType.YuanSpring] += Main.rand.Next(2, 6);
                    break;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var kvp in Territories)
            {
                var t = kvp.Value;
                var resTag = new TagCompound();
                foreach (var r in t.Resources)
                    resTag[r.Key.ToString()] = r.Value;

                list.Add(new TagCompound
                {
                    ["id"] = t.TerritoryID,
                    ["name"] = t.DisplayName,
                    ["type"] = (int)t.Type,
                    ["status"] = (int)t.Status,
                    ["owner"] = (int)t.OwnerFaction,
                    ["attacker"] = (int)t.AttackerFaction,
                    ["defense"] = t.DefenseLevel,
                    ["maxDefense"] = t.MaxDefense,
                    ["contestTimer"] = t.ContestTimer,
                    ["resources"] = resTag,
                });
            }
            tag["territories"] = list;
            tag["territoryDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            Territories.Clear();
            FactionTerritories.Clear();

            var list = tag.GetList<TagCompound>("territories");
            if (list == null) return;

            foreach (var t in list)
            {
                var node = new TerritoryNode
                {
                    TerritoryID = t.GetString("id"),
                    DisplayName = t.GetString("name"),
                    Type = (TerritoryType)t.GetInt("type"),
                    Status = (TerritoryStatus)t.GetInt("status"),
                    OwnerFaction = (FactionID)t.GetInt("owner"),
                    AttackerFaction = (FactionID)t.GetInt("attacker"),
                    DefenseLevel = t.GetInt("defense"),
                    MaxDefense = t.GetInt("maxDefense"),
                    ContestTimer = t.GetInt("contestTimer"),
                };

                if (t.TryGet("resources", out TagCompound resTag))
                {
                    foreach (NodeResourceType resType in System.Enum.GetValues<NodeResourceType>())
                    {
                        string key = resType.ToString();
                        if (resTag.ContainsKey(key))
                            node.Resources[resType] = resTag.GetInt(key);
                    }
                }

                Territories[node.TerritoryID] = node;
                if (!FactionTerritories.ContainsKey(node.OwnerFaction))
                    FactionTerritories[node.OwnerFaction] = new List<string>();
                FactionTerritories[node.OwnerFaction].Add(node.TerritoryID);
            }

            _lastDay = tag.GetInt("territoryDayCounter");
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
