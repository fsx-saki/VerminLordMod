using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;
using Terraria.ID;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // QuestSystem — 任务/委托系统大框
    //
    // 职责：
    // 1. 管理所有可用的任务定义（QuestDefinition）
    // 2. 订阅 QuestCompletedEvent，处理任务完成逻辑
    // 3. 根据玩家声望/修为/剧情阶段解锁新任务
    // 4. 提供任务查询接口（NPC 可查询可领取的任务列表）
    //
    // TODO:
    //   - 填充具体任务定义数据（QuestRegistry）
    //   - 实现任务条件检查逻辑
    //   - 实现任务奖励发放逻辑
    //   - 实现任务UI显示
    //   - 实现任务失败/超时逻辑
    // ============================================================

    public enum QuestType
    {
        MainStory,      // 主线剧情任务
        FactionQuest,   // 家族委托
        SideQuest,      // 支线任务
        DailyQuest,     // 每日委托
        BountyQuest,    // 悬赏任务
        CollectionQuest // 收集任务
    }

    public enum QuestState
    {
        Locked,         // 未解锁
        Available,      // 可领取
        Active,         // 进行中
        Completed,      // 已完成
        Failed,         // 已失败
        Expired         // 已过期
    }

    public enum QuestRewardType
    {
        Reputation,     // 声望
        YuanStones,     // 元石
        Item,           // 物品
        QiExp,          // 真元经验
        StoryProgress   // 剧情推进
    }

    public class QuestDefinition
    {
        public string QuestID;
        public string Title;
        public string Description;
        public string BriefDescription;
        public QuestType Type;
        public FactionID IssuingFaction;
        public int RequiredGuLevel;
        public int RequiredRepLevel;
        public StoryPhase RequiredStoryPhase;
        public List<string> PrerequisiteQuestIDs = new();
        public List<QuestObjective> Objectives = new();
        public List<QuestReward> Rewards = new();
        public int ExpireDays;
        public bool IsRepeatable;
        public string IssuingNPCType;
    }

    public class QuestObjective
    {
        public string ObjectiveID;
        public string Description;
        public ObjectiveType Type;
        public int TargetAmount;
        public int CurrentAmount;
        public bool IsCompleted => CurrentAmount >= TargetAmount;
        public string TargetNPCName;
        public int TargetItemType;
        public int TargetNPCType;
        public FactionID TargetFaction;
    }

    public enum ObjectiveType
    {
        KillNPC,            // 杀指定NPC
        KillNPCType,        // 杀指定类型NPC
        CollectItem,        // 收集物品
        ReachRepLevel,      // 达到声望等级
        EnterTerritory,     // 进入领地
        TalkToNPC,          // 与NPC对话
        DefendWave,         // 防守波次
        CompleteStoryPhase, // 完成剧情阶段
        CraftItem,          // 炼化物品
        SearchNode,         // 探索资源节点
        SurviveTribulation  // 渡过天劫
    }

    public class QuestReward
    {
        public QuestRewardType Type;
        public int Amount;
        public FactionID TargetFaction;
        public int ItemType;
        public string StoryFlag;
    }

    public class QuestInstance
    {
        public string QuestID;
        public QuestState State;
        public int AcceptedDay;
        public int ExpireDay;
        public List<QuestObjective> Objectives = new();
        public bool IsExpired => ExpireDay > 0 && (int)(Main.time / 36000) > ExpireDay;
        public bool IsCompleted => Objectives.TrueForAll(o => o.IsCompleted);
    }

    public class QuestSystem : ModSystem
    {
        public static QuestSystem Instance => ModContent.GetInstance<QuestSystem>();

        public Dictionary<string, QuestDefinition> QuestRegistry = new();

        public override void OnWorldLoad()
        {
            QuestRegistry.Clear();
            RegisterAllQuests();
        }

        public override void OnWorldUnload()
        {
            QuestRegistry.Clear();
        }

        private bool _isSubscribed = false;

        public override void PostUpdateWorld()
        {
            if (!_isSubscribed)
            {
                EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
                EventBus.Subscribe<NPCDeathEvent>(OnNPCDeathForQuest);
                _isSubscribed = true;
            }
        }

        private void RegisterAllQuests()
        {
            RegisterMainStoryQuests();
            RegisterGuYueFactionQuests();
            RegisterBaiFactionQuests();
            RegisterXiongFactionQuests();
            RegisterTieFactionQuests();
            RegisterWangFactionQuests();
            RegisterZhaoFactionQuests();
            RegisterJiaFactionQuests();
            RegisterScatteredFactionQuests();
            RegisterDailyQuests();
            RegisterBountyQuests();
        }

        private void RegisterMainStoryQuests()
        {
            // === Stage1: 一转·蛊师入门 ===
            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-01",
                Title = "开窍仪式",
                Description = "参加古月山寨的开窍仪式，测试你的资质",
                BriefDescription = "参加开窍仪式",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                RequiredStoryPhase = StoryPhase.Arrival,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_elder", Description = "与学堂家老对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "学堂家老" }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 1 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 100 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-02",
                Title = "学堂入门",
                Description = "进入学堂，学习蛊师基础知识",
                BriefDescription = "完成学堂训练",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                RequiredStoryPhase = StoryPhase.AwakeningCeremony,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_instructor", Description = "与学堂教头对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "学堂教头" },
                    new QuestObjective { ObjectiveID = "collect_yuanshi", Description = "收集元石", Type = ObjectiveType.CollectItem, TargetAmount = 5, TargetItemType = ModContent.ItemType<global::VerminLordMod.Content.Items.Consumables.YuanS>() },
                    new QuestObjective { ObjectiveID = "kill_beasts", Description = "猎杀荒兽", Type = ObjectiveType.KillNPCType, TargetAmount = 3 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 200 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-03",
                Title = "药堂求助",
                Description = "药堂家老需要帮助采集药材，这是获得家族认可的机会",
                BriefDescription = "帮助药堂采集药材",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                RequiredStoryPhase = StoryPhase.SchoolTraining,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_medicine", Description = "与药堂家老对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "药堂家老" },
                    new QuestObjective { ObjectiveID = "collect_herbs", Description = "采集灵草", Type = ObjectiveType.CollectItem, TargetAmount = 3 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 5, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 300 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-04",
                Title = "贾金生之死",
                Description = "贾金生被发现死在荒野中，你需要做出选择",
                BriefDescription = "调查贾金生之死",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                RequiredStoryPhase = StoryPhase.MedicineRequest,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_tiexue", Description = "与铁血冷对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "铁血冷" },
                    new QuestObjective { ObjectiveID = "search_body", Description = "搜索贾金生尸体", Type = ObjectiveType.SearchNode, TargetAmount = 1 },
                    new QuestObjective { ObjectiveID = "talk_fangyuan", Description = "与方源对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "方源" }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 500 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-05",
                Title = "花酒行者传承",
                Description = "在青茅山深处发现了花酒行者的传承",
                BriefDescription = "获取花酒行者传承",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.JiaJinShengDeath,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "enter_cave", Description = "进入花酒洞府", Type = ObjectiveType.EnterTerritory, TargetAmount = 1 },
                    new QuestObjective { ObjectiveID = "defeat_will", Description = "击败花酒行者意志", Type = ObjectiveType.KillNPC, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 1 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 800 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-06",
                Title = "家族认可",
                Description = "获得古月家族的正式认可，成为正式蛊师",
                BriefDescription = "获得家族认可",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.HuaJiuInheritance,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "reach_rep", Description = "达到古月家族声望等级3", Type = ObjectiveType.ReachRepLevel, TargetAmount = 3, TargetFaction = FactionID.GuYue },
                    new QuestObjective { ObjectiveID = "talk_chief", Description = "与古月博对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "古月博" }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 1000 }
                }
            });

            // === Stage2: 二转·家族争锋 ===
            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-07",
                Title = "三寨大比",
                Description = "参加青茅山三寨大比，展示你的实力",
                BriefDescription = "参加三寨大比",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.PreTournament,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_chief_tournament", Description = "与古月博对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "古月博" },
                    new QuestObjective { ObjectiveID = "win_final", Description = "赢得决赛", Type = ObjectiveType.KillNPC, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 50 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 1500 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-08",
                Title = "天鹤来袭",
                Description = "天鹤上人袭击青茅山，保卫家园！",
                BriefDescription = "抵御天鹤上人",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.TournamentFinal,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "survive_waves", Description = "抵御天鹤攻击", Type = ObjectiveType.DefendWave, TargetAmount = 3 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 10, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 2000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-09",
                Title = "白凝冰的抉择",
                Description = "白凝冰的空窍无法支撑，她做出了惊人的决定",
                BriefDescription = "面对白凝冰的抉择",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.TianHeAttack,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_bainingbing", Description = "与白凝冰对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "白凝冰" }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 1500 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-10",
                Title = "血祭之夜",
                Description = "青茅山发生了不可挽回的事件……",
                BriefDescription = "经历血祭之夜",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.BaiNingBingIceSeal,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_fangyuan_blood", Description = "与方源对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "方源" },
                    new QuestObjective { ObjectiveID = "survive_blood", Description = "在血祭中存活", Type = ObjectiveType.DefendWave, TargetAmount = 5 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 3000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-11",
                Title = "地脉守护者",
                Description = "击败地脉守护者，打破青茅山的封印",
                BriefDescription = "击败地脉守护者",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.BloodSacrifice,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "defeat_guardian", Description = "击败地脉守护者", Type = ObjectiveType.KillNPC, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 1 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 5000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-12",
                Title = "离开青茅山",
                Description = "青茅山已不再安全，是时候踏入更广阔的世界了",
                BriefDescription = "离开青茅山",
                Type = QuestType.MainStory,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                RequiredStoryPhase = StoryPhase.LeftQingMao,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_chief_leave", Description = "与古月博告别", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "古月博" },
                    new QuestObjective { ObjectiveID = "get_letter", Description = "获取推荐信", Type = ObjectiveType.CollectItem, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.StoryProgress, Amount = 1 }
                }
            });

            // === Stage3: 三转·南疆流浪 ===
            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-13",
                Title = "南疆初到",
                Description = "到达南疆散修营地，开始新的生活",
                BriefDescription = "到达南疆",
                Type = QuestType.MainStory,
                RequiredGuLevel = 3,
                RequiredStoryPhase = StoryPhase.SouthBorderArrival,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "enter_south", Description = "进入南疆荒野", Type = ObjectiveType.EnterTerritory, TargetAmount = 1 },
                    new QuestObjective { ObjectiveID = "talk_taibai", Description = "与太白云生对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 1, TargetNPCName = "太白云生" }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 2000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-14",
                Title = "商心慈",
                Description = "与商心慈建立关系，了解南疆的局势",
                BriefDescription = "帮助商心慈",
                Type = QuestType.MainStory,
                RequiredGuLevel = 3,
                RequiredStoryPhase = StoryPhase.ShangXinCiMeet,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "talk_shangxinci", Description = "与商心慈对话", Type = ObjectiveType.TalkToNPC, TargetAmount = 3, TargetNPCName = "商心慈" },
                    new QuestObjective { ObjectiveID = "collect_herbs_south", Description = "采集南疆药材", Type = ObjectiveType.CollectItem, TargetAmount = 5 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 1 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 3000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-15",
                Title = "三王传承",
                Description = "完成三王传承副本，获得真传力量",
                BriefDescription = "完成三王传承",
                Type = QuestType.MainStory,
                RequiredGuLevel = 3,
                RequiredStoryPhase = StoryPhase.ThreeKingsInheritance,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "defeat_inheritance", Description = "击败传承守护者", Type = ObjectiveType.KillNPC, TargetAmount = 1 },
                    new QuestObjective { ObjectiveID = "collect_fragment", Description = "获取真传碎片", Type = ObjectiveType.CollectItem, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 3 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 5000 }
                }
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "MQ-16",
                Title = "春秋蝉",
                Description = "在三王传承深处发现了春秋蝉的残影",
                BriefDescription = "获取春秋蝉残影",
                Type = QuestType.MainStory,
                RequiredGuLevel = 3,
                RequiredStoryPhase = StoryPhase.ChunQiuChanFragment,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveID = "collect_cicada", Description = "获取春秋蝉残影", Type = ObjectiveType.CollectItem, TargetAmount = 1 }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Item, Amount = 1 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 3000 }
                }
            });

            // === Stage4~6: 简略注册 ===
            string[] stage4to6Quests = new[]
            {
                "MQ-17|义天山异变|YiTianShanAppears",
                "MQ-18|大同风|DaTongFeng",
                "MQ-19|方源真面目|FangYuanReveal",
                "MQ-20|义天山终战|YiTianShanComplete",
                "MQ-21|北原初到|NorthDesertArrival",
                "MQ-22|王庭结盟|WangTingAlly",
                "MQ-23|长生天|ChangShengTianContact",
                "MQ-24|太白云生|TaiBaiYunShengDeath",
                "MQ-25|天庭前奏|HeavenPrelude",
                "MQ-26|宿命大战|DestinyWarBegin",
                "MQ-27|龙公之战|LongGongPhase1",
                "MQ-28|再战龙公|LongGongPhase2",
                "MQ-29|宿命碎裂|DestinyShattered",
                "MQ-30|升仙|Ascension"
            };

            foreach (var entry in stage4to6Quests)
            {
                var parts = entry.Split('|');
                if (parts.Length == 3 && System.Enum.TryParse<StoryPhase>(parts[2], out var sp))
                {
                    RegisterQuest(new QuestDefinition
                    {
                        QuestID = parts[0],
                        Title = parts[1],
                        Description = parts[1],
                        BriefDescription = parts[1],
                        Type = QuestType.MainStory,
                        RequiredStoryPhase = sp
                    });
                }
            }
        }

        private void RegisterGuYueFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "guyue_first_hunt",
                Title = "初次狩猎",
                Description = "古月家族的新人试炼：猎杀10只史莱姆证明你的实力。",
                BriefDescription = "猎杀10只史莱姆",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_slimes",
                        Description = "猎杀史莱姆",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 10,
                        TargetNPCType = Terraria.ID.NPCID.GreenSlime,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "guyue_collect_herbs",
                Title = "采集灵草",
                Description = "为家族药堂采集5株月光草。",
                BriefDescription = "采集5株月光草",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_daybloom",
                        Description = "采集月光草",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 5,
                        TargetItemType = Terraria.ID.ItemID.Daybloom,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 15, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 3 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "guyue_prove_strength",
                Title = "证明实力",
                Description = "击败克苏鲁之眼，向家族证明你的实力。",
                BriefDescription = "击败克苏鲁之眼",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.GuYue,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_eoc",
                        Description = "击败克苏鲁之眼",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 1,
                        TargetNPCType = Terraria.ID.NPCID.EyeofCthulhu,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 50, TargetFaction = FactionID.GuYue },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 20 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 100 },
                },
            });
        }

        private void RegisterBaiFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "bai_ice_trial",
                Title = "寒冰试炼",
                Description = "白家入门试炼：在雪原中收集冰晶，证明你的耐寒能力。",
                BriefDescription = "收集10个冰晶",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Bai,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_ice",
                        Description = "收集冰块",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 10,
                        TargetItemType = Terraria.ID.ItemID.IceBlock,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.Bai },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bai_jade_craft",
                Title = "玉器炼制",
                Description = "为白家炼制玉器，需要收集宝石和石材。",
                BriefDescription = "收集5颗宝石",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Bai,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_gems",
                        Description = "收集宝石",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 5,
                        TargetItemType = Terraria.ID.ItemID.Amethyst,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 30, TargetFaction = FactionID.Bai },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 10 },
                    new QuestReward { Type = QuestRewardType.Item, ItemType = Terraria.ID.ItemID.Emerald, Amount = 1 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bai_frozen_heart",
                Title = "冰封之心",
                Description = "击败冰霜巨人，获取冰封之心献给白家族长。",
                BriefDescription = "击败冰霜巨人",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Bai,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_ice_golem",
                        Description = "击败冰霜巨人",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 1,
                        TargetNPCType = Terraria.ID.NPCID.IceGolem,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 60, TargetFaction = FactionID.Bai },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 25 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 150 },
                },
            });
        }

        private void RegisterXiongFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "xiong_bone_collect",
                Title = "骨材收集",
                Description = "熊家锻造需要大量骨材，去地下收集骨头。",
                BriefDescription = "收集20根骨头",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Xiong,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_bones",
                        Description = "收集骨头",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 20,
                        TargetItemType = Terraria.ID.ItemID.Bone,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.Xiong },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "xiong_iron_smelt",
                Title = "铁骨锻造",
                Description = "为熊家铁匠铺收集铁矿石，锻造武器。",
                BriefDescription = "收集15个铁矿石",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Xiong,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_iron",
                        Description = "收集铁矿石",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 15,
                        TargetItemType = Terraria.ID.ItemID.IronOre,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 25, TargetFaction = FactionID.Xiong },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 8 },
                    new QuestReward { Type = QuestRewardType.Item, ItemType = Terraria.ID.ItemID.IronBar, Amount = 3 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "xiong_skeleton_king",
                Title = "骷髅之王",
                Description = "击败骷髅王，证明熊家的勇武之名。",
                BriefDescription = "击败骷髅王",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Xiong,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_skeletron",
                        Description = "击败骷髅王",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 1,
                        TargetNPCType = Terraria.ID.NPCID.SkeletronHead,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 70, TargetFaction = FactionID.Xiong },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 30 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 200 },
                },
            });
        }

        private void RegisterTieFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "tie_forge_apprentice",
                Title = "锻炉学徒",
                Description = "铁家入门试炼：在锻炉旁工作，收集燃料。",
                BriefDescription = "收集20个木材",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Tie,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_wood",
                        Description = "收集木材",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 20,
                        TargetItemType = Terraria.ID.ItemID.Wood,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 15, TargetFaction = FactionID.Tie },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 3 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "tie_weapon_craft",
                Title = "武器锻造",
                Description = "为铁家锻造一批武器，需要大量金属。",
                BriefDescription = "收集10个铁锭",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Tie,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_bars",
                        Description = "收集铁锭",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 10,
                        TargetItemType = Terraria.ID.ItemID.IronBar,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 30, TargetFaction = FactionID.Tie },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 10 },
                    new QuestReward { Type = QuestRewardType.Item, ItemType = Terraria.ID.ItemID.SilverBar, Amount = 3 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "tie_hellforge",
                Title = "地狱锻炉",
                Description = "深入地狱，收集狱石为铁家打造传说武器。",
                BriefDescription = "收集15个狱石",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Tie,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_hellstone",
                        Description = "收集狱石",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 15,
                        TargetItemType = Terraria.ID.ItemID.Hellstone,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 60, TargetFaction = FactionID.Tie },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 25 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 150 },
                },
            });
        }

        private void RegisterWangFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "wang_water_test",
                Title = "水之试炼",
                Description = "王家入门试炼：在海洋中收集珊瑚。",
                BriefDescription = "收集5个珊瑚",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Wang,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_coral",
                        Description = "收集珊瑚",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 5,
                        TargetItemType = Terraria.ID.ItemID.Coral,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.Wang },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "wang_crystal_grow",
                Title = "水晶培育",
                Description = "王家需要水晶碎片用于阵法研究。",
                BriefDescription = "收集8个水晶碎片",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Wang,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_crystal",
                        Description = "收集水晶碎片",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 8,
                        TargetItemType = Terraria.ID.ItemID.CrystalShard,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 30, TargetFaction = FactionID.Wang },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 12 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "wang_water_elemental",
                Title = "水元素之核",
                Description = "击败水元素，获取水之精华。",
                BriefDescription = "击败水元素",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Wang,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_water_elemental",
                        Description = "击败水元素",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 3,
                        TargetNPCType = Terraria.ID.NPCID.WaterSphere,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 50, TargetFaction = FactionID.Wang },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 20 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 120 },
                },
            });
        }

        private void RegisterZhaoFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "zhao_shadow_test",
                Title = "暗影试炼",
                Description = "赵家入门试炼：在黑暗中收集暗影材料。",
                BriefDescription = "收集10个暗影鳞片",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Zhao,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_shadow_scale",
                        Description = "收集暗影鳞片",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 10,
                        TargetItemType = Terraria.ID.ItemID.ShadowScale,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.Zhao },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "zhao_infiltration",
                Title = "潜入任务",
                Description = "潜入敌对领地，获取情报。",
                BriefDescription = "进入敌对领地",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Zhao,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "enter_territory",
                        Description = "进入敌对领地",
                        Type = ObjectiveType.EnterTerritory,
                        TargetAmount = 1,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 35, TargetFaction = FactionID.Zhao },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 15 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "zhao_wall_of_flesh",
                Title = "血肉之墙",
                Description = "击败血肉之墙，为赵家开辟地狱通道。",
                BriefDescription = "击败血肉之墙",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Zhao,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_wall_of_flesh",
                        Description = "击败血肉之墙",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 1,
                        TargetNPCType = Terraria.ID.NPCID.WallofFlesh,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 80, TargetFaction = FactionID.Zhao },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 40 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 300 },
                },
            });
        }

        private void RegisterJiaFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "jia_trade_apprentice",
                Title = "商道学徒",
                Description = "贾家入门试炼：收集金币证明你的商业头脑。",
                BriefDescription = "收集5金币",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Jia,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_gold",
                        Description = "收集金币",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 5,
                        TargetItemType = Terraria.ID.ItemID.GoldCoin,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 15, TargetFaction = FactionID.Jia },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 8 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "jia_silk_trade",
                Title = "丝绸贸易",
                Description = "为贾家收集丝绸，用于贸易。",
                BriefDescription = "收集15个丝绸",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Jia,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_silk",
                        Description = "收集丝绸",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 15,
                        TargetItemType = Terraria.ID.ItemID.Silk,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 25, TargetFaction = FactionID.Jia },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 10 },
                    new QuestReward { Type = QuestRewardType.Item, ItemType = Terraria.ID.ItemID.GoldCoin, Amount = 1 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "jia_rare_trade",
                Title = "稀有贸易",
                Description = "为贾家收集稀有材料用于高端贸易。",
                BriefDescription = "收集3个光明之魂",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Jia,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_soul_light",
                        Description = "收集光明之魂",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 3,
                        TargetItemType = Terraria.ID.ItemID.SoulofLight,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 40, TargetFaction = FactionID.Jia },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 20 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 100 },
                },
            });
        }

        private void RegisterScatteredFactionQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "scattered_survival",
                Title = "荒野求生",
                Description = "散修入门试炼：在荒野中生存并收集资源。",
                BriefDescription = "收集15个蘑菇",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_mushroom",
                        Description = "收集蘑菇",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 15,
                        TargetItemType = Terraria.ID.ItemID.Mushroom,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 10, TargetFaction = FactionID.Scattered },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 3 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "scattered_prove_worth",
                Title = "证明价值",
                Description = "散修需要证明自己的价值，猎杀强大的怪物。",
                BriefDescription = "猎杀5只恶魔之眼",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 2,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_demon_eye",
                        Description = "猎杀恶魔之眼",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 5,
                        TargetNPCType = Terraria.ID.NPCID.DemonEye,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 20, TargetFaction = FactionID.Scattered },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 8 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "scattered_ancient_ruins",
                Title = "远古遗迹",
                Description = "探索远古遗迹，寻找失落的传承。",
                BriefDescription = "探索地下遗迹",
                Type = QuestType.FactionQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "search_node",
                        Description = "探索资源节点",
                        Type = ObjectiveType.SearchNode,
                        TargetAmount = 3,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.Reputation, Amount = 40, TargetFaction = FactionID.Scattered },
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 15 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 100 },
                },
            });
        }

        private void RegisterDailyQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "daily_hunt",
                Title = "每日狩猎",
                Description = "猎杀任意怪物，收集战利品。",
                BriefDescription = "猎杀20只怪物",
                Type = QuestType.DailyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                IsRepeatable = true,
                ExpireDays = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_any",
                        Description = "猎杀任意怪物",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 20,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 3 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 30 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "daily_collect_ore",
                Title = "采矿委托",
                Description = "采集矿石资源。",
                BriefDescription = "采集10个矿石",
                Type = QuestType.DailyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                IsRepeatable = true,
                ExpireDays = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_ore",
                        Description = "采集矿石",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 10,
                        TargetItemType = Terraria.ID.ItemID.IronOre,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 5 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "daily_fish",
                Title = "钓鱼委托",
                Description = "为酒楼收集新鲜鱼类。",
                BriefDescription = "钓5条鱼",
                Type = QuestType.DailyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                IsRepeatable = true,
                ExpireDays = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_fish",
                        Description = "收集鱼类",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 5,
                        TargetItemType = Terraria.ID.ItemID.Bass,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 4 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 20 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "daily_herb_collect",
                Title = "采药委托",
                Description = "为药堂采集药材。",
                BriefDescription = "采集8株草药",
                Type = QuestType.DailyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                IsRepeatable = true,
                ExpireDays = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "collect_herb",
                        Description = "采集草药",
                        Type = ObjectiveType.CollectItem,
                        TargetAmount = 8,
                        TargetItemType = Terraria.ID.ItemID.Daybloom,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 3 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 25 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "daily_patrol",
                Title = "巡逻委托",
                Description = "在家族领地巡逻，确保安全。",
                BriefDescription = "巡逻领地",
                Type = QuestType.DailyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 2,
                IsRepeatable = true,
                ExpireDays = 1,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_any_patrol",
                        Description = "猎杀入侵怪物",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 10,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 6 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 40 },
                },
            });
        }

        private void RegisterBountyQuests()
        {
            RegisterQuest(new QuestDefinition
            {
                QuestID = "bounty_skeleton",
                Title = "悬赏：骷髅",
                Description = "清除墓地中的骷髅，为民除害。",
                BriefDescription = "猎杀15只骷髅",
                Type = QuestType.BountyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 2,
                IsRepeatable = true,
                ExpireDays = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_skeleton",
                        Description = "猎杀骷髅",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 15,
                        TargetNPCType = Terraria.ID.NPCID.Skeleton,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 10 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 50 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bounty_zombie",
                Title = "悬赏：僵尸",
                Description = "清除夜间出没的僵尸群。",
                BriefDescription = "猎杀20只僵尸",
                Type = QuestType.BountyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 1,
                IsRepeatable = true,
                ExpireDays = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_zombie",
                        Description = "猎杀僵尸",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 20,
                        TargetNPCType = Terraria.ID.NPCID.Zombie,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 8 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 30 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bounty_hornet",
                Title = "悬赏：毒蜂",
                Description = "清除丛林中的毒蜂巢穴。",
                BriefDescription = "猎杀15只毒蜂",
                Type = QuestType.BountyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 2,
                IsRepeatable = true,
                ExpireDays = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_hornet",
                        Description = "猎杀毒蜂",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 15,
                        TargetNPCType = Terraria.ID.NPCID.Hornet,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 12 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 60 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bounty_hell_bat",
                Title = "悬赏：地狱蝙蝠",
                Description = "清除地狱中的蝙蝠群。",
                BriefDescription = "猎杀10只地狱蝙蝠",
                Type = QuestType.BountyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 3,
                IsRepeatable = true,
                ExpireDays = 3,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_hell_bat",
                        Description = "猎杀地狱蝙蝠",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 10,
                        TargetNPCType = Terraria.ID.NPCID.Hellbat,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 15 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 80 },
                },
            });

            RegisterQuest(new QuestDefinition
            {
                QuestID = "bounty_mimic",
                Title = "悬赏：宝箱怪",
                Description = "清除地下潜伏的宝箱怪。",
                BriefDescription = "猎杀3只宝箱怪",
                Type = QuestType.BountyQuest,
                IssuingFaction = FactionID.Scattered,
                RequiredGuLevel = 3,
                IsRepeatable = true,
                ExpireDays = 5,
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveID = "kill_mimic",
                        Description = "猎杀宝箱怪",
                        Type = ObjectiveType.KillNPCType,
                        TargetAmount = 3,
                        TargetNPCType = Terraria.ID.NPCID.Mimic,
                    }
                },
                Rewards = new List<QuestReward>
                {
                    new QuestReward { Type = QuestRewardType.YuanStones, Amount = 20 },
                    new QuestReward { Type = QuestRewardType.QiExp, Amount = 100 },
                },
            });
        }

        private void RegisterQuest(QuestDefinition quest)
        {
            QuestRegistry[quest.QuestID] = quest;
        }

        public List<QuestDefinition> GetAvailableQuests(Player player)
        {
            var tracker = player.GetModPlayer<QuestTrackerPlayer>();
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            var storyPhase = StoryManager.Instance.GetPhase(player);

            var available = new List<QuestDefinition>();
            foreach (var quest in QuestRegistry.Values)
            {
                if (tracker.HasActiveQuest(quest.QuestID)) continue;
                if (tracker.HasCompletedQuest(quest.QuestID) && !quest.IsRepeatable) continue;

                if (qiRealm.GuLevel < quest.RequiredGuLevel) continue;
                if (quest.IssuingFaction != FactionID.Scattered)
                {
                    var rep = guWorld.GetRepLevel(quest.IssuingFaction);
                    if ((int)rep < quest.RequiredRepLevel) continue;
                }
                if (storyPhase < quest.RequiredStoryPhase) continue;
                if (!AllPrerequisitesCompleted(tracker, quest)) continue;

                available.Add(quest);
            }
            return available;
        }

        private bool AllPrerequisitesCompleted(QuestTrackerPlayer tracker, QuestDefinition quest)
        {
            if (quest.PrerequisiteQuestIDs == null || quest.PrerequisiteQuestIDs.Count == 0)
                return true;

            foreach (var prereqID in quest.PrerequisiteQuestIDs)
            {
                if (!tracker.HasCompletedQuest(prereqID))
                    return false;
            }
            return true;
        }

        public QuestDefinition GetQuestDefinition(string questID)
        {
            QuestRegistry.TryGetValue(questID, out var def);
            return def;
        }

        public QuestInstance CreateQuestInstance(QuestDefinition definition)
        {
            var instance = new QuestInstance
            {
                QuestID = definition.QuestID,
                State = QuestState.Active,
                AcceptedDay = (int)(Main.time / 36000),
                ExpireDay = definition.ExpireDays > 0
                    ? (int)(Main.time / 36000) + definition.ExpireDays
                    : 0,
                Objectives = new List<QuestObjective>()
            };

            foreach (var objDef in definition.Objectives)
            {
                instance.Objectives.Add(new QuestObjective
                {
                    ObjectiveID = objDef.ObjectiveID,
                    Description = objDef.Description,
                    Type = objDef.Type,
                    TargetAmount = objDef.TargetAmount,
                    CurrentAmount = 0,
                    TargetNPCName = objDef.TargetNPCName,
                    TargetItemType = objDef.TargetItemType,
                    TargetNPCType = objDef.TargetNPCType,
                    TargetFaction = objDef.TargetFaction
                });
            }

            return instance;
        }

        public void CompleteQuest(Player player, string questID)
        {
            var tracker = player.GetModPlayer<QuestTrackerPlayer>();
            var instance = tracker.GetActiveQuest(questID);
            if (instance == null) return;

            instance.State = QuestState.Completed;
            GrantRewards(player, QuestRegistry[questID]);
            tracker.RemoveActiveQuest(questID);
            tracker.AddCompletedQuest(questID);

            EventBus.Publish(new QuestCompletedEvent
            {
                PlayerID = player.whoAmI,
                QuestID = questID,
                QuestType = QuestRegistry[questID].Type,
                IssuingFaction = QuestRegistry[questID].IssuingFaction
            });
        }

        private void GrantRewards(Player player, QuestDefinition quest)
        {
            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();

            foreach (var reward in quest.Rewards)
            {
                switch (reward.Type)
                {
                    case QuestRewardType.YuanStones:
                        player.QuickSpawnItem(player.GetSource_GiftOrReward(),
                            ModContent.ItemType<Content.Items.Consumables.YuanS>(), reward.Amount);
                        break;
                    case QuestRewardType.Reputation:
                        guWorld.AddReputation(reward.TargetFaction, reward.Amount, $"完成任务: {quest.Title}");
                        break;
                    case QuestRewardType.QiExp:
                        qiRealm.BreakthroughProgress += reward.Amount;
                        break;
                    case QuestRewardType.Item:
                        if (reward.ItemType > 0)
                        {
                            player.QuickSpawnItem(player.GetSource_GiftOrReward(), reward.ItemType, reward.Amount);
                        }
                        break;
                }
            }

            if (player.whoAmI == Main.myPlayer)
                Main.NewText($"任务完成：{quest.Title}！", Color.Gold);
        }

        public void OnQuestCompleted(QuestCompletedEvent evt)
        {
            // 奖励已在CompleteQuest中发放，此处不再重复
        }

        private void OnNPCDeathForQuest(NPCDeathEvent evt)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active) continue;

                var tracker = player.GetModPlayer<QuestTrackerPlayer>();
                foreach (var quest in tracker.ActiveQuests)
                {
                    foreach (var obj in quest.Objectives)
                    {
                        if (obj.Type == ObjectiveType.KillNPCType && !obj.IsCompleted)
                        {
                            if (obj.TargetNPCType == 0 || evt.NPCType == obj.TargetNPCType)
                            {
                                obj.CurrentAmount++;
                                if (obj.CurrentAmount >= obj.TargetAmount)
                                {
                                    if (player.whoAmI == Main.myPlayer)
                                        Main.NewText($"任务目标完成：{obj.Description}", Color.Cyan);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateObjectiveProgress(Player player, string questID, string objectiveID, int amount)
        {
            var tracker = player.GetModPlayer<QuestTrackerPlayer>();
            var quest = tracker.GetActiveQuest(questID);
            if (quest == null) return;

            foreach (var obj in quest.Objectives)
            {
                if (obj.ObjectiveID == objectiveID && !obj.IsCompleted)
                {
                    obj.CurrentAmount = System.Math.Min(obj.TargetAmount, obj.CurrentAmount + amount);
                    if (obj.CurrentAmount >= obj.TargetAmount)
                    {
                        if (player.whoAmI == Main.myPlayer)
                            Main.NewText($"任务目标完成：{obj.Description}", Color.Cyan);
                    }
                    break;
                }
            }
        }
    }

    // ============================================================
    // QuestTrackerPlayer — 玩家任务追踪器
    //
    // 职责：
    // 1. 存储玩家的活跃任务列表和已完成任务记录
    // 2. 提供任务领取/放弃/完成接口
    // 3. 每帧更新任务目标进度
    // 4. Save/Load 任务进度
    //
    // TODO:
    //   - 实现目标进度自动更新（击杀/收集/对话等）
    //   - 实现任务超时检查
    //   - 实现任务UI交互
    // ============================================================

    public class QuestTrackerPlayer : ModPlayer
    {
        public List<QuestInstance> ActiveQuests = new();
        public HashSet<string> CompletedQuestIDs = new();
        public Dictionary<string, int> CompletedQuestCounts = new();

        public bool HasActiveQuest(string questID)
        {
            return ActiveQuests.Exists(q => q.QuestID == questID);
        }

        public bool HasCompletedQuest(string questID)
        {
            return CompletedQuestIDs.Contains(questID);
        }

        public QuestInstance GetActiveQuest(string questID)
        {
            return ActiveQuests.Find(q => q.QuestID == questID);
        }

        public void AcceptQuest(string questID)
        {
            var system = QuestSystem.Instance;
            var definition = system.GetQuestDefinition(questID);
            if (definition == null) return;

            var instance = system.CreateQuestInstance(definition);
            ActiveQuests.Add(instance);

            Main.NewText($"接受任务：{definition.Title}", Microsoft.Xna.Framework.Color.Yellow);
        }

        public void AbandonQuest(string questID)
        {
            var instance = GetActiveQuest(questID);
            if (instance == null) return;

            ActiveQuests.Remove(instance);

            var definition = QuestSystem.Instance.GetQuestDefinition(questID);
            Main.NewText($"放弃任务：{definition?.Title ?? questID}", Microsoft.Xna.Framework.Color.Gray);
        }

        public void RemoveActiveQuest(string questID)
        {
            ActiveQuests.RemoveAll(q => q.QuestID == questID);
        }

        public void AddCompletedQuest(string questID)
        {
            CompletedQuestIDs.Add(questID);
            if (!CompletedQuestCounts.ContainsKey(questID))
                CompletedQuestCounts[questID] = 0;
            CompletedQuestCounts[questID]++;
        }

        public void UpdateObjectiveProgress(ObjectiveType type, int amount, int targetType = -1)
        {
            // TODO: 实现目标进度自动更新
            foreach (var quest in ActiveQuests)
            {
                foreach (var obj in quest.Objectives)
                {
                    if (obj.Type == type && (targetType == -1 || obj.TargetItemType == targetType || obj.TargetNPCType == targetType))
                    {
                        obj.CurrentAmount += amount;
                    }
                }

                if (quest.IsCompleted)
                {
                    QuestSystem.Instance.CompleteQuest(Player, quest.QuestID);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            var activeList = new List<TagCompound>();
            foreach (var quest in ActiveQuests)
            {
                var objList = new List<TagCompound>();
                foreach (var obj in quest.Objectives)
                {
                    objList.Add(new TagCompound
                    {
                        ["objID"] = obj.ObjectiveID,
                        ["current"] = obj.CurrentAmount,
                    });
                }

                activeList.Add(new TagCompound
                {
                    ["questID"] = quest.QuestID,
                    ["state"] = (int)quest.State,
                    ["acceptedDay"] = quest.AcceptedDay,
                    ["expireDay"] = quest.ExpireDay,
                    ["objectives"] = objList,
                });
            }
            tag["activeQuests"] = activeList;
            tag["completedQuestIDs"] = new List<string>(CompletedQuestIDs);

            var countTag = new TagCompound();
            foreach (var kvp in CompletedQuestCounts)
                countTag[kvp.Key] = kvp.Value;
            tag["completedCounts"] = countTag;
        }

        public override void LoadData(TagCompound tag)
        {
            ActiveQuests.Clear();
            CompletedQuestIDs.Clear();
            CompletedQuestCounts.Clear();

            var completedList = tag.GetList<string>("completedQuestIDs");
            if (completedList != null)
                CompletedQuestIDs = new HashSet<string>(completedList);

            var activeList = tag.GetList<TagCompound>("activeQuests");
            if (activeList != null)
            {
                foreach (var t in activeList)
                {
                    var quest = new QuestInstance
                    {
                        QuestID = t.GetString("questID"),
                        State = (QuestState)t.GetInt("state"),
                        AcceptedDay = t.GetInt("acceptedDay"),
                        ExpireDay = t.GetInt("expireDay"),
                        Objectives = new List<QuestObjective>(),
                    };

                    var objList = t.GetList<TagCompound>("objectives");
                    if (objList != null)
                    {
                        foreach (var ot in objList)
                        {
                            quest.Objectives.Add(new QuestObjective
                            {
                                ObjectiveID = ot.GetString("objID"),
                                CurrentAmount = ot.GetInt("current"),
                            });
                        }
                    }

                    var def = QuestSystem.Instance?.GetQuestDefinition(quest.QuestID);
                    if (def != null)
                    {
                        foreach (var obj in quest.Objectives)
                        {
                            var defObj = def.Objectives.Find(o => o.ObjectiveID == obj.ObjectiveID);
                            if (defObj != null)
                            {
                                obj.Description = defObj.Description;
                                obj.Type = defObj.Type;
                                obj.TargetAmount = defObj.TargetAmount;
                                obj.TargetNPCType = defObj.TargetNPCType;
                                obj.TargetItemType = defObj.TargetItemType;
                            }
                        }
                    }

                    ActiveQuests.Add(quest);
                }
            }

            if (tag.TryGet("completedCounts", out TagCompound countTag))
            {
                foreach (NodeResourceType resType in System.Enum.GetValues<NodeResourceType>())
                {
                    string key = resType.ToString();
                    if (countTag.ContainsKey(key))
                        CompletedQuestCounts[key] = countTag.GetInt(key);
                }
            }
        }

        public override void PreUpdate()
        {
            for (int i = ActiveQuests.Count - 1; i >= 0; i--)
            {
                var quest = ActiveQuests[i];
                if (quest.IsExpired)
                {
                    quest.State = QuestState.Expired;
                    ActiveQuests.RemoveAt(i);
                    if (Player.whoAmI == Main.myPlayer)
                        Main.NewText($"任务已过期：{quest.QuestID}", Microsoft.Xna.Framework.Color.Gray);
                }
            }
        }
    }

    // ============================================================
    // 任务相关事件
    // ============================================================

    public class QuestCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
        public QuestType QuestType;
        public FactionID IssuingFaction;
    }

    public class QuestAcceptedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
    }

    public class QuestAbandonedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
    }
}