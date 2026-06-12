using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 剧情NPC生成系统 — 在阶段推进时自动刷出关键NPC
    ///
    /// 当StoryPhase推进到特定阶段时，在玩家附近生成对应的剧情NPC。
    /// 这些NPC携带剧情对话，玩家需要与他们交互才能继续推进剧情。
    /// </summary>
    public class StoryNPCSpawnerSystem : ModSystem
    {
        private bool _subscribed = false;

        /// <summary> 阶段→NPC生成定义映射 </summary>
        private static readonly Dictionary<StoryPhase, StoryNPCSpawnDef> PhaseSpawnMap = new()
        {
            // Stage1
            { StoryPhase.Arrival, new StoryNPCSpawnDef("古月守门蛊师", "学堂家老", "你来到了古月山寨的地界。") },
            { StoryPhase.AwakeningCeremony, new StoryNPCSpawnDef("学堂家老", null, "开窍仪式即将开始。") },
            { StoryPhase.SchoolTraining, new StoryNPCSpawnDef("学堂教头", null, "学堂训练开始了。") },
            { StoryPhase.MedicineRequest, new StoryNPCSpawnDef("药堂家老", null, "药堂需要你的帮助。") },
            { StoryPhase.FamilyRecognition, new StoryNPCSpawnDef("古月博", null, "族长要见你。") },

            // Stage2
            { StoryPhase.PreTournament, new StoryNPCSpawnDef("古月博", "白凝冰", "三寨大比即将开始！") },
            { StoryPhase.TianHeAttack, new StoryNPCSpawnDef("御堂家老", null, "天鹤上人来袭！") },
            { StoryPhase.BaiNingBingIceSeal, new StoryNPCSpawnDef("白凝冰", null, "白凝冰的空窍……") },
            { StoryPhase.BloodSacrifice, new StoryNPCSpawnDef("方源", null, "血祭之夜……") },

            // Stage3
            { StoryPhase.SouthBorderArrival, new StoryNPCSpawnDef("太白云生", "商心慈", "你到达了南疆散修营地。") },
            { StoryPhase.ShangXinCiMeet, new StoryNPCSpawnDef("商心慈", null, "商心慈需要帮助。") },
            { StoryPhase.ThreeKingsInheritance, new StoryNPCSpawnDef("太白云生", null, "三王传承即将开启。") },

            // Stage4
            { StoryPhase.YiTianShanAppears, new StoryNPCSpawnDef("太白云生", null, "义天山异变！") },
            { StoryPhase.FangYuanReveal, new StoryNPCSpawnDef("方源", null, "方源暴露真面目！") },

            // Stage5
            { StoryPhase.NorthDesertArrival, new StoryNPCSpawnDef("黑楼兰", "冰塞川", "你到达了北原王庭。") },
            { StoryPhase.TaiBaiYunShengDeath, new StoryNPCSpawnDef("太白云生", "方源", "太白云生……") },
            { StoryPhase.HeavenPrelude, new StoryNPCSpawnDef("铜公", "眉公", "天庭使者降临。") },

            // Stage6
            { StoryPhase.DestinyWarBegin, new StoryNPCSpawnDef("天庭使者", null, "宿命大战开始！") },
            { StoryPhase.FactionChoice, new StoryNPCSpawnDef("影无邪", "铜公", "选择你的阵营。") },
        };

        public override void PostUpdateWorld()
        {
            if (!_subscribed)
            {
                EventBus.Subscribe<StoryPhaseAdvancedEvent>(OnPhaseAdvanced);
                _subscribed = true;
            }
        }

        public override void OnWorldUnload()
        {
            _subscribed = false;
        }

        private void OnPhaseAdvanced(StoryPhaseAdvancedEvent e)
        {
            var phase = (StoryPhase)e.NewPhase;
            if (!PhaseSpawnMap.TryGetValue(phase, out var def))
                return;

            // 只对本地玩家生效
            Player player = Main.player[e.PlayerID];
            if (player == null || !player.active)
                return;

            // 延迟生成（等待1秒后执行，避免在事件处理中直接生成）
            // 使用简单的计时器机制
            _pendingSpawns.Add(new PendingSpawn
            {
                PlayerID = e.PlayerID,
                Definition = def,
                DelayTicks = 60 // 1秒延迟
            });
        }

        private readonly List<PendingSpawn> _pendingSpawns = new();

        public override void PostUpdateNPCs()
        {
            // 处理延迟生成
            for (int i = _pendingSpawns.Count - 1; i >= 0; i--)
            {
                var spawn = _pendingSpawns[i];
                spawn.DelayTicks--;

                if (spawn.DelayTicks <= 0)
                {
                    SpawnStoryNPCs(spawn);
                    _pendingSpawns.RemoveAt(i);
                }
            }
        }

        private void SpawnStoryNPCs(PendingSpawn spawn)
        {
            Player player = Main.player[spawn.PlayerID];
            if (player == null || !player.active) return;

            var def = spawn.Definition;

            // 生成主NPC
            SpawnNPCNearPlayer(def.PrimaryNPCName, player);

            // 生成次要NPC
            if (!string.IsNullOrEmpty(def.SecondaryNPCName))
            {
                SpawnNPCNearPlayer(def.SecondaryNPCName, player, offset: 80);
            }

            // 显示消息
            if (!string.IsNullOrEmpty(def.Message))
            {
                Main.NewText($"[蛊世界] {def.Message}", Color.Gold);
            }
        }

        /// <summary> 在玩家附近生成指定名称的NPC </summary>
        private void SpawnNPCNearPlayer(string npcName, Player player, int offset = 40)
        {
            // 通过名称查找NPC类型
            int npcType = FindNPCTypeByName(npcName);
            if (npcType <= 0)
            {
                // 找不到对应NPC类型，只显示消息
                Main.NewText($"[蛊世界] {npcName}出现在附近。", Color.Yellow);
                return;
            }

            // 计算生成位置（玩家前方偏移）
            int dir = player.direction != 0 ? player.direction : 1;
            int spawnX = (int)(player.Center.X / 16f) + dir * (offset / 16);
            int spawnY = (int)(player.Center.Y / 16f);

            // 确保生成位置有效
            if (spawnX < 0 || spawnX >= Main.maxTilesX || spawnY < 0 || spawnY >= Main.maxTilesY)
                return;

            // 找到合适的地面位置
            while (spawnY < Main.maxTilesY - 10 && !global::Terraria.WorldGen.SolidTile(spawnX, spawnY))
                spawnY++;

            int npcIndex = NPC.NewNPC(null, spawnX * 16, spawnY * 16 - 20, npcType);
            if (npcIndex >= 0 && npcIndex < Main.npc.Length)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    // 标记为剧情NPC（不随时间消失）
                    npc.SpawnedFromStatue = true; // 防止自然消失
                }
            }
        }

        /// <summary> 通过名称查找NPC类型ID </summary>
        private int FindNPCTypeByName(string name)
        {
            foreach (var modNPC in ModContent.GetContent<ModNPC>())
            {
                if (modNPC.Name == name || modNPC.DisplayName.Value == name)
                    return modNPC.Type;
            }
            return -1;
        }

        // ==================== 内部类 ====================

        private class StoryNPCSpawnDef
        {
            public string PrimaryNPCName;
            public string SecondaryNPCName;
            public string Message;

            public StoryNPCSpawnDef(string primary, string secondary, string message)
            {
                PrimaryNPCName = primary;
                SecondaryNPCName = secondary;
                Message = message;
            }
        }

        private class PendingSpawn
        {
            public int PlayerID;
            public StoryNPCSpawnDef Definition;
            public int DelayTicks;
        }
    }
}
