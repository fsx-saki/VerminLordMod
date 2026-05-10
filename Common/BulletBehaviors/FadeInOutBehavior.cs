using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 渐入渐出行为 — 弹幕在生命周期内按阶段控制透明度。
    /// 适用于火焰墙、光环、爆炸残留等需要平滑出现的弹幕。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new FadeInOutBehavior
    ///   {
    ///       FadeInDuration = 0.2f,   // 前 20% 时间渐入
    ///       FadeOutStart = 0.7f,     // 后 30% 时间渐出
    ///       MaxAlpha = 0,            // 最亮时 alpha=0
    ///       MinAlpha = 255           // 完全透明时 alpha=255
    ///   });
    /// </summary>
    public class FadeInOutBehavior : IBulletBehavior
    {
        public string Name => "FadeInOut";

        /// <summary>渐入阶段占生命周期的比例（0~1）</summary>
        public float FadeInDuration { get; set; } = 0.2f;

        /// <summary>渐出开始的生命周期比例（0~1）</summary>
        public float FadeOutStart { get; set; } = 0.7f;

        /// <summary>最亮时的 alpha 值（0=完全不透明）</summary>
        public int MaxAlpha { get; set; } = 0;

        /// <summary>完全透明时的 alpha 值（255=完全透明）</summary>
        public int MinAlpha { get; set; } = 255;

        /// <summary>总生命周期（帧），从 projectile.timeLeft 获取</summary>
        public int TotalLife { get; set; } = 0; // 0=自动从 projectile.timeLeft 获取

        // 内部状态
        private int _totalLife;
        private int _timer;

        public FadeInOutBehavior() { }

        public FadeInOutBehavior(float fadeInDuration, float fadeOutStart)
        {
            FadeInDuration = fadeInDuration;
            FadeOutStart = fadeOutStart;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _timer = 0;
            _totalLife = TotalLife > 0 ? TotalLife : projectile.timeLeft;
        }

        public void Update(Projectile projectile)
        {
            _timer++;

            float lifeRatio = _timer / (float)_totalLife;

            if (lifeRatio < FadeInDuration)
            {
                // 渐入阶段：从 MinAlpha 到 MaxAlpha
                float t = lifeRatio / FadeInDuration;
                projectile.alpha = (int)MathHelper.Lerp(MinAlpha, MaxAlpha, t);
            }
            else if (lifeRatio > FadeOutStart)
            {
                // 渐出阶段：从 MaxAlpha 到 MinAlpha
                float t = (lifeRatio - FadeOutStart) / (1f - FadeOutStart);
                projectile.alpha = (int)MathHelper.Lerp(MaxAlpha, MinAlpha, t);
            }
            else
            {
                // 中间阶段：最亮
                projectile.alpha = MaxAlpha;
            }

            // 钳制
            projectile.alpha = (int)MathHelper.Clamp(projectile.alpha, 0, 255);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
