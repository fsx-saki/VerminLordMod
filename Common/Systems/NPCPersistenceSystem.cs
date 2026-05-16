using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Content.NPCs.GuMasters;
using VerminLordMod.Content.NPCs.GuYue;
using VerminLordMod.Content.NPCs.Commoners;

namespace VerminLordMod.Common.Systems
{
    public enum NPCPersistenceType
    {
        None = 0,               // 普通NPC，可被泰拉刷新机制正常处理
        Persistent,             // 持久化NPC — 死亡后不消失，复活
        FactionMember,          // 家族成员 — 永不刷新，有固定身份
        StoryCritical,          // 剧情关键NPC — 永不死亡/刷新
        SubworldResident,       // 小世界居民 — 仅在小世界内存在
    }

    public enum NPCDeathPolicy
    {
        Respawn,                // 死亡后重新生成（普通NPC）
        RespawnAfterDelay,      // 延迟复活（家族成员，数天后复活）
        PermaDeath,             // 永久死亡（被替代者接替）
        Immortal,               // 不死（剧情关键NPC）
        CorpseThenReplace,      // 留下尸体后被替代者接替
    }

    public class PersistentNPCData
    {
        public int NPCType;
        public string UniqueID;
        public string DisplayName;
        public NPCPersistenceType PersistenceType;
        public NPCDeathPolicy DeathPolicy;
        public FactionID Faction;
        public string RoleID;
        public Vector2 HomePosition;
        public int RespawnDelayDays;
        public int DeathCount;
        public int DaysSinceDeath;
        public bool IsAlive;
        public bool HasSpawned;
        public Dictionary<string, string> CustomData = new();
    }

    public class NPCPersistenceSystem : ModSystem
    {
        public static NPCPersistenceSystem Instance => ModContent.GetInstance<NPCPersistenceSystem>();

        public Dictionary<string, PersistentNPCData> RegisteredNPCs = new();
        public List<string> DeadNPCQueue = new();

        public override void OnWorldLoad()
        {
            RegisteredNPCs.Clear();
            DeadNPCQueue.Clear();
            RegisterFactionMembers();
        }

        private void RegisterFactionMembers()
        {
            AutoRegisterGuMasterNPCs();

            RegisterPersistentNPC(new PersistentNPCData
            {
                NPCType = ModContent.NPCType<GuYueChief>(),
                UniqueID = "guyue_chief",
                DisplayName = "古月族长",
                PersistenceType = NPCPersistenceType.FactionMember,
                DeathPolicy = NPCDeathPolicy.CorpseThenReplace,
                Faction = FactionID.GuYue,
                RoleID = "chief",
                RespawnDelayDays = 7,
            });

            RegisterPersistentNPC(new PersistentNPCData
            {
                NPCType = ModContent.NPCType<GuYueChiElder>(),
                UniqueID = "guyue_chi_elder",
                DisplayName = "古月赤脉家老",
                PersistenceType = NPCPersistenceType.FactionMember,
                DeathPolicy = NPCDeathPolicy.CorpseThenReplace,
                Faction = FactionID.GuYue,
                RoleID = "chi_elder",
                RespawnDelayDays = 5,
            });

            RegisterPersistentNPC(new PersistentNPCData
            {
                NPCType = ModContent.NPCType<GuYueMoElder>(),
                UniqueID = "guyue_mo_elder",
                DisplayName = "古月漠脉家老",
                PersistenceType = NPCPersistenceType.FactionMember,
                DeathPolicy = NPCDeathPolicy.CorpseThenReplace,
                Faction = FactionID.GuYue,
                RoleID = "mo_elder",
                RespawnDelayDays = 5,
            });

            RegisterPersistentNPC(new PersistentNPCData
            {
                NPCType = ModContent.NPCType<GuYueMedicineElder>(),
                UniqueID = "guyue_medicine_elder",
                DisplayName = "古月药脉家老",
                PersistenceType = NPCPersistenceType.FactionMember,
                DeathPolicy = NPCDeathPolicy.CorpseThenReplace,
                Faction = FactionID.GuYue,
                RoleID = "medicine_elder",
                RespawnDelayDays = 5,
            });
        }

        private void AutoRegisterGuMasterNPCs()
        {
            var guMasterTypes = new (string className, string displayName, FactionID faction, string roleID)[]
            {
                ("GuYuePatrolGuMaster", "古月巡逻蛊师", FactionID.GuYue, "patrol"),
                ("BanditGuMaster", "流寇蛊师", FactionID.Scattered, "bandit"),
                ("WildGuMaster", "野生蛊师", FactionID.Scattered, "wild"),
                ("HermitGuMaster", "隐修蛊师", FactionID.Scattered, "hermit"),
                ("EvilCultivatorGuMaster", "邪修蛊师", FactionID.Scattered, "evil"),
                ("ShadowAssassinGuMaster", "影刺蛊师", FactionID.Scattered, "assassin"),
            };

            foreach (var (className, displayName, faction, roleID) in guMasterTypes)
            {
                int npcType = Mod.Find<ModNPC>(className)?.Type ?? 0;
                if (npcType == 0) continue;

                string uniqueID = $"{faction.ToString().ToLower()}_{roleID}";
                if (RegisteredNPCs.ContainsKey(uniqueID)) continue;

                RegisterPersistentNPC(new PersistentNPCData
                {
                    NPCType = npcType,
                    UniqueID = uniqueID,
                    DisplayName = displayName,
                    PersistenceType = NPCPersistenceType.Persistent,
                    DeathPolicy = NPCDeathPolicy.RespawnAfterDelay,
                    Faction = faction,
                    RoleID = roleID,
                    RespawnDelayDays = 3,
                });
            }
        }

        private void RegisterPersistentNPC(PersistentNPCData data)
        {
            RegisteredNPCs[data.UniqueID] = data;
        }

        public bool IsPersistentNPC(int npcType)
        {
            foreach (var kvp in RegisteredNPCs)
            {
                if (kvp.Value.NPCType == npcType)
                    return kvp.Value.PersistenceType != NPCPersistenceType.None;
            }
            return false;
        }

        public bool ShouldBlockVanillaSpawn(int npcType)
        {
            foreach (var kvp in RegisteredNPCs)
            {
                if (kvp.Value.NPCType == npcType && kvp.Value.PersistenceType >= NPCPersistenceType.FactionMember)
                    return true;
            }
            return false;
        }

        public PersistentNPCData GetDataForNPC(int npcType)
        {
            foreach (var kvp in RegisteredNPCs)
            {
                if (kvp.Value.NPCType == npcType)
                    return kvp.Value;
            }
            return null;
        }

        public void OnPersistentNPCDeath(NPC npc)
        {
            var data = GetDataForNPC(npc.type);
            if (data == null) return;

            data.IsAlive = false;
            data.DeathCount++;
            data.DaysSinceDeath = 0;

            switch (data.DeathPolicy)
            {
                case NPCDeathPolicy.RespawnAfterDelay:
                    DeadNPCQueue.Add(data.UniqueID);
                    break;

                case NPCDeathPolicy.CorpseThenReplace:
                    CreateCorpse(npc, data);
                    TriggerSuccession(data);
                    DeadNPCQueue.Add(data.UniqueID);
                    break;

                case NPCDeathPolicy.PermaDeath:
                    CreateCorpse(npc, data);
                    TriggerSuccession(data);
                    break;

                case NPCDeathPolicy.Immortal:
                    data.IsAlive = true;
                    break;
            }

            EventBus.Publish(new FactionMemberDeathEvent
            {
                NPCType = npc.type,
                UniqueID = data.UniqueID,
                Faction = data.Faction,
                RoleID = data.RoleID,
                DeathPolicy = data.DeathPolicy,
            });
        }

        public override void PostUpdateWorld()
        {
            for (int i = DeadNPCQueue.Count - 1; i >= 0; i--)
            {
                var uid = DeadNPCQueue[i];
                if (RegisteredNPCs.TryGetValue(uid, out var data))
                {
                    data.DaysSinceDeath++;
                    if (data.DaysSinceDeath >= data.RespawnDelayDays)
                    {
                        data.IsAlive = true;
                        data.HasSpawned = false;
                        DeadNPCQueue.RemoveAt(i);
                    }
                }
            }

            SpawnUnspawnedNPCs();
        }

        private void SpawnUnspawnedNPCs()
        {
            foreach (var kvp in RegisteredNPCs)
            {
                var data = kvp.Value;
                if (!data.IsAlive || data.HasSpawned) continue;

                int existingCount = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == data.NPCType)
                        existingCount++;
                }

                if (existingCount > 0)
                {
                    data.HasSpawned = true;
                    continue;
                }

                if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) continue;

                Vector2 spawnPos = data.HomePosition;
                if (spawnPos == Vector2.Zero)
                {
                    spawnPos = Main.spawnTileX < Main.maxTilesX / 2
                        ? new Vector2(Main.spawnTileX * 16 + 800, Main.spawnTileY * 16)
                        : new Vector2(Main.spawnTileX * 16 - 800, Main.spawnTileY * 16);
                }

                int npcIndex = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(),
                    (int)spawnPos.X, (int)spawnPos.Y, data.NPCType);
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                {
                    data.HasSpawned = true;
                }
            }
        }

        private void CreateCorpse(NPC npc, PersistentNPCData data)
        {
            if (npc == null || !npc.active) return;

            int corpseType = ModContent.ProjectileType<Common.Entities.NpcCorpse>();
            int projIndex = Projectile.NewProjectile(
                Terraria.Entity.GetSource_NaturalSpawn(),
                npc.Center, Microsoft.Xna.Framework.Vector2.Zero,
                corpseType, 0, 0, Main.myPlayer);

            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                var corpse = (Common.Entities.NpcCorpse)Main.projectile[projIndex].ModProjectile;
                corpse.CorpseType = Common.Entities.CorpseType.Monster;
                corpse.SourceNPCType = npc.type;
                corpse.SourceNPCName = data.DisplayName;
                corpse.OwnerName = data.DisplayName;

                for (int i = 0; i < npc.extraValue; i++)
                {
                    if (Main.rand.NextFloat() < 0.3f)
                    {
                        corpse.RemainingItems.Add(new Item(ModContent.ItemType<Content.Items.Consumables.YuanS>())
                        {
                            stack = Main.rand.Next(1, 5)
                        });
                    }
                }
            }
        }

        private void TriggerSuccession(PersistentNPCData data)
        {
            var powerSystem = FactionPowerSystem.Instance;
            if (powerSystem == null) return;

            var structure = powerSystem.GetPowerStructure(data.Faction);
            if (structure == null) return;

            structure.InternalConflictLevel = System.Math.Min(100, structure.InternalConflictLevel + 15);
            structure.StabilityScore = System.Math.Max(0, structure.StabilityScore - 10);

            if (Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                Main.NewText($"【{data.Faction}】{data.DisplayName}已陨落，家族内部权力动荡...",
                    Microsoft.Xna.Framework.Color.DarkOrange);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var dataList = new List<TagCompound>();
            foreach (var kvp in RegisteredNPCs)
            {
                var d = kvp.Value;
                var subTag = new TagCompound
                {
                    ["uid"] = d.UniqueID,
                    ["isAlive"] = d.IsAlive,
                    ["hasSpawned"] = d.HasSpawned,
                    ["deathCount"] = d.DeathCount,
                    ["daysSinceDeath"] = d.DaysSinceDeath,
                    ["homeX"] = d.HomePosition.X,
                    ["homeY"] = d.HomePosition.Y,
                };
                dataList.Add(subTag);
            }
            tag["persistentNPCs"] = dataList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.TryGet("persistentNPCs", out List<TagCompound> dataList))
            {
                foreach (var subTag in dataList)
                {
                    string uid = subTag.GetString("uid");
                    if (RegisteredNPCs.TryGetValue(uid, out var data))
                    {
                        data.IsAlive = subTag.GetBool("isAlive");
                        data.HasSpawned = subTag.GetBool("hasSpawned");
                        data.DeathCount = subTag.GetInt("deathCount");
                        data.DaysSinceDeath = subTag.GetInt("daysSinceDeath");
                        data.HomePosition = new Vector2(subTag.GetFloat("homeX"), subTag.GetFloat("homeY"));
                    }
                }
            }
        }
    }

    public class FactionMemberDeathEvent : GuWorldEvent
    {
        public int NPCType;
        public string UniqueID;
        public FactionID Faction;
        public string RoleID;
        public NPCDeathPolicy DeathPolicy;
    }
}
