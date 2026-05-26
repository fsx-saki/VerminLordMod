using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class ShenHaiManYuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.5f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 180, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.6f, 1.2f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 80;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.4f)
            {
                target.AddBuff(BuffID.Electrified, 180);
            }

            if (Projectile.owner == Main.myPlayer)
            {
                float chainRange = 200f;
                NPC nearest = null;
                float nearestDist = chainRange;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && !npc.friendly && npc.whoAmI != target.whoAmI)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearest = npc;
                        }
                    }
                }

                if (nearest != null)
                {
                    Vector2 dir = Vector2.Normalize(nearest.Center - Projectile.Center) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir,
                        Type, (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(2))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 0, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
