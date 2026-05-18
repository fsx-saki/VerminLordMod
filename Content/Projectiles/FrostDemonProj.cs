using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles.Zero;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// FrostDemonGu弹幕 — 道道
    /// </summary>
    public class FrostDemonProj : BaseBullet
    {
        private const float BlastRadius = 200f;
        private const int FragmentBurstCount = 24;
        private const float FragmentBurstSpeed = 6f;
        private const int FreezeDuration = 25;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 30, bodyRadius: 18f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(100, 200, 255, 200),
                ColorEnd = new Color(100, 200, 255, 200),
                SwirlSpeed = 0.04f,
                ReturnForce = 0.6f,
                JitterStrength = 0.2f,
                ShrinkOverLife = false,
                StretchOnMove = false,
                StretchFactor = 0f,
                EnableLight = false
            });

            Behaviors.Add(new IceTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new ScaleOverLifeBehavior(0.2f, 1.5f, animateAlpha: true, startAlpha: 150, endAlpha: 0)
            {
                EnableLight = true,
                LightColor = new Vector3(0.15f, 0.35f, 0.8f)
            });

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 1f,
                Knockback = 8f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.Frostburn, 240),
                    (BuffID.Chilled, 180),
                    (BuffID.Frozen, FreezeDuration)
                }
            });

            Behaviors.Add(new OnKillProjectileBurstBehavior
            {
                ProjectileType = ModContent.ProjectileType<IceFragmentProj>(),
                Count = FragmentBurstCount,
                Speed = FragmentBurstSpeed,
                SpeedMin = 2f,
                UseRandomVelocity = true,
                DamageMultiplier = 0f,
                SpreadRadius = 15f
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 30,
                KillSpeed = 6f,
                KillSizeMultiplier = 1.0f,
                KillFragmentLife = 35,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(140, 210, 255, 255),
                ColorEnd = new Color(30, 100, 220, 0)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 40,
                        DustType = ModContent.DustType<IceBlizzardStarDust>(),
                        Color = new Color(180, 230, 255, 200),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.3f,
                        SpeedMin = 2f,
                        SpeedMax = 7f,
                        SpreadRadius = 40f
                    },
                    new()
                    {
                        Count = 60,
                        DustType = ModContent.DustType<IceBlizzardSnowDust>(),
                        Color = new Color(200, 240, 255, 180),
                        ScaleMin = 0.5f,
                        ScaleMax = 1f,
                        SpeedMin = 2f,
                        SpeedMax = 9f,
                        SpreadRadius = 50f,
                        NoGravity = false,
                        VelYMin = -4f,
                        VelYMax = -1f,
                        UseCircularVelocity = false
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}