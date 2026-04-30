using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 搜尸/掠夺系统（LootSystem）
    /// 
    /// 实现「靠近→提示→点击展开格子UI」的交互流程：
    /// 1. 玩家靠近尸体（80px）→ 尸体上方显示"点击搜索"提示
    /// 2. 玩家点击交互键 → 展开战利品格子UI，显示尸体中的物品
    /// 3. 玩家离开尸体（>100px）→ 关闭格子UI，靠近后重新显示提示
    /// 4. 玩家点击格子中的物品 → 拾取该物品
    /// 
    /// 支持两种尸体：
    /// - 怪物尸体（NpcCorpse.Type == Monster/Boss）：由 GlobalNPCCorpseHandler 生成
    /// - 玩家尸体（NpcCorpse.Type == Player）：由 NpcDeathHandler 生成
    /// 
    /// 依赖：
    /// - NpcCorpse（通用尸体实体）
    /// - NpcDeathHandler（死亡处理）
    /// - EventBus（事件发布）
    /// </summary>
    public class LootSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static LootSystem Instance => ModContent.GetInstance<LootSystem>();

        // ===== 配置常量 =====

        /// <summary> 暴露掉落比例 </summary>
        public const float ExposedDropRate = 0.3f;

        /// <summary> 玩家检测范围（靠近后显示提示） </summary>
        public const float PlayerDetectionRange = 80f;

        /// <summary> 交互范围（点击搜索/拾取） </summary>
        public const float InteractionRange = 80f;

        /// <summary> 关闭UI的距离（离开尸体多远关闭格子UI） </summary>
        public const float CloseUIRange = 100f;

        /// <summary> 基地搜刮判定阈值（真元占据 > 此值视为高价值） </summary>
        public const int BaseHighValueThreshold = 30;

        // ===== 格子UI状态 =====
        /// <summary> 当前打开的尸体格子UI（Key = Player.whoAmI, Value = 尸体） </summary>
        private readonly Dictionary<int, NpcCorpse> _openLootUIs = new();

        /// <summary> 上一帧的鼠标右键状态（用于上升沿检测） </summary>
        private bool _lastMouseRight;

        // ============================================================
        // 暴露掉落（击杀瞬间）
        // ============================================================

        /// <summary>
        /// 计算暴露掉落：从玩家背包中随机选择 30% 物品散落。
        /// 媒介武器和空窍石不掉落。
        /// </summary>
        public List<Item> CalculateExposedDrops(Player player, float rate = ExposedDropRate)
        {
            var drops = new List<Item>();
            foreach (Item item in player.inventory)
            {
                if (item == null || item.IsAir) continue;
                if (IsEssentialItem(item)) continue; // 媒介/空窍石不掉落
                if (Main.rand.NextFloat() < rate)
                {
                    drops.Add(item.Clone());
                    item.TurnToAir();
                }
            }
            return drops;
        }

        /// <summary>
        /// 判断是否为关键物品（死亡不掉落）
        /// </summary>
        public bool IsEssentialItem(Item item)
        {
            return item.type == ModContent.ItemType<GuMediumWeapon>()
                || item.type == ModContent.ItemType<KongQiaoStone>();
        }

        // ============================================================
        // 格子UI管理
        // ============================================================

        /// <summary>
        /// 打开尸体战利品格子UI。
        /// 由玩家在尸体附近按交互键触发，或由 UpdateCorpseDetection 自动触发。
        /// </summary>
        public void OpenLootUI(Player player, NpcCorpse corpse)
        {
            if (_openLootUIs.ContainsKey(player.whoAmI))
            {
                // 如果已经打开同一个尸体的UI，不做任何事
                if (_openLootUIs[player.whoAmI] == corpse)
                    return;
                // 否则先关闭之前的
                CloseLootUI(player);
            }

            _openLootUIs[player.whoAmI] = corpse;
            corpse.IsLootUIOpen = true;

            // 打开轻量 UI
            if (player.whoAmI == Main.myPlayer)
            {
                CorpseLootUI.Instance.Open(corpse);
            }
        }

        /// <summary>
        /// 关闭尸体战利品格子UI。
        /// </summary>
        public void CloseLootUI(Player player)
        {
            if (_openLootUIs.TryGetValue(player.whoAmI, out var corpse))
            {
                if (corpse != null && corpse.Projectile.active)
                {
                    corpse.IsLootUIOpen = false;
                }
                _openLootUIs.Remove(player.whoAmI);
            }

            // 关闭轻量 UI
            if (player.whoAmI == Main.myPlayer)
            {
                CorpseLootUI.Instance.Close();
            }
        }

        /// <summary>
        /// 从尸体中拾取指定索引的物品。
        /// 由格子UI点击触发。
        /// </summary>
        public bool TakeItemFromCorpse(Player player, int itemIndex)
        {
            if (!_openLootUIs.TryGetValue(player.whoAmI, out var corpse))
                return false;

            if (corpse == null || !corpse.Projectile.active)
            {
                CloseLootUI(player);
                return false;
            }

            // 检查玩家是否还在交互范围内
            if (!corpse.IsPlayerNearby(player, InteractionRange))
            {
                CloseLootUI(player);
                return false;
            }

            if (itemIndex < 0 || itemIndex >= corpse.RemainingItems.Count)
                return false;

            var item = corpse.RemainingItems[itemIndex];
            if (item == null || item.IsAir)
                return false;

            // 从尸体中移除并给玩家
            corpse.RemainingItems.RemoveAt(itemIndex);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), item);

            if (player.whoAmI == Main.myPlayer)
                Main.NewText($"获得: {item.Name}", Color.Green);

            // 如果尸体空了，自动关闭UI
            if (!corpse.HasRemainingItems())
            {
                CloseLootUI(player);
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("尸体已被搜刮干净。", Color.Gray);
            }

            return true;
        }

        /// <summary>
        /// 一键拾取所有物品。
        /// </summary>
        public bool TakeAllFromCorpse(Player player)
        {
            if (!_openLootUIs.TryGetValue(player.whoAmI, out var corpse))
                return false;

            if (corpse == null || !corpse.Projectile.active)
            {
                CloseLootUI(player);
                return false;
            }

            // 检查玩家是否还在交互范围内
            if (!corpse.IsPlayerNearby(player, InteractionRange))
            {
                CloseLootUI(player);
                return false;
            }

            if (!corpse.HasRemainingItems())
            {
                CloseLootUI(player);
                return false;
            }

            int count = 0;
            foreach (var item in corpse.RemainingItems.ToList())
            {
                if (item != null && !item.IsAir)
                {
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), item);
                    count++;
                }
            }
            corpse.RemainingItems.Clear();

            if (player.whoAmI == Main.myPlayer && count > 0)
                Main.NewText($"搜尸完成！获得 {count} 件物品。", Color.Green);

            CloseLootUI(player);
            return true;
        }

        /// <summary>
        /// 获取玩家当前打开的尸体
        /// </summary>
        public NpcCorpse GetOpenLootCorpse(int playerID)
        {
            _openLootUIs.TryGetValue(playerID, out var corpse);
            return corpse;
        }

        /// <summary>
        /// 检查玩家是否打开了格子UI
        /// </summary>
        public bool IsLootUIOpen(int playerID)
        {
            return _openLootUIs.ContainsKey(playerID);
        }

        // ============================================================
        // 玩家交互检测
        // ============================================================

        /// <summary>
        /// 检测玩家是否在尸体附近并按下了交互键。
        /// 由 PostUpdateWorld 每帧调用。
        /// 规则：
        /// - 未搜索的尸体（有物品）→ 打开 UI（标记已搜索）
        /// - 未搜索的尸体（空）→ 标记已搜索，不打开 UI
        /// - 已搜索过且空的尸体 → 不再响应交互
        /// - 已搜索过且有物品的尸体 → 由 UpdateCorpseDetection 自动处理
        /// </summary>
        public bool TryInteractWithCorpse(Player player)
        {
            // 仅在本地玩家执行
            if (player.whoAmI != Main.myPlayer) return false;

            // 查找附近的尸体
            NpcCorpse corpse = FindCorpseNearPlayer(player, InteractionRange);
            if (corpse == null) return false;

            // 检测鼠标右键的上升沿（按下瞬间，避免持续触发）
            // 使用 Main.mouseRight（原始鼠标状态）而非 Player.controlUseTile，
            // 因为 controlUseTile 可能被原版物块放置逻辑消耗掉
            bool rightClick = Main.mouseRight && !_lastMouseRight;
            if (!rightClick) return false;

            // 已搜索过且空的尸体 → 不再响应交互
            if (corpse.HasBeenSearchedByPlayer && !corpse.HasRemainingItems())
                return false;

            // 标记已被玩家搜索
            corpse.HasBeenSearchedByPlayer = true;

            // 只有有物品的尸体才打开 UI
            if (corpse.HasRemainingItems())
            {
                OpenLootUI(player, corpse);
            }

            return true;
        }

        /// <summary>
        /// 每帧更新玩家靠近检测和格子UI状态。
        /// </summary>
        public void UpdateCorpseDetection(Player player)
        {
            // 更新所有尸体的 HasPlayerNearby 标记
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse corpse)
                {
                    bool wasNearby = corpse.HasPlayerNearby;
                    corpse.HasPlayerNearby = corpse.IsPlayerNearby(player, PlayerDetectionRange);

                    // 已搜索过且有物品的尸体：靠近自动显示UI，远离自动隐藏
                    if (corpse.HasBeenSearchedByPlayer && corpse.HasRemainingItems())
                    {
                        if (corpse.HasPlayerNearby && !corpse.IsLootUIOpen)
                        {
                            // 玩家靠近 → 自动打开UI
                            OpenLootUI(player, corpse);
                        }
                        else if (!corpse.HasPlayerNearby && corpse.IsLootUIOpen)
                        {
                            // 玩家远离 → 自动关闭UI
                            CloseLootUI(player);
                        }
                    }
                }
            }

            // 检查已打开的格子UI是否需要关闭
            if (_openLootUIs.TryGetValue(player.whoAmI, out var openCorpse))
            {
                if (openCorpse == null || !openCorpse.Projectile.active)
                {
                    CloseLootUI(player);
                    return;
                }

                // 玩家离开太远 → 关闭UI
                if (!openCorpse.IsPlayerNearby(player, CloseUIRange))
                {
                    CloseLootUI(player);
                }
            }
        }

        /// <summary>
        /// 查找玩家附近的尸体（包括怪物和玩家尸体）
        /// </summary>
        private static NpcCorpse FindCorpseNearPlayer(Player player, float radius)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse corpse)
                {
                    if (Vector2.Distance(player.Center, corpse.Projectile.Center) < radius)
                        return corpse;
                }
            }
            return null;
        }

        // ============================================================
        // 基地搜刮规则（D-15）
        // ============================================================

        /// <summary>
        /// 判断 NPC 是否可以攻击基地中的玩家。
        /// NPC 只攻击在场且携带高价值物品的玩家，不翻箱子。
        /// </summary>
        public bool CanNPCAttackBase(NPC npc, Player player)
        {
            if (!player.active) return false; // 玩家不在场

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            // "高价值" = 携带元石 > 50 或启用高阶蛊虫
            bool hasHighValue = qiResource.QiOccupied > BaseHighValueThreshold;

            return hasHighValue;
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void Load()
        {
            // 初始化轻量 UI
            CorpseLootUI.Instance.Initialize();
        }

        public override void PostUpdateWorld()
        {
            // 每帧更新所有玩家的靠近检测和格子UI状态
            foreach (Player player in Main.player)
            {
                if (player.active && player.whoAmI == Main.myPlayer)
                {
                    UpdateCorpseDetection(player);

                    // 检测玩家交互（仅在本地客户端）
                    if (Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer >= 0)
                    {
                        TryInteractWithCorpse(player);
                    }
                }
            }

            // 保存鼠标右键状态供下一帧上升沿检测
            _lastMouseRight = Main.mouseRight;
        }
    }
}
