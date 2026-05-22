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
    /// 至尊仙胎蛊弹幕 — 九转变化道
    /// 至尊之力，毁天灭地
    /// </summary>
    public class ZhiZunXianTaiGuProj : BaseBullet
    {
        private const float FlySpeed = 25f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.5f, 0.0f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.FireworkFountain_Red,
                SpawnChance = 1,
                DustScale = 2.0f,
                VelocityMultiplier = 0.03f,
                NoGravity = true,
                RandomSpeed = 3.0f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.BrokenArmor, 480),
                    (BuffID.OnFire, 480),
                    (BuffID.CursedInferno, 480)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.FireworkFountain_Red,
                DustCount = 20,
                SpeedMin = 3f,
                SpeedMax = 8f,
                ScaleMin = 1.5f,
                ScaleMax = 3.0f,
                Color = Color.Goldenrod
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new() { DustType = DustID.FireworkFountain_Red, Count = 30, SpeedMin = 5f, SpeedMax = 15f, ScaleMin = 2.0f, ScaleMax = 4.0f },
                    new() { DustType = DustID.FireworkFountain_Yellow, Count = 20, SpeedMin = 3f, SpeedMax = 10f, ScaleMin = 1.5f, ScaleMax = 3.0f },
                    new() { DustType = DustID.GoldFlame, Count = 15, SpeedMin = 2f, SpeedMax = 8f, ScaleMin = 1.0f, ScaleMax = 2.5f }
                }
            });
        }
    }
}
