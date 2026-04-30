using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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
        /// NPC类型到具体ModNPC类型的映射
        /// </summary>
        private static int GetNPCTypeId(GuYueNPCType type) => type switch
        {
            GuYueNPCType.Chief => ModContent.NPCType<GuYueChief>(),
            GuYueNPCType.SchoolElder => ModContent.NPCType<GuYueSchoolElder>(),
            GuYueNPCType.MedicineElder => ModContent.NPCType<GuYueMedicineElder>(),
            GuYueNPCType.DefenseElder => ModContent.NPCType<GuYueDefenseElder>(),
            GuYueNPCType.ChiElder => ModContent.NPCType<GuYueChiElder>(),
            GuYueNPCType.MoElder => ModContent.NPCType<GuYueMoElder>(),
            GuYueNPCType.MedicinePulseElder => ModContent.NPCType<GuYueMedicinePulseElder>(),
            GuYueNPCType.SecondTurnGuMaster => ModContent.NPCType<GuYueSecondTurnGuMaster>(),
            GuYueNPCType.FirstTurnGuMaster => ModContent.NPCType<GuYueFirstTurnGuMaster>(),
            GuYueNPCType.FistInstructor => ModContent.NPCType<GuYueFistInstructor>(),
            GuYueNPCType.Servant => ModContent.NPCType<GuYueServant>(),
            GuYueNPCType.Commoner => ModContent.NPCType<GuYueCommoner>(),
            _ => 0,
        };

        /// <summary>
        /// 在小世界中生成所有NPC
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

            // 否则按权重生成新NPC
            int groundLevel = Main.maxTilesY / 2 + 20;
            int centerX = Main.maxTilesX / 2;

            // 定义各区域的NPC生成位置
            var spawnZones = new Dictionary<GuYueNPCType, Vector2[]>
            {
                // 族长在议事厅
                [GuYueNPCType.Chief] = new[] { new Vector2(centerX, groundLevel - 14) },
                // 学堂家老在学堂区域（左侧）
                [GuYueNPCType.SchoolElder] = new[] { new Vector2(centerX - 60, groundLevel - 8) },
                // 药堂家老在药堂区域（右侧偏内）
                [GuYueNPCType.MedicineElder] = new[] { new Vector2(centerX + 55, groundLevel - 8) },
                // 御堂家老在御堂区域（右侧）
                [GuYueNPCType.DefenseElder] = new[] { new Vector2(centerX + 80, groundLevel - 8) },
                // 赤脉家老在左侧居住区
                [GuYueNPCType.ChiElder] = new[] { new Vector2(centerX - 80, groundLevel - 8) },
                // 漠脉家老在右侧居住区
                [GuYueNPCType.MoElder] = new[] { new Vector2(centerX + 30, groundLevel - 8) },
                // 药脉家老在药堂附近
                [GuYueNPCType.MedicinePulseElder] = new[] { new Vector2(centerX + 45, groundLevel - 8) },
            };

            // 生成固定位置的重要NPC
            foreach (var (type, positions) in spawnZones)
            {
                foreach (var pos in positions)
                {
                    int npcTypeId = GetNPCTypeId(type);
                    if (npcTypeId > 0)
                    {
                        int npcIdx = NPC.NewNPC(null, (int)pos.X * 16, (int)pos.Y * 16, npcTypeId);
                        if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
                        {
                            var npc = Main.npc[npcIdx];
                            npc.homeTileX = (int)pos.X;
                            npc.homeTileY = (int)pos.Y;
                        }
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
                int count = SpawnWeights[type];
                for (int i = 0; i < count; i++)
                {
                    int npcTypeId = GetNPCTypeId(type);
                    if (npcTypeId <= 0) continue;

                    // 在可居住区域随机分布
                    Vector2 spawnPos = GetRandomSpawnPosition(centerX, groundLevel);
                    int npcIdx = NPC.NewNPC(null, (int)spawnPos.X * 16, (int)spawnPos.Y * 16, npcTypeId);
                    if (npcIdx >= 0 && npcIdx < Main.maxNPCs)
                    {
                        var npc = Main.npc[npcIdx];
                        npc.homeTileX = (int)spawnPos.X;
                        npc.homeTileY = (int)spawnPos.Y;
                    }
                }
            }

            HasInitialized = true;
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
                if (npc.ModNPC is GuYueNPCBase guYueNPC)
                {
                    SavedNPCs.Add(new SavedNPCData
                    {
                        NPCType = guYueNPC.GetNPCType(),
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
                int npcTypeId = GetNPCTypeId(data.NPCType);
                if (npcTypeId <= 0) continue;

                int npcIdx = NPC.NewNPC(null, (int)data.PositionX, (int)data.PositionY, npcTypeId);
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
                if (npc.active && npc.ModNPC is GuYueNPCBase)
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
