namespace VerminLordMod.Common.GuBehaviors
{
    public enum TacticalTrigger
    {
        None,
        OnCombo10,           // 连击10次
        OnPerfectDodge,      // 完美闪避
        OnLowHealth,         // 生命<30%
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
