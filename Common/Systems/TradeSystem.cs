using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 交易/定价系统（TradeSystem）
    /// 
    /// 实现「完全自由定价」的交易系统：
    /// 1. NPC 根据玩家急需程度、声望、自身原型坐地起价
    /// 2. 无价格上限，玩家可选择接受/拒绝/动手
    /// 3. MVA 阶段只实现城镇 NPC（学堂家老/药堂家老/贾家商人）
    /// 
    /// 定价公式：
    /// 最终价格 = 基础价格 × 急需度 × (1/声望折扣) × (1/原型亲和)
    /// 
    /// 依赖：
    /// - GuWorldPlayer（声望系统）
    /// - QiResourcePlayer（真元/元石）
    /// - GuMasterBase（NPC 原型/亲和度）
    /// - EventBus（交易完成事件）
    /// 
    /// MVA 简化：
    /// - 急需度：仅根据生命值和真元判断
    /// - 声望折扣：线性插值
    /// - 原型亲和：通过 CooperationBias 获取
    /// </summary>
    public class TradeSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static TradeSystem Instance => ModContent.GetInstance<TradeSystem>();

        // ===== 配置常量 =====

        /// <summary> 濒死生命阈值 </summary>
        private const float CriticalHealthThreshold = 0.3f;

        /// <summary> 低生命阈值 </summary>
        private const float LowHealthThreshold = 0.5f;

        /// <summary> 低真元阈值 </summary>
        private const float LowQiThreshold = 20f;

        /// <summary> 声望折扣：敌对 (-100) 时的倍率 </summary>
        private const float HostileMultiplier = 2.0f;

        /// <summary> 声望折扣：友好 (100) 时的倍率 </summary>
        private const float FriendlyMultiplier = 0.5f;

        /// <summary> 坐地起价判定阈值（价格 > 基础价值 × 此值） </summary>
        private const float PriceGougeThreshold = 2.0f;

        // ============================================================
        // 动态定价计算
        // ============================================================

        /// <summary>
        /// 计算动态价格。
        /// 公式：基础价格 × 急需度 × (1/声望折扣) × (1/原型亲和)
        /// </summary>
        public float CalculatePrice(Player player, NPC npc, Item item)
        {
            if (item == null || item.IsAir) return 0f;

            float basePrice = item.value;
            if (basePrice <= 0) basePrice = 1f; // 防止零价格

            // 1. 急需度：玩家是否急需此物品
            float urgency = CalculateUrgency(player, item);

            // 2. 声望折扣：友好 = 0.5，敌对 = 2.0，中立 = 1.0
            float reputationMultiplier = CalculateReputationMultiplier(player, npc);

            // 3. 原型亲和：交易型 NPC 更温和，掠夺型更狠
            float archetypeMultiplier = CalculateArchetypeMultiplier(npc);

            // D-16: 完全自由，无上限
            float finalPrice = basePrice * urgency * (1f / reputationMultiplier) * (1f / archetypeMultiplier);

            // 确保至少为 1
            return System.Math.Max(1f, finalPrice);
        }

        /// <summary>
        /// 计算急需度。
        /// MVA 简化：根据玩家生命值和真元判断
        /// </summary>
        private float CalculateUrgency(Player player, Item item)
        {
            float healthPct = (float)player.statLife / player.statLifeMax2;

            // 濒死急需治疗 = 3 倍
            if (healthPct < CriticalHealthThreshold && item.healLife > 0)
                return 3.0f;

            // 低生命需要治疗 = 2 倍
            if (healthPct < LowHealthThreshold && item.healLife > 0)
                return 2.0f;

            // 真元低 = 急需元石类物品
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < LowQiThreshold)
                return 1.5f;

            return 1.0f;
        }

        /// <summary>
        /// 计算声望折扣倍率。
        /// 声望 → 折扣：敌对(-100) = 2.0 倍，中立(0) = 1.0，友好(100) = 0.5 倍
        /// </summary>
        private float CalculateReputationMultiplier(Player player, NPC npc)
        {
            var guWorldPlayer = player.GetModPlayer<GuWorldPlayer>();
            var faction = FactionID.Scattered;

            if (npc.ModNPC is GuMasterBase guMaster)
            {
                faction = guMaster.GetFaction();
            }

            int rep = guWorldPlayer.GetRepPoints(faction); // -1000 ~ 1000

            // 将声望映射到 [-100, 100] 范围
            float normalizedRep = MathHelper.Clamp(rep / 10f, -100f, 100f);

            // 线性插值：敌对(-100) = 2.0，中立(0) = 1.0，友好(100) = 0.5
            return MathHelper.Lerp(HostileMultiplier, FriendlyMultiplier, (normalizedRep + 100f) / 200f);
        }

        /// <summary>
        /// 计算原型亲和倍率。
        /// 通过 GuMasterBase 的 CooperationBias 获取。
        /// 如果 NPC 不是 GuMasterBase，返回 1.0（中立）。
        /// </summary>
        private float CalculateArchetypeMultiplier(NPC npc)
        {
            if (npc.ModNPC is GuMasterBase guMaster)
            {
                // CooperationBias: 0.5（交易型/温和）~ 1.5（掠夺型/狠）
                // 这里简化处理：根据性格判断
                return guMaster.GetPersonality() switch
                {
                    GuPersonality.Benevolent => 0.6f,   // 仁慈：给折扣
                    GuPersonality.Greedy => 1.3f,        // 贪婪：坐地起价
                    GuPersonality.Aggressive => 1.2f,    // 好斗：略高
                    GuPersonality.Proud => 1.1f,         // 高傲：略高
                    GuPersonality.Cautious => 0.9f,      // 谨慎：略低
                    GuPersonality.Loyal => 0.8f,         // 忠诚：给家族盟友折扣
                    GuPersonality.Treacherous => 1.4f,   // 反复无常：最高
                    _ => 1.0f                             // 中立
                };
            }

            // 非 GuMasterBase 的 NPC（如原版城镇 NPC）使用默认值
            return 1.0f;
        }

        // ============================================================
        // 交易完成处理
        // ============================================================

        /// <summary>
        /// 交易完成后发布事件。
        /// </summary>
        public void OnTradeCompleted(Player player, NPC npc, List<Item> sold, List<Item> bought, int yuanStoneDelta)
        {
            bool isGouged = false;
            if (bought.Count > 0 && bought[0] != null && !bought[0].IsAir)
            {
                float calculatedPrice = CalculatePrice(player, npc, bought[0]);
                isGouged = calculatedPrice > bought[0].value * PriceGougeThreshold;
            }

            EventBus.Publish(new TradeCompletedEvent
            {
                PlayerID = player.whoAmI,
                NPCType = npc.type,
                SoldItemTypes = sold.Select(i => i.type).ToList(),
                BoughtItemTypes = bought.Select(i => i.type).ToList(),
                YuanStoneDelta = yuanStoneDelta,
                IsPriceGouged = isGouged
            });

            // 坐地起价时提示玩家
            if (isGouged && player.whoAmI == Main.myPlayer)
            {
                string npcName = npc.GivenOrTypeName;
                Main.NewText($"{npcName} 坐地起价！价格远超正常价值！", Color.OrangeRed);
            }
        }

        /// <summary>
        /// 检查交易是否被坐地起价
        /// </summary>
        public bool IsPriceGouged(Player player, NPC npc, Item item)
        {
            float calculatedPrice = CalculatePrice(player, npc, item);
            return calculatedPrice > item.value * PriceGougeThreshold;
        }

        /// <summary>
        /// 获取价格倍率的详细说明（用于 UI 显示）
        /// </summary>
        public string GetPriceBreakdown(Player player, NPC npc, Item item)
        {
            float basePrice = item.value;
            float urgency = CalculateUrgency(player, item);
            float repMult = CalculateReputationMultiplier(player, npc);
            float archMult = CalculateArchetypeMultiplier(npc);
            float finalPrice = basePrice * urgency * (1f / repMult) * (1f / archMult);

            return $"基础价: {basePrice}\n" +
                   $"急需度: x{urgency:F1}\n" +
                   $"声望折扣: x{repMult:F2}\n" +
                   $"原型亲和: x{archMult:F2}\n" +
                   $"最终价: {(int)finalPrice}";
        }

        // ============================================================
        // 治疗服务定价
        // ============================================================

        /// <summary>
        /// 计算治疗服务的价格（用于药堂家老的治疗按钮）
        /// </summary>
        public float CalculateHealPrice(Player player, NPC npc)
        {
            // 使用治疗药水作为价格基准
            var healPotion = new Item(ItemID.HealingPotion);
            float baseHealPrice = CalculatePrice(player, npc, healPotion);

            // 根据玩家缺失的生命值调整
            float missingHealth = player.statLifeMax2 - player.statLife;
            float healthRatio = (float)missingHealth / player.statLifeMax2;

            // 缺失生命越多，治疗越贵
            return baseHealPrice * (1f + healthRatio);
        }
    }
}
