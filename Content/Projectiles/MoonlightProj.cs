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
    /// 月光弹幕 — 使用 IBulletBehavior 架构重写。
    /// 原版 110 行 → 新版 ~50 行，减少约 55% 代码量。
    /// </summary>
    public class MoonlightProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行 + 光照
            Behaviors.Add(new AimBehavior(speed: 0f) // speed=0 保持初始速度
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.8f, 1.9f, 2.0f)
            });

            // 2. 虚影拖尾（使用自定义拖尾贴图）
            var trailBehavior = new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false // 让 GlowDrawBehavior 处理最终绘制
            };
            Behaviors.Add(trailBehavior);

            // 3. 发光绘制（自动管理 Additive → AlphaBlend 切换）
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(120, 200, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = false // 光照已由 AimBehavior 处理
            });

            // 4. 死亡粒子
            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.BlueFairy,
                DustCount = 30,
                DustSpeed = 5f,
                DustScale = 1.5f,
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
            // 初始化拖尾（在 RegisterBehaviors 之后执行）
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
                trailBehavior.TrailManager.AddGhostTrail(trailTex,
                    color: new Color(120, 200, 255),
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
