using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // CultivationLoopSystem — 修为进阶闭环大框
    //
    // 系统定位：
    // 修为进阶是蛊师的核心成长路径。当前 QiRealmPlayer 有破境逻辑，
    // 但缺乏完整的闭环：开窍→修炼→破境→天劫→新境界→新空窍→更多蛊虫。
    // 此系统串联所有修为相关的子系统。
    //
    // 功能规划：
    // 1. 开窍觉醒（AwakeningSystem 已有，需串联）
    // 2. 日常修炼：真元积累、空窍巩固
    // 3. 小境界突破（QiRealmPlayer.StageUp 已有）
    // 4. 大境界突破（QiRealmPlayer.LevelUp 已有）
    // 5. 天劫渡劫（HeavenTribulationSystem 已有，需串联）
    // 6. 破境后的闭环：新空窍格数→更多蛊虫→更强修为→再次破境
    // 7. 修为瓶颈：每个大境界后期有瓶颈期，需要特殊突破条件
    //
    // 闭环流程：
    //   真元修炼 → 真元满 → 消耗元石破境 → 
    //   破境成功 → 天劫预警 → 天劫降临 → 
    //   渡劫成功 → 新境界解锁 → 空窍扩容 →
    //   炼化新蛊虫 → 继续修炼 → 循环
    //
    // TODO:
    //   - 实现日常修炼真元积累（修炼台Tile）
    //   - 实现天劫渡劫成功/失败的闭环效果
    //   - 实现破境瓶颈机制
    //   - 实现修为等级到空窍格数的映射表
    //   - 实现境界解锁内容（新区域、新配方、新蛊虫）
    //   - 创建修炼台Tile
    // ============================================================

    public class CultivationLoopSystem : ModSystem
    {
        public static CultivationLoopSystem Instance => ModContent.GetInstance<CultivationLoopSystem>();

        // ===== 修为等级 → 空窍格数映射 =====
        // TODO: 完善映射表，确保每个境界都有合理的格数
        public static int GetMaxKongQiaoSlots(int guLevel)
        {
            return guLevel switch
            {
                0 => 0,     // 未开窍
                1 => 3,     // 一转：3格（1本命+2辅蛊）
                2 => 5,     // 二转：5格
                3 => 7,     // 三转：7格
                4 => 9,     // 四转：9格
                5 => 12,    // 五转：12格
                6 => 15,    // 六转：15格
                7 => 18,    // 七转：18格
                8 => 21,    // 八转：21格
                9 => 25,    // 九转：25格
                _ => 25
            };
        }

        // ===== 修为等级 → 真元上限映射 =====
        public static int GetMaxQiForLevel(int guLevel, int stage)
        {
            int baseQi = guLevel * 100 + stage * 25;
            return baseQi;
        }

        // ===== 破境瓶颈检查 =====
        public static bool HasBreakthroughBottleneck(int guLevel, int stage)
        {
            // TODO: 完善瓶颈条件
            // 三转巅峰到四转是瓶颈
            // 需要特殊条件（天劫成功、族长批准等）
            return guLevel == 3 && stage == 3;
        }

        // ===== 境界解锁内容 =====
        public static string GetUnlockedContent(int guLevel)
        {
            // TODO: 完善境界解锁内容列表
            return guLevel switch
            {
                1 => "一转：基础蛊虫、学堂训练、青茅山探索",
                2 => "二转：中级蛊虫、家族委托、天劫预警、小型阵法",
                3 => "三转：高级蛊虫、家族职位、中型阵法、领地进入",
                4 => "四转：稀有蛊虫、族长级权限、大型阵法、家族战争",
                5 => "五转：传说蛊虫、南疆通行、顶阶阵法",
                _ => "未知境界"
            };
        }

        // ===== 修炼效率 =====
        public static float GetCultivationEfficiency(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            float efficiency = 1.0f;
            efficiency += qiRealm.GuLevel * 0.05f;
            // TODO: 灵池加成、修炼台加成、丹药加成
            return efficiency;
        }

        public override void PostUpdateWorld()
        {
            // TODO: 每日自动修炼真元积累
            // 如果玩家在修炼台/灵池附近，自动积累真元
        }
    }
}