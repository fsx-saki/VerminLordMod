using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // AuctionSystem — 蛊虫拍卖行系统大框
    //
    // 系统定位：
    // 拍卖行是蛊师交易稀有蛊虫的高级场所。
    // NPC和玩家都可以在拍卖行竞拍物品。
    //
    // 功能规划：
    // 1. 拍卖品上架：玩家/NPC上架蛊虫/丹药/武器
    // 2. 竞拍机制：设定起拍价，NPC和玩家轮流竞价
    // 3. 拍卖周期：每7天一次拍卖会
    // 4. NPC竞价AI：根据信念和性格决定是否竞拍
    // 5. 拍卖行NPC：拍卖师（主持拍卖）
    // 6. 拍卖行Tile：拍卖台
    // 7. 拍卖行UI：查看当前拍卖品、竞价面板
    //
    // TODO:
    //   - 实现拍卖品数据结构
    //   - 实现拍卖周期触发
    //   - 实现NPC竞价AI
    //   - 实现拍卖行UI
    //   - 创建拍卖行NPC
    //   - 创建拍卖台Tile
    // ============================================================

    public class AuctionItem
    {
        public int ItemType;
        public int Stack;
        public int StartingPrice;         // 起拍价（元石）
        public int CurrentBid;             // 当前最高竞价
        public int BidderPlayerID;         // 当前最高竞价者
        public int SellerPlayerID;         // 上架者
        public int AuctionID;
        public int RemainingTimeTicks;     // 剩余竞拍时间
        public bool IsNPCItem;             // NPC上架的物品
        public FactionID SellerFaction;    // NPC上架时的所属家族
    }

    public class AuctionSystem : ModSystem
    {
        public static AuctionSystem Instance => ModContent.GetInstance<AuctionSystem>();

        public List<AuctionItem> CurrentAuctions = new();
        public List<AuctionItem> CompletedAuctions = new();
        private int _nextAuctionDay;
        private const int AuctionIntervalDays = 7;

        public override void OnWorldLoad()
        {
            CurrentAuctions.Clear();
            CompletedAuctions.Clear();
            _nextAuctionDay = AuctionIntervalDays;
        }

        public override void PostUpdateWorld()
        {
            int currentDay = (int)(Main.time / 36000);
            if (currentDay >= _nextAuctionDay && Main.dayTime && Main.time < 3600)
            {
                StartNewAuctionSession();
                _nextAuctionDay = currentDay + AuctionIntervalDays;
            }

            for (int i = CurrentAuctions.Count - 1; i >= 0; i--)
            {
                CurrentAuctions[i].RemainingTimeTicks--;
                if (CurrentAuctions[i].RemainingTimeTicks <= 0)
                {
                    FinalizeAuction(CurrentAuctions[i]);
                    CompletedAuctions.Add(CurrentAuctions[i]);
                    CurrentAuctions.RemoveAt(i);
                }
                else if (CurrentAuctions[i].RemainingTimeTicks % 3600 == 0)
                {
                    NPCBidOnItem(CurrentAuctions[i]);
                }
            }
        }

        private void StartNewAuctionSession()
        {
            Main.NewText("拍卖会开始！快去看看有什么好东西！", Microsoft.Xna.Framework.Color.Gold);

            var economySystem = FactionEconomySystem.Instance;
            if (economySystem == null) return;

            foreach (var kvp in economySystem.Economies)
            {
                var economy = kvp.Value;
                if (economy.State == EconomyState.Collapsed || economy.State == EconomyState.Crisis) continue;

                int itemCount = Main.rand.Next(1, 4);
                for (int j = 0; j < itemCount; j++)
                {
                    var auction = new AuctionItem
                    {
                        ItemType = GetRandomAuctionItem(economy.Faction),
                        Stack = Main.rand.Next(1, 5),
                        StartingPrice = Main.rand.Next(50, 500),
                        SellerPlayerID = -1,
                        RemainingTimeTicks = 36000 * 3,
                        IsNPCItem = true,
                        SellerFaction = economy.Faction,
                        AuctionID = CurrentAuctions.Count + j,
                    };
                    auction.CurrentBid = auction.StartingPrice;
                    CurrentAuctions.Add(auction);
                }
            }
        }

        private int GetRandomAuctionItem(FactionID faction)
        {
            var items = new List<int>();
            switch (faction)
            {
                case FactionID.GuYue:
                    items.Add(ModContent.ItemType<Content.Items.Consumables.YuanS>());
                    items.Add(ModContent.ItemType<Content.Items.Consumables.YuanS>());
                    break;
                case FactionID.Jia:
                    items.Add(ModContent.ItemType<Content.Items.Consumables.YuanS>());
                    break;
                default:
                    items.Add(ModContent.ItemType<Content.Items.Consumables.YuanS>());
                    break;
            }
            return items.Count > 0 ? items[Main.rand.Next(items.Count)] : ModContent.ItemType<Content.Items.Consumables.YuanS>();
        }

        public void PlayerPlaceBid(Player player, int auctionID, int bidAmount)
        {
            var item = CurrentAuctions.Find(a => a.AuctionID == auctionID);
            if (item == null) return;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < bidAmount)
            {
                Main.NewText("真元不足，无法竞价", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            if (bidAmount <= item.CurrentBid) return;

            item.CurrentBid = bidAmount;
            item.BidderPlayerID = player.whoAmI;

            Main.NewText($"你以 {bidAmount} 元石竞价了拍卖品！", Microsoft.Xna.Framework.Color.Yellow);
        }

        public void PlayerListAuction(Player player, int itemType, int stack, int startingPrice)
        {
            var auction = new AuctionItem
            {
                ItemType = itemType,
                Stack = stack,
                StartingPrice = startingPrice,
                CurrentBid = startingPrice,
                SellerPlayerID = player.whoAmI,
                RemainingTimeTicks = 36000 * 3,  // 3天竞拍期
                IsNPCItem = false
            };
            CurrentAuctions.Add(auction);
        }

        private void FinalizeAuction(AuctionItem auction)
        {
            if (auction.BidderPlayerID >= 0)
            {
                var buyer = Main.player[auction.BidderPlayerID];
                if (buyer != null && buyer.active)
                {
                    buyer.QuickSpawnItem(buyer.GetSource_GiftOrReward(), auction.ItemType, auction.Stack);
                    Main.NewText($"恭喜！你以 {auction.CurrentBid} 元石拍得了拍卖品！", Microsoft.Xna.Framework.Color.Green);
                }
            }

            if (auction.SellerPlayerID >= 0)
            {
                var seller = Main.player[auction.SellerPlayerID];
                if (seller != null && seller.active)
                {
                    seller.GetModPlayer<GuWorldPlayer>().AddReputation(
                        seller.GetModPlayer<GuWorldPlayer>().CurrentAlly, 10, "拍卖成功");
                }
            }
        }

        private void NPCBidOnItem(AuctionItem item)
        {
            if (Main.rand.NextFloat() > 0.3f) return;

            int npcBid = item.CurrentBid + Main.rand.Next(10, 100);
            item.CurrentBid = npcBid;
            item.BidderPlayerID = -1;
        }
    }
}