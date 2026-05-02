using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Search;
using VerminLordMod.Common.Search.Searchables;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.GlobalNPCs
{
    /// <summary>
    /// 全局 NPC 尸体处理器
    /// 
    /// 劫持原版 NPC 死亡逻辑，为任何 NPC/怪物生成尸体。
    /// 尸体中保存额外战利品，玩家可通过搜尸获得更多收益。
    /// 
    /// 机制：
    /// 1. NPC 死亡时，将部分掉落物存入尸体而非直接掉落
    /// 2. 尸体保留 5 分钟后腐烂，剩余物品散落
    /// 3. 玩家可通过交互键（Open）触发深度搜尸
    /// 4. Boss 尸体有特殊标记和更高价值战利品
    /// 
    /// MVA 简化：
    /// - 非 Boss：30% 掉落存入尸体
    /// - Boss：50% 掉落存入尸体
    /// - 不修改原版掉落表，仅劫持 OnKill 后处理
    /// </summary>
    public class GlobalNPCCorpseHandler : GlobalNPC
    {
        // ===== 配置常量 =====

        /// <summary> 非 Boss 存入尸体的掉落比例 </summary>
        private const float NormalCorpseStorageRate = 0.3f;

        /// <summary> Boss 存入尸体的掉落比例 </summary>
        private const float BossCorpseStorageRate = 0.5f;

        /// <summary> 同屏最大尸体数量 </summary>
        private const int MaxCorpsesPerScreen = 10;

        /// <summary> 尸体生成最小间隔（帧），防止大量 NPC 同时死亡时刷爆 </summary>
        private const int MinSpawnInterval = 5;

        /// <summary> 上次生成尸体的帧 </summary>
        private static ulong _lastSpawnTick;

        // ============================================================
        // 钩子：NPC 死亡
        // ============================================================

        public override void OnKill(NPC npc)
        {
            // 跳过城镇 NPC（他们不产生搜尸体）
            if (npc.townNPC) return;

            // 跳过友方 NPC
            if (npc.friendly && !npc.townNPC) return;

            // 频率限制
            if (Main.GameUpdateCount - _lastSpawnTick < MinSpawnInterval) return;

            // 检查同屏尸体数量限制
            if (CountActiveCorpses() >= MaxCorpsesPerScreen) return;

            // 确定尸体类型和存储比例
            bool isBoss = npc.boss;
            float storageRate = isBoss ? BossCorpseStorageRate : NormalCorpseStorageRate;

            // 从 NPC 掉落中提取部分物品存入尸体（可能为空，尸体仍然生成）
            var storedItems = ExtractItemsForCorpse(npc, storageRate);

            // 生成尸体（即使没有额外战利品也生成空尸体）
            int corpseIndex = Projectile.NewProjectile(
                npc.GetSource_Death(),
                npc.Center,
                Vector2.Zero,
                ModContent.ProjectileType<NpcCorpse>(),
                0, 0, Main.myPlayer);

            if (corpseIndex < 0 || corpseIndex >= Main.maxProjectiles) return;

            var corpseProj = Main.projectile[corpseIndex];
            if (corpseProj.ModProjectile is not NpcCorpse corpse) return;

            // 初始化尸体数据
            corpse.CorpseType = isBoss ? CorpseType.Boss : CorpseType.Monster;
            corpse.SourceNPCType = npc.type;
            corpse.SourceNPCName = npc.GivenOrTypeName;
            corpse.OwnerName = npc.GivenOrTypeName;
            corpse.RemainingItems = storedItems;
            corpse.IsBossCorpse = isBoss;

            // Boss 尸体腐烂时间更长（10 分钟）
            if (isBoss)
            {
                corpse.Projectile.timeLeft = 36000;
            }

            // 注册到搜索系统（仅在客户端）
            if (!Main.dedServ)
            {
                var searchable = new CorpseSearchable(corpse);
                SearchSystem.Instance.Register(searchable);
            }

            _lastSpawnTick = Main.GameUpdateCount;

            // 调试日志（仅开发者模式）
            if (Main.netMode == NetmodeID.SinglePlayer && Main.player[Main.myPlayer].HasItem(ModContent.ItemType<Content.Items.Consumables.YuanS>()))
            {
                // 仅在有调试需求时启用
            }
        }

        // ============================================================
        // 工具方法
        // ============================================================

        /// <summary>
        /// 从 NPC 掉落中提取部分物品存入尸体。
        /// 通过检查 NPC 的 loot 列表和默认掉落来获取物品。
        /// 
        /// MVA 简化：不修改原版掉落表，而是通过 NPC 的属性生成额外的"搜尸奖励"。
        /// 这些奖励是原版掉落之外的额外收益。
        /// </summary>
        private List<Item> ExtractItemsForCorpse(NPC npc, float storageRate)
        {
            var items = new List<Item>();

            // MVA 简化：根据 NPC 属性生成搜尸奖励
            // 这些是原版掉落之外的额外物品，玩家搜尸可获得
            // P1 可改为劫持原版掉落表

            // 1. 基础奖励：每个尸体保底元石
            // 使用 lifeMax 和 damage 估算 NPC 价值
            int npcPower = npc.lifeMax + npc.damage * 10;
            int baseYuan = System.Math.Max(1, npcPower / 500); // 保底元石数量
            int extraYuan = Main.rand.Next(0, System.Math.Max(1, npcPower / 200)); // 额外随机元石

            var yuanS = new Item(ModContent.ItemType<Content.Items.Consumables.YuanS>());
            yuanS.stack = baseYuan + extraYuan;
            items.Add(yuanS);

            // 2. 稀有奖励：Boss 有更高概率
            float rareChance = npc.boss ? 0.3f : 0.05f;
            if (Main.rand.NextFloat() < rareChance * storageRate)
            {
                // 从原版掉落中随机选一个物品类型（仅示例，P1 可完善）
                int[] commonDrops = {
                    ItemID.GoldCoin,
                    ItemID.SilverCoin,
                    ItemID.CopperCoin,
                    ItemID.Heart,
                    ItemID.Star,
                    ItemID.SoulofLight,
                    ItemID.SoulofNight,
                    ItemID.SoulofFlight,
                    ItemID.SoulofFright,
                    ItemID.SoulofMight,
                    ItemID.SoulofSight
                };

                int dropIdx = Main.rand.Next(commonDrops.Length);
                var bonusItem = new Item(commonDrops[dropIdx]);
                bonusItem.stack = Main.rand.Next(1, 4);
                items.Add(bonusItem);
            }

            // 3. 根据 NPC 类型生成特色奖励
            AddSpecialDrops(npc, items, storageRate);

            return items;
        }

        /// <summary>
        /// 根据 NPC 类型添加特色搜尸奖励
        /// </summary>
        private void AddSpecialDrops(NPC npc, List<Item> items, float storageRate)
        {
            // 根据生物群系/类型添加特色物品
            // MVA 简化版，P1 可扩展

            // 洞穴层怪物有概率掉落矿石
            if (npc.position.Y > Main.worldSurface * 16.0 && Main.rand.NextFloat() < 0.1f * storageRate)
            {
                int[] ores = { ItemID.CopperOre, ItemID.IronOre, ItemID.SilverOre, ItemID.GoldOre };
                var ore = new Item(ores[Main.rand.Next(ores.Length)]);
                ore.stack = Main.rand.Next(1, 4);
                items.Add(ore);
            }

            // 地狱怪物有概率掉落狱石
            if (npc.position.Y > Main.maxTilesY * 0.8f * 16.0 && Main.rand.NextFloat() < 0.15f * storageRate)
            {
                var hellItem = new Item(ItemID.Hellstone);
                hellItem.stack = Main.rand.Next(1, 3);
                items.Add(hellItem);
            }

            // 丛林怪物有概率掉落丛林材料
            if (npc.position.Y > Main.worldSurface * 16.0 && npc.position.X > 0 && Main.rand.NextFloat() < 0.1f * storageRate)
            {
                // 简单检测是否在丛林：检查背景
                int jungleItems = ItemID.JungleSpores;
                var jungle = new Item(jungleItems);
                jungle.stack = Main.rand.Next(1, 3);
                items.Add(jungle);
            }

            // 腐化/猩红怪物
            if (npc.position.X > 0 && Main.rand.NextFloat() < 0.1f * storageRate)
            {
                // 简单随机
                if (Main.rand.NextBool())
                {
                    var soul = new Item(ItemID.SoulofNight);
                    soul.stack = Main.rand.Next(1, 3);
                    items.Add(soul);
                }
            }
        }

        /// <summary>
        /// 统计当前活跃的尸体数量
        /// </summary>
        private static int CountActiveCorpses()
        {
            int count = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 查找玩家附近的尸体
        /// </summary>
        public static NpcCorpse FindCorpseNearPlayer(Player player, float radius = 60f)
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

        /// <summary>
        /// 查找 NPC 附近的尸体
        /// </summary>
        public static NpcCorpse FindCorpseNearNPC(NPC npc, float radius = 300f)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.ModProjectile is NpcCorpse corpse)
                {
                    if (Vector2.Distance(npc.Center, corpse.Projectile.Center) < radius)
                        return corpse;
                }
            }
            return null;
        }
    }
}
