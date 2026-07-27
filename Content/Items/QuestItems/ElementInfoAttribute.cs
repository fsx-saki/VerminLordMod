// ============================================================
// ElementInfoAttribute - 元素弹幕信息标记
//
// 用于标记弹幕类，提供：
// - Icon：图标标识（如 "F" 表示火、"W" 表示水）
// - DisplayName：显示名称（中文）
//
// ElementTester 在加载时通过反射扫描所有带此标记的类，
// 自动构建弹幕选择列表，无需手动维护。
// ============================================================
using System;

namespace VerminLordMod.Content.Items.QuestItems
{
    /// <summary>
    /// 标记一个弹幕类，使其出现在 ElementTester 的选择菜单中。
    /// </summary>
    /// <param name="icon">图标标识，用于视觉分组（如 "F"=火, "W"=水, "Wi"=风, "I"=冰, "V"=虚空）</param>
    /// <param name="displayName">菜单中显示的中文名称</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ElementInfoAttribute(string icon, string displayName) : Attribute
    {
        /// <summary>图标标识（如 "F", "W", "Wi", "I", "V"）</summary>
        public string Icon { get; } = icon;

        /// <summary>菜单中显示的中文名称</summary>
        public string DisplayName { get; } = displayName;
    }
}
