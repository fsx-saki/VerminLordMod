using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// DaoHenConflictSystem — 道痕冲突系统（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 计算多个蛊虫道痕之间的冲突掩码
    /// 2. 检测两条道痕路径是否存在冲突
    /// 3. 为 KongQiaoSlot.DaoHenTags 提供默认值填充
    /// 
    /// MVA 阶段：
    /// - 不实现冲突检测逻辑（P2 再填充）
    /// - 只确认 KongQiaoSlot.DaoHenTags 字段已预埋
    /// - 提供 CalculateConflictMask 和 HasConflict 接口占位
    /// - 提供 GetDefaultDaoHenTag 方法为已知蛊虫填充默认道痕
    /// 
    /// 依赖：
    /// - KongQiaoPlayer（空窍系统）
    /// - KongQiaoSlot（蛊虫槽位，包含 DaoHenTags 字段）
    /// </summary>
    public class DaoHenConflictSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static DaoHenConflictSystem Instance => ModContent.GetInstance<DaoHenConflictSystem>();

        // ============================================================
        // 道痕路径枚举
        // ============================================================

        /// <summary>
        /// 道痕路径（位掩码枚举，支持组合）。
        /// </summary>
        [System.Flags]
        public enum DaoPath : ulong
        {
            None = 0,
            Fire = 1 << 0,      // 炎道
            Ice = 1 << 1,       // 冰道
            Force = 1 << 2,     // 力道
            Wind = 1 << 3,      // 风道
            Blood = 1 << 4,     // 血道
            Wisdom = 1 << 5,    // 智道
            Moon = 1 << 6,      // 月道
            Poison = 1 << 7,    // 毒道
            Wood = 1 << 8,      // 木道
            Earth = 1 << 9,     // 土道
            Light = 1 << 10,    // 光道
            Dark = 1 << 11,     // 暗道
            Soul = 1 << 12,     // 魂道
            Sword = 1 << 13,    // 剑道
            Formation = 1 << 14,// 阵道
            Healing = 1 << 15,  // 医道
        }

        // ============================================================
        // 冲突掩码
        // ============================================================

        /// <summary>
        /// 冲突掩码结构体。
        /// </summary>
        public struct ConflictMask
        {
            /// <summary> 位掩码 </summary>
            public ulong Mask;

            /// <summary> 是否为空 </summary>
            public bool IsEmpty => Mask == 0;

            /// <summary> 设置冲突位 </summary>
            public void SetConflict(DaoPath path)
            {
                Mask |= (ulong)path;
            }

            /// <summary> 检查是否有冲突 </summary>
            public bool HasConflict(DaoPath path)
            {
                return (Mask & (ulong)path) != 0;
            }

            /// <summary> 合并另一个冲突掩码 </summary>
            public void Merge(ConflictMask other)
            {
                Mask |= other.Mask;
            }
        }

        // ============================================================
        // 道痕冲突规则表
        // ============================================================

        /// <summary>
        /// 道痕冲突规则。
        /// Key: 道痕路径 A
        /// Value: 与 A 冲突的道痕路径列表
        /// 
        /// 基于小说设定：
        /// - 炎道 ↔ 冰道（水火不容）
        /// - 光道 ↔ 暗道（对立）
        /// - 血道 ↔ 医道（杀戮与救治）
        /// - 其他冲突 P2 再填充
        /// </summary>
        public static readonly Dictionary<DaoPath, List<DaoPath>> ConflictRules = new()
        {
            [DaoPath.Fire] = new List<DaoPath> { DaoPath.Ice },
            [DaoPath.Ice] = new List<DaoPath> { DaoPath.Fire },
            [DaoPath.Light] = new List<DaoPath> { DaoPath.Dark },
            [DaoPath.Dark] = new List<DaoPath> { DaoPath.Light },
            [DaoPath.Blood] = new List<DaoPath> { DaoPath.Healing },
            [DaoPath.Healing] = new List<DaoPath> { DaoPath.Blood },
            // P2 扩展更多冲突规则
        };

        // ============================================================
        // 默认道痕映射表
        // ============================================================

        /// <summary>
        /// 已知蛊虫的默认道痕标签。
        /// Key: 物品类型 ID
        /// Value: 道痕路径
        /// 
        /// MVA 阶段：只填充已知蛊虫。
        /// P1 扩展：从数据库或配置加载。
        /// </summary>
        public static readonly Dictionary<int, DaoPath> DefaultDaoHenMap = new()
        {
            // 月光蛊 → 月道
            // 酒虫 → None（无道痕）
            // 骨枪蛊 → 力道
            // 炎心蛊 → 炎道
            // 冰蚕蛊 → 冰道
            // 风铃蛊 → 风道
            // 血颅蛊 → 血道
            // 智慧蛊 → 智道
            // 以下为占位，P1 再填充具体 TypeID
        };

        // ============================================================
        // 核心接口
        // ============================================================

        /// <summary>
        /// 计算多个活跃蛊虫之间的冲突掩码。
        /// </summary>
        public ConflictMask CalculateConflictMask(List<KongQiaoSlot> activeGus)
        {
            var mask = new ConflictMask();

            // 收集所有活跃蛊虫的道痕
            var activePaths = new List<DaoPath>();
            foreach (var slot in activeGus)
            {
                if (slot.IsActive)
                {
                    var path = GetDaoPathFromTag(slot.DaoHenTags);
                    if (path != DaoPath.None)
                        activePaths.Add(path);
                }
            }

            // 检查每对蛊虫之间的冲突
            for (int i = 0; i < activePaths.Count; i++)
            {
                for (int j = i + 1; j < activePaths.Count; j++)
                {
                    if (HasConflict(activePaths[i], activePaths[j]))
                    {
                        mask.SetConflict(activePaths[i]);
                        mask.SetConflict(activePaths[j]);
                    }
                }
            }

            return mask;
        }

        /// <summary>
        /// 检查两条道痕路径是否存在冲突。
        /// </summary>
        public bool HasConflict(DaoPath pathA, DaoPath pathB)
        {
            if (pathA == DaoPath.None || pathB == DaoPath.None)
                return false;

            if (ConflictRules.TryGetValue(pathA, out var conflicts))
            {
                return conflicts.Contains(pathB);
            }

            return false;
        }

        /// <summary>
        /// 检查冲突掩码中是否包含指定道痕路径的冲突。
        /// </summary>
        public bool HasConflict(ConflictMask mask, DaoPath pathA, DaoPath pathB)
        {
            return mask.HasConflict(pathA) || mask.HasConflict(pathB);
        }

        // ============================================================
        // 工具方法
        // ============================================================

        /// <summary>
        /// 从 DaoHenTags 位掩码中提取道痕路径。
        /// MVA 简化：每个蛊虫只有一条道痕路径。
        /// P1 扩展：支持多条道痕路径组合。
        /// </summary>
        public static DaoPath GetDaoPathFromTag(ulong daoHenTags)
        {
            if (daoHenTags == 0) return DaoPath.None;

            // 遍历所有枚举值，找到匹配的位
            foreach (DaoPath path in System.Enum.GetValues<DaoPath>())
            {
                if (path != DaoPath.None && ((ulong)path & daoHenTags) != 0)
                    return path;
            }

            return DaoPath.None;
        }

        /// <summary>
        /// 获取蛊虫物品的默认道痕标签。
        /// 在 KongQiaoPlayer.TryRefineGu 中调用。
        /// </summary>
        public static ulong GetDefaultDaoHenTag(int guTypeID)
        {
            if (DefaultDaoHenMap.TryGetValue(guTypeID, out var path))
            {
                return (ulong)path;
            }
            return 0; // 未知蛊虫：无道痕
        }

        /// <summary>
        /// 注册蛊虫的默认道痕标签。
        /// 由各蛊虫物品在 ModItem.SetStaticDefaults 中调用。
        /// </summary>
        public static void RegisterDaoHenTag(int guTypeID, DaoPath path)
        {
            DefaultDaoHenMap[guTypeID] = path;
        }

        /// <summary>
        /// 获取道痕路径的中文名称。
        /// </summary>
        public static string GetDaoPathName(DaoPath path)
        {
            return path switch
            {
                DaoPath.None => "无",
                DaoPath.Fire => "炎道",
                DaoPath.Ice => "冰道",
                DaoPath.Force => "力道",
                DaoPath.Wind => "风道",
                DaoPath.Blood => "血道",
                DaoPath.Wisdom => "智道",
                DaoPath.Moon => "月道",
                DaoPath.Poison => "毒道",
                DaoPath.Wood => "木道",
                DaoPath.Earth => "土道",
                DaoPath.Light => "光道",
                DaoPath.Dark => "暗道",
                DaoPath.Soul => "魂道",
                DaoPath.Sword => "剑道",
                DaoPath.Formation => "阵道",
                DaoPath.Healing => "医道",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取冲突掩码的文本描述。
        /// </summary>
        public static string GetConflictDescription(ConflictMask mask)
        {
            if (mask.IsEmpty) return "无冲突";

            var conflicts = new List<string>();
            foreach (DaoPath path in System.Enum.GetValues<DaoPath>())
            {
                if (path != DaoPath.None && mask.HasConflict(path))
                {
                    conflicts.Add(GetDaoPathName(path));
                }
            }

            return $"道痕冲突：{string.Join("、", conflicts)}";
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 预留：P2 阶段实现冲突检测
            // 每帧检查所有在线玩家的空窍，计算冲突掩码
            // MVA 阶段不执行实际检测
        }
    }
}
