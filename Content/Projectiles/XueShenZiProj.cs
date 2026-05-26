using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class XueShenZiProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.02f, 0.02f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 10, 10),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.45f,
                GlowAlphaDecay = 0.13f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.OnFire, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Blood,
                DustCount = 12,
                SpeedMin = 1f,
                SpeedMax = 5f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = new Color(160, 10, 10)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player owner = Main.player[Projectile.owner];
                IEntitySource source = owner.GetSource_FromThis();
                int childDamage = (int)(Projectile.damage * 0.5f);
                float spreadAngle = MathHelper.ToRadians(30f);
                float baseAngle = Projectile.velocity.ToRotation();

                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + (i == 0 ? -spreadAngle : spreadAngle);
                    Vector2 vel = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(
                        source,
                        target.Center,
                        vel,
                        ModContent.ProjectileType<XueShenZiChildProj>(),
                        childDamage,
                        2f,
                        Projectile.owner
                    );
                }
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
