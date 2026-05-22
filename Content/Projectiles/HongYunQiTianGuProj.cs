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
    /// 鸿运齐天蛊弹幕 — 八转运道
    /// 汇聚天地气运之力
    /// </summary>
    public class HongYunQiTianGuProj : BaseBullet
    {
        private const float FlySpeed = 22f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.8f, 1.0f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.RainbowRod,
                SpawnChance = 1,
                DustScale = 1.3f,
                VelocityMultiplier = 0.05f,
                NoGravity = true,
                RandomSpeed = 2.2f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Lucky, 600),
                    (BuffID.Wrath, 600)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.RainbowRod,
                DustCount = 14,
                SpeedMin = 2f,
                SpeedMax = 5f,
                ScaleMin = 1.1f,
                ScaleMax = 2.2f,
                Color = Color.Cyan
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new() { DustType = DustID.RainbowRod, Count = 18, SpeedMin = 3f, SpeedMax = 8f, ScaleMin = 1.2f, ScaleMax = 2.8f }
                }
            });
        }
    }
}
