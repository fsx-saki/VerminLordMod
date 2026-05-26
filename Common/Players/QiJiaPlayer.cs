using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Common.Players
{
    public class QiJiaPlayer : ModPlayer
    {
        public bool QiJiaActive { get; set; }

        public override void ResetEffects()
        {
            QiJiaActive = false;
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (!QiJiaActive) return;

            if (Main.rand.NextFloat() >= 0.3f) return;

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
                int reflectDamage = (int)(hurtInfo.Damage * 0.5f);
                if (reflectDamage < 1) reflectDamage = 1;

                Vector2 vel = (nearestNpc.Center - Player.Center).SafeNormalize(Vector2.Zero) * 8f;
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    vel,
                    ModContent.ProjectileType<QiJiaReflectProj>(),
                    reflectDamage,
                    2f,
                    Player.whoAmI
                );
            }
        }

        public override void PostUpdateEquips()
        {
            if (QiJiaActive)
            {
                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = new Vector2(
                        Main.rand.NextFloat(-20f, 20f),
                        Main.rand.NextFloat(-20f, 20f)
                    );
                    Dust d = Dust.NewDustDirect(Player.Center + offset - new Vector2(4, 4), 8, 8,
                        Terraria.ID.DustID.AncientLight, 0, 0, Scale: 0.6f);
                    d.noGravity = true;
                    d.velocity = offset.SafeNormalize(Vector2.Zero) * 0.5f;
                }
            }
        }
    }
}
