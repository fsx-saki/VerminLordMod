using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 浮动/跳动行为 — 弹幕在垂直方向上做正弦波动。
    /// 适用于火焰、光球、悬浮物等有浮动效果的弹幕。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new BobBehavior(amplitude: 6f, frequency: 0.08f));
    /// </summary>
    public class BobBehavior : IBulletBehavior
    {
        public string Name => "Bob";

        /// <summary>跳动幅度（像素）</summary>
        public float Amplitude { get; set; } = 6f;

        /// <summary>跳动频率（弧度/帧）</summary>
        public float Frequency { get; set; } = 0.08f;

        /// <summary>是否在 X 轴上也浮动（true=圆形浮动，false=仅垂直）</summary>
        public bool BobInX { get; set; } = false;

        /// <summary>X 轴浮动幅度（仅 BobInX=true 时生效）</summary>
        public float AmplitudeX { get; set; } = 3f;

        /// <summary>X 轴浮动频率（仅 BobInX=true 时生效）</summary>
        public float FrequencyX { get; set; } = 0.05f;

        /// <summary>是否影响弹幕位置（true=修改 position，false=仅影响 visual position 供绘制用）</summary>
        public bool AffectPosition { get; set; } = true;

        /// <summary>是否随机初始相位</summary>
        public bool RandomizePhase { get; set; } = true;

        // 内部状态
        private float _phase;
        private float _phaseX;
        private Vector2 _baseCenter;

        public BobBehavior() { }

        public BobBehavior(float amplitude, float frequency)
        {
            Amplitude = amplitude;
            Frequency = frequency;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _baseCenter = projectile.Center;
            _phase = RandomizePhase ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f;
            _phaseX = RandomizePhase ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f;
        }

        public void Update(Projectile projectile)
        {
            if (!AffectPosition) return;

            _phase += Frequency;
            float bobY = (float)Math.Sin(_phase) * Amplitude;

            float bobX = 0f;
            if (BobInX)
            {
                _phaseX += FrequencyX;
                bobX = (float)Math.Sin(_phaseX) * AmplitudeX;
            }

            // 在基础位置上叠加浮动偏移
            projectile.Center = _baseCenter + new Vector2(bobX, bobY);
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
