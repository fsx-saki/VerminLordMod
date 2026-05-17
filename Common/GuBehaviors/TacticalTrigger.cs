/// <summary>
/// 战术触发类型枚举 — 定义战斗中可触发战术Buff的条件。
/// 由 ITacticalTriggerProvider 在武器上定义，TacticalTriggerSystem 在战斗循环中评估。
/// </summary>
namespace VerminLordMod.Common.GuBehaviors
{
    public enum TacticalTrigger
    {
        None,
        OnCombo10,           // 连击10次
        OnPerfectDodge,      // 完美闪避
        OnLowHealth,         // 生命&lt;30%
        OnStandingStill2s,   // 静止2秒
        OnBackstab,          // 背击
        OnKill,              // 击杀
        OnHitTaken5,         // 受击5次
        OnAllyDeath,         // 队友死亡
        OnEmptyQi,           // 空真元
        OnNightTime,         // 夜晚
        OnAirborne,          // 在空中
    }
}
