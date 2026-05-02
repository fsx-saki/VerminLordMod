using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 血月弹幕 — 使用 IBulletBehavior 架构重写。
    /// 直线飞行 + 红色光华拖尾 + 发光效果。
    /// </summary>
    public class BloodMoonProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行 + 光照
            Behaviors.Add(new AimBehavior(speed: 0f) // speed=0 保持初始速度
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.8f, 0.3f, 0.3f) // 红色光照
            });

            // 2. 红色光华拖尾（使用 MoonlightProjTail 贴图）
            var trailBehavior = new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            };
            Behaviors.Add(trailBehavior);

            // 3. 红色发光效果
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(220, 60, 60),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = false
            });

            // 4. 死亡粒子（血尘）
            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.Blood,
                DustCount = 20,
                DustSpeed = 4f,
                DustScale = 1.2f,
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            // 初始化红色光华拖尾
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
                trailBehavior.TrailManager.AddGhostTrail(trailTex,
                    color: new Color(220, 60, 60),
                    maxPositions: 16,
                    widthScale: 1f,
                    lengthScale: 1f,
                    alpha: 1f,
                    recordInterval: 2,
                    enableGlow: false);
            }
        }
    }
}
