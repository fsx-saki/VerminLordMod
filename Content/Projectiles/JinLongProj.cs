using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 金龙蛊弹幕 — 金道
    /// </summary>
    public class JinLongProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: 1f / 22f)
            {
                Range = 700f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 215, 0),
                GlowLayers = 3,
                GlowBaseScale = 1.5f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 30,
                FragmentLife = 25,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 80f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 6,
                ColorStart = new Color(255, 215, 0, 255),
                ColorEnd = new Color(200, 150, 0, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.95f,
                InertiaFactor = 0.25f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Ichor, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.GoldFlame,
                DustCount = 15,
                SpeedMin = 2f,
                SpeedMax = 5f,
                ScaleMin = 1.2f,
                ScaleMax = 2.5f,
                Color = Color.Gold
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = DustID.GoldFlame,
                        Color = new Color(255, 200, 50),
                        ScaleMin = 1f,
                        ScaleMax = 2f,
                        SpeedMin = 2f,
                        SpeedMax = 7f,
                        SpreadRadius = 15f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
