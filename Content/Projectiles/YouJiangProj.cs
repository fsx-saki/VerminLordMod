using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class YouJiangProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 7f, trackingWeight: 1f / 25f)
            {
                Range = 500f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.2f, 0.8f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(140, 60, 220),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.2f, 0.8f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.3f)
            {
                target.AddBuff(BuffID.Confused, 180);
            }

            target.AddBuff(BuffID.OnFire, 120);
        }
    }
}
