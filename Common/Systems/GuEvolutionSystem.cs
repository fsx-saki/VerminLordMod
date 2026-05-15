using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // GuEvolutionSystem — 蛊虫进化/升阶系统大框
    //
    // 系统定位：
    // 蛊虫可通过吞噬、喂养、环境刺激等方式进化到更高阶。
    // 对应小说中蛊虫升阶的核心机制。
    //
    // 功能规划：
    // 1. 蛊虫升阶路径（一转蛊 → 二转蛊 → ...）
    // 2. 升阶条件：吞噬同属低阶蛊 + 真元消耗 + 修为等级
    // 3. 升阶风险：失败可能导致蛊虫死亡/降阶
    // 4. 变异进化：特殊条件可能触发变异（产出稀有蛊虫）
    // 5. 蛊虫忠诚度：进化后忠诚度变化
    // 6. 本命蛊进阶：本命蛊进阶需要更严格的条件
    //
    // TODO:
    //   - 实现蛊虫升阶路径数据
    //   - 实现升阶成功率计算
    //   - 实现升阶UI
    //   - 实现变异触发逻辑
    //   - 实现失败惩罚逻辑
    // ============================================================

    public enum EvolutionType
    {
        NormalUpgrade,      // 正常升阶：一转→二转
        Mutation,           // 变异进化：触发特殊变异
        DevourUpgrade,      // 吞噬升阶：吞噬同属蛊虫升阶
        Fusion,             // 融合进化：两只蛊虫融合
        SpecialUpgrade      // 特殊条件进化（天劫/祭坛等）
    }

    public class EvolutionPath
    {
        public int SourceGuType;            // 低阶蛊虫物品Type
        public int TargetGuType;            // 高阶蛊虫物品Type
        public EvolutionType Type;
        public int RequiredGuLevel;         // 玩家修为等级要求
        public int RequiredQiCost;          // 真元消耗
        public float BaseSuccessRate;       // 基础成功率
        public List<int> RequiredMaterials; // 需要吞噬的材料蛊虫Type
        public List<int> RequiredMaterialCounts;
        public bool IsMainGuOnly;           // 仅本命蛊可进化
        public int FailurePenaltyType;      // 失败惩罚类型（0=无/1=降阶/2=死亡）
    }

    public class GuEvolutionSystem : ModSystem
    {
        public static GuEvolutionSystem Instance => ModContent.GetInstance<GuEvolutionSystem>();

        public Dictionary<int, List<EvolutionPath>> EvolutionPaths = new();

        public override void OnWorldLoad()
        {
            EvolutionPaths.Clear();
            RegisterEvolutionPaths();
        }

        private void RegisterEvolutionPaths()
        {
            RegisterPath(ModContent.ItemType<Content.Items.Weapons.One.Moonlight>(),
                ModContent.ItemType<Content.Items.Weapons.Two.MoonlightPro>(),
                EvolutionType.NormalUpgrade, 2, 50, 0.7f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.MoonlightPro>(),
                ModContent.ItemType<Content.Items.Weapons.Three.GoldMoon>(),
                EvolutionType.NormalUpgrade, 3, 100, 0.6f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.GhostFireGu>(),
                ModContent.ItemType<Content.Items.Weapons.Three.GhostFireGuPro>(),
                EvolutionType.NormalUpgrade, 3, 80, 0.6f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.BloodQiGu>(),
                ModContent.ItemType<Content.Items.Weapons.Three.BloodMoonGu>(),
                EvolutionType.NormalUpgrade, 3, 80, 0.55f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.IceAwlGu>(),
                ModContent.ItemType<Content.Items.Weapons.Three.IceCrystalGu>(),
                EvolutionType.NormalUpgrade, 3, 80, 0.6f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.LightningSpearGu>(),
                ModContent.ItemType<Content.Items.Weapons.Three.ThunderBallGu>(),
                EvolutionType.NormalUpgrade, 3, 80, 0.55f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Three.GoldMoon>(),
                ModContent.ItemType<Content.Items.Weapons.Four.FourStarCubeGu>(),
                EvolutionType.NormalUpgrade, 4, 150, 0.5f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Three.BloodMoonGu>(),
                ModContent.ItemType<Content.Items.Weapons.Four.BloodSkullGu>(),
                EvolutionType.NormalUpgrade, 4, 150, 0.45f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Four.BloodSkullGu>(),
                ModContent.ItemType<Content.Items.Weapons.Five.BloodHandprintGu>(),
                EvolutionType.NormalUpgrade, 5, 250, 0.4f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Four.FourStarCubeGu>(),
                ModContent.ItemType<Content.Items.Weapons.Five.FiveStarLinkedBeadGu>(),
                EvolutionType.NormalUpgrade, 5, 250, 0.4f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Five.FiveStarLinkedBeadGu>(),
                ModContent.ItemType<Content.Items.Weapons.Six.DarkArrowGu>(),
                EvolutionType.NormalUpgrade, 6, 400, 0.35f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.FourFlavorWineBugWeapon>(),
                ModContent.ItemType<Content.Items.Weapons.Three.SevenWineBugWeapon>(),
                EvolutionType.NormalUpgrade, 3, 100, 0.6f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Three.SevenWineBugWeapon>(),
                ModContent.ItemType<Content.Items.Weapons.Four.NineWineBugWeapon>(),
                EvolutionType.NormalUpgrade, 4, 180, 0.5f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.BigStrengthGu>(),
                ModContent.ItemType<Content.Items.Weapons.Four.GiantSpiritBodyGu>(),
                EvolutionType.DevourUpgrade, 4, 200, 0.4f);

            RegisterPath(ModContent.ItemType<Content.Items.Weapons.Two.BigSoulGu>(),
                ModContent.ItemType<Content.Items.Weapons.Four.GiantSpiritIntentGu>(),
                EvolutionType.DevourUpgrade, 4, 200, 0.4f);
        }

        private void RegisterPath(int sourceType, int targetType, EvolutionType evoType,
            int requiredLevel, int qiCost, float baseRate,
            List<int> materials = null, List<int> matCounts = null,
            bool mainGuOnly = false, int failurePenalty = 1)
        {
            if (!EvolutionPaths.ContainsKey(sourceType))
                EvolutionPaths[sourceType] = new List<EvolutionPath>();

            EvolutionPaths[sourceType].Add(new EvolutionPath
            {
                SourceGuType = sourceType,
                TargetGuType = targetType,
                Type = evoType,
                RequiredGuLevel = requiredLevel,
                RequiredQiCost = qiCost,
                BaseSuccessRate = baseRate,
                RequiredMaterials = materials ?? new List<int>(),
                RequiredMaterialCounts = matCounts ?? new List<int>(),
                IsMainGuOnly = mainGuOnly,
                FailurePenaltyType = failurePenalty,
            });
        }

        public List<EvolutionPath> GetAvailablePaths(Player player, int guItemType)
        {
            var paths = new List<EvolutionPath>();
            if (!EvolutionPaths.TryGetValue(guItemType, out var allPaths)) return paths;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            foreach (var path in allPaths)
            {
                if (qiRealm.GuLevel < path.RequiredGuLevel) continue;
                if (qiResource.QiCurrent < path.RequiredQiCost) continue;
                if (!HasRequiredMaterials(player, path)) continue;
                paths.Add(path);
            }
            return paths;
        }

        private bool HasRequiredMaterials(Player player, EvolutionPath path)
        {
            if (path.RequiredMaterials.Count == 0) return true;

            for (int i = 0; i < path.RequiredMaterials.Count; i++)
            {
                int requiredType = path.RequiredMaterials[i];
                int requiredCount = i < path.RequiredMaterialCounts.Count
                    ? path.RequiredMaterialCounts[i] : 1;

                int playerCount = 0;
                for (int j = 0; j < player.inventory.Length; j++)
                {
                    if (player.inventory[j].type == requiredType)
                        playerCount += player.inventory[j].stack;
                }

                if (playerCount < requiredCount) return false;
            }
            return true;
        }

        private void ConsumeMaterials(Player player, EvolutionPath path)
        {
            for (int i = 0; i < path.RequiredMaterials.Count; i++)
            {
                int requiredType = path.RequiredMaterials[i];
                int requiredCount = i < path.RequiredMaterialCounts.Count
                    ? path.RequiredMaterialCounts[i] : 1;

                int remaining = requiredCount;
                for (int j = 0; j < player.inventory.Length && remaining > 0; j++)
                {
                    if (player.inventory[j].type == requiredType)
                    {
                        int consume = System.Math.Min(remaining, player.inventory[j].stack);
                        player.inventory[j].stack -= consume;
                        remaining -= consume;
                        if (player.inventory[j].stack <= 0)
                            player.inventory[j].TurnToAir();
                    }
                }
            }
        }

        public bool AttemptEvolution(Player player, EvolutionPath path)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < path.RequiredQiCost) return false;

            float successRate = CalculateSuccessRate(player, path);
            bool success = Main.rand.NextFloat() <= successRate;

            qiResource.ConsumeQi(path.RequiredQiCost);
            ConsumeMaterials(player, path);

            if (success)
            {
                ReplaceGuItem(player, path);
                Main.NewText("蛊虫进化成功！", Color.Green);
            }
            else
            {
                ApplyFailurePenalty(player, path);
                Main.NewText("蛊虫进化失败！", Color.Red);
            }
            return success;
        }

        private void ReplaceGuItem(Player player, EvolutionPath path)
        {
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == path.SourceGuType)
                {
                    player.inventory[i].TurnToAir();
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), path.TargetGuType);
                    return;
                }
            }
        }

        private float CalculateSuccessRate(Player player, EvolutionPath path)
        {
            float rate = path.BaseSuccessRate;
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            rate += qiRealm.GuLevel * 0.05f;

            var guSoulPlayer = player.GetModPlayer<GuSoulPlayer>();
            if (guSoulPlayer.HasMainGu && guSoulPlayer.MainGu.GuItemID == path.SourceGuType)
            {
                rate += guSoulPlayer.MainGu.BondLevel * 0.03f;
            }

            return MathHelper.Clamp(rate, 0.05f, 0.95f);
        }

        private void ApplyFailurePenalty(Player player, EvolutionPath path)
        {
            switch (path.FailurePenaltyType)
            {
                case 0:
                    break;
                case 1:
                    for (int i = 0; i < player.inventory.Length; i++)
                    {
                        if (player.inventory[i].type == path.SourceGuType)
                        {
                            player.inventory[i].TurnToAir();
                            Main.NewText("蛊虫降阶失败，彻底损毁！", Color.OrangeRed);
                            return;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < player.inventory.Length; i++)
                    {
                        if (player.inventory[i].type == path.SourceGuType)
                        {
                            player.inventory[i].TurnToAir();
                            break;
                        }
                    }
                    var qiRealm = player.GetModPlayer<QiRealmPlayer>();
                    qiRealm.BreakthroughProgress = System.Math.Max(0, qiRealm.BreakthroughProgress - 20f);
                    Main.NewText("蛊虫死亡！修为受损...", Color.DarkRed);
                    break;
            }
        }
    }
}