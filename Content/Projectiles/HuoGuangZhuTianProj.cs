using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// HuoGuangZhuTianGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// HuoGuangZhuTianGu弹幕 — 道道
    /// </summary>


    /// <summary>



    /// HuoGuangZhuTianGu弹幕 — 火道



    /// </summary>



    public class HuoGuangZhuTianProj : BaseBullet
    {
        private const float BlastRadius = 300f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ScaleOverLifeBehavior(0.3f, 4f, animateAlpha: true, startAlpha: 120, endAlpha: 0)
            {
                EnableLight = true,
                LightColor = new Vector3(1f, 0.7f, 0.2f)
            });

            Behaviors.Add(new ParticleBodyBehavior(particleCount: 40, bodyRadius: 35f)
            {
                ParticleSize = 0.6f,
                ColorStart = new Color(255, 200, 50, 220),
                ColorEnd = new Color(255, 100, 20, 100),
                SwirlSpeed = 0.06f,
                ReturnForce = 0.7f,
                JitterStrength = 0.25f,
                ShrinkOverLife = false,
                StretchOnMove = false,
                StretchFactor = 0f,
                EnableLight = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 1.5f,
                Knockback = 10f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.OnFire, 360),
                    (BuffID.CursedInferno, 180)
                }
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 35,
                KillSpeed = 8f,
                KillSizeMultiplier = 1.5f,
                KillFragmentLife = 35,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(255, 200, 50, 255),
                ColorEnd = new Color(200, 50, 0, 0)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 40,
                        DustType = DustID.Torch,
                        Color = new Color(255, 180, 30, 220),
                        ScaleMin = 0.8f,
                        ScaleMax = 2f,
                        SpeedMin = 3f,
                        SpeedMax = 10f,
                        SpreadRadius = 50f
                    },
                    new()
                    {
                        Count = 30,
                        DustType = DustID.AncientLight,
                        Color = new Color(255, 230, 100, 200),
                        ScaleMin = 0.5f,
                        ScaleMax = 1.2f,
                        SpeedMin = 2f,
                        SpeedMax = 8f,
                        SpreadRadius = 40f,
                        NoGravity = true
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
            Projectile.timeLeft = 55;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
