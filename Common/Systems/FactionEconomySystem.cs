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
            // TODO: 完善各家族的默认经济数据
            long baseReserve = faction == FactionID.GuYue ? 10000 : 5000;
            long baseIncome = faction == FactionID.GuYue ? 500 : 200;
            long baseExpense = faction == FactionID.GuYue ? 300 : 150;

            return new FactionEconomy
            {
                Faction = faction,
                YuanStoneReserve = baseReserve,
                DailyIncome = baseIncome,
                DailyExpense = baseExpense,
                State = EconomyState.Stable,
                NPCCount = faction == FactionID.GuYue ? 15 : 8,
                MaxNPCCount = faction == FactionID.GuYue ? 15 : 8,
                DefenseLevel = 3,
                ShopVariety = faction == FactionID.GuYue ? 20 : 10
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
            // TODO: 扣除玩家物品，增加家族库存
            economy.YuanStoneReserve += amount * 10;
            player.GetModPlayer<GuWorldPlayer>().AddReputation(faction, amount * 5, "贡献物资");
            UpdateEconomyState(economy);
        }

        public void PlayerSteal(Player player, FactionID faction, int itemType, int amount)
        {
            if (!Economies.TryGetValue(faction, out var economy)) return;
            // TODO: 从家族库存偷取物品
            economy.YuanStoneReserve -= amount * 10;
            player.GetModPlayer<GuWorldPlayer>().RemoveReputation(faction, amount * 20, "偷盗物资");
            UpdateEconomyState(economy);
        }

        private void UpdateEconomyState(FactionEconomy economy)
        {
            economy.State = CalculateEconomyState(economy);
            // TODO: 经济状态影响NPC数量/商店/防御
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存家族经济数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载家族经济数据
        }
    }
}