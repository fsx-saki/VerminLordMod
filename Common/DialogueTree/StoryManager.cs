using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree.Actions;
using VerminLordMod.Content.Dialogues.Stories;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// StoryManager — 剧情推进核心管理器。
    ///
    /// 职责：
    /// 1. 追踪每个玩家的剧情阶段和完成状态
    /// 2. 根据阶段决定哪些 NPC 携带哪些剧情对话
    /// 3. 处理阶段推进条件（完成前置剧情 → 解锁下一阶段）
    /// 4. 持久化剧情进度
    ///
    /// 工作流程：
    ///   Player 进入小世界 → StoryManager 检查当前阶段 →
    ///   根据阶段为 NPC 绑定对应的 StoryDialogueProvider →
    ///   Player 与 NPC 对话 → 剧情推进 → 阶段更新 →
    ///   下一阶段 NPC 刷新/对话更新
    ///
    /// 使用方式：
    ///   <code>
    ///   // 获取当前阶段
    ///   var phase = StoryManager.Instance.GetPhase(player);
    ///
    ///   // 推进到下一阶段
    ///   StoryManager.Instance.AdvancePhase(player);
    ///
    ///   // 完成某个剧情
    ///   StoryManager.Instance.CompleteStory(player, "Story_GuYueVillageEntry");
    ///   </code>
    /// </summary>
    public class StoryManager : ModSystem
    {
        // ===== 单例 =====
        public static StoryManager Instance => ModContent.GetInstance<StoryManager>();

        // ===== 玩家剧情进度 =====
        private readonly Dictionary<string, PlayerStoryProgress> _playerProgress = new();

        // ===== 阶段 → 剧情 ID 映射 =====
        // 每个阶段对应一个主要剧情，完成后自动推进到下一阶段
        private static readonly Dictionary<StoryPhase, string> PhaseStoryMap = new()
        {
            { StoryPhase.Arrival, "Story_GuYueVillageEntry" },
            { StoryPhase.SchoolTraining, "Story_GuYueSchoolTrial" },
            { StoryPhase.MedicineRequest, "Story_GuYueMedicineRequest" },
            { StoryPhase.FamilyRecognition, "Story_GuYueFamilyRecognition" },
            { StoryPhase.Crisis, "Story_GuYueCrisis" },
            { StoryPhase.Finale, "Story_GuYueFinale" },
        };

        // ===== 阶段顺序 =====
        private static readonly List<StoryPhase> PhaseOrder = new()
        {
            StoryPhase.NotEntered,
            StoryPhase.Arrival,
            StoryPhase.SchoolTraining,
            StoryPhase.MedicineRequest,
            StoryPhase.FamilyRecognition,
            StoryPhase.Crisis,
            StoryPhase.Finale,
        };

        // ============================================================
        // 阶段查询
        // ============================================================

        /// <summary> 获取玩家的当前阶段 </summary>
        public StoryPhase GetPhase(Player player)
        {
            return GetProgress(player).CurrentPhase;
        }

        /// <summary> 获取玩家的剧情进度对象 </summary>
        public PlayerStoryProgress GetProgress(Player player)
        {
            string name = player.name;
            if (!_playerProgress.TryGetValue(name, out var progress))
            {
                progress = new PlayerStoryProgress();
                _playerProgress[name] = progress;
            }
            return progress;
        }

        /// <summary> 检查玩家是否已完成指定剧情 </summary>
        public bool HasCompleted(Player player, string storyID)
        {
            return GetProgress(player).HasCompleted(storyID);
        }

        /// <summary> 检查玩家是否已达到或超过指定阶段 </summary>
        public bool HasReachedPhase(Player player, StoryPhase phase)
        {
            var current = GetPhase(player);
            return PhaseOrder.IndexOf(current) >= PhaseOrder.IndexOf(phase);
        }

        // ============================================================
        // 阶段推进
        // ============================================================

        /// <summary> 推进到下一阶段（自动完成当前阶段的剧情） </summary>
        public void AdvancePhase(Player player)
        {
            var progress = GetProgress(player);
            int currentIdx = PhaseOrder.IndexOf(progress.CurrentPhase);

            if (currentIdx < 0 || currentIdx >= PhaseOrder.Count - 1)
                return;

            // 完成当前阶段的剧情
            if (PhaseStoryMap.TryGetValue(progress.CurrentPhase, out var storyID))
            {
                progress.CompleteStory(storyID);
            }

            // 推进到下一阶段
            progress.CurrentPhase = PhaseOrder[currentIdx + 1];

            // 触发阶段进入事件
            OnPhaseEnter(player, progress.CurrentPhase);
        }

        /// <summary> 直接设置阶段（用于调试或特殊跳转） </summary>
        public void SetPhase(Player player, StoryPhase phase)
        {
            var progress = GetProgress(player);
            progress.CurrentPhase = phase;
            OnPhaseEnter(player, phase);
        }

        /// <summary> 标记某个剧情为已完成 </summary>
        public void CompleteStory(Player player, string storyID)
        {
            var progress = GetProgress(player);
            progress.CompleteStory(storyID);

            // 检查是否当前阶段的剧情已完成，自动推进
            if (PhaseStoryMap.TryGetValue(progress.CurrentPhase, out var phaseStory)
                && phaseStory == storyID)
            {
                AdvancePhase(player);
            }
        }

        /// <summary> 设置剧情标记 </summary>
        public void SetStoryFlag(Player player, string key, string value)
        {
            GetProgress(player).SetFlag(key, value);
        }

        /// <summary> 获取剧情标记 </summary>
        public string GetStoryFlag(Player player, string key)
        {
            return GetProgress(player).GetFlag(key);
        }

        // ============================================================
        // 阶段进入事件
        // ============================================================

        /// <summary>
        /// 当玩家进入新阶段时调用。
        /// 子类可重写此方法以添加阶段进入时的特殊逻辑
        /// （如刷出新的 NPC、解锁区域、全局通知等）。
        /// </summary>
        protected virtual void OnPhaseEnter(Player player, StoryPhase phase)
        {
            switch (phase)
            {
                case StoryPhase.Arrival:
                    Main.NewText("[古月山寨] 你来到了古月山寨的地界。", Microsoft.Xna.Framework.Color.Gold);
                    break;
                case StoryPhase.SchoolTraining:
                    Main.NewText("[古月山寨] 守门蛊师放行，你进入了山寨内部。", Microsoft.Xna.Framework.Color.Gold);
                    break;
                case StoryPhase.MedicineRequest:
                    Main.NewText("[古月山寨] 学堂教头认可了你的实力。", Microsoft.Xna.Framework.Color.Gold);
                    break;
                case StoryPhase.FamilyRecognition:
                    Main.NewText("[古月山寨] 你完成了药堂的委托，家族开始重视你。", Microsoft.Xna.Framework.Color.Gold);
                    break;
                case StoryPhase.Crisis:
                    Main.NewText("[古月山寨] 警报！山寨遭遇袭击！", Microsoft.Xna.Framework.Color.Red);
                    break;
                case StoryPhase.Finale:
                    Main.NewText("[古月山寨] 危机解除，你成为了古月家族的座上宾。", Microsoft.Xna.Framework.Color.Gold);
                    break;
            }
        }

        // ============================================================
        // NPC 绑定
        // ============================================================

        /// <summary>
        /// 根据当前阶段，为 NPC 绑定对应的剧情对话提供者。
        /// 返回应该绑定到该 NPC 的 StoryDialogueProvider，如果没有则返回 null。
        /// </summary>
        public StoryDialogueProvider GetStoryForNPC(Player player, NPC npc)
        {
            var phase = GetPhase(player);

            // 检查 NPC 类型，返回对应阶段的剧情
            if (npc.ModNPC is GuMasterBase guMaster)
            {
                string npcTypeName = guMaster.GetType().Name;

                switch (phase)
                {
                    case StoryPhase.Arrival:
                        // 守门蛊师触发"山寨来客"剧情
                        if (npcTypeName.Contains("PatrolGuMaster") || npcTypeName.Contains("Commoner"))
                            return CreateProvider<GuYueVillageEntry>();
                        break;

                    case StoryPhase.SchoolTraining:
                        // 学堂教头触发"学堂考验"剧情
                        if (npcTypeName.Contains("SchoolElder") || npcTypeName.Contains("FistInstructor"))
                            return CreateProvider<GuYueSchoolTrial>();
                        break;

                    case StoryPhase.MedicineRequest:
                        // 药堂家老触发"药堂秘方"剧情
                        if (npcTypeName.Contains("MedicineElder") || npcTypeName.Contains("MedicinePulseElder"))
                            return CreateProvider<GuYueMedicineRequest>();
                        break;

                    case StoryPhase.FamilyRecognition:
                        // 族长触发"家族认可"剧情
                        if (npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueFamilyRecognition>();
                        break;

                    case StoryPhase.Crisis:
                        // 御堂家老触发"危机降临"剧情
                        if (npcTypeName.Contains("DefenseElder") || npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueCrisis>();
                        break;

                    case StoryPhase.Finale:
                        // 族长触发终章
                        if (npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueFinale>();
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建剧情提供者实例并绑定到 NPC
        /// </summary>
        private StoryDialogueProvider CreateProvider<T>() where T : StoryDialogueProvider, new()
        {
            return new T();
        }

        // ============================================================
        // 持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var progressList = new List<TagCompound>();
            foreach (var kvp in _playerProgress)
            {
                var data = kvp.Value.Save();
                data["playerName"] = kvp.Key;
                progressList.Add(data);
            }
            tag["storyProgress"] = progressList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _playerProgress.Clear();

            if (tag.TryGet("storyProgress", out List<TagCompound> progressList))
            {
                foreach (var data in progressList)
                {
                    string playerName = data.GetString("playerName");
                    var progress = PlayerStoryProgress.Load(data);
                    _playerProgress[playerName] = progress;
                }
            }
        }
    }
}

// ===== 临时占位：尚未实现的剧情 =====
// 这些将在后续步骤中实现
namespace VerminLordMod.Common.DialogueTree
{
    /// <summary> 家族认可剧情（占位） </summary>
    public class GuYueFamilyRecognition : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFamilyRecognition";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月族长端坐在议事厅主位上，示意你上前。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}微笑着看向你：\"听说你在学堂和药堂的表现都不错。" +
                "作为族长，我很高兴看到有才华的年轻人加入我们古月家族。\"\n\n" +
                "他站起身，走到你面前：\"从今天起，你就是我古月家族的正式客卿了。" +
                "希望你能为家族的发展出一份力。\"")
                .AddOption("多谢族长！", "accept", DialogueOptionType.Exit,
                    tooltip: "接受客卿身份")
                .AddOption("我需要考虑一下", "decline", DialogueOptionType.Exit,
                    tooltip: "婉拒客卿身份");

            b.StartNode("accept",
                "{npcName}拍了拍你的肩膀：\"好！明日我让人给你安排住处。" +
                "家族藏经阁对你开放，你可以随意查阅。\"")
                .EndsDialogue();

            b.StartNode("decline",
                "{npcName}有些意外，但并未强求：\"也罢，人各有志。" +
                "古月山寨的大门永远为你敞开。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary> 危机降临剧情（占位） </summary>
    public class GuYueCrisis : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueCrisis";
        protected override string DisplayName => "古月御堂家老";
        protected override string GreetingText =>
            "御堂家老神色凝重地站在寨墙上，眺望远方。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}看到你来，急促地说：\"你来得正好！" +
                "探子来报，有一伙来历不明的蛊师正在向山寨逼近。" +
                "人数不少，来者不善！\"\n\n" +
                "他握紧腰间的蛊虫袋：\"我需要你帮忙防守山寨。" +
                "愿意助我一臂之力吗？\"")
                .AddOption("义不容辞！", "defend", DialogueOptionType.Combat,
                    tooltip: "参与山寨防御")
                .AddOption("我去通知其他人", "alert", DialogueOptionType.Quest,
                    tooltip: "帮忙疏散民众");

            b.StartNode("defend",
                "{npcName}重重地点头：\"好！随我来寨墙！\"")
                .EndsDialogue();

            b.StartNode("alert",
                "{npcName}松了口气：\"也好，你去通知寨民躲进地窖。" +
                "我在这里守着。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary> 终章剧情（占位） </summary>
    public class GuYueFinale : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFinale";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月族长在议事厅设宴，庆祝危机解除。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}举起酒杯：\"这次多亏了你，古月山寨才能化险为夷。" +
                "我代表整个古月家族，敬你一杯！\"\n\n" +
                "他郑重地取出一块令牌：\"这是古月家族的客卿令牌。" +
                "从今往后，你就是我古月家族最尊贵的客人。" +
                "无论你走到哪里，只要出示此令，古月弟子必当鼎力相助！\"")
                .AddOption("谢族长！", "end", DialogueOptionType.Exit);

            b.StartNode("end",
                "宴会上欢声笑语不断。你看着手中的令牌，知道自己在古月山寨的旅程告一段落。" +
                "但在这片广袤的天地间，还有更多的故事等待着你……")
                .EndsDialogue();

            return b.Build();
        }
    }
}