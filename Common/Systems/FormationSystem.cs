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
            FormationRegistry["mizong_1"] = new FormationDefinition
            {
                Type = FormationType.MizongZhen,
                Rank = FormationRank.Rank1,
                DisplayName = "一阶迷踪阵",
                Description = "降低区域内敌人感知，防御型阵法",
                RequiredGuLevel = 1,
                BaseRange = 480f,
                BaseMaintenanceCost = 1,
                ActivationQiCost = 50,
                Effect = new FormationEffect { NPCPERceptionReduction = 0.3f },
            };

            FormationRegistry["guattack_1"] = new FormationDefinition
            {
                Type = FormationType.GuAttackZhen,
                Rank = FormationRank.Rank1,
                DisplayName = "一阶蛊阵",
                Description = "对区域内敌人造成持续伤害",
                RequiredGuLevel = 1,
                BaseRange = 320f,
                BaseMaintenanceCost = 2,
                ActivationQiCost = 80,
                Effect = new FormationEffect { DamagePerTick = 5 },
            };

            FormationRegistry["spirit_1"] = new FormationDefinition
            {
                Type = FormationType.SpiritZhen,
                Rank = FormationRank.Rank1,
                DisplayName = "一阶灵阵",
                Description = "区域内回复真元和生命",
                RequiredGuLevel = 2,
                BaseRange = 400f,
                BaseMaintenanceCost = 1,
                ActivationQiCost = 60,
                Effect = new FormationEffect { QiRegenBonus = 0.5f, LifeRegenBonus = 0.3f },
            };

            FormationRegistry["trap_1"] = new FormationDefinition
            {
                Type = FormationType.TrapZhen,
                Rank = FormationRank.Rank1,
                DisplayName = "一阶困阵",
                Description = "减速区域内敌人",
                RequiredGuLevel = 2,
                BaseRange = 256f,
                BaseMaintenanceCost = 2,
                ActivationQiCost = 70,
                Effect = new FormationEffect { MovementSpeedModifier = -0.3f, SlowDuration = 120 },
            };

            FormationRegistry["kill_3"] = new FormationDefinition
            {
                Type = FormationType.KillZhen,
                Rank = FormationRank.Rank3,
                DisplayName = "三阶杀阵",
                Description = "对区域内敌人造成高额伤害",
                RequiredGuLevel = 3,
                BaseRange = 200f,
                BaseMaintenanceCost = 5,
                ActivationQiCost = 200,
                Effect = new FormationEffect { DamagePerTick = 30 },
            };

            FormationRegistry["detection_1"] = new FormationDefinition
            {
                Type = FormationType.DetectionZhen,
                Rank = FormationRank.Rank1,
                DisplayName = "一阶探阵",
                Description = "侦测区域内隐藏的NPC和资源",
                RequiredGuLevel = 1,
                BaseRange = 600f,
                BaseMaintenanceCost = 1,
                ActivationQiCost = 40,
                Effect = new FormationEffect { RevealHiddenNPCs = true },
            };
        }

        public bool ActivateFormation(FormationType type, FormationRank rank,
            Microsoft.Xna.Framework.Point center, int coreGuType, Player owner)
        {
            var qiRealm = owner.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < (int)rank + 1)
            {
                Main.NewText($"需要{(int)rank + 1}转蛊师才能激活此阵法", Microsoft.Xna.Framework.Color.Red);
                return false;
            }

            var def = GetDefinition(type, rank);
            if (def == null) return false;

            var qiResource = owner.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < def.ActivationQiCost)
            {
                Main.NewText($"真元不足，需要{def.ActivationQiCost}点真元", Microsoft.Xna.Framework.Color.Red);
                return false;
            }

            qiResource.ConsumeQi(def.ActivationQiCost);

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
            Main.NewText($"【阵法】{def.DisplayName}已激活！", Microsoft.Xna.Framework.Color.Cyan);
            return true;
        }

        private FormationDefinition GetDefinition(FormationType type, FormationRank rank)
        {
            foreach (var def in FormationRegistry.Values)
            {
                if (def.Type == type && def.Rank == rank)
                    return def;
            }
            return null;
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
            int baseCost = type switch
            {
                FormationType.KillZhen => 5,
                FormationType.GuAttackZhen => 3,
                FormationType.TrapZhen => 2,
                FormationType.SummonZhen => 2,
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
            for (int i = ActiveFormations.Count - 1; i >= 0; i--)
            {
                var formation = ActiveFormations[i];
                if (!formation.IsActive) continue;

                formation.MaintenanceTimer++;
                if (formation.MaintenanceTimer >= 60)
                {
                    formation.MaintenanceTimer = 0;
                    var owner = Main.player[formation.OwnerPlayerID];
                    if (owner == null || !owner.active)
                    {
                        formation.IsActive = false;
                        continue;
                    }

                    var qiResource = owner.GetModPlayer<QiResourcePlayer>();
                    if (qiResource.QiCurrent < formation.MaintenanceCostPerTick)
                    {
                        formation.IsActive = false;
                        Main.NewText($"【阵法】阵法因真元不足而失效", Microsoft.Xna.Framework.Color.Red);
                        continue;
                    }

                    qiResource.ConsumeQi(formation.MaintenanceCostPerTick);
                }

                ApplyFormationEffects(formation);
            }
        }

        private void ApplyFormationEffects(FormationInstance formation)
        {
            var def = GetDefinition(formation.Type, formation.Rank);
            if (def == null) return;

            var center = new Microsoft.Xna.Framework.Vector2(
                formation.CenterTile.X * 16 + 8,
                formation.CenterTile.Y * 16 + 8);

            switch (formation.Type)
            {
                case FormationType.GuAttackZhen:
                case FormationType.KillZhen:
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        var npc = Main.npc[i];
                        if (!npc.active || npc.friendly) continue;
                        if (Microsoft.Xna.Framework.Vector2.Distance(npc.Center, center) < formation.RangePixels)
                        {
                            int damage = (int)(def.Effect.DamagePerTick * formation.EffectMultiplier);
                            npc.life -= damage;
                            if (npc.life <= 0) npc.checkDead();
                        }
                    }
                    break;

                case FormationType.SpiritZhen:
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        var player = Main.player[i];
                        if (!player.active) continue;
                        if (Microsoft.Xna.Framework.Vector2.Distance(player.Center, center) < formation.RangePixels)
                        {
                            if (Main.GameUpdateCount % 60 == 0)
                            {
                                player.statLife += (int)(def.Effect.LifeRegenBonus * formation.EffectMultiplier * 10);
                                if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
                            }
                        }
                    }
                    break;

                case FormationType.TrapZhen:
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        var npc = Main.npc[i];
                        if (!npc.active || npc.friendly) continue;
                        if (Microsoft.Xna.Framework.Vector2.Distance(npc.Center, center) < formation.RangePixels)
                        {
                            npc.velocity *= 0.7f;
                        }
                    }
                    break;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var f in ActiveFormations)
            {
                list.Add(new TagCompound
                {
                    ["type"] = (int)f.Type,
                    ["rank"] = (int)f.Rank,
                    ["centerX"] = f.CenterTile.X,
                    ["centerY"] = f.CenterTile.Y,
                    ["coreGu"] = f.CoreGuTypeID,
                    ["owner"] = f.OwnerPlayerID,
                    ["active"] = f.IsActive,
                });
            }
            tag["formations"] = list;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveFormations.Clear();

            var list = tag.GetList<TagCompound>("formations");
            if (list == null) return;

            foreach (var t in list)
            {
                ActiveFormations.Add(new FormationInstance
                {
                    Type = (FormationType)t.GetInt("type"),
                    Rank = (FormationRank)t.GetInt("rank"),
                    CenterTile = new Microsoft.Xna.Framework.Point(t.GetInt("centerX"), t.GetInt("centerY")),
                    CoreGuTypeID = t.GetInt("coreGu"),
                    OwnerPlayerID = t.GetInt("owner"),
                    IsActive = t.GetBool("active"),
                    RangePixels = GetFormationRange((FormationType)t.GetInt("type"), (FormationRank)t.GetInt("rank")),
                    MaintenanceCostPerTick = GetMaintenanceCost((FormationType)t.GetInt("type"), (FormationRank)t.GetInt("rank")),
                });
            }
        }
    }
}