// ============================================================
// EasyDraw - SpriteBatch 状态保持辅助类
// 参考自 MiroonOS_Public (https://github.com/MiroonOS/MiroonOS_Public)
// 在切换 BlendState/SpriteSortMode 时保持其他渲染状态不变
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// SpriteBatch 状态保持辅助类。
    /// 在切换 BlendState/SpriteSortMode 时，通过反射保持其他渲染状态（采样器、深度模板、光栅化器、特效、变换矩阵）不变。
    /// </summary>
    public static class EasyDraw
    {
        /// <summary>
        /// 仅切换 BlendState，保持其他状态不变
        /// </summary>
        public static void AnotherDraw(BlendState blendState)
        {
            if (IsDrawBegin())
            {
                Main.spriteBatch.End();
            }
            var samplerState = GetField<SamplerState>("samplerState");
            var depthStencilState = GetField<DepthStencilState>("depthStencilState");
            var rasterizerState = GetField<RasterizerState>("rasterizerState");
            var effect = GetField<Effect>("customEffect");
            var matrix = GetField<Matrix>("transformMatrix");

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        /// <summary>
        /// 仅切换 SpriteSortMode，保持其他状态不变
        /// </summary>
        public static void AnotherDraw(SpriteSortMode spriteSortMode)
        {
            if (IsDrawBegin())
            {
                Main.spriteBatch.End();
            }
            var blendState = GetField<BlendState>("blendState");
            var samplerState = GetField<SamplerState>("samplerState");
            var depthStencilState = GetField<DepthStencilState>("depthStencilState");
            var rasterizerState = GetField<RasterizerState>("rasterizerState");
            var effect = GetField<Effect>("customEffect");
            var matrix = GetField<Matrix>("transformMatrix");

            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        /// <summary>
        /// 切换 SpriteSortMode 和 BlendState，保持其他状态不变
        /// </summary>
        public static void AnotherDraw(SpriteSortMode spriteSortMode, BlendState blendState)
        {
            if (IsDrawBegin())
            {
                Main.spriteBatch.End();
            }
            var samplerState = GetField<SamplerState>("samplerState");
            var depthStencilState = GetField<DepthStencilState>("depthStencilState");
            var rasterizerState = GetField<RasterizerState>("rasterizerState");
            var effect = GetField<Effect>("customEffect");
            var matrix = GetField<Matrix>("transformMatrix");

            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        /// <summary>
        /// 切换 BlendState 和变换矩阵，保持其他状态不变
        /// </summary>
        public static void AnotherDraw(BlendState blendState, Matrix matrix)
        {
            if (IsDrawBegin())
            {
                Main.spriteBatch.End();
            }
            var samplerState = GetField<SamplerState>("samplerState");
            var depthStencilState = GetField<DepthStencilState>("depthStencilState");
            var rasterizerState = GetField<RasterizerState>("rasterizerState");
            var effect = GetField<Effect>("customEffect");

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        /// <summary>
        /// 切换 SpriteSortMode、BlendState 和变换矩阵，保持其他状态不变
        /// </summary>
        public static void AnotherDraw(SpriteSortMode spriteSortMode, BlendState blendState, Matrix matrix)
        {
            if (IsDrawBegin())
            {
                Main.spriteBatch.End();
            }
            var samplerState = GetField<SamplerState>("samplerState");
            var depthStencilState = GetField<DepthStencilState>("depthStencilState");
            var rasterizerState = GetField<RasterizerState>("rasterizerState");
            var effect = GetField<Effect>("customEffect");

            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
        }

        /// <summary>
        /// 获取当前变换矩阵
        /// </summary>
        public static Matrix GetMatrix()
        {
            return GetField<Matrix>("transformMatrix");
        }

        /// <summary>
        /// 获取当前混合模式
        /// </summary>
        public static BlendState GetBlendState()
        {
            if (!IsDrawBegin())
            {
                return BlendState.AlphaBlend;
            }
            return GetField<BlendState>("blendState");
        }

        /// <summary>
        /// 判断 SpriteBatch 是否已经开始绘制
        /// </summary>
        public static bool IsDrawBegin()
        {
            return GetField<bool>("beginCalled");
        }

        /// <summary>
        /// 通过反射获取 SpriteBatch 的私有字段值
        /// </summary>
        private static T GetField<T>(string fieldName)
        {
            var fieldInfo = typeof(SpriteBatch).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)fieldInfo.GetValue(Main.spriteBatch);
        }
    }
}
