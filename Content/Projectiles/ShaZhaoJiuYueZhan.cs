using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 酒月斩杀招弹幕
    /// 月光蛊 + 酒虫组合释放的高伤害特效弹幕
    /// </summary>
    public class ShaZhaoJiuYueZhan : ModProjectile
    {
        private Texture2D mainTexture;
        private readonly TrailManager trailManager = new TrailManager();

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.penetrate = 3;           // 穿透 3 个敌人
            Projectile.timeLeft = 120;          // 2 秒存在时间
            Projectile.alpha = 50;             // 半透明
            Projectile.light = 0.8f;           // 发光
            Projectile.scale = 1.5f;
            Projectile.extraUpdates = 1;        // 额外更新，更平滑
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            mainTexture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/ShaZhaoJiuYueZhanTail").Value;

            // 初始化金色拖尾效果（发光效果在PreDraw中手动控制）
            trailManager.AddGhostTrail(trailTex,
                color: new Color(255, 220, 100),
                maxPositions: 20,
                widthScale: 1.5f,
                lengthScale: 1.2f,
                alpha: 1f,
                recordInterval: 1,
                enableGlow: false);
        }

        public override void AI()
        {
            trailManager.Update(Projectile.Center, Projectile.velocity);

            // 旋转效果
            Projectile.rotation += 0.2f;

            // 发光效果
            Lighting.AddLight(Projectile.Center, 2.0f, 1.8f, 0.5f);

            // 拖尾粒子
            if (Main.rand.NextBool(2))
            {
                int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.MagicMirror, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    150, Color.Orange, 1.2f);
                Main.dust[dustId].noGravity = true;
            }

            // 轻微弯曲效果（模拟追踪感）
            Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin(Main.GameUpdateCount * 0.05f) * 0.02f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中特效：月光爆裂
            for (int i = 0; i < 8; i++)
            {
                int dustId = Dust.NewDust(target.position, target.width, target.height,
                    DustID.MagicMirror, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    150, Color.Gold, 1.5f);
                Main.dust[dustId].noGravity = true;
            }

            // 命中音效
            SoundEngine.PlaySound(SoundID.Item10, target.position);
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡特效
            for (int i = 0; i < 20; i++)
            {
                int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.MagicMirror, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    150, Color.Orange, 1.8f);
                Main.dust[dustId].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 先绘制拖尾（TrailManager统一管理Additive混合模式）
            trailManager.Draw(Main.spriteBatch);
            
            // 手动绘制发光层 + 本体（避免引擎默认绘制导致的重复）
            if (mainTexture != null)
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 origin = mainTexture.Size() * 0.5f;
                float scale = Projectile.scale;
                
                // 发光层（Additive混合，由TrailManager的Draw已开启）
                Color glowColor = new Color(255, 220, 100) * 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    float gs = scale * (1.2f + i * 0.4f);
                    float ga = 0.5f - i * 0.15f;
                    Main.spriteBatch.Draw(mainTexture, drawPos, null, glowColor * ga,
                        Projectile.rotation, origin, gs, SpriteEffects.None, 0f);
                }
                
                // 结束Additive，切回正常混合绘制本体
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                // 绘制本体（正常混合，只绘制一次）
                Color drawColor = Projectile.GetAlpha(lightColor);
                Main.spriteBatch.Draw(mainTexture, drawPos, null, drawColor,
                    Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            return false; // 阻止引擎默认绘制
        }
    }
}
