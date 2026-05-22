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
    /// 驭兽蛊弹幕 — 七转兽道
    /// 召唤荒兽虚影攻击敌人
    /// </summary>
    public class YuShouGuProj : BaseBullet
    {
        private const float FlySpeed = 18f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.3f, 0.1f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Bone,
                SpawnChance = 2,
                DustScale = 1.2f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 2.0f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.BrokenArmor, 300),
                    (BuffID.Weak, 300)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Bone,
                DustCount = 12,
                SpeedMin = 2f,
                SpeedMax = 5f,
                ScaleMin = 1.0f,
                ScaleMax = 2.0f,
                Color = Color.DarkRed
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new() { DustType = DustID.Bone, Count = 15, SpeedMin = 3f, SpeedMax = 8f, ScaleMin = 1.0f, ScaleMax = 2.5f }
                }
            });
        }
    }
}
