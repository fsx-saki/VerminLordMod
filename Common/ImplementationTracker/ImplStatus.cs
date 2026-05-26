using System;
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Common.ImplementationTracker
{
    /// <summary>
    /// 标记一个 Content 对象的实现状态。
    /// 用于追踪整个 Mod 中哪些蛊虫、物品、NPC、弹幕等是占位实现，哪些已完整实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ImplStatusAttribute : Attribute
    {
        /// <summary>
        /// 当前实现状态
        /// </summary>
        public ImplStatus Status { get; }

        /// <summary>
        /// 可选的备注说明（如 "仅有贴图"、"基础逻辑完成"、"待测试"）
        /// </summary>
        public string Note { get; }

        /// <summary>
        /// 计划实现的转数（仅对蛊虫有效，如 "一转"、"二转"、"六转"）
        /// </summary>
        public string PlannedTurn { get; }

        /// <summary>
        /// 对应的 Dao 类型（如 "水"、"星"、"木"、"食" 等）
        /// </summary>
        public string DaoType { get; }

        /// <summary>
        /// 标记一个 Content 对象的实现状态
        /// </summary>
        /// <param name="status">实现状态</param>
        /// <param name="note">备注说明</param>
        /// <param name="plannedTurn">计划转数</param>
        /// <param name="daoType">Dao 类型</param>
        public ImplStatusAttribute(
            ImplStatus status = ImplStatus.Placeholder,
            string note = "",
            string plannedTurn = "",
            string daoType = "")
        {
            Status = status;
            Note = note ?? "";
            PlannedTurn = plannedTurn ?? "";
            DaoType = daoType ?? "";
        }
    }

    /// <summary>
    /// 实现状态枚举
    /// </summary>
    public enum ImplStatus
    {
        /// <summary>占位实现：仅有基础框架，逻辑不完整或未测试</summary>
        Placeholder = 0,

        /// <summary>部分实现：有基本功能但缺少细节或需要优化</summary>
        Partial = 1,

        /// <summary>完整实现：功能完整，经过测试</summary>
        Implemented = 2,

        /// <summary>已废弃：不再使用</summary>
        Deprecated = 3,

        /// <summary>待创建：仅有计划，尚未创建文件</summary>
        Planned = 4,
    }
}
