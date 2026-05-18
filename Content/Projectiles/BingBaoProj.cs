using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 冰爆蛊弹幕 — 冰雪道
    /// </summary>

    public class BingBaoProj : BaseBullet
    {
        private const float BlastRadius = 180f;
        private const int FreezeDuration = 20;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ScaleOverLifeBehavior(0.2f, 1.3f, animateAlpha: true, startAlpha: 150, endAlpha: 0)
            {
                EnableLight = true,
                LightColor = new Vector3(0.15f, 0.35f, 0.8f)
            });

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 1f,
                Knockback = 6f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.Frostburn, 180),
                    (BuffID.Chilled, 120),
                    (BuffID.Frozen, FreezeDuration)
                }
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 20,
                KillSpeed = 5f,
                KillSizeMultiplier = 1.0f,
                KillFragmentLife = 30,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(140, 210, 255, 255),
                ColorEnd = new Color(30, 100, 220, 0)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 25,
                        DustType = DustID.Ice,
                        Color = new Color(180, 230, 255, 200),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.2f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 30f
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
            Projectile.timeLeft = 40;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
