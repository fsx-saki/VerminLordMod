using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // AlchemySystem — 丹药/炼丹系统大框
    //
    // 系统定位：
    // 丹药是蛊师的核心消耗品，类似原版的药水但更丰富。
    // 炼丹是将药材 → 丹药的转化过程。
    //
    // 功能规划：
    // 1. 丹药配方注册（药材组合 → 丹药产出）
    // 2. 炼丹炉 Tile（放置在炼丹炉旁才能炼制）
    // 3. 炼丹成功率（受修为等级、药材品质影响）
    // 4. 丹药品质系统（下品/中品/上品/极品）
    // 5. 丹药效果：回元丹（回复真元）、疗伤丹（回复生命）、
    //    破境丹（突破辅助）、解毒丹（清除蛊毒）、
    //    灵识丹（搜索加成）、铁骨丹（防御加成）等
    //
    // TODO:
    //   - 实现丹药配方数据
    //   - 实现炼丹炉Tile
    //   - 实现炼丹UI（选择配方、投入药材、开始炼制）
    //   - 实现炼丹成功率计算
    //   - 实现丹药品质随机
    //   - 创建所有丹药ModItem
    //   - 创建药材ModItem
    // ============================================================

    public enum PillQuality
    {
        Low,        // 下品 - 基础效果
        Medium,     // 中品 - 1.5倍效果
        High,       // 上品 - 2倍效果
        Supreme     // 极品 - 3倍效果，稀有
    }

    public enum PillType
    {
        HealingPill,        // 疗伤丹 - 回复生命
        QiRecoveryPill,     // 回元丹 - 回复真元
        BreakthroughPill,   // 破境丹 - 突破辅助
        DetoxPill,          // 解毒丹 - 清除蛊毒
        PerceptionPill,     // 灵识丹 - 搜索加成
        DefensePill,        // 铁骨丹 - 防御加成
        StrengthPill,       // 强身丹 - 攻击加成
        SpeedPill,          // 神行丹 - 移动速度
        VisionPill,         // 开目丹 - 增加视野
        AwakeningPill       // 开窍丹 - 辅助开窍
    }

    public class PillRecipe
    {
        public PillType OutputPill;
        public List<int> RequiredHerbTypes = new();
        public List<int> RequiredHerbCounts = new();
        public int RequiredGuLevel;
        public float BaseSuccessRate;
        public int CraftTimeTicks;
    }

    public class PillCraftResult
    {
        public PillType Type;
        public PillQuality Quality;
        public int ItemType;
        public bool Success;
    }

    public class AlchemySystem : ModSystem
    {
        public static AlchemySystem Instance => ModContent.GetInstance<AlchemySystem>();

        public Dictionary<PillType, PillRecipe> PillRecipes = new();

        public override void OnWorldLoad()
        {
            PillRecipes.Clear();
            RegisterPillRecipes();
        }

        private void RegisterPillRecipes()
        {
            // TODO: 注册所有丹药配方
            // PillRecipes[PillType.HealingPill] = new PillRecipe { ... };
            // PillRecipes[PillType.QiRecoveryPill] = new PillRecipe { ... };
            // etc.
        }

        public float CalculateSuccessRate(Player player, PillRecipe recipe)
        {
            float rate = recipe.BaseSuccessRate;
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            rate += qiRealm.GuLevel * 0.05f;
            // TODO: 药材品质加成、丹药熟练度加成
            return MathHelper.Clamp(rate, 0f, 1f);
        }

        public PillQuality RollQuality(Player player)
        {
            float luck = player.GetModPlayer<EffectsPlayer>().LuckModifier;
            float roll = Main.rand.NextFloat() + luck * 0.1f;
            // TODO: 完善品质随机逻辑
            return roll switch
            {
                >= 0.95f => PillQuality.Supreme,
                >= 0.7f => PillQuality.High,
                >= 0.4f => PillQuality.Medium,
                _ => PillQuality.Low
            };
        }

        public PillCraftResult CraftPill(Player player, PillType type)
        {
            if (!PillRecipes.TryGetValue(type, out var recipe)) return null;

            float successRate = CalculateSuccessRate(player, recipe);
            bool success = Main.rand.NextFloat() <= successRate;

            if (!success) return new PillCraftResult { Type = type, Success = false };

            var quality = RollQuality(player);
            // TODO: 根据 quality 和 type 找到对应的 ModItem type
            return new PillCraftResult
            {
                Type = type,
                Quality = quality,
                Success = true,
                ItemType = GetPillItemType(type, quality)
            };
        }

        private int GetPillItemType(PillType type, PillQuality quality)
        {
            // TODO: 返回对应丹药物品的 ModContent.ItemType
            return 0;
        }
    }

    // ============================================================
    // AlchemyPlayer — 玩家炼丹熟练度追踪
    // ============================================================

    public class AlchemyPlayer : ModPlayer
    {
        public int AlchemyExp;                  // 炼丹经验
        public int AlchemyLevel;                // 炼丹等级 (0-10)
        public Dictionary<PillType, int> PillCraftCounts = new();  // 各丹药炼制次数
        public float AlchemySuccessBonus;       // 炼丹成功率加成

        public void AddAlchemyExp(PillType type, int exp)
        {
            AlchemyExp += exp;
            if (!PillCraftCounts.ContainsKey(type)) PillCraftCounts[type] = 0;
            PillCraftCounts[type]++;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            // TODO: 炼丹等级提升逻辑
        }

        public override void SaveData(TagCompound tag)
        {
            tag["AlchemyExp"] = AlchemyExp;
            tag["AlchemyLevel"] = AlchemyLevel;
        }

        public override void LoadData(TagCompound tag)
        {
            AlchemyExp = tag.GetInt("AlchemyExp");
            AlchemyLevel = tag.GetInt("AlchemyLevel");
        }
    }
}