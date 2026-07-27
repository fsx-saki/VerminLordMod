using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Common.QuestSystem.Storyline
{
    /// <summary>
    /// 内置触发条件模板库。
    /// 仿照 PRT 系统"复制+改参数"哲学，所有条件都是可组合的。
    ///
    /// 使用示例：
    /// <code>
    /// TriggerConditions.Add(StorylineConditions.BossDefeated(NPCID.KingSlime));
    /// TriggerConditions.Add(StorylineConditions.ItemObtained(ItemID.GoldCoin, 10));
    /// TriggerConditions.Add(StorylineConditions.Always()); // 立刻触发
    /// </code>
    /// </summary>
    public static class StorylineConditions
    {
        /// <summary>总是满足 — 用于无前置条件的起始任务</summary>
        public static IStorylineCondition Always()
            => new AlwaysCondition();

        /// <summary>击败指定 Boss — 使用 DownBossSystem 检测</summary>
        public static IStorylineCondition BossDefeated(int npcType)
            => new BossDefeatedCondition(npcType);

        /// <summary>拥有指定数量的物品</summary>
        public static IStorylineCondition ItemObtained(int itemType, int amount = 1)
            => new ItemObtainedCondition(itemType, amount);

        /// <summary>到达指定剧情阶段</summary>
        public static IStorylineCondition StoryPhaseReached(StoryPhase phase)
            => new StoryPhaseCondition(phase);

        /// <summary>前置主线任务已完成（接受 QuestNode 类型名）</summary>
        public static IStorylineCondition QuestCompleted(string questId)
            => new QuestCompletedCondition(questId);

        // =============================================================
        // 内部实现
        // =============================================================

        private class AlwaysCondition : IStorylineCondition
        {
            public string Description => "无条件";
            public bool IsMet(Player player) => true;
        }

        private class BossDefeatedCondition : IStorylineCondition
        {
            private readonly int _npcType;
            public string Description => $"击败 {NPCID.Search.GetName(_npcType)}";
            public BossDefeatedCondition(int npcType) => _npcType = npcType;
            public bool IsMet(Player player)
            {
                // 检查内置原版 Boss 标记
                return _npcType switch
                {
                    NPCID.KingSlime => NPC.downedSlimeKing,
                    NPCID.EyeofCthulhu => NPC.downedBoss1,
                    NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail => NPC.downedBoss2,
                    NPCID.BrainofCthulhu => NPC.downedBoss2,
                    NPCID.SkeletronHead => NPC.downedBoss3,
                    NPCID.QueenBee => NPC.downedQueenBee,
                    NPCID.WallofFlesh => Main.hardMode,
                    NPCID.TheDestroyer => NPC.downedMechBoss1,
                    NPCID.SkeletronPrime => NPC.downedMechBoss2,
                    NPCID.Retinazer or NPCID.Spazmatism => NPC.downedMechBoss3,
                    NPCID.Plantera => NPC.downedPlantBoss,
                    NPCID.Golem => NPC.downedGolemBoss,
                    NPCID.DukeFishron => NPC.downedFishron,
                    NPCID.HallowBoss => NPC.downedEmpressOfLight,
                    NPCID.CultistBoss => NPC.downedAncientCultist,
                    NPCID.MoonLordCore => NPC.downedMoonlord,
                    _ => false // 模组 Boss 请在子类中自定义条件
                };
            }
        }

        private class ItemObtainedCondition : IStorylineCondition
        {
            private readonly int _itemType;
            private readonly int _amount;
            public string Description => $"拥有 {_amount} 个 {ItemID.Search.GetName(_itemType)}";
            public ItemObtainedCondition(int itemType, int amount)
            { _itemType = itemType; _amount = amount; }
            public bool IsMet(Player player)
            {
                int count = 0;
                foreach (var item in player.inventory)
                    if (!item.IsAir && item.type == _itemType)
                        count += item.stack;
                return count >= _amount;
            }
        }

        private class StoryPhaseCondition : IStorylineCondition
        {
            private readonly StoryPhase _phase;
            public string Description => $"到达剧情阶段 {_phase}";
            public StoryPhaseCondition(StoryPhase phase) => _phase = phase;

            /// <summary>
            /// 检查剧情阶段。当 StoryPhase 系统完全接入 ModPlayer 后，
            /// 此处将改为从玩家数据读取。目前作为预留接口。
            /// </summary>
            public bool IsMet(Player player)
            {
                // TODO: 接入 PlayerStoryProgress 追踪后修改此处
                // 目前返回 false（需手动在 StorylineQuest 子类中重写条件逻辑）
                return false;
            }
        }

        private class QuestCompletedCondition : IStorylineCondition
        {
            private readonly string _questId;
            public string Description => $"完成 {_questId}";
            public QuestCompletedCondition(string questId) => _questId = questId;
            public bool IsMet(Player player)
            {
                var ql = player.GetModPlayer<QLPlayer>();
                return ql?.GetQuestData(_questId).IsCompleted ?? false;
            }
        }
    }
}
