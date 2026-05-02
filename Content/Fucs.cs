using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VerminLordMod.Content
{
    /// <summary>
    /// 工具类 - 提供通用功能
    /// </summary>
    public static class Fucs
    {
        /// <summary>
        /// 将值压入数组开头（从后往前移，新值放在index 0）
        /// </summary>
        [Obsolete("请使用 TrailManager + GhostTrail 替代。参考: trailManager.AddGhostTrail(...); trailManager.Update(...);")]
        public static void Push<T>(T value, ref T[] array) where T : struct
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                array[i] = array[i - 1];
            }
            array[0] = value;
        }

        /// <summary>
        /// 更新拖尾数组 - 在AI中调用
        /// </summary>
        [Obsolete("请使用 TrailManager.Update() 替代。")]
        public static void UpdateProjectileTrail(Projectile projectile, Vector2[] oldPosi, ref int frametime, int recordInterval = 2)
        {
            oldPosi[0] = projectile.Center;
            if (frametime % recordInterval == 0)
            {
                for (int i = oldPosi.Length - 1; i > 0; i--)
                {
                    oldPosi[i] = oldPosi[i - 1];
                }
            }
            frametime++;
        }

        /// <summary>
        /// 绘制拖尾 - 复制自CycloneProj风格
        /// </summary>
        /// <param name="sb">SpriteBatch</param>
        /// <param name="oldPosi">位置数组</param>
        /// <param name="trailTex">拖尾贴图</param>
        /// <param name="width">拖尾宽度</param>
        /// <param name="length">拖尾长度</param>
        /// <param name="color">拖尾颜色</param>
        /// <param name="alpha">整体透明度(0-1)</param>
        /// <param name="offset">固定偏移量（全局XY偏移，用于修正贴图宽度导致的视觉效果偏移）</param>
        [Obsolete("请使用 TrailManager + GhostTrail 替代。拖尾绘制由 GhostTrail.Draw() 自动处理。")]
        public static void DrawProjectileTrail(SpriteBatch sb, Vector2[] oldPosi, Texture2D trailTex, float width, float length, Color color, float alpha = 1f, Vector2 offset = default)
        {
            for (int i = 1; i < oldPosi.Length; i++)
            {
                Vector2 start = oldPosi[i - 1] + offset;
                Vector2 end = oldPosi[i] + offset;
                if (start == Vector2.Zero || end == Vector2.Zero) continue;

                Vector2 diff = end - start;
                float segLength = diff.Length();
                if (segLength == 0) continue;

                float rotation = diff.ToRotation();
                float fadeAlpha = (1f - (float)i / oldPosi.Length) * alpha;
                Color drawColor = color * fadeAlpha * 0.8f;

                Vector2 scale = new Vector2(length, width);
                Vector2 drawPos = (start + end) / 2f - Main.screenPosition;
                sb.Draw(trailTex, drawPos, null, drawColor, rotation, trailTex.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// 绘制本体发光效果 - 无虚影偏移
        /// </summary>
        /// <param name="sb">SpriteBatch</param>
        /// <param name="mainTexture">本体贴图</param>
        /// <param name="position">屏幕坐标位置</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="scale">缩放</param>
        /// <param name="color">颜色</param>
        /// <param name="glowLayers">发光层数</param>
        [Obsolete("请使用 TrailManager + GhostTrail (EnableGlow=true) 替代。")]
        public static void DrawProjectileGlow(SpriteBatch sb, Texture2D mainTexture, Vector2 position, float rotation, float scale, Color color, int glowLayers = 5)
        {
            Vector2 origin = mainTexture.Size() * 0.5f;
            for (int i = 0; i < glowLayers; i++)
            {
                float glowScale = scale * (1.2f + i * 0.5f);
                float glowAlpha = 0.8f - i * 0.15f;
                Color glowColor = color * glowAlpha;
                sb.Draw(mainTexture, position, null, glowColor, rotation, origin, glowScale, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// 开始Additive混合模式绘制
        /// </summary>
        [Obsolete("Additive 模式由 TrailManager.Draw() 统一管理。如需手动控制，请直接使用 sb.Begin/End。")]
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }

        /// <summary>
        /// 结束Additive混合模式
        /// </summary>
        [Obsolete("Additive 模式由 TrailManager.Draw() 统一管理。如需手动控制，请直接使用 sb.Begin/End。")]
        public static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// 绘制发光投射物（本体发光 + 拖尾）
        /// </summary>
        /// <param name="sb">SpriteBatch</param>
        /// <param name="projectile">投射物</param>
        /// <param name="mainTexture">本体贴图</param>
        /// <param name="trailTex">拖尾贴图</param>
        /// <param name="oldPosi">拖尾位置数组</param>
        /// <param name="icolor">发光颜色</param>
        /// <param name="tcolor">发光颜色</param>
        /// <param name="trailWidth">拖尾宽度</param>
        /// <param name="trailLength">拖尾长度</param>
        /// <param name="trailAlpha">拖尾透明度</param>
        /// <param name="glowLayers">发光层数</param>
        /// <param name="trailOffset">拖尾固定偏移量（全局XY偏移，用于修正贴图宽度导致的视觉效果偏移）</param>
        [Obsolete("请使用 TrailManager + GhostTrail (EnableGlow=true) 替代。")]
        public static void DrawGlowingProjectile(SpriteBatch sb, Projectile projectile, Texture2D mainTexture,
            Texture2D trailTex, Vector2[] oldPosi, Color icolor, Color tcolor, float trailWidth = 1f, float trailLength = 1f,
            float trailAlpha = 1f, int glowLayers = 5, Vector2 trailOffset = default)
        {
            BeginAdditive(sb);

            Vector2 pos = projectile.Center - Main.screenPosition;
            DrawProjectileGlow(sb, mainTexture, pos, projectile.rotation, projectile.scale, icolor, glowLayers);
            DrawProjectileTrail(sb, oldPosi, trailTex, trailWidth, trailLength, tcolor, trailAlpha, trailOffset);

            EndAdditive(sb);
        }
    }
}
