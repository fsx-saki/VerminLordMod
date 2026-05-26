using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Common.Players
{
    public class TaiGuHunHePlayer : ModPlayer
    {
        public bool TaiGuHunHeActive { get; set; }

        public override void ResetEffects()
        {
            TaiGuHunHeActive = false;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            TryReflectSoulDamage(npc, hurtInfo.Damage);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (!proj.npcProj) return;

            NPC nearestNpc = null;
            float nearestDist = 600f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestNpc = npc;
                    }
                }
            }

            if (nearestNpc != null)
            {
                TryReflectSoulDamage(nearestNpc, hurtInfo.Damage);
            }
        }

        private void TryReflectSoulDamage(NPC attacker, int damage)
        {
            if (!TaiGuHunHeActive)
                return;

            if (Main.rand.NextFloat() >= 0.20f)
                return;

            if (attacker == null || !attacker.active)
                return;

            int reflectDamage = (int)(damage * 0.5f);
            if (reflectDamage < 1) reflectDamage = 1;

            attacker.SimpleStrikeNPC(reflectDamage, 0, false, 0f, DamageClass.Default, true, Player.luck);

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(attacker.position, attacker.width, attacker.height, DustID.Ghost,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 1.2f);
                d.noGravity = true;
            }

            CombatText.NewText(attacker.Hitbox, new Color(150, 100, 255), reflectDamage);
        }

        public override void PostUpdateEquips()
        {
            if (TaiGuHunHeActive && Main.rand.NextBool(8))
            {
                Vector2 offset = new Vector2(
                    Main.rand.NextFloat(-20f, 20f),
                    Main.rand.NextFloat(-20f, 20f)
                );
                Dust d = Dust.NewDustDirect(Player.Center + offset - new Vector2(4, 4), 8, 8,
                    DustID.Ghost, 0, 0, Scale: 0.6f);
                d.noGravity = true;
                d.velocity = offset.SafeNormalize(Vector2.Zero) * 0.5f;
            }
        }
    }
}
