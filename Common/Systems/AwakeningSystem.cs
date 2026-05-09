using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 觉醒任务系统（D-40）。
    ///
    /// 职责：
    /// 1. 管理玩家觉醒状态（未开始 → 寻访 → 试炼 → 已觉醒）
    /// 2. 提供觉醒任务链：与蛊师对话 → 接受试炼 → 完成试炼 → 觉醒
    /// 3. 觉醒路径选择影响初始势力关系
    /// 4. 发布 AwakeningCompletedEvent 供其他系统响应
    /// </summary>
    public class AwakeningSystem : ModSystem
    {
        // ===== 单例 =====
        public static AwakeningSystem Instance => ModContent.GetInstance<AwakeningSystem>();

        // ===== 觉醒阶段 =====
        public enum AwakeningStage
        {
            NotStarted,     // 未开始
            Seeking,        // 寻访（寻找蛊师引路人）
            Trial,          // 试炼（完成蛊师考验）
            Awakened,       // 已觉醒
        }

        // ===== 觉醒路径 =====
        public enum AwakeningPath
        {
            GuYueApprentice,    // 古月学徒 — 初始 GuYue 声望 +50
            WanderingCultivator, // 散修 — 初始所有势力中立
            Reincarnator,       // 转世者 — 初始 DaoHen +10
        }

        // ===== 玩家状态 =====
        public Dictionary<int, AwakeningStage> PlayerStages = new();
        public Dictionary<int, AwakeningPath> PlayerPaths = new();

        // ===== 试炼任务追踪 =====
        /// <summary> 试炼目标 NPC 类型 </summary>
        public Dictionary<int, int> PlayerTrialMentor = new();

        /// <summary> 试炼进度 [0, 100] </summary>
        public Dictionary<int, float> PlayerTrialProgress = new();

        /// <summary> 试炼描述 </summary>
        public Dictionary<int, string> PlayerTrialDescription = new();

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            PlayerStages.Clear();
            PlayerPaths.Clear();
            PlayerTrialMentor.Clear();
            PlayerTrialProgress.Clear();
            PlayerTrialDescription.Clear();
        }

        public override void OnWorldUnload()
        {
            PlayerStages.Clear();
            PlayerPaths.Clear();
            PlayerTrialMentor.Clear();
            PlayerTrialProgress.Clear();
            PlayerTrialDescription.Clear();
        }

        // ============================================================
        // 觉醒流程
        // ============================================================

        /// <summary>
        /// 检查玩家是否已觉醒。
        /// </summary>
        public bool IsAwakened(Player player)
        {
            return PlayerStages.TryGetValue(player.whoAmI, out var stage) && stage == AwakeningStage.Awakened;
        }

        /// <summary>
        /// 获取玩家当前觉醒阶段。
        /// </summary>
        public AwakeningStage GetStage(Player player)
        {
            return PlayerStages.TryGetValue(player.whoAmI, out var stage) ? stage : AwakeningStage.NotStarted;
        }

        /// <summary>
        /// 开始觉醒流程。
        /// 由 DialogueSystem 在检测到未觉醒玩家与蛊师对话时调用。
        /// </summary>
        public void StartAwakening(Player player, int mentorWhoAmI)
        {
            if (IsAwakened(player)) return;

            PlayerStages[player.whoAmI] = AwakeningStage.Seeking;
            PlayerTrialMentor[player.whoAmI] = mentorWhoAmI;

            Main.NewText("[觉醒] 你遇到了愿意引你入道的蛊师，修炼之路由此开始...", Microsoft.Xna.Framework.Color.Gold);
        }

        /// <summary>
        /// 玩家选择觉醒路径。
        /// 影响初始势力关系和初始属性。
        /// </summary>
        public void ChoosePath(Player player, AwakeningPath path)
        {
            if (!PlayerStages.TryGetValue(player.whoAmI, out var stage) || stage != AwakeningStage.Seeking)
                return;

            PlayerPaths[player.whoAmI] = path;

            switch (path)
            {
                case AwakeningPath.GuYueApprentice:
                    // 古月学徒：初始 GuYue 声望 +50
                    var guWorldPlayer = player.GetModPlayer<GuWorldPlayer>();
                    guWorldPlayer.AddReputation(FactionID.GuYue, 50, "觉醒：古月学徒");
                    Main.NewText("[觉醒] 你选择拜入古月门下，古月声望 +50", Microsoft.Xna.Framework.Color.LightGreen);
                    break;

                case AwakeningPath.WanderingCultivator:
                    // 散修：所有势力中立（无额外声望）
                    Main.NewText("[觉醒] 你选择成为散修，不依附任何势力", Microsoft.Xna.Framework.Color.LightGreen);
                    break;

                case AwakeningPath.Reincarnator:
                    // 转世者：初始 DaoHen +10
                    var daoHenPlayer = player.GetModPlayer<DaoHenPlayer>();
                    // 为所有 DaoType 增加 10 道痕
                    foreach (DaoType dao in System.Enum.GetValues<DaoType>())
                    {
                        daoHenPlayer.AddDaoHen(dao, 10f);
                    }
                    Main.NewText("[觉醒] 你觉醒了前世记忆，所有道痕 +10", Microsoft.Xna.Framework.Color.LightGreen);
                    break;
            }

            // 进入试炼阶段
            PlayerStages[player.whoAmI] = AwakeningStage.Trial;
            AssignTrial(player);
        }

        /// <summary>
        /// 为玩家分配试炼任务。
        /// </summary>
        private void AssignTrial(Player player)
        {
            // 基础试炼：收集 5 个元石
            PlayerTrialProgress[player.whoAmI] = 0f;
            PlayerTrialDescription[player.whoAmI] = "收集 5 个元石，证明你有修炼的资质";

            Main.NewText("[觉醒] 蛊师交给你一个试炼：" + PlayerTrialDescription[player.whoAmI], Microsoft.Xna.Framework.Color.Azure);
        }

        /// <summary>
        /// 推进试炼进度。
        /// 由其他系统（如 ResourceNodeSystem 采集元石时）调用。
        /// </summary>
        public void AdvanceTrial(Player player, float amount)
        {
            if (!PlayerStages.TryGetValue(player.whoAmI, out var stage) || stage != AwakeningStage.Trial)
                return;

            if (!PlayerTrialProgress.TryGetValue(player.whoAmI, out var progress))
                progress = 0f;

            progress += amount;
            PlayerTrialProgress[player.whoAmI] = progress;

            if (progress >= 100f)
            {
                CompleteAwakening(player);
            }
        }

        /// <summary>
        /// 完成觉醒。
        /// </summary>
        public void CompleteAwakening(Player player)
        {
            if (!PlayerStages.TryGetValue(player.whoAmI, out var stage) || stage != AwakeningStage.Trial)
                return;

            PlayerStages[player.whoAmI] = AwakeningStage.Awakened;

            // 调用 QiRealmPlayer 的觉醒初始化
            var qiRealmPlayer = player.GetModPlayer<QiRealmPlayer>();
            qiRealmPlayer.OnAwakening();

            // 发布觉醒完成事件
            EventBus.Publish(new AwakeningCompletedEvent
            {
                PlayerID = player.whoAmI
            });

            Main.NewText("[觉醒] 你成功踏上了修炼之路！", Microsoft.Xna.Framework.Color.Gold);
        }

        /// <summary>
        /// 重置玩家觉醒状态（用于测试或特殊事件）。
        /// </summary>
        public void ResetPlayer(Player player)
        {
            PlayerStages.Remove(player.whoAmI);
            PlayerPaths.Remove(player.whoAmI);
            PlayerTrialMentor.Remove(player.whoAmI);
            PlayerTrialProgress.Remove(player.whoAmI);
            PlayerTrialDescription.Remove(player.whoAmI);
        }

        // ============================================================
        // 对话集成
        // ============================================================

        /// <summary>
        /// 获取未觉醒玩家的觉醒对话文本。
        /// 由 DialogueSystem 或 GuMasterBase 调用。
        /// </summary>
        public string GetAwakeningDialogue(Player player, string npcName)
        {
            if (IsAwakened(player))
                return null;

            var stage = GetStage(player);

            return stage switch
            {
                AwakeningStage.NotStarted =>
                    $"你尚未踏入修炼之道。若你有心，我可以引你入门。",
                AwakeningStage.Seeking =>
                    $"你已决定踏上修炼之路。告诉我，你选择哪条道路？",
                AwakeningStage.Trial =>
                    $"你的试炼尚未完成。{GetTrialDescription(player)}",
                _ => null,
            };
        }

        /// <summary>
        /// 获取试炼描述。
        /// </summary>
        public string GetTrialDescription(Player player)
        {
            if (!PlayerTrialDescription.TryGetValue(player.whoAmI, out var desc))
                return "完成蛊师的考验";
            return desc;
        }

        /// <summary>
        /// 获取试炼进度文本。
        /// </summary>
        public string GetTrialProgressText(Player player)
        {
            if (!PlayerTrialProgress.TryGetValue(player.whoAmI, out var progress))
                return "0%";
            return $"{System.Math.Min(100f, progress):F0}%";
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var stages = new List<TagCompound>();
            var paths = new List<TagCompound>();
            var trials = new List<TagCompound>();

            foreach (var kvp in PlayerStages)
            {
                stages.Add(new TagCompound
                {
                    ["playerID"] = kvp.Key,
                    ["stage"] = (int)kvp.Value,
                });
            }

            foreach (var kvp in PlayerPaths)
            {
                paths.Add(new TagCompound
                {
                    ["playerID"] = kvp.Key,
                    ["path"] = (int)kvp.Value,
                });
            }

            foreach (var kvp in PlayerTrialMentor)
            {
                var trial = new TagCompound
                {
                    ["playerID"] = kvp.Key,
                    ["mentor"] = kvp.Value,
                };
                if (PlayerTrialProgress.TryGetValue(kvp.Key, out var progress))
                    trial["progress"] = progress;
                if (PlayerTrialDescription.TryGetValue(kvp.Key, out var desc))
                    trial["description"] = desc;
                trials.Add(trial);
            }

            tag["awakeningStages"] = stages;
            tag["awakeningPaths"] = paths;
            tag["awakeningTrials"] = trials;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            PlayerStages.Clear();
            PlayerPaths.Clear();
            PlayerTrialMentor.Clear();
            PlayerTrialProgress.Clear();
            PlayerTrialDescription.Clear();

            if (tag.TryGet("awakeningStages", out List<TagCompound> stages))
            {
                foreach (var entry in stages)
                {
                    PlayerStages[entry.GetInt("playerID")] = (AwakeningStage)entry.GetInt("stage");
                }
            }

            if (tag.TryGet("awakeningPaths", out List<TagCompound> paths))
            {
                foreach (var entry in paths)
                {
                    PlayerPaths[entry.GetInt("playerID")] = (AwakeningPath)entry.GetInt("path");
                }
            }

            if (tag.TryGet("awakeningTrials", out List<TagCompound> trials))
            {
                foreach (var entry in trials)
                {
                    int pid = entry.GetInt("playerID");
                    PlayerTrialMentor[pid] = entry.GetInt("mentor");
                    PlayerTrialProgress[pid] = entry.GetFloat("progress");
                    PlayerTrialDescription[pid] = entry.GetString("description");
                }
            }
        }
    }
}
