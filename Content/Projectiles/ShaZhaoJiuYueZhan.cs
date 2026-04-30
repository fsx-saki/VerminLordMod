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

            // 初始化金色拖尾效果
            trailManager.AddGhostTrail(trailTex,
                color: new Color(255, 220, 100),
                maxPositions: 20,
                widthScale: 1.5f,
                lengthScale: 1.2f,
                alpha: 1f,
                recordInterval: 1,
                enableGlow: true);
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
            trailManager.Draw(Main.spriteBatch);
            return true;
        }
    }
}
