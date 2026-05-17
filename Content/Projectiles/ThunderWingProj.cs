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
    public class ThunderWingProj : BaseBullet
    {
        private const float FlySpeed = 16f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.15f, 0.3f, 0.8f)));

            Behaviors.Add(new LightningTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 180, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.5f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f
            });

            Behaviors.Add(new ParticleBodyBehavior(particleCount: 20, bodyRadius: 14f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(80, 180, 255, 220),
                ColorEnd = new Color(80, 180, 255, 220),
                SwirlSpeed = 0.04f,
                ReturnForce = 0.6f,
                JitterStrength = 0.2f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.4f,
                EnableLight = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Electric,
                DustCount = 8,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.5f,
                ScaleMax = 1f,
                Color = new Color(100, 180, 255)
            });

            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.Electric,
                DustCount = 20,
                DustSpeed = 4f,
                DustScale = 1f,
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 40;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                owner.velocity = Projectile.velocity * 1.5f;
                owner.immune = true;
                owner.immuneTime = 20;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}