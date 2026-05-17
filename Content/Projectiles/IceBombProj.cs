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
    public class IceBombProj : BaseBullet
    {
        private const float FlySpeed = 8f;
        private const int FragmentSpawnCount = 12;
        private const float FragmentSpawnSpeed = 5f;
        private const float BlastRadius = 120f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.2f, 0.4f, 0.7f)));

            Behaviors.Add(new IceTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new ParticleBodyBehavior(particleCount: 16, bodyRadius: 14f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(140, 210, 255, 220),
                ColorEnd = new Color(140, 210, 255, 220),
                SwirlSpeed = 0.03f,
                ReturnForce = 0.5f,
                JitterStrength = 0.12f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.25f,
                EnableLight = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new KillOnContactBehavior());

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 0f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.Frostburn, 120),
                    (BuffID.Chilled, 90)
                }
            });

            Behaviors.Add(new OnKillProjectileBurstBehavior
            {
                ProjectileType = ModContent.ProjectileType<IceFragmentProj>(),
                Count = FragmentSpawnCount,
                Speed = FragmentSpawnSpeed,
                DamageMultiplier = 0.3f,
                KnockbackMultiplier = 0.5f
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = ModContent.DustType<IceBlizzardStarDust>(),
                        Color = new Color(180, 230, 255, 200),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.2f,
                        SpeedMin = 1f,
                        SpeedMax = 4f,
                        SpreadRadius = 20f
                    },
                    new()
                    {
                        Count = 30,
                        DustType = ModContent.DustType<IceBlizzardSnowDust>(),
                        Color = new Color(200, 240, 255, 180),
                        ScaleMin = 0.4f,
                        ScaleMax = 0.9f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 25f,
                        NoGravity = false,
                        VelYMin = -3f,
                        VelYMax = -1f,
                        UseCircularVelocity = false
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}