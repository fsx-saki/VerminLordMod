using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class XingMoProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 8f, trackingWeight: 1f / 20f)
            {
                Range = 500f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 50, 220),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.35f,
                GlowBaseAlpha = 0.45f,
                GlowAlphaDecay = 0.13f,
                GlowAlphaMultiplier = 0.25f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.2f, 0.8f)
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Confused, 180),
                    (BuffID.CursedInferno, 120)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.PurpleTorch,
                DustCount = 10,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = new Color(160, 50, 220)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 100;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.25f)
                target.AddBuff(BuffID.Confused, 180);
            if (Main.rand.NextFloat() < 0.15f)
                target.AddBuff(BuffID.CursedInferno, 120);
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
