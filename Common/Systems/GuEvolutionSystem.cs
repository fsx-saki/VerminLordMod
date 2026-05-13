using System.Collections.Generic;
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
            // TODO: 注册所有蛊虫升阶路径
            // Example: 一转月光蛊 → 二转月辉蛊
            // AddPath(new EvolutionPath {
            //     SourceGuType = ModContent.ItemType<Moonlight>(),
            //     TargetGuType = ModContent.ItemType<MoonGlowGu>(),
            //     ...
            // });
        }

        public List<EvolutionPath> GetAvailablePaths(Player player, int guItemType)
        {
            var paths = new List<EvolutionPath>();
            if (!EvolutionPaths.TryGetValue(guItemType, out var allPaths)) return paths;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            foreach (var path in allPaths)
            {
                if (qiRealm.GuLevel < path.RequiredGuLevel) continue;
                // TODO: 检查材料是否充足
                paths.Add(path);
            }
            return paths;
        }

        public bool AttemptEvolution(Player player, EvolutionPath path)
        {
            float successRate = CalculateSuccessRate(player, path);
            bool success = Main.rand.NextFloat() <= successRate;

            if (success)
            {
                // TODO: 替换蛊虫为高阶版本
                Main.NewText($"蛊虫进化成功！", Microsoft.Xna.Framework.Color.Green);
            }
            else
            {
                ApplyFailurePenalty(player, path);
                Main.NewText($"蛊虫进化失败！", Microsoft.Xna.Framework.Color.Red);
            }
            return success;
        }

        private float CalculateSuccessRate(Player player, EvolutionPath path)
        {
            float rate = path.BaseSuccessRate;
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            rate += qiRealm.GuLevel * 0.05f;
            // TODO: 蛊虫忠诚度加成、炼化熟练度加成
            return MathHelper.Clamp(rate, 0f, 0.95f);
        }

        private void ApplyFailurePenalty(Player player, EvolutionPath path)
        {
            // TODO: 实现失败惩罚
            switch (path.FailurePenaltyType)
            {
                case 0: break;                     // 无惩罚
                case 1: /* 蛊虫降阶 */ break;
                case 2: /* 蛊虫死亡 */ break;
            }
        }
    }
}