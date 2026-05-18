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
    /// 修罗尸蛊弹幕 — 变道
    /// </summary>
    public class XXShiProj : BaseBullet
    {
        private const float FlySpeed = 13f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.2f, 0.8f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.PurpleTorch,
                SpawnChance = 3,
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.5f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Confused, 90)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.PurpleTorch,
                DustCount = 8,
                SpeedMin = 1f,
                SpeedMax = 3f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = Color.MediumOrchid
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 10,
                        DustType = DustID.PurpleTorch,
                        Color = Color.MediumOrchid,
                        ScaleMin = 0.6f,
                        ScaleMax = 1.2f,
                        SpeedMin = 1f,
                        SpeedMax = 4f,
                        SpreadRadius = 8f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
