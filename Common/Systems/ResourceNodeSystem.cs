using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// ResourceNodeSystem — 有限资源点系统（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 管理世界中的资源点（元泉、蛊虫矿脉、药园、魂石矿）
    /// 2. 资源点具有有限储量，枯竭后按周期恢复
    /// 3. NPC 与玩家竞争资源点
    /// 
    /// MVA 阶段：
    /// - 只实现 1 个资源点类型「元泉」（YuanSpring）
    /// - 每周刷新（7 天 = 42000 帧），恢复一半储量
    /// - 无 NPC 竞争逻辑（P1 扩展）
    /// - 资源点位置硬编码在 GuYue 领地附近
    /// 
    /// 依赖：
    /// - EventBus（发布 ResourceDepletedEvent）
    /// - YuanS（元石物品）
    /// - GuWorldSystem（势力控制）
    /// </summary>
    public class ResourceNodeSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static ResourceNodeSystem Instance => ModContent.GetInstance<ResourceNodeSystem>();

        // ===== 配置常量 =====
        /// <summary> 元泉恢复周期（7 天 = 42000 帧） </summary>
        public const int REGEN_INTERVAL = 42000;

        /// <summary> 元泉恢复比例 </summary>
        public const float REGEN_RATIO = 0.5f;

        /// <summary> 元泉最大储量 </summary>
        public const int YUAN_SPRING_MAX = 50;

        /// <summary> 元泉初始储量 </summary>
        public const int YUAN_SPRING_INITIAL = 30;

        // ===== 运行时数据 =====
        /// <summary> 活跃资源点列表 </summary>
        public List<ResourceNode> ActiveNodes = new();

        // ============================================================
        // 资源点数据结构
        // ============================================================

        /// <summary> 单个资源点 </summary>
        public class ResourceNode
        {
            /// <summary> 世界坐标 </summary>
            public Vector2 Position;

            /// <summary> 资源类型 </summary>
            public ResourceType Type;

            /// <summary> 当前储量 </summary>
            public int CurrentAmount;

            /// <summary> 最大储量 </summary>
            public int MaxAmount;

            /// <summary> 恢复计时器（帧数） </summary>
            public int RegenTimer;

            /// <summary> 控制势力 </summary>
            public FactionID ControllingFaction;

            /// <summary> 是否枯竭 </summary>
            public bool IsDepleted => CurrentAmount <= 0;

            /// <summary> 唯一标识 </summary>
            public int NodeID;

            /// <summary> 显示名称 </summary>
            public string DisplayName;
        }

        // ============================================================
        // 初始化
        // ============================================================

        public override void OnWorldLoad()
        {
            ActiveNodes.Clear();
            InitializeDefaultNodes();
        }

        /// <summary>
        /// 初始化默认资源点。
        /// MVA 阶段：在古月领地附近生成 1 个元泉。
        /// </summary>
        private void InitializeDefaultNodes()
        {
            // 古月山寨中心坐标附近（硬编码，P1 改为配置）
            AddNode(new ResourceNode
            {
                Position = new Vector2(Main.maxTilesX * 8f, Main.maxTilesY * 8f), // 世界中心附近
                Type = ResourceType.SpiritWell, // MVA: SpiritWell 作为元泉占位
                CurrentAmount = YUAN_SPRING_INITIAL,
                MaxAmount = YUAN_SPRING_MAX,
                RegenTimer = 0,
                ControllingFaction = FactionID.GuYue,
                NodeID = 1,
                DisplayName = "古月元泉"
            });
        }

        /// <summary>
        /// 添加资源点。
        /// </summary>
        public void AddNode(ResourceNode node)
        {
            ActiveNodes.Add(node);
        }

        // ============================================================
        // 世界更新
        // ============================================================

        public override void PreUpdateWorld()
        {
            foreach (var node in ActiveNodes)
            {
                if (node.IsDepleted)
                {
                    node.RegenTimer++;
                    if (node.RegenTimer >= REGEN_INTERVAL)
                    {
                        // 恢复一半储量
                        node.CurrentAmount = (int)(node.MaxAmount * REGEN_RATIO);
                        node.RegenTimer = 0;

                        // 发布恢复事件
                        EventBus.Publish(new ResourceDepletedEvent
                        {
                            Type = node.Type,
                            Position = node.Position,
                            ControllingFaction = node.ControllingFaction
                        });

                        // 通知玩家
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            Main.NewText($"{node.DisplayName}已恢复部分元石。", Color.Cyan);
                        }
                    }
                }
            }
        }

        // ============================================================
        // 采集接口
        // ============================================================

        /// <summary>
        /// 玩家采集资源点。
        /// </summary>
        public bool Harvest(Player player, ResourceNode node, int amount)
        {
            if (node.IsDepleted)
            {
                Main.NewText($"{node.DisplayName}已经枯竭了。", Color.Gray);
                return false;
            }

            int actual = System.Math.Min(amount, node.CurrentAmount);
            node.CurrentAmount -= actual;

            // 给予玩家元石
            for (int i = 0; i < actual; i++)
            {
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<YuanS>(), 1);
            }

            if (node.IsDepleted)
            {
                Main.NewText($"{node.DisplayName}枯竭了。需要等待一段时间才能恢复。", Color.Gray);
            }
            else
            {
                Main.NewText($"采集了 {actual} 块元石。{node.DisplayName}剩余 {node.CurrentAmount}/{node.MaxAmount}", Color.Cyan);
            }

            return true;
        }

        /// <summary>
        /// 查找玩家附近的资源点。
        /// </summary>
        public ResourceNode FindNearestNode(Player player, float maxDistance = 200f)
        {
            ResourceNode nearest = null;
            float minDist = maxDistance;

            foreach (var node in ActiveNodes)
            {
                float dist = Vector2.Distance(player.Center, node.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = node;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 获取指定位置的资源点。
        /// </summary>
        public ResourceNode GetNodeAt(Vector2 position, float radius = 60f)
        {
            foreach (var node in ActiveNodes)
            {
                if (Vector2.Distance(position, node.Position) < radius)
                    return node;
            }
            return null;
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var nodeData = new List<TagCompound>();
            foreach (var node in ActiveNodes)
            {
                nodeData.Add(new TagCompound
                {
                    ["posX"] = node.Position.X,
                    ["posY"] = node.Position.Y,
                    ["type"] = (int)node.Type,
                    ["current"] = node.CurrentAmount,
                    ["max"] = node.MaxAmount,
                    ["regenTimer"] = node.RegenTimer,
                    ["faction"] = node.ControllingFaction.ToString(),
                    ["nodeID"] = node.NodeID,
                    ["displayName"] = node.DisplayName
                });
            }
            tag["resourceNodes"] = nodeData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveNodes.Clear();
            if (tag.TryGet("resourceNodes", out List<TagCompound> nodeData))
            {
                foreach (var entry in nodeData)
                {
                    var node = new ResourceNode
                    {
                        Position = new Vector2(entry.GetFloat("posX"), entry.GetFloat("posY")),
                        Type = (ResourceType)entry.GetInt("type"),
                        CurrentAmount = entry.GetInt("current"),
                        MaxAmount = entry.GetInt("max"),
                        RegenTimer = entry.GetInt("regenTimer"),
                        NodeID = entry.GetInt("nodeID"),
                        DisplayName = entry.GetString("displayName")
                    };
                    if (System.Enum.TryParse<FactionID>(entry.GetString("faction"), out var fid))
                        node.ControllingFaction = fid;

                    ActiveNodes.Add(node);
                }
            }
            else
            {
                // 首次加载或数据丢失时初始化默认节点
                InitializeDefaultNodes();
            }
        }
    }

}
