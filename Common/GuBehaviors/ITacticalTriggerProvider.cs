using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Common.GuBehaviors
{
    public struct ActiveTacticalBuff
    {
        public TacticalTrigger Trigger;
        public int RemainingFrames;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float CritBonus;
        public float LifeStealBonus;
        public float DefenseBonus;
    }

    public interface ITacticalTriggerProvider
    {
        TacticalTrigger[] Triggers { get; }
        void CheckTriggers(Player player, out List<ActiveTacticalBuff> activeBuffs);
    }
}
