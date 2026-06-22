using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Abstractions
{
    public enum GuCategory
    {
        None = 0,
        Attack,         // 攻击蛊 - 主动催动造成伤害
        Defense,        // 防御蛊 - 被动提供防御/护盾
        Support,        // 辅助蛊 - 治疗/加速/侦查
        Utility,        // 功能蛊 - 移动/采集/建造
        Special,        // 特殊蛊 - 本命蛊/变异蛊
    }

    public enum GuElement
    {
        None = 0,
        Wind,           // 风
        Water,          // 水
        Fire,           // 火
        Earth,          // 土
        Lightning,      // 雷
        Ice,            // 冰
        Moon,           // 月
        Blood,          // 血
        Bone,           // 骨
        Poison,         // 毒
        Soul,           // 魂
        Dark,           // 暗
        Light,          // 光
        Dream,          // 梦
        Charm,          // 魅
        Void,           // 虚
    }

    /// <summary>
    /// 蛊虫核心接口 — 所有蛊虫物品必须实现的基础契约。
    /// 定义蛊虫的通用属性：等级、真元消耗、炼化度、忠诚度、分类、元素、道痕标签。
    /// </summary>
    public interface IGu
    {
        /// <summary>蛊虫转数（1-10），决定基础威力和真元消耗</summary>
        int GuLevel { get; }

        /// <summary>催动真元消耗（每次攻击/使用）</summary>
        float QiCost { get; }

        /// <summary>炼化控制真元消耗（持续压制/炼化时）</summary>
        float ControlQiCost { get; }

        /// <summary>炼化进度 [0, 1]，1.0 表示完全炼化</summary>
        float ControlRate { get; set; }

        /// <summary>是否已被炼化控制</summary>
        bool IsControlled { get; }

        /// <summary>忠诚度 [0, 100]，影响蛊虫反噬概率和攻击效率</summary>
        float Loyalty { get; set; }

        /// <summary>蛊虫分类（攻击/防御/辅助/功能/特殊）</summary>
        GuCategory Category { get; }

        /// <summary>蛊虫元素属性，决定道痕类型和克制关系</summary>
        GuElement Element { get; }

        /// <summary>道痕标签位掩码，标记此蛊虫关联的道痕效果</summary>
        ulong DaoHenTags { get; }
    }

    // 注：原有的 IAttackGu / IDefenseGu / ISupportGu / IMainGu 四个子接口已删除。
    // 原因：Grep 全仓确认除定义文件与 plans 文档外零实现、零消费，属纯死代码。
    // 攻击/防御/辅助的区分现由 GuBaseItem.Slot（出装路径）承载；
    // 本命蛊标记由 KongQiaoSlot.IsMainGu 承载（空窍层）。
    // 蛊虫的分类/元素/转数等通用属性已在 IGu 本体中定义，无需子接口。

    /// <summary>
    /// 蛊虫元素辅助工具 — 提供元素到道痕效果标签的映射和显示名称。
    /// </summary>
    public static class GuElementHelper
    {
        /// <summary>将蛊虫元素映射为对应的道痕效果标签（用于道痕冲突系统）</summary>
        public static DaoEffectTags ToDaoEffectTag(GuElement element)
        {
            return element switch
            {
                GuElement.Fire => DaoEffectTags.DoT,
                GuElement.Ice => DaoEffectTags.Slow,
                GuElement.Lightning => DaoEffectTags.Chain,
                GuElement.Moon => DaoEffectTags.MoonMark,
                GuElement.Blood => DaoEffectTags.LifeSteal,
                GuElement.Bone => DaoEffectTags.ArmorShred,
                GuElement.Poison => DaoEffectTags.StrongDoT,
                GuElement.Soul => DaoEffectTags.Mark,
                GuElement.Dark => DaoEffectTags.Fear,
                GuElement.Light => DaoEffectTags.Heal,
                GuElement.Dream => DaoEffectTags.Charm,
                GuElement.Charm => DaoEffectTags.Charm,
                GuElement.Wind => DaoEffectTags.Push,
                GuElement.Water => DaoEffectTags.Pull,
                GuElement.Void => DaoEffectTags.Silence,
                _ => DaoEffectTags.None,
            };
        }

        /// <summary>获取元素的中文显示名称</summary>
        public static string GetDisplayName(GuElement element)
        {
            return element switch
            {
                GuElement.Wind => "风",
                GuElement.Water => "水",
                GuElement.Fire => "火",
                GuElement.Earth => "土",
                GuElement.Lightning => "雷",
                GuElement.Ice => "冰",
                GuElement.Moon => "月",
                GuElement.Blood => "血",
                GuElement.Bone => "骨",
                GuElement.Poison => "毒",
                GuElement.Soul => "魂",
                GuElement.Dark => "暗",
                GuElement.Light => "光",
                GuElement.Dream => "梦",
                GuElement.Charm => "魅",
                GuElement.Void => "虚",
                _ => "无",
            };
        }

        /// <summary>获取蛊虫分类的中文显示名称</summary>
        public static string GetCategoryDisplayName(GuCategory category)
        {
            return category switch
            {
                GuCategory.Attack => "攻击蛊",
                GuCategory.Defense => "防御蛊",
                GuCategory.Support => "辅助蛊",
                GuCategory.Utility => "功能蛊",
                GuCategory.Special => "特殊蛊",
                _ => "未知",
            };
        }
    }
}
