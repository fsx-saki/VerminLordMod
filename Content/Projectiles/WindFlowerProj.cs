using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// WindFlowerGu弹幕 — 风道
    /// </summary>
    public class WindFlowerProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new FriendlyHomingBehavior(homingStrength: 0.08f, maxSpeed: FlySpeed, detectionRange: 600f)
            {
                ArriveDistance = 20f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.1f, 0.5f, 0.15f)));

            Behaviors.Add(new WindTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 255, 180),
                GlowLayers = 2,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.25f,
                GlowBaseAlpha = 0.35f,
                GlowAlphaDecay = 0.1f,
                GlowAlphaMultiplier = 0.2f
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Grass,
                DustCount = 8,
                SpeedMin = 1f,
                SpeedMax = 3f,
                ScaleMin = 0.5f,
                ScaleMax = 1f,
                Color = new Color(140, 240, 160)
            });

            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.Grass,
                DustCount = 12,
                DustSpeed = 3f,
                DustScale = 0.8f,
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}