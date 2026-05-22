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
    /// 招灾蛊弹幕 — 七转运道
    /// 招来灾祸降临敌身
    /// </summary>
    public class ZhaoZaiGuProj : BaseBullet
    {
        private const float FlySpeed = 16f;

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
                DustScale = 1.1f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.8f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Cursed, 180),
                    (BuffID.ShadowFlame, 300)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Shadowflame,
                DustCount = 10,
                SpeedMin = 1.5f,
                SpeedMax = 4f,
                ScaleMin = 0.9f,
                ScaleMax = 1.8f,
                Color = Color.Purple
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new() { DustType = DustID.Shadowflame, Count = 12, SpeedMin = 2f, SpeedMax = 6f, ScaleMin = 1.0f, ScaleMax = 2.0f }
                }
            });
        }
    }
}
