using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Common.ImplementationTracker
{
    /// <summary>
    /// 实现状态追踪器 — 核心类。
    /// 负责：
    /// 1. 运行时扫描所有已加载的 Mod 类型，收集 [ImplStatus] 标记
    /// 2. 提供查询接口（按分类、按状态、按转数等）
    /// 3. 生成实现状态报告
    /// 4. 管理待办清单
    /// </summary>
    public class ImplementationTracker : ModSystem
    {
        /// <summary>
        /// 所有被追踪的 Content 对象
        /// </summary>
        public static List<TrackedEntry> AllEntries { get; private set; } = new();

        /// <summary>
        /// 按分类分组的条目
        /// </summary>
        public static Dictionary<string, List<TrackedEntry>> ByCategory { get; private set; } = new();

        /// <summary>
        /// 按状态分组的条目
        /// </summary>
        public static Dictionary<ImplStatus, List<TrackedEntry>> ByStatus { get; private set; } = new();

        /// <summary>
        /// 按转数分组的条目（仅蛊虫）
        /// </summary>
        public static Dictionary<string, List<TrackedEntry>> ByTurn { get; private set; } = new();

        /// <summary>
        /// 按 Dao 类型分组的条目
        /// </summary>
        public static Dictionary<string, List<TrackedEntry>> ByDaoType { get; private set; } = new();

        public override void PostSetupContent()
        {
            ScanAllTypes();
        }

        /// <summary>
        /// 扫描所有已加载的 Mod 类型，收集 [ImplStatus] 标记
        /// </summary>
        private void ScanAllTypes()
        {
            AllEntries.Clear();
            ByCategory.Clear();
            ByStatus.Clear();
            ByTurn.Clear();
            ByDaoType.Clear();

            var modTypes = ModContent.GetInstance<VerminLordMod>().GetType().Assembly.GetTypes();

            foreach (var type in modTypes)
            {
                // 只处理 Content 命名空间下的类型
                if (!type.Namespace?.StartsWith("VerminLordMod.Content") == true)
                    continue;

                var attr = type.GetCustomAttribute<ImplStatusAttribute>();
                if (attr == null)
                    continue;

                var entry = new TrackedEntry
                {
                    TypeName = type.Name,
                    FullTypeName = type.FullName,
                    Namespace = type.Namespace,
                    Category = InferCategory(type.Namespace),
                    Turn = InferTurn(type.Namespace, type.Name),
                    DaoType = attr.DaoType,
                    Status = attr.Status,
                    Note = attr.Note,
                    PlannedTurn = attr.PlannedTurn,
                    HasTexture = CheckTexture(type),
                    BaseType = type.BaseType?.Name ?? "",
                };

                AllEntries.Add(entry);
            }

            // 构建索引
            ByCategory = AllEntries.GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            ByStatus = AllEntries.GroupBy(e => e.Status)
                .ToDictionary(g => g.Key, g => g.ToList());

            ByTurn = AllEntries.Where(e => !string.IsNullOrEmpty(e.Turn))
                .GroupBy(e => e.Turn)
                .ToDictionary(g => g.Key, g => g.ToList());

            ByDaoType = AllEntries.Where(e => !string.IsNullOrEmpty(e.DaoType))
                .GroupBy(e => e.DaoType)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// 从命名空间推断分类
        /// </summary>
        private static string InferCategory(string ns)
        {
            if (string.IsNullOrEmpty(ns)) return "Unknown";

            // 去掉 "VerminLordMod.Content." 前缀
            var relative = ns.Replace("VerminLordMod.Content.", "");

            // 取第一段作为主分类
            var parts = relative.Split('.');
            if (parts.Length == 0) return "Unknown";

            var mainCategory = parts[0];

            // 对于 Items 下的子分类，进一步细分
            if (mainCategory == "Items" && parts.Length > 1)
            {
                var subCategory = parts[1];
                // 如果是 Weapons/Accessories/Consumables 下的转数目录，进一步细分
                if ((subCategory == "Weapons" || subCategory == "Accessories") && parts.Length > 2)
                {
                    return $"Items.{subCategory}.{parts[2]}";
                }
                return $"Items.{subCategory}";
            }

            // 对于 NPCs 下的子分类
            if (mainCategory == "NPCs" && parts.Length > 1)
            {
                return $"NPCs.{parts[1]}";
            }

            // 对于 Buffs 下的子分类
            if (mainCategory == "Buffs" && parts.Length > 1)
            {
                return $"Buffs.{parts[1]}";
            }

            return mainCategory;
        }

        /// <summary>
        /// 从命名空间和类名推断转数
        /// </summary>
        private static string InferTurn(string ns, string typeName)
        {
            if (string.IsNullOrEmpty(ns)) return "";

            var parts = ns.Split('.');
            foreach (var part in parts)
            {
                switch (part)
                {
                    case "Zero": return "零转";
                    case "Test": return "测试";
                    case "One": return "一转";
                    case "Two": return "二转";
                    case "Three": return "三转";
                    case "Four": return "四转";
                    case "Five": return "五转";
                    case "Six": return "六转";
                    case "Seven": return "七转";
                    case "Eight": return "八转";
                    case "Nine": return "九转";
                }
            }
            return "";
        }

        /// <summary>
        /// 检查类型是否有对应的贴图文件
        /// </summary>
        private static bool CheckTexture(Type type)
        {
            try
            {
                // 尝试获取贴图 — 如果 Mod 已加载，可以通过 ModContent.RequestTexture 检查
                // 但更简单的方式是检查文件是否存在
                var mod = ModContent.GetInstance<VerminLordMod>();
                if (mod == null) return false;

                // 通过反射尝试获取贴图
                var textureMethod = typeof(ModContent).GetMethod("RequestTexture",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(string) },
                    null);

                if (textureMethod != null)
                {
                    try
                    {
                        var result = textureMethod.Invoke(null, new object[] { type.FullName.Replace('.', '/') });
                        return result != null;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            return false;
        }

        #region 查询接口

        /// <summary>
        /// 获取指定分类下的所有条目
        /// </summary>
        public static List<TrackedEntry> GetByCategory(string category)
        {
            return ByCategory.TryGetValue(category, out var list) ? list : new();
        }

        /// <summary>
        /// 获取指定状态的所有条目
        /// </summary>
        public static List<TrackedEntry> GetByStatus(ImplStatus status)
        {
            return ByStatus.TryGetValue(status, out var list) ? list : new();
        }

        /// <summary>
        /// 获取指定转数的所有条目
        /// </summary>
        public static List<TrackedEntry> GetByTurn(string turn)
        {
            return ByTurn.TryGetValue(turn, out var list) ? list : new();
        }

        /// <summary>
        /// 获取指定 Dao 类型的所有条目
        /// </summary>
        public static List<TrackedEntry> GetByDaoType(string daoType)
        {
            return ByDaoType.TryGetValue(daoType, out var list) ? list : new();
        }

        /// <summary>
        /// 获取占位实现的数量
        /// </summary>
        public static int PlaceholderCount => GetByStatus(ImplStatus.Placeholder).Count;

        /// <summary>
        /// 获取完整实现的数量
        /// </summary>
        public static int ImplementedCount => GetByStatus(ImplStatus.Implemented).Count;

        /// <summary>
        /// 获取总追踪数
        /// </summary>
        public static int TotalCount => AllEntries.Count;

        /// <summary>
        /// 获取实现进度百分比
        /// </summary>
        public static double ProgressPercent =>
            TotalCount > 0 ? Math.Round((double)ImplementedCount / TotalCount * 100, 1) : 0;

        #endregion

        #region 报告生成

        /// <summary>
        /// 生成完整的实现状态报告
        /// </summary>
        public static string GenerateReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("# 蛊界 Mod 实现状态报告");
            sb.AppendLine();
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("## 总体统计");
            sb.AppendLine();
            sb.AppendLine($"| 指标 | 数值 |");
            sb.AppendLine($"|------|------|");
            sb.AppendLine($"| 总追踪对象 | {TotalCount} |");
            sb.AppendLine($"| ✅ 完整实现 | {ImplementedCount} |");
            sb.AppendLine($"| ⚠️ 部分实现 | {GetByStatus(ImplStatus.Partial).Count} |");
            sb.AppendLine($"| 🚧 占位实现 | {PlaceholderCount} |");
            sb.AppendLine($"| ❌ 已废弃 | {GetByStatus(ImplStatus.Deprecated).Count} |");
            sb.AppendLine($"| 📋 待创建 | {GetByStatus(ImplStatus.Planned).Count} |");
            sb.AppendLine($"| 📊 完成率 | {ProgressPercent}% |");
            sb.AppendLine();

            sb.AppendLine("## 按分类统计");
            sb.AppendLine();
            sb.AppendLine("| 分类 | 总数 | ✅ 完成 | ⚠️ 部分 | 🚧 占位 | 完成率 |");
            sb.AppendLine("|------|------|--------|---------|---------|--------|");

            foreach (var kvp in ByCategory.OrderBy(k => k.Key))
            {
                var entries = kvp.Value;
                var total = entries.Count;
                var done = entries.Count(e => e.Status == ImplStatus.Implemented);
                var partial = entries.Count(e => e.Status == ImplStatus.Partial);
                var placeholder = entries.Count(e => e.Status == ImplStatus.Placeholder);
                var rate = total > 0 ? Math.Round((double)done / total * 100, 1) : 0;
                sb.AppendLine($"| {kvp.Key} | {total} | {done} | {partial} | {placeholder} | {rate}% |");
            }
            sb.AppendLine();

            sb.AppendLine("## 按转数统计（蛊虫）");
            sb.AppendLine();
            sb.AppendLine("| 转数 | 总数 | ✅ 完成 | ⚠️ 部分 | 🚧 占位 | 完成率 |");
            sb.AppendLine("|------|------|--------|---------|---------|--------|");

            foreach (var kvp in ByTurn.OrderBy(k => k.Key))
            {
                var entries = kvp.Value;
                var total = entries.Count;
                var done = entries.Count(e => e.Status == ImplStatus.Implemented);
                var partial = entries.Count(e => e.Status == ImplStatus.Partial);
                var placeholder = entries.Count(e => e.Status == ImplStatus.Placeholder);
                var rate = total > 0 ? Math.Round((double)done / total * 100, 1) : 0;
                sb.AppendLine($"| {kvp.Key} | {total} | {done} | {partial} | {placeholder} | {rate}% |");
            }
            sb.AppendLine();

            sb.AppendLine("## 占位实现清单（待优化）");
            sb.AppendLine();
            sb.AppendLine("| 类型名 | 分类 | 转数 | Dao | 基类 | 备注 |");
            sb.AppendLine("|--------|------|------|-----|------|------|");

            foreach (var entry in AllEntries.Where(e => e.Status == ImplStatus.Placeholder)
                .OrderBy(e => e.Category).ThenBy(e => e.TypeName))
            {
                sb.AppendLine($"| {entry.TypeName} | {entry.Category} | {entry.Turn} | {entry.DaoType} | {entry.BaseType} | {entry.Note} |");
            }
            sb.AppendLine();

            sb.AppendLine("## 完整实现清单");
            sb.AppendLine();
            sb.AppendLine("| 类型名 | 分类 | 转数 | Dao | 备注 |");
            sb.AppendLine("|--------|------|------|-----|------|");

            foreach (var entry in AllEntries.Where(e => e.Status == ImplStatus.Implemented)
                .OrderBy(e => e.Category).ThenBy(e => e.TypeName))
            {
                sb.AppendLine($"| {entry.TypeName} | {entry.Category} | {entry.Turn} | {entry.DaoType} | {entry.Note} |");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成待办清单（Markdown 格式）
        /// </summary>
        public static string GenerateTodoList()
        {
            var sb = new StringBuilder();

            sb.AppendLine("# 蛊界 Mod 实现待办清单");
            sb.AppendLine();
            sb.AppendLine($"更新日期: {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine($"总进度: {ImplementedCount}/{TotalCount} ({ProgressPercent}%)");
            sb.AppendLine();

            // 按分类列出待办
            foreach (var kvp in ByCategory.OrderBy(k => k.Key))
            {
                var placeholders = kvp.Value.Where(e => e.Status == ImplStatus.Placeholder).ToList();
                var partials = kvp.Value.Where(e => e.Status == ImplStatus.Partial).ToList();
                if (placeholders.Count == 0 && partials.Count == 0)
                    continue;

                sb.AppendLine($"## {kvp.Key}");
                sb.AppendLine();

                if (placeholders.Count > 0)
                {
                    sb.AppendLine($"### 🚧 占位实现（{placeholders.Count} 个）");
                    foreach (var entry in placeholders.OrderBy(e => e.TypeName))
                    {
                        var turnInfo = !string.IsNullOrEmpty(entry.Turn) ? $" [{entry.Turn}]" : "";
                        var daoInfo = !string.IsNullOrEmpty(entry.DaoType) ? $" ({entry.DaoType})" : "";
                        var noteInfo = !string.IsNullOrEmpty(entry.Note) ? $" — {entry.Note}" : "";
                        sb.AppendLine($"- [ ] {entry.TypeName}{turnInfo}{daoInfo}{noteInfo}");
                    }
                    sb.AppendLine();
                }

                if (partials.Count > 0)
                {
                    sb.AppendLine($"### ⚠️ 部分实现（{partials.Count} 个）");
                    foreach (var entry in partials.OrderBy(e => e.TypeName))
                    {
                        var turnInfo = !string.IsNullOrEmpty(entry.Turn) ? $" [{entry.Turn}]" : "";
                        var daoInfo = !string.IsNullOrEmpty(entry.DaoType) ? $" ({entry.DaoType})" : "";
                        var noteInfo = !string.IsNullOrEmpty(entry.Note) ? $" — {entry.Note}" : "";
                        sb.AppendLine($"- [~] {entry.TypeName}{turnInfo}{daoInfo}{noteInfo}");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// 单个被追踪的 Content 对象
    /// </summary>
    public class TrackedEntry
    {
        /// <summary>类名（简短）</summary>
        public string TypeName { get; set; }

        /// <summary>完整类型名</summary>
        public string FullTypeName { get; set; }

        /// <summary>命名空间</summary>
        public string Namespace { get; set; }

        /// <summary>分类（如 Items.Weapons.One, NPCs.Commoners, Buffs.AddToSelf 等）</summary>
        public string Category { get; set; }

        /// <summary>转数（如 "一转"、"二转"、"六转"）</summary>
        public string Turn { get; set; }

        /// <summary>Dao 类型（如 "水"、"星"、"木"、"食"）</summary>
        public string DaoType { get; set; }

        /// <summary>实现状态</summary>
        public ImplStatus Status { get; set; }

        /// <summary>备注</summary>
        public string Note { get; set; }

        /// <summary>计划转数</summary>
        public string PlannedTurn { get; set; }

        /// <summary>是否有贴图</summary>
        public bool HasTexture { get; set; }

        /// <summary>基类名</summary>
        public string BaseType { get; set; }

        public override string ToString()
        {
            var statusChar = Status switch
            {
                ImplStatus.Placeholder => "🚧",
                ImplStatus.Partial => "⚠️",
                ImplStatus.Implemented => "✅",
                ImplStatus.Deprecated => "❌",
                ImplStatus.Planned => "📋",
                _ => "❓"
            };
            return $"{statusChar} {TypeName} [{Category}]";
        }
    }
}
