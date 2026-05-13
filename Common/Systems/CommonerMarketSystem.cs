using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // CommonerMarketSystem — 凡人市场系统大框
    //
    // 系统定位：
    // 凡人市场是蛊世界中的基础经济场所，普通NPC在此交易日常物资。
    // 与蛊师交易不同，凡人市场价格稳定、品类固定。
    //
    // 功能规划：
    // 1. 市场摊位：不同职业NPC对应不同摊位类型
    // 2. 摊位类型：食物摊、药材摊、日用品摊、工具摊、蛊材摊（稀有）
    // 3. 市场刷新：每日刷新部分商品的库存和价格
    // 4. 区域市场：不同领地有不同的市场（古月市场 vs 白家市场）
    // 5. 供求关系：大量购买某商品 → 价格上涨
    // 6. 特殊事件：商队到来时增加稀有商品
    //
    // TODO:
    //   - 实现摊位数据结构
    //   - 实现每日刷新逻辑
    //   - 实现供求价格变动
    //   - 实现市场UI
    //   - 创建市场Tile（市场摊位方块）
    // ============================================================

    public enum MarketStallType
    {
        FoodStall,          // 食物摊 - 食材、熟食
        HerbStall,          // 药材摊 - 基础药材
        DailyGoodsStall,    // 日用品摊 - 木材、布料、陶器
        ToolStall,          // 工具摊 - 矿具、渔具、农具
        GuMaterialStall,    // 蛊材摊 - 空窍石、炼蛊辅助材料（稀有）
        WeaponStall,        // 武器摊 - 凡人武器、低级蛊虫武器
        BookStall           // 书摊 - 蛊术典籍、情报
    }

    public class MarketStall
    {
        public MarketStallType Type;
        public FactionID LocationFaction;
        public NPC VendorNPC;
        public List<MarketItem> CurrentItems = new();
        public int RefreshDay;
        public float PriceModifier = 1.0f;   // 受供求影响
    }

    public class MarketItem
    {
        public int ItemType;
        public int StackSize;
        public int BasePrice;
        public int CurrentPrice;
        public int Stock;                  // 当前库存
        public int MaxStock;
        public bool IsLimited;             // 是否限量
        public float RarityWeight;         // 刷新概率权重
    }

    public class CommonerMarketSystem : ModSystem
    {
        public static CommonerMarketSystem Instance => ModContent.GetInstance<CommonerMarketSystem>();

        public List<MarketStall> ActiveStalls = new();
        private int _lastRefreshDay = -1;

        public override void OnWorldLoad()
        {
            ActiveStalls.Clear();
            InitializeDefaultMarkets();
        }

        private void InitializeDefaultMarkets()
        {
            // TODO: 创建古月市场摊位
            // ActiveStalls.Add(CreateGuYueFoodStall());
            // ActiveStalls.Add(CreateGuYueHerbStall());
            // etc.
        }

        public override void PreUpdateWorld()
        {
            // 每日刷新市场
            int currentDay = (int)(Main.time / 36000);
            if (currentDay != _lastRefreshDay && Main.dayTime && Main.time < 1f)
            {
                _lastRefreshDay = currentDay;
                RefreshAllStalls();
            }
        }

        private void RefreshAllStalls()
        {
            // TODO: 实现市场每日刷新逻辑
            foreach (var stall in ActiveStalls)
            {
                RefreshStallInventory(stall);
                UpdatePrices(stall);
            }
        }

        private void RefreshStallInventory(MarketStall stall)
        {
            // TODO: 根据摊位类型和权重随机刷新商品
        }

        private void UpdatePrices(MarketStall stall)
        {
            // TODO: 根据供求关系调整价格
            foreach (var item in stall.CurrentItems)
            {
                item.CurrentPrice = (int)(item.BasePrice * stall.PriceModifier);
            }
        }

        public MarketStall GetStallForNPC(NPC npc)
        {
            // TODO: 根据NPC职业返回对应摊位
            return null;
        }

        public List<MarketItem> GetStallItems(MarketStallType type, FactionID faction)
        {
            var stall = ActiveStalls.Find(s => s.Type == type && s.LocationFaction == faction);
            return stall?.CurrentItems ?? new List<MarketItem>();
        }

        public void RecordPurchase(int itemType, int amount)
        {
            // TODO: 记录购买量，影响供求价格
        }
    }
}