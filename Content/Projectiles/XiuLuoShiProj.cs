using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// XiuLuoShiGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// XiuLuoShiGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// XiuLuoShiGu弹幕 — 道道
    /// </summary>



    /// <summary>




    /// XiuLuoShiGu弹幕 — 道道




    /// </summary>




    public class XiuLuoShiProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.1f, 0.5f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Shadowflame,
                SpawnChance = 2,
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.5f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.ShadowFlame, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Shadowflame,
                DustCount = 10,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 1f,
                ScaleMax = 2f,
                Color = Color.Purple
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.Shadowflame,
                        Color = new Color(120, 20, 120),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
