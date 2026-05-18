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
    /// 挽澜蛊弹幕 — 力道
    /// </summary>
    public class WanLanProj : BaseBullet
    {
        private const float FlySpeed = 15f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.4f, 0.1f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.AmberBolt,
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
                    (BuffID.BrokenArmor, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.AmberBolt,
                DustCount = 8,
                SpeedMin = 1f,
                SpeedMax = 3f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = Color.OrangeRed
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 10,
                        DustType = DustID.AmberBolt,
                        Color = Color.OrangeRed,
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
            Projectile.timeLeft = 300;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
