using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 发光绘制行为 — 在弹幕周围绘制多层发光效果。
    /// 自动管理 SpriteBatch 状态切换（Additive → AlphaBlend）。
    /// 适用于 MoonlightProj、MeteorProj 等需要发光效果的弹幕。
    /// </summary>
    public class GlowDrawBehavior : IBulletBehavior
    {
        public string Name => "GlowDraw";

        /// <summary>发光颜色</summary>
        public Color GlowColor { get; set; } = new Color(120, 200, 255);

        /// <summary>发光层数</summary>
        public int GlowLayers { get; set; } = 3;

        /// <summary>发光基础缩放</summary>
        public float GlowBaseScale { get; set; } = 1.2f;

        /// <summary>每层缩放增量</summary>
        public float GlowScaleIncrement { get; set; } = 0.4f;

        /// <summary>发光基础透明度</summary>
        public float GlowBaseAlpha { get; set; } = 0.5f;

        /// <summary>每层透明度衰减</summary>
        public float GlowAlphaDecay { get; set; } = 0.15f;

        /// <summary>发光透明度整体倍率</summary>
        public float GlowAlphaMultiplier { get; set; } = 0.3f;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色 (R, G, B)</summary>
        public Vector3 LightColor { get; set; } = new Vector3(1.8f, 1.9f, 2.0f);

        /// <summary>自定义纹理（null 则使用弹幕默认纹理）</summary>
        public Texture2D CustomTexture { get; set; } = null;

        // 内部缓存
        private Texture2D _mainTexture;

        public GlowDrawBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _mainTexture = CustomTexture ?? Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
        }

        public void Update(Projectile projectile)
        {
            if (EnableLight)
            {
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (_mainTexture == null) return true;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 origin = _mainTexture.Size() * 0.5f;
            float scale = projectile.scale;

            // === 发光层（Additive 混合）===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color glowColor = GlowColor * GlowAlphaMultiplier;
            for (int i = 0; i < GlowLayers; i++)
            {
                float gs = scale * (GlowBaseScale + i * GlowScaleIncrement);
                float ga = GlowBaseAlpha - i * GlowAlphaDecay;
                spriteBatch.Draw(_mainTexture, drawPos, null, glowColor * ga,
                    projectile.rotation, origin, gs, SpriteEffects.None, 0f);
            }

            // === 本体（AlphaBlend 混合）===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color drawColor = projectile.GetAlpha(lightColor);
            spriteBatch.Draw(_mainTexture, drawPos, null, drawColor,
                projectile.rotation, origin, scale, SpriteEffects.None, 0f);

            return false; // 阻止引擎默认绘制
        }
    }
}
