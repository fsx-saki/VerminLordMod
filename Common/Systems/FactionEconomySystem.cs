using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // FactionEconomySystem — 家族经济闭环大框
    //
    // 系统定位：
    // 每个家族有独立的经济体，包括收入、支出、库存、贸易。
    // 玩家的行为（交易/掠夺/悬赏）影响家族经济。
    //
    // 功能规划：
    // 1. 家族收入来源：资源节点产出、领地税收、商队贸易
    // 2. 家族支出：NPC薪酬、防御维护、阵法消耗、丹药储备
    // 3. 家族库存：元石、药材、蛊虫、武器、丹药
    // 4. 家族贸易：不同家族之间的物资交换
    // 5. 玩家影响：偷盗家族库存→经济受损，贡献物资→经济改善
    // 6. 经济状态→功能影响：经济差→NPC减少/防御降级/商店缺货
    //
    // 闭环流程：
    //   资源节点产出 → 家族收入 → 
    //   家族支出（NPC/防御/丹药） → 经济平衡 →
    //   经济状态 → 影响NPC数量/商店商品/防御强度 →
    //   玩家行为 → 改变经济状态 → 循环
    //
    // TODO:
    //   - 实现家族经济数据结构
    //   - 实现每日收支结算
    //   - 实现经济状态对NPC/商店的影响
    //   - 实现家族间贸易
    //   - 实现玩家偷盗/贡献交互
    // ============================================================

    public enum EconomyState
    {
        Flourishing,     // 繁盛 - NPC充足、商品齐全、防御完善
        Stable,          // 稳定 - 正常运作
        Strained,        // 紧张 - NPC减少、商品缺货、防御降级
        Crisis,          // 危机 - NPC大量减少、功能停摆
        Collapsed        // 崩溃 - 家族解散
    }

    public class FactionEconomy
    {
        public FactionID Faction;
        public long YuanStoneReserve;               // 元石储备
        public long DailyIncome;                     // 日收入
        public long DailyExpense;                    // 日支出
        public EconomyState State;                   // 经济状态
        public Dictionary<int, int> ResourceStock = new();  // 物资库存（itemType→数量）
        public int NPCCount;                         // 当前NPC数量
        public int MaxNPCCount;                      // 正常NPC数量
        public int DefenseLevel;                     // 防御等级 (0-5)
        public int ShopVariety;                      // 商店商品种类数
        public List<TradeRoute> ActiveTradeRoutes = new();
    }

    public class TradeRoute
    {
        public FactionID SourceFaction;
        public FactionID TargetFaction;
        public int TradeItem;                        // 交易物品Type
        public int TradeQuantity;                    // 交易量
        public int FrequencyDays;                    // 交易频率（几天一次）
    }

    public class FactionEconomySystem : ModSystem
    {
        public static FactionEconomySystem Instance => ModContent.GetInstance<FactionEconomySystem>();

        public Dictionary<FactionID, FactionEconomy> Economies = new();

        public override void OnWorldLoad()
        {
            Economies.Clear();
            InitializeAllEconomies();
        }

        private void InitializeAllEconomies()
        {
            foreach (FactionID faction in System.Enum.GetValues<FactionID>())
            {
                if (faction == FactionID.None || faction == FactionID.Scattered) continue;
                Economies[faction] = CreateDefaultEconomy(faction);
            }
        }

        private FactionEconomy CreateDefaultEconomy(FactionID faction)
        {
            return faction switch
            {
                FactionID.GuYue => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 10000,
                    DailyIncome = 500,
                    DailyExpense = 300,
                    State = EconomyState.Stable,
                    NPCCount = 15,
                    MaxNPCCount = 15,
                    DefenseLevel = 3,
                    ShopVariety = 20,
                },
                FactionID.Bai => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 8000,
                    DailyIncome = 400,
                    DailyExpense = 250,
                    State = EconomyState.Stable,
                    NPCCount = 12,
                    MaxNPCCount = 12,
                    DefenseLevel = 3,
                    ShopVariety = 15,
                },
                FactionID.Xiong => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 6000,
                    DailyIncome = 300,
                    DailyExpense = 200,
                    State = EconomyState.Stable,
                    NPCCount = 10,
                    MaxNPCCount = 10,
                    DefenseLevel = 2,
                    ShopVariety = 12,
                },
                FactionID.Tie => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 7000,
                    DailyIncome = 350,
                    DailyExpense = 220,
                    State = EconomyState.Stable,
                    NPCCount = 10,
                    MaxNPCCount = 10,
                    DefenseLevel = 2,
                    ShopVariety = 12,
                },
                FactionID.Jia => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 15000,
                    DailyIncome = 800,
                    DailyExpense = 400,
                    State = EconomyState.Flourishing,
                    NPCCount = 8,
                    MaxNPCCount = 8,
                    DefenseLevel = 1,
                    ShopVariety = 25,
                },
                FactionID.Wang => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 5000,
                    DailyIncome = 250,
                    DailyExpense = 300,
                    State = EconomyState.Strained,
                    NPCCount = 8,
                    MaxNPCCount = 8,
                    DefenseLevel = 4,
                    ShopVariety = 8,
                },
                FactionID.Zhao => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 4000,
                    DailyIncome = 200,
                    DailyExpense = 250,
                    State = EconomyState.Strained,
                    NPCCount = 6,
                    MaxNPCCount = 6,
                    DefenseLevel = 2,
                    ShopVariety = 6,
                },
                _ => new FactionEconomy
                {
                    Faction = faction,
                    YuanStoneReserve = 3000,
                    DailyIncome = 150,
                    DailyExpense = 100,
                    State = EconomyState.Stable,
                    NPCCount = 5,
                    MaxNPCCount = 5,
                    DefenseLevel = 1,
                    ShopVariety = 5,
                },
            };
        }

        public EconomyState CalculateEconomyState(FactionEconomy economy)
        {
            long netIncome = economy.DailyIncome - economy.DailyExpense;
            float reserveRatio = (float)economy.YuanStoneReserve / (economy.DailyExpense * 30);

            if (reserveRatio > 5f && netIncome > 0) return EconomyState.Flourishing;
            if (reserveRatio > 2f && netIncome >= 0) return EconomyState.Stable;
            if (reserveRatio > 0.5f) return EconomyState.Strained;
            if (reserveRatio > 0) return EconomyState.Crisis;
            return EconomyState.Collapsed;
        }

        public void PlayerContribute(Player player, FactionID faction, int itemType, int amount)
        {
            if (!Economies.TryGetValue(faction, out var economy)) return;

            for (int i = 0; i < 50; i++)
            {
                var item = player.inventory[i];
                if (item.type == itemType && item.stack >= amount)
                {
                    item.stack -= amount;
                    if (item.stack <= 0) item.TurnToAir();

                    if (!economy.ResourceStock.ContainsKey(itemType))
                        economy.ResourceStock[itemType] = 0;
                    economy.ResourceStock[itemType] += amount;
                    economy.YuanStoneReserve += amount * 10;
                    break;
                }
            }

            player.GetModPlayer<GuWorldPlayer>().AddReputation(faction, amount * 5, "贡献物资");
            UpdateEconomyState(economy);
        }

        public void PlayerSteal(Player player, FactionID faction, int itemType, int amount)
        {
            if (!Economies.TryGetValue(faction, out var economy)) return;
            if (!economy.ResourceStock.TryGetValue(itemType, out var stock) || stock < amount) return;

            economy.ResourceStock[itemType] -= amount;
            economy.YuanStoneReserve -= amount * 10;

            player.QuickSpawnItem(player.GetSource_GiftOrReward(), itemType, amount);
            player.GetModPlayer<GuWorldPlayer>().RemoveReputation(faction, amount * 20, "偷盗物资");
            UpdateEconomyState(economy);
        }

        private void UpdateEconomyState(FactionEconomy economy)
        {
            economy.State = CalculateEconomyState(economy);

            switch (economy.State)
            {
                case EconomyState.Flourishing:
                    economy.NPCCount = economy.MaxNPCCount;
                    economy.DefenseLevel = System.Math.Min(5, economy.DefenseLevel + 1);
                    economy.ShopVariety = System.Math.Min(30, economy.ShopVariety + 5);
                    break;
                case EconomyState.Stable:
                    economy.NPCCount = economy.MaxNPCCount;
                    break;
                case EconomyState.Strained:
                    economy.NPCCount = System.Math.Max(1, economy.MaxNPCCount - 3);
                    economy.DefenseLevel = System.Math.Max(1, economy.DefenseLevel - 1);
                    economy.ShopVariety = System.Math.Max(3, economy.ShopVariety - 5);
                    break;
                case EconomyState.Crisis:
                    economy.NPCCount = System.Math.Max(1, economy.MaxNPCCount / 2);
                    economy.DefenseLevel = System.Math.Max(0, economy.DefenseLevel - 2);
                    economy.ShopVariety = System.Math.Max(1, economy.ShopVariety / 2);
                    break;
                case EconomyState.Collapsed:
                    economy.NPCCount = 0;
                    economy.DefenseLevel = 0;
                    economy.ShopVariety = 0;
                    break;
            }
        }

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            foreach (var kvp in Economies)
            {
                var economy = kvp.Value;
                economy.YuanStoneReserve += economy.DailyIncome - economy.DailyExpense;
                if (economy.YuanStoneReserve < 0) economy.YuanStoneReserve = 0;
                UpdateEconomyState(economy);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var kvp in Economies)
            {
                var e = kvp.Value;
                var stockTag = new TagCompound();
                foreach (var s in e.ResourceStock)
                    stockTag[s.Key.ToString()] = s.Value;

                list.Add(new TagCompound
                {
                    ["faction"] = (int)e.Faction,
                    ["reserve"] = e.YuanStoneReserve,
                    ["income"] = e.DailyIncome,
                    ["expense"] = e.DailyExpense,
                    ["state"] = (int)e.State,
                    ["npcCount"] = e.NPCCount,
                    ["maxNpc"] = e.MaxNPCCount,
                    ["defense"] = e.DefenseLevel,
                    ["shopVariety"] = e.ShopVariety,
                    ["stock"] = stockTag,
                });
            }
            tag["economies"] = list;
            tag["econDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            Economies.Clear();

            var list = tag.GetList<TagCompound>("economies");
            if (list == null) return;

            foreach (var t in list)
            {
                var economy = new FactionEconomy
                {
                    Faction = (FactionID)t.GetInt("faction"),
                    YuanStoneReserve = t.GetLong("reserve"),
                    DailyIncome = t.GetLong("income"),
                    DailyExpense = t.GetLong("expense"),
                    State = (EconomyState)t.GetInt("state"),
                    NPCCount = t.GetInt("npcCount"),
                    MaxNPCCount = t.GetInt("maxNpc"),
                    DefenseLevel = t.GetInt("defense"),
                    ShopVariety = t.GetInt("shopVariety"),
                };

                if (t.TryGet("stock", out TagCompound stockTag))
                {
                    foreach (NodeResourceType resType in System.Enum.GetValues<NodeResourceType>())
                    {
                        string key = resType.ToString();
                        if (stockTag.ContainsKey(key))
                            economy.ResourceStock[(int)resType] = stockTag.GetInt(key);
                    }
                }

                Economies[economy.Faction] = economy;
            }

            _lastDay = tag.GetInt("econDayCounter");
        }
    }
}