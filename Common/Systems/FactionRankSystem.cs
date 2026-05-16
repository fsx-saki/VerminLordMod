using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum FactionRank
    {
        Outsider,
        Guest,
        OuterDisciple,
        InnerDisciple,
        CoreDisciple,
        Elder,
        GrandElder,
        ViceLeader,
        Leader,
        Ancestor
    }

    public class RankRequirement
    {
        public FactionRank Rank;
        public int RequiredReputation;
        public int RequiredGuLevel;
        public int RequiredContribution;
        public List<string> RequiredQuests = new();
        public string Title;
        public string Description;
        public List<RankBenefit> Benefits = new();
    }

    public class RankBenefit
    {
        public string Name;
        public string Description;
        public float QiRegenBonus;
        public float DamageBonus;
        public float DefenseBonus;
        public int DailyYuanStoneIncome;
        public int MaxBreedingSlotsBonus;
        public bool CanAccessFactionVault;
        public bool CanIssueQuests;
        public bool CanManageTerritory;
    }

    public class FactionRankSystem : ModSystem
    {
        public static FactionRankSystem Instance => ModContent.GetInstance<FactionRankSystem>();

        public Dictionary<FactionID, Dictionary<FactionRank, RankRequirement>> RankRequirements = new();

        public override void OnWorldLoad()
        {
            RankRequirements.Clear();
            RegisterRankRequirements();
        }

        private void RegisterRankRequirements()
        {
            foreach (FactionID faction in System.Enum.GetValues<FactionID>())
            {
                if (faction == FactionID.None || faction == FactionID.Scattered) continue;
                RegisterFactionRanks(faction);
            }
        }

        private void RegisterFactionRanks(FactionID faction)
        {
            var ranks = new Dictionary<FactionRank, RankRequirement>();

            ranks[FactionRank.Outsider] = new RankRequirement
            {
                Rank = FactionRank.Outsider,
                RequiredReputation = 0,
                RequiredGuLevel = 0,
                RequiredContribution = 0,
                Title = "外人",
                Description = "尚未被家族正式接纳的外来者。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "基础交易", Description = "可与家族商人进行基础交易。", DailyYuanStoneIncome = 0 },
                }
            };

            ranks[FactionRank.Guest] = new RankRequirement
            {
                Rank = FactionRank.Guest,
                RequiredReputation = 50,
                RequiredGuLevel = 1,
                RequiredContribution = 100,
                Title = "客卿",
                Description = "被家族接纳的客卿，享有基本待遇。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "客卿俸禄", Description = "每日领取1枚元石。", DailyYuanStoneIncome = 1 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+5%。", QiRegenBonus = 0.05f },
                }
            };

            ranks[FactionRank.OuterDisciple] = new RankRequirement
            {
                Rank = FactionRank.OuterDisciple,
                RequiredReputation = 150,
                RequiredGuLevel = 2,
                RequiredContribution = 300,
                Title = "外门弟子",
                Description = "正式的外门弟子，可参与家族日常事务。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "弟子俸禄", Description = "每日领取3枚元石。", DailyYuanStoneIncome = 3 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+10%。", QiRegenBonus = 0.10f },
                    new RankBenefit { Name = "蛊虫槽位", Description = "额外+1蛊虫培养槽位。", MaxBreedingSlotsBonus = 1 },
                }
            };

            ranks[FactionRank.InnerDisciple] = new RankRequirement
            {
                Rank = FactionRank.InnerDisciple,
                RequiredReputation = 400,
                RequiredGuLevel = 3,
                RequiredContribution = 800,
                Title = "内门弟子",
                Description = "内门弟子，可接触家族核心资源。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "内门俸禄", Description = "每日领取5枚元石。", DailyYuanStoneIncome = 5 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+15%。", QiRegenBonus = 0.15f },
                    new RankBenefit { Name = "蛊虫槽位", Description = "额外+2蛊虫培养槽位。", MaxBreedingSlotsBonus = 2 },
                    new RankBenefit { Name = "家族宝库", Description = "可访问家族宝库。", CanAccessFactionVault = true },
                }
            };

            ranks[FactionRank.CoreDisciple] = new RankRequirement
            {
                Rank = FactionRank.CoreDisciple,
                RequiredReputation = 800,
                RequiredGuLevel = 4,
                RequiredContribution = 2000,
                Title = "核心弟子",
                Description = "核心弟子，家族重点培养对象。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "核心俸禄", Description = "每日领取10枚元石。", DailyYuanStoneIncome = 10 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+20%。", QiRegenBonus = 0.20f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+5%。", DamageBonus = 0.05f },
                    new RankBenefit { Name = "蛊虫槽位", Description = "额外+3蛊虫培养槽位。", MaxBreedingSlotsBonus = 3 },
                    new RankBenefit { Name = "发布任务", Description = "可向低阶弟子发布任务。", CanIssueQuests = true },
                }
            };

            ranks[FactionRank.Elder] = new RankRequirement
            {
                Rank = FactionRank.Elder,
                RequiredReputation = 1500,
                RequiredGuLevel = 5,
                RequiredContribution = 5000,
                Title = "长老",
                Description = "家族长老，参与家族决策。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "长老俸禄", Description = "每日领取20枚元石。", DailyYuanStoneIncome = 20 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+25%。", QiRegenBonus = 0.25f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+10%。", DamageBonus = 0.10f },
                    new RankBenefit { Name = "防御加成", Description = "防御+10%。", DefenseBonus = 0.10f },
                    new RankBenefit { Name = "领地管理", Description = "可管理家族领地。", CanManageTerritory = true },
                }
            };

            ranks[FactionRank.GrandElder] = new RankRequirement
            {
                Rank = FactionRank.GrandElder,
                RequiredReputation = 3000,
                RequiredGuLevel = 6,
                RequiredContribution = 10000,
                Title = "大长老",
                Description = "家族大长老，执掌家族大权。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "大长老俸禄", Description = "每日领取40枚元石。", DailyYuanStoneIncome = 40 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+30%。", QiRegenBonus = 0.30f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+15%。", DamageBonus = 0.15f },
                    new RankBenefit { Name = "防御加成", Description = "防御+15%。", DefenseBonus = 0.15f },
                }
            };

            ranks[FactionRank.ViceLeader] = new RankRequirement
            {
                Rank = FactionRank.ViceLeader,
                RequiredReputation = 5000,
                RequiredGuLevel = 7,
                RequiredContribution = 20000,
                Title = "副族长",
                Description = "家族副族长，一人之下万人之上。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "副族长俸禄", Description = "每日领取60枚元石。", DailyYuanStoneIncome = 60 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+35%。", QiRegenBonus = 0.35f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+20%。", DamageBonus = 0.20f },
                    new RankBenefit { Name = "防御加成", Description = "防御+20%。", DefenseBonus = 0.20f },
                }
            };

            ranks[FactionRank.Leader] = new RankRequirement
            {
                Rank = FactionRank.Leader,
                RequiredReputation = 8000,
                RequiredGuLevel = 8,
                RequiredContribution = 40000,
                Title = "族长",
                Description = "家族族长，统领全族。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "族长俸禄", Description = "每日领取100枚元石。", DailyYuanStoneIncome = 100 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+40%。", QiRegenBonus = 0.40f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+25%。", DamageBonus = 0.25f },
                    new RankBenefit { Name = "防御加成", Description = "防御+25%。", DefenseBonus = 0.25f },
                }
            };

            ranks[FactionRank.Ancestor] = new RankRequirement
            {
                Rank = FactionRank.Ancestor,
                RequiredReputation = 15000,
                RequiredGuLevel = 9,
                RequiredContribution = 80000,
                Title = "太上长老",
                Description = "家族太上长老，传说中的存在。",
                Benefits = new List<RankBenefit>
                {
                    new RankBenefit { Name = "太上俸禄", Description = "每日领取200枚元石。", DailyYuanStoneIncome = 200 },
                    new RankBenefit { Name = "修炼加成", Description = "真元恢复速度+50%。", QiRegenBonus = 0.50f },
                    new RankBenefit { Name = "伤害加成", Description = "伤害+30%。", DamageBonus = 0.30f },
                    new RankBenefit { Name = "防御加成", Description = "防御+30%。", DefenseBonus = 0.30f },
                }
            };

            RankRequirements[faction] = ranks;
        }

        public RankRequirement GetRankRequirement(FactionID faction, FactionRank rank)
        {
            if (RankRequirements.TryGetValue(faction, out var ranks) &&
                ranks.TryGetValue(rank, out var req))
                return req;
            return null;
        }

        public FactionRank GetCurrentRank(Player player, FactionID faction)
        {
            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            if (rankPlayer.FactionRanks.TryGetValue(faction, out var rank))
                return rank;
            return FactionRank.Outsider;
        }

        public bool CanPromote(Player player, FactionID faction)
        {
            var currentRank = GetCurrentRank(player, faction);
            var nextRank = GetNextRank(currentRank);
            if (nextRank == currentRank) return false;

            var req = GetRankRequirement(faction, nextRank);
            if (req == null) return false;

            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();

            int rep = guWorld.GetRepPoints(faction);
            if (rep < req.RequiredReputation) return false;
            if (qiRealm.GuLevel < req.RequiredGuLevel) return false;
            if (rankPlayer.GetContribution(faction) < req.RequiredContribution) return false;

            foreach (var questID in req.RequiredQuests)
            {
                var tracker = player.GetModPlayer<QuestTrackerPlayer>();
                if (!tracker.HasCompletedQuest(questID)) return false;
            }

            return true;
        }

        public bool PromotePlayer(Player player, FactionID faction)
        {
            if (!CanPromote(player, faction)) return false;

            var currentRank = GetCurrentRank(player, faction);
            var nextRank = GetNextRank(currentRank);
            var req = GetRankRequirement(faction, nextRank);

            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            rankPlayer.FactionRanks[faction] = nextRank;

            EventBus.Publish(new FactionRankChangedEvent
            {
                PlayerID = player.whoAmI,
                Faction = faction,
                OldRank = currentRank,
                NewRank = nextRank,
            });

            if (player.whoAmI == Main.myPlayer)
                Main.NewText($"晋升成功！你在{GetFactionName(faction)}的职位提升为：{req.Title}", Color.Gold);

            return true;
        }

        private FactionRank GetNextRank(FactionRank current)
        {
            return current switch
            {
                FactionRank.Outsider => FactionRank.Guest,
                FactionRank.Guest => FactionRank.OuterDisciple,
                FactionRank.OuterDisciple => FactionRank.InnerDisciple,
                FactionRank.InnerDisciple => FactionRank.CoreDisciple,
                FactionRank.CoreDisciple => FactionRank.Elder,
                FactionRank.Elder => FactionRank.GrandElder,
                FactionRank.GrandElder => FactionRank.ViceLeader,
                FactionRank.ViceLeader => FactionRank.Leader,
                FactionRank.Leader => FactionRank.Ancestor,
                _ => FactionRank.Ancestor,
            };
        }

        public List<RankBenefit> GetActiveBenefits(Player player, FactionID faction)
        {
            var rank = GetCurrentRank(player, faction);
            var benefits = new List<RankBenefit>();

            var current = FactionRank.Outsider;
            while ((int)current <= (int)rank)
            {
                var req = GetRankRequirement(faction, current);
                if (req?.Benefits != null)
                    benefits.AddRange(req.Benefits);
                current = GetNextRank(current);
                if (current == FactionRank.Outsider) break;
            }

            return benefits;
        }

        public float GetTotalQiRegenBonus(Player player)
        {
            float total = 0f;
            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            foreach (var kvp in rankPlayer.FactionRanks)
            {
                var benefits = GetActiveBenefits(player, kvp.Key);
                foreach (var b in benefits)
                    total += b.QiRegenBonus;
            }
            return total;
        }

        public float GetTotalDamageBonus(Player player)
        {
            float total = 0f;
            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            foreach (var kvp in rankPlayer.FactionRanks)
            {
                var benefits = GetActiveBenefits(player, kvp.Key);
                foreach (var b in benefits)
                    total += b.DamageBonus;
            }
            return total;
        }

        public int GetTotalDailyYuanStoneIncome(Player player)
        {
            int total = 0;
            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            foreach (var kvp in rankPlayer.FactionRanks)
            {
                var benefits = GetActiveBenefits(player, kvp.Key);
                foreach (var b in benefits)
                    total += b.DailyYuanStoneIncome;
            }
            return total;
        }

        private string GetFactionName(FactionID faction)
        {
            return faction switch
            {
                FactionID.GuYue => "古月家族",
                FactionID.Bai => "白家",
                FactionID.Xiong => "熊家",
                FactionID.Tie => "铁家",
                FactionID.Wang => "汪家",
                FactionID.Zhao => "赵家",
                FactionID.Jia => "贾家",
                _ => "未知家族",
            };
        }
    }

    public class FactionRankPlayer : ModPlayer
    {
        public Dictionary<FactionID, FactionRank> FactionRanks = new();
        public Dictionary<FactionID, int> FactionContributions = new();

        public override void Initialize()
        {
            FactionRanks.Clear();
            FactionContributions.Clear();
        }

        public int GetContribution(FactionID faction)
        {
            FactionContributions.TryGetValue(faction, out var val);
            return val;
        }

        public void AddContribution(FactionID faction, int amount)
        {
            if (!FactionContributions.ContainsKey(faction))
                FactionContributions[faction] = 0;
            FactionContributions[faction] += amount;
        }

        public override void SaveData(TagCompound tag)
        {
            var rankList = new List<TagCompound>();
            foreach (var kvp in FactionRanks)
            {
                rankList.Add(new TagCompound
                {
                    ["faction"] = (int)kvp.Key,
                    ["rank"] = (int)kvp.Value,
                });
            }
            tag["factionRanks"] = rankList;

            var contribList = new List<TagCompound>();
            foreach (var kvp in FactionContributions)
            {
                contribList.Add(new TagCompound
                {
                    ["faction"] = (int)kvp.Key,
                    ["amount"] = kvp.Value,
                });
            }
            tag["factionContributions"] = contribList;
        }

        public override void LoadData(TagCompound tag)
        {
            FactionRanks.Clear();
            FactionContributions.Clear();

            var rankList = tag.GetList<TagCompound>("factionRanks");
            if (rankList != null)
            {
                foreach (var t in rankList)
                {
                    FactionRanks[(FactionID)t.GetInt("faction")] = (FactionRank)t.GetInt("rank");
                }
            }

            var contribList = tag.GetList<TagCompound>("factionContributions");
            if (contribList != null)
            {
                foreach (var t in contribList)
                {
                    FactionContributions[(FactionID)t.GetInt("faction")] = t.GetInt("amount");
                }
            }
        }
    }

    public class FactionRankChangedEvent : GuWorldEvent
    {
        public int PlayerID;
        public FactionID Faction;
        public FactionRank OldRank;
        public FactionRank NewRank;
    }
}