using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class CombatBehavior : BaseNPCBehavior
    {
        public override string Name => "CombatBehavior";

        private readonly int _damage;
        private readonly float _knockback;
        private readonly int _cooldown;
        private readonly int _randExtraCooldown;
        private readonly int _projType;
        private readonly int _attackDelay;
        private readonly float _projSpeed;
        private readonly float _gravityCorrection;
        private readonly float _randomOffset;

        public CombatBehavior(
            int damage = 10, float knockback = 2f,
            int cooldown = 30, int randExtraCooldown = 10,
            int projType = 0, int attackDelay = 1,
            float projSpeed = 8f, float gravityCorrection = 0f, float randomOffset = 0f)
        {
            _damage = damage;
            _knockback = knockback;
            _cooldown = cooldown;
            _randExtraCooldown = randExtraCooldown;
            _projType = projType;
            _attackDelay = attackDelay;
            _projSpeed = projSpeed;
            _gravityCorrection = gravityCorrection;
            _randomOffset = randomOffset;
        }

        public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
        {
            damage = _damage;
            knockback = _knockback;
        }

        public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = _cooldown;
            randExtraCooldown = _randExtraCooldown;
        }

        public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
        {
            projType = _projType;
            attackDelay = _attackDelay;
        }

        public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = _projSpeed;
            gravityCorrection = _gravityCorrection;
            randomOffset = _randomOffset;
        }
    }
}