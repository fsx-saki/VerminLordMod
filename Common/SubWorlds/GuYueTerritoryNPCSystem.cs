using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Content.NPCs.GuYue;

namespace VerminLordMod.Common.SubWorlds
{
    /// <summary>
    /// GuYueTerritoryNPCSystem — 古月山寨小世界NPC管理系统
    /// 
    /// 职责：
    /// 1. 进入小世界时按比例自动生成古月家族NPC
    /// 2. 退出小世界时保存NPC位置和状态
    /// 3. 再次进入时恢复NPC
    /// 4. 根据剧情阶段动态调整NPC生成和对话
    /// 
    /// NPC生成比例（以50个NPC为基准）：
    /// - 族长 × 1
    /// - 学堂家老 × 1
    /// - 药堂家老 × 1
    /// - 御堂家老 × 1
    /// - 赤脉家老 × 1
    /// - 漠脉家老 × 1
    /// - 药脉家老 × 1
    /// - 二转蛊师 × 5
    /// - 一转蛊师 × 12
    /// - 拳脚教头 × 3
    /// - 杂役 × 10
    /// - 凡人 × 14
    /// 
    /// 剧情阶段影响：
    /// - Arrival: 只生成守门蛊师和少量凡人，山寨大门紧闭
    /// - SchoolTraining: 解锁学堂区域，生成教头和学员
    /// - MedicineRequest: 解锁药堂区域，生成药老和药童
    /// - FamilyRecognition: 解锁议事厅，族长出现
    /// - Crisis: 生成更多御堂守卫，部分NPC转为战斗状态
    /// - Finale: 所有NPC回归和平状态
    /// </summary>
    public class GuYueTerritoryNPCSystem : ModSystem
    {
        /// <summary> 保存的NPC数据列表 </summary>
        public static List<SavedNPCData> SavedNPCs = new();

        /// <summary> 是否已初始化（防止重复生成） </summary>
        public static bool HasInitialized = false;

        /// <summary> 各NPC类型的生成权重 </summary>
        private static readonly Dictionary<GuYueNPCType, int> SpawnWeights = new()
        {
            { GuYueNPCType.Chief, 1 },
            { GuYueNPCType.SchoolElder, 1 },
            { GuYueNPCType.MedicineElder, 1 },
            { GuYueNPCType.DefenseElder, 1 },
            { GuYueNPCType.ChiElder, 1 },
            { GuYueNPCType.MoElder, 1 },
            { GuYueNPCType.MedicinePulseElder, 1 },
            { GuYueNPCType.SecondTurnGuMaster, 5 },
            { GuYueNPCType.FirstTurnGuMaster, 12 },
            { GuYueNPCType.FistInstructor, 3 },
            { GuYueNPCType.Servant, 10 },
            { GuYueNPCType.Commoner, 14 },
        };

        /// <summary>
        /// 各剧情阶段对应的NPC生成权重调整。
        /// 某些NPC在特定阶段不生成（权重为0），
        /// 某些NPC在特定阶段生成更多。
        /// </summary>
        private static readonly Dictionary<StoryPhase, Dictionary<GuYueNPCType, int>> PhaseSpawnWeights = new()
        {
            [StoryPhase.Arrival] = new()
            {
                { GuYueNPCType.Chief, 0 },
                { GuYueNPCType.SchoolElder, 0 },
                { GuYueNPCType.MedicineElder, 0 },
                { GuYueNPCType.DefenseElder, 0 },
                { GuYueNPCType.ChiElder, 0 },
                { GuYueNPCType.MoElder, 0 },
                { GuYueNPCType.MedicinePulseElder, 0 },
                { GuYueNPCType.SecondTurnGuMaster, 1 },
                { GuYueNPCType.FirstTurnGuMaster, 2 },
                { GuYueNPCType.FistInstructor, 0 },
                { GuYueNPCType.Servant, 3 },
                { GuYueNPCType.Commoner, 5 },
            },
            [StoryPhase.SchoolTraining] = new()
            {
                { GuYueNPCType.Chief, 0 },
                { GuYueNPCType.SchoolElder, 1 },
                { GuYueNPCType.MedicineElder, 0 },
                { GuYueNPCType.DefenseElder, 0 },
                { GuYueNPCType.ChiElder, 0 },
                { GuYueNPCType.MoElder, 0 },
                { GuYueNPCType.MedicinePulseElder, 0 },
                { GuYueNPCType.SecondTurnGuMaster, 3 },
                { GuYueNPCType.FirstTurnGuMaster, 8 },
                { GuYueNPCType.FistInstructor, 3 },
                { GuYueNPCType.Servant, 5 },
                { GuYueNPCType.Commoner, 8 },
            },
            [StoryPhase.MedicineRequest] = new()
            {
                { GuYueNPCType.Chief, 0 },
                { GuYueNPCType.SchoolElder, 1 },
                { GuYueNPCType.MedicineElder, 1 },
                { GuYueNPCType.DefenseElder, 0 },
                { GuYueNPCType.ChiElder, 1 },
                { GuYueNPCType.MoElder, 1 },
                { GuYueNPCType.MedicinePulseElder, 1 },
                { GuYueNPCType.SecondTurnGuMaster, 4 },
                { GuYueNPCType.FirstTurnGuMaster, 10 },
                { GuYueNPCType.FistInstructor, 3 },
                { GuYueNPCType.Servant, 8 },
                { GuYueNPCType.Commoner, 12 },
            },
            [StoryPhase.FamilyRecognition] = new()
            {
                { GuYueNPCType.Chief, 1 },
                { GuYueNPCType.SchoolElder, 1 },
                { GuYueNPCType.MedicineElder, 1 },
                { GuYueNPCType.DefenseElder, 1 },
                { GuYueNPCType.ChiElder, 1 },
                { GuYueNPCType.MoElder, 1 },
                { GuYueNPCType.MedicinePulseElder, 1 },
                { GuYueNPCType.SecondTurnGuMaster, 5 },
                { GuYueNPCType.FirstTurnGuMaster, 12 },
                { GuYueNPCType.FistInstructor, 3 },
                { GuYueNPCType.Servant, 10 },
                { GuYueNPCType.Commoner, 14 },
            },
            [StoryPhase.Crisis] = new()
            {
                { GuYueNPCType.Chief, 1 },
                { GuYueNPCType.SchoolElder, 1 },
                { GuYueNPCType.MedicineElder, 1 },
                { GuYueNPCType.DefenseElder, 2 },
                { GuYueNPCType.ChiElder, 1 },
                { GuYueNPCType.MoElder, 1 },
                { GuYueNPCType.MedicinePulseElder, 1 },
                { GuYueNPCType.SecondTurnGuMaster, 8 },
                { GuYueNPCType.FirstTurnGuMaster, 15 },
                { GuYueNPCType.FistInstructor, 5 },
                { GuYueNPCType.Servant, 5 },
                { GuYueNPCType.Commoner, 5 },
            },
            [StoryPhase.Finale] = new()
            {
                { GuYueNPCType.Chief, 1 },
                { GuYueNPCType.SchoolElder, 1 },
                { GuYueNPCType.MedicineElder, 1 },
                { GuYueNPCType.DefenseElder, 1 },
                { GuYueNPCType.ChiElder, 1 },
                { GuYueNPCType.MoElder, 1 },
                { GuYueNPCType.MedicinePulseElder, 1 },
                { GuYueNPCType.SecondTurnGuMaster, 5 },
                { GuYueNPCType.FirstTurnGuMaster, 12 },
                { GuYueNPCType.FistInstructor, 3 },
                { GuYueNPCType.Servant, 12 },
                { GuYueNPCType.Commoner, 16 },
            },
        };

        /// <summary>
        /// NPC类型到具体ModNPC类型的映射
        /// 所有古月家族NPC统一使用 GuYueVillager 类，
        /// 通过 SetNPCType 方法区分身份。
        /// </summary>
        private static int GetNPCTypeId(GuYueNPCType type) => ModContent.NPCType<GuYueVillager>();

        /// <summary>
        /// 创建指定类型的古月家族NPC并设置其身份
        /// </summary>
        private static int CreateGuYueNPC(GuYueNPCType type, Vector2 position)
        {
            int npcTypeId = ModContent.NPCType<GuYueVillager>();
            int npcIdx = NPC.NewNPC(null, (int)position.X, (int)position.Y, npcTypeId);
            if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
            {
                var npc = Main.npc[npcIdx];
                if (npc.ModNPC is GuYueVillager villager)
                {
                    villager.SetNPCType(type);
                }
            }
            return npcIdx;
        }

        /// <summary>
        /// 在小世界中生成所有NPC（根据当前剧情阶段调整）
        /// </summary>
        public static void SpawnAllNPCs()
        {
            if (!SubworldSystem.IsActive<GuYueTerritory>()) return;

            // 如果已有保存的NPC数据，恢复它们
            if (SavedNPCs.Count > 0)
            {
                RestoreSavedNPCs();
                return;
            }

            // 获取当前玩家的剧情阶段
            StoryPhase currentPhase = StoryPhase.Arrival;
            Player player = Main.LocalPlayer;
            if (player != null && player.active)
            {
                currentPhase = StoryManager.Instance.GetPhase(player);
            }

            SpawnPhaseNPCs(currentPhase);
            HasInitialized = true;
        }

        /// <summary>
        /// 根据剧情阶段生成对应的NPC
        /// </summary>
        public static void SpawnPhaseNPCs(StoryPhase phase)
        {
            if (!SubworldSystem.IsActive<GuYueTerritory>()) return;

            int groundLevel = Main.maxTilesY / 2 + 20;
            int centerX = Main.maxTilesX / 2;

            // 获取当前阶段的权重配置
            var weights = GetWeightsForPhase(phase);

            // 定义各区域的NPC生成位置（所有阶段都生成固定位置的重要NPC）
            var spawnZones = new Dictionary<GuYueNPCType, Vector2[]>
            {
                [GuYueNPCType.Chief] = new[] { new Vector2(centerX, groundLevel - 14) },
                [GuYueNPCType.SchoolElder] = new[] { new Vector2(centerX - 60, groundLevel - 8) },
                [GuYueNPCType.MedicineElder] = new[] { new Vector2(centerX + 55, groundLevel - 8) },
                [GuYueNPCType.DefenseElder] = new[] { new Vector2(centerX + 80, groundLevel - 8) },
                [GuYueNPCType.ChiElder] = new[] { new Vector2(centerX - 80, groundLevel - 8) },
                [GuYueNPCType.MoElder] = new[] { new Vector2(centerX + 30, groundLevel - 8) },
                [GuYueNPCType.MedicinePulseElder] = new[] { new Vector2(centerX + 45, groundLevel - 8) },
            };

            // 生成固定位置的重要NPC（只生成权重 > 0 的）
            foreach (var (type, positions) in spawnZones)
            {
                if (!weights.TryGetValue(type, out int weight) || weight <= 0)
                    continue;

                foreach (var pos in positions)
                {
                    int npcIdx = CreateGuYueNPC(type, new Vector2(pos.X * 16, pos.Y * 16));
                    if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
                    {
                        var npc = Main.npc[npcIdx];
                        npc.homeTileX = (int)pos.X;
                        npc.homeTileY = (int)pos.Y;
                    }
                }
            }

            // 生成普通NPC（按权重分布在居住区和公共区域）
            var commonTypes = new[]
            {
                GuYueNPCType.SecondTurnGuMaster,
                GuYueNPCType.FirstTurnGuMaster,
                GuYueNPCType.FistInstructor,
                GuYueNPCType.Servant,
                GuYueNPCType.Commoner,
            };

            foreach (var type in commonTypes)
            {
                if (!weights.TryGetValue(type, out int count) || count <= 0)
                    continue;

                for (int i = 0; i < count; i++)
                {
                    Vector2 spawnPos = GetRandomSpawnPosition(centerX, groundLevel);
                    int npcIdx = CreateGuYueNPC(type, new Vector2(spawnPos.X * 16, spawnPos.Y * 16));
                    if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
                    {
                        var npc = Main.npc[npcIdx];
                        npc.homeTileX = (int)spawnPos.X;
                        npc.homeTileY = (int)spawnPos.Y;
                    }
                }
            }
        }

        /// <summary>
        /// 当剧情阶段推进时，刷新NPC配置。
        /// 清除旧NPC，按新阶段重新生成。
        /// </summary>
        public static void OnPhaseAdvance(StoryPhase newPhase)
        {
            if (!SubworldSystem.IsActive<GuYueTerritory>()) return;

            // 清除所有现有NPC
            ClearAllNPCs();

            // 按新阶段重新生成
            SpawnPhaseNPCs(newPhase);

            // 清空保存数据，下次进入时重新生成
            SavedNPCs.Clear();
            HasInitialized = false;

            Main.NewText($"[古月山寨] 山寨的氛围发生了变化……", Microsoft.Xna.Framework.Color.Gold);
        }

        /// <summary>
        /// 获取指定阶段的NPC生成权重
        /// </summary>
        private static Dictionary<GuYueNPCType, int> GetWeightsForPhase(StoryPhase phase)
        {
            if (PhaseSpawnWeights.TryGetValue(phase, out var phaseWeights))
                return phaseWeights;

            // 默认使用完整权重
            return new Dictionary<GuYueNPCType, int>(SpawnWeights);
        }

        /// <summary>
        /// 获取随机生成位置（在可居住区域内）
        /// </summary>
        private static Vector2 GetRandomSpawnPosition(int centerX, int groundLevel)
        {
            // 在 -100 到 +100 范围内随机X，在地面附近随机Y
            int x = centerX + Main.rand.Next(-100, 101);
            int y = groundLevel - Main.rand.Next(5, 15);
            return new Vector2(x, y);
        }

        /// <summary>
        /// 保存当前小世界中所有古月NPC的状态
        /// </summary>
        public static void SaveCurrentNPCs()
        {
            SavedNPCs.Clear();

            if (!SubworldSystem.IsActive<GuYueTerritory>()) return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (!npc.active) continue;

                // 只保存古月家族的NPC
                if (npc.ModNPC is GuYueVillager guYueVillager)
                {
                    SavedNPCs.Add(new SavedNPCData
                    {
                        NPCType = guYueVillager.NPCType,
                        PositionX = npc.position.X,
                        PositionY = npc.position.Y,
                        HomeTileX = npc.homeTileX,
                        HomeTileY = npc.homeTileY,
                        Life = npc.life,
                        DisplayName = npc.GivenName,
                    });
                }
            }
        }

        /// <summary>
        /// 恢复保存的NPC
        /// </summary>
        private static void RestoreSavedNPCs()
        {
            foreach (var data in SavedNPCs)
            {
                int npcIdx = CreateGuYueNPC(data.NPCType, new Vector2(data.PositionX, data.PositionY));
                if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
                {
                    var npc = Main.npc[npcIdx];
                    npc.homeTileX = data.HomeTileX;
                    npc.homeTileY = data.HomeTileY;
                    npc.life = data.Life;
                    if (!string.IsNullOrEmpty(data.DisplayName))
                    {
                        npc.GivenName = data.DisplayName;
                    }
                }
            }

            HasInitialized = true;
        }

        /// <summary>
        /// 清除小世界中所有古月NPC
        /// </summary>
        public static void ClearAllNPCs()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (npc.active && npc.ModNPC is GuYueVillager)
                {
                    npc.active = false;
                }
            }
        }

        // ============================================================
        // 持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            // 保存NPC数据
            var npcDataList = new List<TagCompound>();
            foreach (var data in SavedNPCs)
            {
                npcDataList.Add(new TagCompound
                {
                    ["npcType"] = data.NPCType.ToString(),
                    ["posX"] = data.PositionX,
                    ["posY"] = data.PositionY,
                    ["homeX"] = data.HomeTileX,
                    ["homeY"] = data.HomeTileY,
                    ["life"] = data.Life,
                    ["name"] = data.DisplayName ?? "",
                });
            }
            tag["guYueNPCs"] = npcDataList;
            tag["guYueNPCInit"] = HasInitialized;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            SavedNPCs.Clear();

            if (tag.TryGet("guYueNPCs", out List<TagCompound> npcDataList))
            {
                foreach (var entry in npcDataList)
                {
                    var typeStr = entry.GetString("npcType");
                    if (System.Enum.TryParse<GuYueNPCType>(typeStr, out var npcType))
                    {
                        SavedNPCs.Add(new SavedNPCData
                        {
                            NPCType = npcType,
                            PositionX = entry.GetFloat("posX"),
                            PositionY = entry.GetFloat("posY"),
                            HomeTileX = entry.GetInt("homeX"),
                            HomeTileY = entry.GetInt("homeY"),
                            Life = entry.GetInt("life"),
                            DisplayName = entry.GetString("name"),
                        });
                    }
                }
            }

            HasInitialized = tag.GetBool("guYueNPCInit");
        }
    }

    /// <summary>
    /// 保存的NPC数据
    /// </summary>
    public class SavedNPCData
    {
        public GuYueNPCType NPCType { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int HomeTileX { get; set; }
        public int HomeTileY { get; set; }
        public int Life { get; set; }
        public string DisplayName { get; set; }
    }
}
