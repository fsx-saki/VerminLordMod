using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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
            foreach (FactionID faction in System.Enum.GetValues<FactionID>())
            {
                if (faction == FactionID.None || faction == FactionID.Scattered) continue;

                ActiveStalls.Add(new MarketStall
                {
                    Type = MarketStallType.FoodStall,
                    LocationFaction = faction,
                    RefreshDay = 0,
                    PriceModifier = 1.0f,
                });

                ActiveStalls.Add(new MarketStall
                {
                    Type = MarketStallType.HerbStall,
                    LocationFaction = faction,
                    RefreshDay = 0,
                    PriceModifier = 1.0f,
                });

                ActiveStalls.Add(new MarketStall
                {
                    Type = MarketStallType.DailyGoodsStall,
                    LocationFaction = faction,
                    RefreshDay = 0,
                    PriceModifier = 1.0f,
                });

                ActiveStalls.Add(new MarketStall
                {
                    Type = MarketStallType.ToolStall,
                    LocationFaction = faction,
                    RefreshDay = 0,
                    PriceModifier = 1.0f,
                });

                if (faction == FactionID.GuYue || faction == FactionID.Jia)
                {
                    ActiveStalls.Add(new MarketStall
                    {
                        Type = MarketStallType.GuMaterialStall,
                        LocationFaction = faction,
                        RefreshDay = 0,
                        PriceModifier = 1.0f,
                    });
                }

                if (faction == FactionID.GuYue || faction == FactionID.Bai)
                {
                    ActiveStalls.Add(new MarketStall
                    {
                        Type = MarketStallType.WeaponStall,
                        LocationFaction = faction,
                        RefreshDay = 0,
                        PriceModifier = 1.0f,
                    });
                }

                if (faction == FactionID.GuYue || faction == FactionID.Wang)
                {
                    ActiveStalls.Add(new MarketStall
                    {
                        Type = MarketStallType.BookStall,
                        LocationFaction = faction,
                        RefreshDay = 0,
                        PriceModifier = 1.0f,
                    });
                }
            }
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
            foreach (var stall in ActiveStalls)
            {
                RefreshStallInventory(stall);
                UpdatePrices(stall);
            }
        }

        private void RefreshStallInventory(MarketStall stall)
        {
            stall.CurrentItems.Clear();

            switch (stall.Type)
            {
                case MarketStallType.FoodStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Mushroom,
                        StackSize = 1,
                        BasePrice = 5,
                        CurrentPrice = 5,
                        Stock = Main.rand.Next(10, 30),
                        MaxStock = 30,
                        RarityWeight = 1.0f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.GlowingMushroom,
                        StackSize = 1,
                        BasePrice = 10,
                        CurrentPrice = 10,
                        Stock = Main.rand.Next(5, 15),
                        MaxStock = 15,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Daybloom,
                        StackSize = 1,
                        BasePrice = 8,
                        CurrentPrice = 8,
                        Stock = Main.rand.Next(5, 15),
                        MaxStock = 15,
                        RarityWeight = 0.7f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.BottledWater,
                        StackSize = 1,
                        BasePrice = 3,
                        CurrentPrice = 3,
                        Stock = Main.rand.Next(5, 20),
                        MaxStock = 20,
                        RarityWeight = 0.8f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Pumpkin,
                        StackSize = 1,
                        BasePrice = 6,
                        CurrentPrice = 6,
                        Stock = Main.rand.Next(5, 15),
                        MaxStock = 15,
                        RarityWeight = 0.5f,
                    });
                    break;

                case MarketStallType.HerbStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Daybloom,
                        StackSize = 1,
                        BasePrice = 10,
                        CurrentPrice = 10,
                        Stock = Main.rand.Next(5, 15),
                        MaxStock = 15,
                        RarityWeight = 0.9f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Moonglow,
                        StackSize = 1,
                        BasePrice = 12,
                        CurrentPrice = 12,
                        Stock = Main.rand.Next(3, 10),
                        MaxStock = 10,
                        RarityWeight = 0.7f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Blinkroot,
                        StackSize = 1,
                        BasePrice = 15,
                        CurrentPrice = 15,
                        Stock = Main.rand.Next(2, 8),
                        MaxStock = 8,
                        RarityWeight = 0.5f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>(),
                        StackSize = 1,
                        BasePrice = 20,
                        CurrentPrice = 20,
                        Stock = Main.rand.Next(1, 5),
                        MaxStock = 5,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Seeds.HealingHerbSeed>(),
                        StackSize = 1,
                        BasePrice = 15,
                        CurrentPrice = 15,
                        Stock = Main.rand.Next(3, 10),
                        MaxStock = 10,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Seeds.QiHerbSeed>(),
                        StackSize = 1,
                        BasePrice = 25,
                        CurrentPrice = 25,
                        Stock = Main.rand.Next(1, 5),
                        MaxStock = 5,
                        RarityWeight = 0.4f,
                    });
                    break;

                case MarketStallType.DailyGoodsStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Wood,
                        StackSize = 1,
                        BasePrice = 2,
                        CurrentPrice = 2,
                        Stock = Main.rand.Next(20, 50),
                        MaxStock = 50,
                        RarityWeight = 1.0f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Torch,
                        StackSize = 1,
                        BasePrice = 1,
                        CurrentPrice = 1,
                        Stock = Main.rand.Next(30, 60),
                        MaxStock = 60,
                        RarityWeight = 1.0f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Rope,
                        StackSize = 1,
                        BasePrice = 5,
                        CurrentPrice = 5,
                        Stock = Main.rand.Next(10, 20),
                        MaxStock = 20,
                        RarityWeight = 0.8f,
                    });
                    break;

                case MarketStallType.ToolStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.CopperPickaxe,
                        StackSize = 1,
                        BasePrice = 50,
                        CurrentPrice = 50,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.CopperAxe,
                        StackSize = 1,
                        BasePrice = 40,
                        CurrentPrice = 40,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.WoodFishingPole,
                        StackSize = 1,
                        BasePrice = 30,
                        CurrentPrice = 30,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        RarityWeight = 0.5f,
                    });
                    break;

                case MarketStallType.GuMaterialStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>(),
                        StackSize = 1,
                        BasePrice = 50,
                        CurrentPrice = 50,
                        Stock = Main.rand.Next(1, 5),
                        MaxStock = 5,
                        IsLimited = true,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.FallenStar,
                        StackSize = 1,
                        BasePrice = 80,
                        CurrentPrice = 80,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        IsLimited = true,
                        RarityWeight = 0.2f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Bone,
                        StackSize = 1,
                        BasePrice = 15,
                        CurrentPrice = 15,
                        Stock = Main.rand.Next(5, 15),
                        MaxStock = 15,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Cobweb,
                        StackSize = 1,
                        BasePrice = 10,
                        CurrentPrice = 10,
                        Stock = Main.rand.Next(10, 25),
                        MaxStock = 25,
                        RarityWeight = 0.7f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Gel,
                        StackSize = 1,
                        BasePrice = 8,
                        CurrentPrice = 8,
                        Stock = Main.rand.Next(10, 30),
                        MaxStock = 30,
                        RarityWeight = 0.8f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Breeding.GuBox>(),
                        StackSize = 1,
                        BasePrice = 100,
                        CurrentPrice = 100,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        IsLimited = true,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Consumables.GuFoodPellet>(),
                        StackSize = 1,
                        BasePrice = 10,
                        CurrentPrice = 10,
                        Stock = Main.rand.Next(5, 20),
                        MaxStock = 20,
                        RarityWeight = 0.7f,
                    });
                    break;

                case MarketStallType.WeaponStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.WoodenSword,
                        StackSize = 1,
                        BasePrice = 20,
                        CurrentPrice = 20,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        RarityWeight = 0.7f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.WoodenBow,
                        StackSize = 1,
                        BasePrice = 25,
                        CurrentPrice = 25,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        RarityWeight = 0.6f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.IronBroadsword,
                        StackSize = 1,
                        BasePrice = 80,
                        CurrentPrice = 80,
                        Stock = Main.rand.Next(1, 2),
                        MaxStock = 2,
                        IsLimited = true,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.IronBow,
                        StackSize = 1,
                        BasePrice = 70,
                        CurrentPrice = 70,
                        Stock = Main.rand.Next(1, 2),
                        MaxStock = 2,
                        IsLimited = true,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.WoodenArrow,
                        StackSize = 1,
                        BasePrice = 2,
                        CurrentPrice = 2,
                        Stock = Main.rand.Next(20, 50),
                        MaxStock = 50,
                        RarityWeight = 0.9f,
                    });
                    break;

                case MarketStallType.BookStall:
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.Book,
                        StackSize = 1,
                        BasePrice = 30,
                        CurrentPrice = 30,
                        Stock = Main.rand.Next(1, 5),
                        MaxStock = 5,
                        RarityWeight = 0.5f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>(),
                        StackSize = 1,
                        BasePrice = 40,
                        CurrentPrice = 40,
                        Stock = Main.rand.Next(1, 3),
                        MaxStock = 3,
                        IsLimited = true,
                        RarityWeight = 0.3f,
                    });
                    stall.CurrentItems.Add(new MarketItem
                    {
                        ItemType = ItemID.SpellTome,
                        StackSize = 1,
                        BasePrice = 150,
                        CurrentPrice = 150,
                        Stock = Main.rand.Next(0, 2),
                        MaxStock = 2,
                        IsLimited = true,
                        RarityWeight = 0.1f,
                    });
                    break;
            }
        }

        private void UpdatePrices(MarketStall stall)
        {
            foreach (var item in stall.CurrentItems)
            {
                float supplyRatio = (float)item.Stock / item.MaxStock;
                float priceMultiplier = 1f + (1f - supplyRatio) * 0.5f;
                item.CurrentPrice = (int)(item.BasePrice * stall.PriceModifier * priceMultiplier);
            }
        }

        public MarketStall GetStallForNPC(NPC npc)
        {
            foreach (var stall in ActiveStalls)
            {
                if (stall.VendorNPC == npc)
                    return stall;
            }

            var occupation = GetNPCOccupation(npc);
            if (occupation == MarketStallType.FoodStall) return null;

            var faction = GetNPCFaction(npc);
            return ActiveStalls.Find(s => s.Type == occupation && s.LocationFaction == faction);
        }

        private MarketStallType GetNPCOccupation(NPC npc)
        {
            string name = npc.FullName?.ToLower() ?? "";
            if (name.Contains("农") || name.Contains("farmer")) return MarketStallType.FoodStall;
            if (name.Contains("药") || name.Contains("herb")) return MarketStallType.HerbStall;
            if (name.Contains("商") || name.Contains("merchant")) return MarketStallType.DailyGoodsStall;
            if (name.Contains("铁") || name.Contains("smith")) return MarketStallType.ToolStall;
            if (name.Contains("蛊") || name.Contains("gu")) return MarketStallType.GuMaterialStall;
            if (name.Contains("武") || name.Contains("weapon")) return MarketStallType.WeaponStall;
            if (name.Contains("书") || name.Contains("scholar")) return MarketStallType.BookStall;
            return MarketStallType.DailyGoodsStall;
        }

        private FactionID GetNPCFaction(NPC npc)
        {
            var persistence = NPCPersistenceSystem.Instance;
            if (persistence != null)
            {
                var data = persistence.GetDataForNPC(npc.type);
                if (data != null && data.Faction != FactionID.None)
                    return data.Faction;
            }
            return FactionID.GuYue;
        }

        public List<MarketItem> GetStallItems(MarketStallType type, FactionID faction)
        {
            var stall = ActiveStalls.Find(s => s.Type == type && s.LocationFaction == faction);
            return stall?.CurrentItems ?? new List<MarketItem>();
        }

        public void RecordPurchase(int itemType, int amount)
        {
            foreach (var stall in ActiveStalls)
            {
                foreach (var item in stall.CurrentItems)
                {
                    if (item.ItemType == itemType)
                    {
                        item.Stock = System.Math.Max(0, item.Stock - amount);
                        stall.PriceModifier += 0.05f * amount;
                        UpdatePrices(stall);
                        return;
                    }
                }
            }
        }
    }
}