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

    public interface IGu
    {
        int GuLevel { get; }
        float QiCost { get; }
        float ControlQiCost { get; }
        float ControlRate { get; set; }
        bool IsControlled { get; }
        float Loyalty { get; set; }
        GuCategory Category { get; }
        GuElement Element { get; }
        ulong DaoHenTags { get; }
    }

    public interface IAttackGu : IGu
    {
        int ProjectileType { get; }
        float DamageMultiplier { get; }
        float AttackCooldown { get; }
    }

    public interface IDefenseGu : IGu
    {
        int DefenseBonus { get; }
        float DamageReduction { get; }
    }

    public interface ISupportGu : IGu
    {
        float HealAmount { get; }
        float BuffDuration { get; }
        int BuffType { get; }
    }

    public interface IMainGu : IGu
    {
        bool IsMainGu { get; }
        float Refinement { get; set; }
    }

    public static class GuElementHelper
    {
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
