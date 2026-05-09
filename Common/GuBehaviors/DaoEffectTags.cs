using System;

namespace VerminLordMod.Common.GuBehaviors
{
    [Flags]
    public enum DaoEffectTags
    {
        None = 0,
        DoT = 1,             // 持续伤害
        StrongDoT = 2,       // 猛烈毒伤
        Slow = 4,            // 减速
        Freeze = 8,          // 冻结
        ArmorShred = 16,     // 碎甲
        Weaken = 32,         // 虚弱（伤害降低）
        LifeSteal = 64,      // 吸血
        Chain = 128,         // 连锁弹射
        Blind = 256,         // 致盲
        Mark = 512,          // 标记（用于后续引爆）
        MoonMark = 1024,     // 月光标记
        Fear = 2048,         // 恐惧
        Charm = 4096,        // 魅惑
        Silence = 8192,      // 沉默
        Disarm = 16384,      // 缴械
        Pull = 32768,        // 拉近
        Push = 65536,        // 击退
        DrainStat = 131072,  // 吸取属性
        QiRestore = 262144,  // 回复真元
        Heal = 524288,       // 回复生命
        Shield = 1048576,    // 护盾
    }
}
