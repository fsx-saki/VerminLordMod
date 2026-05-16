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
        public bool IsExpired => HasExpiry && WorldTimeHelper.CurrentDay > ExpiryDay;
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

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "bai_jade_pavilion",
                DisplayName = "白家玉阁传承",
                Type = InheritanceType.AlchemyManual,
                Rarity = InheritanceRarity.Rare,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.Bai,
                DiscoveryDay = -1,
            });

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "xiong_war_heritage",
                DisplayName = "熊家战魂传承",
                Type = InheritanceType.WeaponTechnique,
                Rarity = InheritanceRarity.Epic,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.Xiong,
                DiscoveryDay = -1,
            });

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "tie_forge_secret",
                DisplayName = "铁家锻造秘传",
                Type = InheritanceType.GuRecipe,
                Rarity = InheritanceRarity.Rare,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.Tie,
                DiscoveryDay = -1,
            });

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "ancient_immortal_cave",
                DisplayName = "上古仙人洞府",
                Type = InheritanceType.Bloodline,
                Rarity = InheritanceRarity.Legendary,
                State = InheritanceState.Hidden,
                OriginalOwnerFaction = FactionID.None,
                DiscoveryDay = -1,
            });

            RegisterInheritance(new InheritanceInstance
            {
                InheritanceID = "forbidden_secret_art",
                DisplayName = "禁断秘术",
                Type = InheritanceType.SecretArt,
                Rarity = InheritanceRarity.Mythic,
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
            instance.DiscoveryDay = WorldTimeHelper.CurrentDay;

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

            if (instance.EntrancePosition != Vector2.Zero)
            {
                player.Teleport(instance.EntrancePosition, 1);
            }

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText($"进入传承秘境：{instance.DisplayName}！", Microsoft.Xna.Framework.Color.Gold);
            }
        }

        public void CompleteInheritance(string id, Player player)
        {
            if (!KnownInheritances.TryGetValue(id, out var instance)) return;

            instance.State = InheritanceState.Completed;
            CompletedInheritances.Add(id);

            DistributeRewards(instance, player);

            EventBus.Publish(new InheritanceCompletedEvent
            {
                InheritanceID = id,
                PlayerID = player.whoAmI,
                Type = instance.Type,
                Rarity = instance.Rarity,
            });
        }

        private void DistributeRewards(InheritanceInstance instance, Player player)
        {
            int yuanStoneAmount = instance.Rarity switch
            {
                InheritanceRarity.Common => Main.rand.Next(5, 15),
                InheritanceRarity.Rare => Main.rand.Next(15, 40),
                InheritanceRarity.Epic => Main.rand.Next(40, 80),
                InheritanceRarity.Legendary => Main.rand.Next(80, 150),
                InheritanceRarity.Mythic => Main.rand.Next(150, 300),
                _ => 5,
            };

            player.QuickSpawnItem(player.GetSource_GiftOrReward(),
                ModContent.ItemType<Content.Items.Consumables.YuanS>(), yuanStoneAmount);

            if (instance.Rarity >= InheritanceRarity.Rare)
            {
                int guCount = instance.Rarity switch
                {
                    InheritanceRarity.Rare => 1,
                    InheritanceRarity.Epic => 2,
                    InheritanceRarity.Legendary => 3,
                    InheritanceRarity.Mythic => 5,
                    _ => 0,
                };

                for (int i = 0; i < guCount; i++)
                {
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(),
                        ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(3, 10));
                }
            }

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText($"获得传承奖励：{instance.DisplayName}！获得{yuanStoneAmount}元石。",
                    Microsoft.Xna.Framework.Color.Gold);
            }
        }

        public override void PostUpdateWorld()
        {
            var expired = new List<string>();
            foreach (var kvp in KnownInheritances)
            {
                if (kvp.Value.IsExpired)
                {
                    kvp.Value.State = InheritanceState.Expired;
                    expired.Add(kvp.Key);
                }
            }
            foreach (var id in expired)
                KnownInheritances.Remove(id);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var kvp in KnownInheritances)
            {
                var i = kvp.Value;
                list.Add(new TagCompound
                {
                    ["id"] = i.InheritanceID,
                    ["name"] = i.DisplayName,
                    ["type"] = (int)i.Type,
                    ["rarity"] = (int)i.Rarity,
                    ["state"] = (int)i.State,
                    ["entranceX"] = i.EntrancePosition.X,
                    ["entranceY"] = i.EntrancePosition.Y,
                    ["faction"] = (int)i.OriginalOwnerFaction,
                    ["discoveryDay"] = i.DiscoveryDay,
                    ["expiryDay"] = i.ExpiryDay,
                });
            }
            tag["inheritances"] = list;

            var completedList = new List<string>(CompletedInheritances);
            tag["completedInheritances"] = completedList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            KnownInheritances.Clear();
            CompletedInheritances.Clear();

            var list = tag.GetList<TagCompound>("inheritances");
            if (list != null)
            {
                foreach (var t in list)
                {
                    var instance = new InheritanceInstance
                    {
                        InheritanceID = t.GetString("id"),
                        DisplayName = t.GetString("name"),
                        Type = (InheritanceType)t.GetInt("type"),
                        Rarity = (InheritanceRarity)t.GetInt("rarity"),
                        State = (InheritanceState)t.GetInt("state"),
                        EntrancePosition = new Vector2(t.GetFloat("entranceX"), t.GetFloat("entranceY")),
                        OriginalOwnerFaction = (FactionID)t.GetInt("faction"),
                        DiscoveryDay = t.GetInt("discoveryDay"),
                        ExpiryDay = t.GetInt("expiryDay"),
                    };
                    KnownInheritances[instance.InheritanceID] = instance;
                }
            }

            var completedList = tag.GetList<string>("completedInheritances");
            if (completedList != null)
                CompletedInheritances.AddRange(completedList);
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
