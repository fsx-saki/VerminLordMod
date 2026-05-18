using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 定音蛊弹幕 — 力道
    /// </summary>

    public class DingYinProj : BaseBullet
    {
        private const float BlastRadius = 250f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ScaleOverLifeBehavior(0.2f, 3f, animateAlpha: true, startAlpha: 100, endAlpha: 0)
            {
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.3f, 0.8f)
            });

            Behaviors.Add(new ParticleBodyBehavior(particleCount: 30, bodyRadius: 25f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(180, 100, 255, 200),
                ColorEnd = new Color(120, 60, 200, 100),
                SwirlSpeed = 0.05f,
                ReturnForce = 0.6f,
                JitterStrength = 0.2f,
                ShrinkOverLife = false,
                StretchOnMove = false,
                StretchFactor = 0f,
                EnableLight = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 1f,
                Knockback = 4f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.Slow, 300),
                    (BuffID.Confused, 180)
                }
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 30,
                        DustType = DustID.Shadowflame,
                        Color = new Color(150, 80, 220, 200),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.3f,
                        SpeedMin = 2f,
                        SpeedMax = 8f,
                        SpreadRadius = 40f
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
            Projectile.timeLeft = 50;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
