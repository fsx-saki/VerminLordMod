using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    public enum InheritanceRarity
    {
        Common = 0,     // 普通传承 — 一转~二转功法/蛊方
        Rare = 1,       // 稀有传承 — 三转~四转功法/蛊方
        Epic = 2,       // 史诗传承 — 五转~六转功法/蛊方
        Legendary = 3,  // 传说传承 — 七转以上功法/蛊方
        Mythic = 4,     // 神话传承 — 仙人遗泽
    }

    public enum InheritanceType
    {
        GuRecipe,           // 蛊方传承 — 解锁特殊蛊虫配方
        CultivationMethod,  // 功法传承 — 修炼效率/特殊能力
        FormationManual,    // 阵法传承 — 解锁高级阵法
        AlchemyManual,      // 炼丹传承 — 解锁高级丹方
        WeaponTechnique,    // 武技传承 — 解锁特殊攻击
        Bloodline,          // 血脉传承 — 永久属性加成
        SecretArt,          // 秘术传承 — 特殊技能
    }

    public enum InheritanceState
    {
        Hidden,             // 未发现
        Discovered,         // 已发现（显示位置标记）
        Entered,            // 已进入（小世界/副本）
        InProgress,         // 试炼进行中
        Completed,          // 已完成
        Failed,             // 试炼失败
        Expired,            // 已过期（限时传承消失）
    }

    public class InheritanceChallenge
    {
        public string ChallengeID;
        public string Description;
        public int RequiredLevel;
        public float Difficulty;
        public List<InheritanceType> RewardTypes = new();
        public int RewardItemID;
        public int TimeLimitTicks;
        public bool IsTimeLimited => TimeLimitTicks > 0;
    }

    public class InheritanceInstance
    {
        public string InheritanceID;
        public string DisplayName;
        public InheritanceType Type;
        public InheritanceRarity Rarity;
        public InheritanceState State;
        public Vector2 EntrancePosition;
        int SubworldID;
        public List<InheritanceChallenge> Challenges = new();
        public int CurrentChallengeIndex;
        public FactionID OriginalOwnerFaction;
        public int DiscoveryDay;
        public int ExpiryDay;
        public bool HasExpiry => ExpiryDay > 0;
        public bool IsExpired => HasExpiry && Main.GameUpdateCount / 36000 > ExpiryDay;
    }

    public class InheritanceSystem : ModSystem
    {
        public static InheritanceSystem Instance => ModContent.GetInstance<InheritanceSystem>();

        public Dictionary<string, InheritanceInstance> KnownInheritances = new();
        public List<string> CompletedInheritances = new();

        public override void OnWorldLoad()
        {
            KnownInheritances.Clear();
            CompletedInheritances.Clear();
            RegisterDefaultInheritances();
        }

        private void RegisterDefaultInheritances()
        {
            // TODO: 注册世界中的传承秘境
            // MVA阶段：硬编码1-2个传承

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "guyue_ancestor",
                DisplayName = "古月先祖传承",
                Type = InheritanceType.GuRecipe,
                Rarity = InheritanceRarity.Rare,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.GuYue,
                DiscoveryDay = -1,
            });

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "wilderness_cave",
                DisplayName = "荒野洞府",
                Type = InheritanceType.CultivationMethod,
                Rarity = InheritanceRarity.Common,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.None,
                DiscoveryDay = -1,
            });
        }

        private void RegisterInheritance(InheritanceInstance instance)
        {
            KnownInheritances[instance.InheritanceID] = instance;
        }

        public void DiscoverInheritance(string id, Player discoverer)
        {
            if (!KnownInheritances.TryGetValue(id, out var instance)) return;
            if (instance.State != InheritanceState.Hidden) return;

            instance.State = InheritanceState.Discovered;
            instance.DiscoveryDay = (int)(Main.GameUpdateCount / 36000);

            EventBus.Publish(new InheritanceDiscoveredEvent
            {
                InheritanceID = id,
                DiscovererID = discoverer.whoAmI,
                Rarity = instance.Rarity,
            });

            if (discoverer.whoAmI == Main.myPlayer)
            {
                Main.NewText($"发现了传承秘境：{instance.DisplayName}！", Microsoft.Xna.Framework.Color.Gold);
            }
        }

        public void EnterInheritance(string id, Player player)
        {
            if (!KnownInheritances.TryGetValue(id, out var instance)) return;
            if (instance.State != InheritanceState.Discovered) return;

            instance.State = InheritanceState.Entered;

            // TODO: 传送玩家到传承小世界
            // SubworldLibrary 集成
        }

        public void CompleteInheritance(string id, Player player)
        {
            if (!KnownInheritances.TryGetValue(id, out var instance)) return;

            instance.State = InheritanceState.Completed;
            CompletedInheritances.Add(id);

            // TODO: 发放传承奖励

            EventBus.Publish(new InheritanceCompletedEvent
            {
                InheritanceID = id,
                PlayerID = player.whoAmI,
                Type = instance.Type,
                Rarity = instance.Rarity,
            });
        }

        public override void PostUpdateWorld()
        {
            // TODO: 检查传承过期
            // TODO: 随机发现触发
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存传承数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载传承数据
        }
    }

    public class InheritanceDiscoveredEvent : GuWorldEvent
    {
        public string InheritanceID;
        public int DiscovererID;
        public InheritanceRarity Rarity;
    }

    public class InheritanceCompletedEvent : GuWorldEvent
    {
        public string InheritanceID;
        public int PlayerID;
        public InheritanceType Type;
        public InheritanceRarity Rarity;
    }
}
