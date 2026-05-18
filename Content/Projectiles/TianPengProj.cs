using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 天蓬蛊弹幕 — 骨道
    /// </summary>

    public class TianPengProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ScaleOverLifeBehavior(0.3f, 2.5f, animateAlpha: true, startAlpha: 100, endAlpha: 0)
            {
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.9f, 1f)
            });

            Behaviors.Add(new ParticleBodyBehavior(particleCount: 20, bodyRadius: 30f)
            {
                ParticleSize = 0.4f,
                ColorStart = new Color(220, 220, 255, 180),
                ColorEnd = new Color(180, 180, 255, 100),
                SwirlSpeed = 0.03f,
                ReturnForce = 0.5f,
                JitterStrength = 0.15f,
                ShrinkOverLife = false,
                StretchOnMove = false,
                StretchFactor = 0f,
                EnableLight = false
            });

            Behaviors.Add(new SuppressDrawBehavior());

            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.AncientLight,
                DustCount = 15,
                DustSpeed = 2f,
                DustScale = 1.2f,
                NoGravity = true
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
            Projectile.timeLeft = 60;
            Projectile.alpha = 255;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
