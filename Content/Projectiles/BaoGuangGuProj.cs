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
    /// 宝光蛊弹幕 — 八转光道
    /// 释放璀璨宝光，照耀天地
    /// </summary>
    public class BaoGuangGuProj : BaseBullet
    {
        private const float FlySpeed = 20f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.9f, 0.5f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.SpectreStaff,
                SpawnChance = 1,
                DustScale = 1.5f,
                VelocityMultiplier = 0.05f,
                NoGravity = true,
                RandomSpeed = 2.5f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Confused, 180),
                    (BuffID.BrokenArmor, 360)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.SpectreStaff,
                DustCount = 15,
                SpeedMin = 2f,
                SpeedMax = 6f,
                ScaleMin = 1.2f,
                ScaleMax = 2.5f,
                Color = Color.Gold
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new() { DustType = DustID.SpectreStaff, Count = 20, SpeedMin = 4f, SpeedMax = 10f, ScaleMin = 1.5f, ScaleMax = 3.0f },
                    new() { DustType = DustID.GoldFlame, Count = 10, SpeedMin = 2f, SpeedMax = 6f, ScaleMin = 1.0f, ScaleMax = 2.0f }
                }
            });
        }
    }
}
