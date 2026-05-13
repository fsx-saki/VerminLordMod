using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // FormationSystem — 阵法系统大框
    //
    // 系统定位：
    // 阵法是蛊师的高级战术手段，通过放置阵法Tile激活区域效果。
    // 不同于迷踪阵（防御型），阵法涵盖攻击/防御/辅助多种类型。
    //
    // 功能规划：
    // 1. 阵法类型：迷踪阵（防御）、蛊阵（攻击）、灵阵（辅助）、
    //    困阵（控制）、杀阵（毁灭）
    // 2. 阵法等级：一阶~五阶，等级越高效果越强、消耗越大
    // 3. 阵法激活：放置阵眼Tile + 插入蛊虫 → 阵法激活
    // 4. 阵法范围：以阵眼为中心的区域效果
    // 5. 阵法维护：持续消耗元石，元石不足阵法失效
    // 6. 阵法组合：多个阵法叠加效果
    //
    // TODO:
    //   - 创建阵法Tile（阵眼Tile）
    //   - 创建阵法Buff（各类型区域效果Buff）
    //   - 实现阵法激活逻辑（插入蛊虫激活）
    //   - 实现阵法维护消耗
    //   - 实现阵法范围效果
    //   - 创建阵眼物品（可放置的阵法道具）
    //   - 实现阵法UI
    // ============================================================

    public enum FormationType
    {
        MizongZhen,         // 迷踪阵 - 降低NPC感知，防御型
        GuAttackZhen,       // 蛊阵 - 范围攻击，持续伤害区域内敌人
        SpiritZhen,         // 灵阵 - 辅助型，区域内回复真元/生命
        TrapZhen,           // 困阵 - 控制型，减速/定身区域内敌人
        KillZhen,           // 杀阵 - 毁灭型，高伤害但消耗巨大
        DetectionZhen,      // 探阵 - 情报型，侦测区域内NPC/资源
        SummonZhen          // 召阵 - 召唤型，在区域内召唤特定NPC
    }

    public enum FormationRank
    {
        Rank1,   // 一阶阵法 - 一转蛊师可用
        Rank2,   // 二阶阵法 - 二转蛊师可用
        Rank3,   // 三阶阵法 - 三转蛊师可用
        Rank4,   // 四阶阵法 - 四转蛊师可用（家老级）
        Rank5    // 五阶阵法 - 五转蛊师可用（族长级）
    }

    public class FormationInstance
    {
        public FormationType Type;
        public FormationRank Rank;
        public Microsoft.Xna.Framework.Point CenterTile;
        public int CoreGuTypeID;           // 阵眼蛊虫的物品类型
        public int OwnerPlayerID;
        public float RangePixels;
        public int MaintenanceTimer;
        public int MaintenanceCostPerTick;
        public bool IsActive;
        public float EffectMultiplier;     // 基于修为和蛊虫品质的效果倍率
    }

    public class FormationDefinition
    {
        public FormationType Type;
        public FormationRank Rank;
        public string DisplayName;
        public string Description;
        public int RequiredGuLevel;
        public int[] CompatibleGuTypes;     // 可作为阵眼的蛊虫类型
        public float BaseRange;
        public int BaseMaintenanceCost;
        public int ActivationQiCost;        // 激活消耗真元
        public FormationEffect Effect;
    }

    public class FormationEffect
    {
        public float AttackDamageBonus;
        public float DefenseBonus;
        public float QiRegenBonus;
        public float LifeRegenBonus;
        public float MovementSpeedModifier;
        public float NPCPERceptionReduction;
        public int DamagePerTick;           // 范围伤害（蛊阵/杀阵）
        public int SlowDuration;            // 减速持续时间（困阵）
        public bool RevealHiddenNPCs;       // 探阵效果
    }

    public class FormationSystem : ModSystem
    {
        public static FormationSystem Instance => ModContent.GetInstance<FormationSystem>();

        public List<FormationInstance> ActiveFormations = new();
        public Dictionary<string, FormationDefinition> FormationRegistry = new();

        public override void OnWorldLoad()
        {
            ActiveFormations.Clear();
            FormationRegistry.Clear();
            RegisterFormationDefinitions();
        }

        private void RegisterFormationDefinitions()
        {
            // TODO: 注册所有阵法定义
        }

        public void ActivateFormation(FormationType type, FormationRank rank,
            Microsoft.Xna.Framework.Point center, int coreGuType, Player owner)
        {
            // TODO: 验证激活条件（修为等级、蛊虫兼容性、真元消耗）
            var instance = new FormationInstance
            {
                Type = type,
                Rank = rank,
                CenterTile = center,
                CoreGuTypeID = coreGuType,
                OwnerPlayerID = owner.whoAmI,
                RangePixels = GetFormationRange(type, rank),
                MaintenanceCostPerTick = GetMaintenanceCost(type, rank),
                IsActive = true,
                EffectMultiplier = CalculateEffectMultiplier(owner, rank)
            };

            ActiveFormations.Add(instance);
        }

        public void DeactivateFormation(Microsoft.Xna.Framework.Point center)
        {
            ActiveFormations.RemoveAll(f => f.CenterTile == center);
        }

        public bool IsPlayerInFormation(Player player, FormationType type)
        {
            foreach (var formation in ActiveFormations)
            {
                if (!formation.IsActive || formation.Type != type) continue;
                var tileCenter = new Microsoft.Xna.Framework.Vector2(
                    formation.CenterTile.X * 16 + 8,
                    formation.CenterTile.Y * 16 + 8);
                if (Microsoft.Xna.Framework.Vector2.Distance(player.Center, tileCenter) < formation.RangePixels)
                    return true;
            }
            return false;
        }

        public float GetFormationRange(FormationType type, FormationRank rank)
        {
            float baseRange = type switch
            {
                FormationType.MizongZhen => 480f,
                FormationType.GuAttackZhen => 320f,
                FormationType.SpiritZhen => 400f,
                FormationType.TrapZhen => 256f,
                FormationType.KillZhen => 200f,
                FormationType.DetectionZhen => 600f,
                FormationType.SummonZhen => 300f,
                _ => 300f
            };
            return baseRange * (1f + (int)rank * 0.2f);
        }

        private int GetMaintenanceCost(FormationType type, FormationRank rank)
        {
            // TODO: 完善维护成本计算
            int baseCost = type switch
            {
                FormationType.KillZhen => 5,
                FormationType.GuAttackZhen => 3,
                _ => 1
            };
            return baseCost * (1 + (int)rank);
        }

        private float CalculateEffectMultiplier(Player owner, FormationRank rank)
        {
            var qiRealm = owner.GetModPlayer<QiRealmPlayer>();
            return 1f + qiRealm.GuLevel * 0.1f + (int)rank * 0.15f;
        }

        public override void PreUpdateWorld()
        {
            // TODO: 每帧更新阵法效果（范围伤害、维护消耗、效果Buff发放）
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存阵法数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载阵法数据
        }
    }
}