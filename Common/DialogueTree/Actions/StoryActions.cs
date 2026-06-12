using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.StoryUI;

namespace VerminLordMod.Common.DialogueTree.Actions
{
    /// <summary>
    /// 触发选择 — 在对话中弹出关键选择面板
    /// 这是连接对话系统与选择追踪系统的桥梁
    /// </summary>
    public class TriggerChoiceAction : BaseDialogueAction
    {
        public override string ActionID => "trigger_choice";
        public override string DisplayName => "做出抉择";
        public override string Description => "面对关键抉择";
        public override DialogueOptionType OptionType => DialogueOptionType.Quest;

        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => false;

        /// <summary> 选择ID（如 S1_JiaJinSheng） </summary>
        public string ChoiceID { get; set; }

        /// <summary> 选择标题 </summary>
        public string ChoiceTitle { get; set; }

        /// <summary> 选择描述 </summary>
        public string ChoiceDescription { get; set; }

        /// <summary> 选项列表 </summary>
        public List<StoryChoicePanel.ChoiceOption> Options { get; set; } = new();

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (string.IsNullOrEmpty(ChoiceID) || Options.Count == 0)
                return DialogueActionResult.FailResult("选择配置错误");

            // 如果已经做出过这个选择，不再弹出
            if (ChoiceTrackerSystem.HasChoice(ChoiceID))
            {
                string desc = ChoiceTrackerSystem.GetChoiceDescription(ChoiceID);
                return DialogueActionResult.SuccessResult($"你已经做出了选择：{desc}");
            }

            // 弹出选择面板
            var uiSystem = ModContent.GetInstance<StoryUISystem>();
            if (uiSystem != null)
            {
                uiSystem.ChoicePanel.ShowChoice(ChoiceID, ChoiceTitle, ChoiceDescription, Options);
            }

            return DialogueActionResult.SuccessResult("你需要做出抉择……");
        }

        // ==================== 预定义选择工厂 ====================

        /// <summary> 创建贾金生之死选择 </summary>
        public static TriggerChoiceAction CreateJiaJinShengChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S1_JiaJinSheng",
                ChoiceTitle = "贾金生之死",
                ChoiceDescription = "贾金生被发现死在荒野中，你怀疑方源与此有关。你该如何行动？",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "帮助方源隐瞒", Tooltip = "方源好感度+20，魔道倾向+10", Value = 0, AccentColor = new Color(180, 80, 80) },
                    new() { Text = "帮助铁血冷调查", Tooltip = "铁血冷信任+20，正道倾向+10", Value = 1, AccentColor = new Color(80, 120, 200) },
                    new() { Text = "旁观不管", Tooltip = "方源好感度+5，中立倾向+10", Value = 2, AccentColor = new Color(150, 150, 150) }
                }
            };
        }

        /// <summary> 创建血祭之夜选择 </summary>
        public static TriggerChoiceAction CreateBloodSacrificeChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S2_BloodSacrifice",
                ChoiceTitle = "血祭之夜",
                ChoiceDescription = "方源在青茅山发动了血祭，无数人丧命。你站在十字路口……",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "加入方源", Tooltip = "魔道倾向+30，方源好感度+20", Value = 0, AccentColor = new Color(200, 50, 50) },
                    new() { Text = "对抗方源", Tooltip = "正道倾向+30，铁血冷信任+20", Value = 1, AccentColor = new Color(50, 150, 255) },
                    new() { Text = "逃离青茅山", Tooltip = "中立倾向+20，安全但失去一切", Value = 2, AccentColor = new Color(150, 150, 150) }
                }
            };
        }

        /// <summary> 创建白凝冰选择 </summary>
        public static TriggerChoiceAction CreateBaiNingBingChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S2_BaiNingBing",
                ChoiceTitle = "白凝冰的抉择",
                ChoiceDescription = "白凝冰的空窍无法支撑，她即将自爆。你要怎么做？",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "帮助白凝冰", Tooltip = "白凝冰好感度+30，冰道材料×5", Value = 0, AccentColor = new Color(100, 180, 255) },
                    new() { Text = "旁观", Tooltip = "无变化，但白凝冰可能死亡", Value = 1, AccentColor = new Color(150, 150, 150) },
                    new() { Text = "敌对", Tooltip = "白凝冰好感度-30，战斗", Value = 2, AccentColor = new Color(200, 80, 80) }
                }
            };
        }

        /// <summary> 创建商心慈选择 </summary>
        public static TriggerChoiceAction CreateShangXinCiChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S3_ShangXinCi",
                ChoiceTitle = "遇见商心慈",
                ChoiceDescription = "商心慈独自在南疆荒野中，看起来需要帮助。你的选择将影响后续剧情……",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "帮助商心慈", Tooltip = "商心慈好感度+15，散修声望+10", Value = 0, AccentColor = new Color(100, 200, 150) },
                    new() { Text = "利用商心慈", Tooltip = "获得50元石，商心慈好感度-5", Value = 1, AccentColor = new Color(200, 150, 50) },
                    new() { Text = "无视", Tooltip = "无变化", Value = 2, AccentColor = new Color(150, 150, 150) }
                }
            };
        }

        /// <summary> 创建方源暴露选择 </summary>
        public static TriggerChoiceAction CreateFangYuanRevealChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S4_FangYuanReveal",
                ChoiceTitle = "方源真面目",
                ChoiceDescription = "\"我乃方源！\"——大同风后，方源暴露了真面目。你的反应将决定一切。",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "追击方源", Tooltip = "正道倾向+20，方源好感度-10", Value = 0, AccentColor = new Color(80, 120, 200) },
                    new() { Text = "放走方源", Tooltip = "方源好感度+10，魔道倾向+10", Value = 1, AccentColor = new Color(180, 80, 80) },
                    new() { Text = "加入方源", Tooltip = "方源好感度+30，魔道倾向+30", Value = 2, AccentColor = new Color(200, 50, 50) }
                }
            };
        }

        /// <summary> 创建太白云生选择 </summary>
        public static TriggerChoiceAction CreateTaiBaiYunShengChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S5_TaiBaiYunSheng",
                ChoiceTitle = "太白云生之死",
                ChoiceDescription = "太白云生为保护众人而战，即将力竭。这是最催泪的时刻……",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "救助太白云生", Tooltip = "太白云生存活，因果业力-30", Value = 0, AccentColor = new Color(100, 200, 150) },
                    new() { Text = "旁观", Tooltip = "太白云生死亡，获得遗物", Value = 1, AccentColor = new Color(150, 150, 150) },
                    new() { Text = "偷袭方源", Tooltip = "方源反击，太白云生仍死，因果业力+80", Value = 2, AccentColor = new Color(200, 50, 50) }
                }
            };
        }

        /// <summary> 创建阵营选择 </summary>
        public static TriggerChoiceAction CreateFactionChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S6_FactionChoice",
                ChoiceTitle = "阵营选择",
                ChoiceDescription = "四柱已破，天庭降临。你必须选择你的立场——这个选择不可逆。",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "正道——与天庭合作", Tooltip = "龙公HP+20%但有盟友，偏向结局A", Value = 0, AccentColor = new Color(255, 215, 0) },
                    new() { Text = "魔道——与影宗合作", Tooltip = "暗杀能力+影宗协助，偏向结局B", Value = 1, AccentColor = new Color(150, 50, 200) },
                    new() { Text = "中立——独自战斗", Tooltip = "无盟友无额外难度，偏向结局C", Value = 2, AccentColor = new Color(150, 150, 150) }
                }
            };
        }

        /// <summary> 创建结局选择 </summary>
        public static TriggerChoiceAction CreateEndingChoice()
        {
            return new TriggerChoiceAction
            {
                ChoiceID = "S7_Ending",
                ChoiceTitle = "终局",
                ChoiceDescription = "蛊界意志已被击败，永生之门就在眼前。你将如何选择这个世界的命运？",
                Options = new List<StoryChoicePanel.ChoiceOption>
                {
                    new() { Text = "成为新天意", Tooltip = "成为世界管理者，可修改世界规则", Value = 0, AccentColor = new Color(255, 215, 0) },
                    new() { Text = "超脱蛊界", Tooltip = "离开蛊界，世界继续运转", Value = 1, AccentColor = new Color(100, 200, 255) },
                    new() { Text = "摧毁天地大蛊", Tooltip = "世界崩塌，选择重塑或放任", Value = 2, AccentColor = new Color(200, 50, 50) }
                }
            };
        }
    }

    /// <summary>
    /// 推进剧情 — 在对话中推进StoryPhase
    /// </summary>
    public class AdvanceStoryAction : BaseDialogueAction
    {
        public override string ActionID => "advance_story";
        public override string DisplayName => "剧情推进";
        public override string Description => "推进主线剧情";
        public override DialogueOptionType OptionType => DialogueOptionType.Quest;

        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => false;

        /// <summary> 要推进到的阶段 </summary>
        public string TargetPhase { get; set; }

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (string.IsNullOrEmpty(TargetPhase))
                return DialogueActionResult.FailResult("目标阶段未设置");

            if (!System.Enum.TryParse<StoryPhase>(TargetPhase, out var phase))
                return DialogueActionResult.FailResult($"无效的阶段：{TargetPhase}");

            var manager = StoryManager.Instance;
            var currentPhase = manager.GetPhase(context.Player);

            // 只允许向前推进
            if ((int)phase <= (int)currentPhase)
                return DialogueActionResult.SuccessResult("剧情已推进到此阶段。");

            manager.SetPhase(context.Player, phase);

            return DialogueActionResult.SuccessResult($"剧情推进：{phase}");
        }
    }

    /// <summary>
    /// 解锁系统 — 在对话中解锁游戏系统
    /// </summary>
    public class UnlockSystemAction : BaseDialogueAction
    {
        public override string ActionID => "unlock_system";
        public override string DisplayName => "系统解锁";
        public override string Description => "解锁新的游戏系统";
        public override DialogueOptionType OptionType => DialogueOptionType.Informative;

        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => false;

        /// <summary> 要解锁的系统名称 </summary>
        public string SystemName { get; set; }

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (string.IsNullOrEmpty(SystemName))
                return DialogueActionResult.FailResult("系统名称未设置");

            // 发布系统解锁事件
            global::VerminLordMod.Common.Events.EventBus.Publish(
                new global::VerminLordMod.Common.Events.SystemUnlockedEvent
                {
                    SystemName = SystemName,
                    PlayerID = context.Player.whoAmI
                });

            return DialogueActionResult.SuccessResult($"解锁了新系统：{SystemName}");
        }
    }
}
