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
            // TODO: 拍卖周期触发
            int currentDay = (int)(Main.time / 36000);
            if (currentDay >= _nextAuctionDay)
            {
                StartNewAuctionSession();
                _nextAuctionDay = currentDay + AuctionIntervalDays;
            }

            // TODO: 处理竞拍倒计时
            for (int i = CurrentAuctions.Count - 1; i >= 0; i--)
            {
                CurrentAuctions[i].RemainingTimeTicks--;
                if (CurrentAuctions[i].RemainingTimeTicks <= 0)
                {
                    FinalizeAuction(CurrentAuctions[i]);
                    CompletedAuctions.Add(CurrentAuctions[i]);
                    CurrentAuctions.RemoveAt(i);
                }
            }
        }

        private void StartNewAuctionSession()
        {
            // TODO: NPC上架拍卖品
            Main.NewText("拍卖会开始！快去看看有什么好东西！", Microsoft.Xna.Framework.Color.Gold);
        }

        public void PlayerPlaceBid(Player player, int auctionID, int bidAmount)
        {
            var item = CurrentAuctions.Find(a => a.AuctionID == auctionID);
            if (item == null) return;

            // TODO: 检查元石余额
            if (bidAmount <= item.CurrentBid) return;

            item.CurrentBid = bidAmount;
            item.BidderPlayerID = player.whoAmI;

            Main.NewText($"你以 {bidAmount} 元石竞价了 {item.ItemType}！", Microsoft.Xna.Framework.Color.Yellow);
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
            // TODO: 结算拍卖结果
            // 买家获得物品，卖家获得元石
            // NPC竞拍者处理
        }

        private void NPCBidOnItem(AuctionItem item)
        {
            // TODO: NPC竞价AI
            // 根据NPC性格和信念决定是否竞拍
            // 贪婪型NPC更容易竞价
        }
    }
}