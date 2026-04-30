// ============================================================
// SimpleLightBox - 轻量级信息框（无标题栏）
// 功能：
// - 淡紫色半透明背景（无边框）
// - 无标题栏
// - 无关闭按钮
// - 无调整大小手柄
// - 不可移动
// - 支持嵌套子面板（SubPanels）
// 与 SimpleInfoBox 的区别：完全没有标题栏区域
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 轻量级信息框状态
/// </summary>
public enum LightBoxState
{
    Closed,
    Opening,
    Open,
    Closing
}

/// <summary>
/// 轻量级信息框 — 淡紫色主题，无标题栏/无关闭/无调整大小/不可移动
/// </summary>
public class SimpleLightBox
{
    // ==================== 属性 ====================
    /// <summary> 提示框矩形（位置和尺寸） </summary>
    public Rectangle BoxRect { get; set; }

    /// <summary> 是否可见 </summary>
    public bool IsVisible { get; private set; }

    /// <summary> 状态 </summary>
    public LightBoxState State { get; private set; } = LightBoxState.Closed;

    // ==================== 颜色主题（淡紫色，与 SimplePanel 一致） ====================
    private static readonly Color BoxBg = new(180, 140, 220, 200);            // 淡紫色半透明背景

    // ==================== 尺寸常量 ====================
    private const int CornerRadius = 8;
    private const float AnimSpeed = 0.12f;
    private const float AnimDamping = 0.65f;

    // ==================== 动画状态 ====================
    private float _openProgress;
    private float _openVelocity;

    // ==================== 子面板列表 ====================
    /// <summary> 提示框内的嵌套子面板列表 </summary>
    public List<SimpleSubPanel> SubPanels { get; set; } = new();

    // ==================== 构造 ====================
    public SimpleLightBox(int width, int height)
    {
        int x = (Main.screenWidth - width) / 2;
        int y = (Main.screenHeight - height) / 2;
        BoxRect = new Rectangle(x, y, width, height);
    }

    // ==================== 打开/关闭 ====================
    /// <summary>
    /// 打开提示框
    /// </summary>
    public void Open()
    {
        if (IsVisible) return;

        IsVisible = true;
        State = LightBoxState.Opening;
        _openProgress = 0f;
        _openVelocity = 0f;
    }

    /// <summary>
    /// 关闭提示框
    /// </summary>
    public void Close()
    {
        if (!IsVisible) return;

        State = LightBoxState.Closing;
        _openProgress = 1f;
        _openVelocity = 0f;
    }

    /// <summary>
    /// 切换提示框
    /// </summary>
    public void Toggle()
    {
        if (IsVisible)
            Close();
        else
            Open();
    }

    // ==================== 更新 ====================
    /// <summary>
    /// 每帧更新提示框
    /// </summary>
    public void Update()
    {
        if (!IsVisible && State == LightBoxState.Closed) return;

        // 更新动画
        UpdateAnimation();

        // 更新交互
        if (State == LightBoxState.Open)
        {
            UpdateSubPanels();
        }

        _lastMouseLeft = Main.mouseLeft;
    }

    /// <summary>
    /// 更新打开/关闭动画
    /// </summary>
    private void UpdateAnimation()
    {
        switch (State)
        {
            case LightBoxState.Opening:
                AnimateFloat(ref _openProgress, ref _openVelocity, 1f);
                if (Math.Abs(_openProgress - 1f) < 0.001f)
                {
                    _openProgress = 1f;
                    State = LightBoxState.Open;
                }
                break;

            case LightBoxState.Closing:
                AnimateFloat(ref _openProgress, ref _openVelocity, 0f);
                if (Math.Abs(_openProgress) < 0.001f)
                {
                    _openProgress = 0f;
                    State = LightBoxState.Closed;
                    IsVisible = false;
                }
                break;
        }
    }

    /// <summary>
    /// 更新所有子面板
    /// </summary>
    private void UpdateSubPanels()
    {
        var contentRect = GetContentRect(BoxRect);
        foreach (var subPanel in SubPanels)
        {
            subPanel.Update(contentRect);
        }
    }

    // ==================== 鼠标状态 ====================
    private bool _lastMouseLeft;

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制提示框
    /// </summary>
    public void Draw(SpriteBatch sb)
    {
        if (!IsVisible && State == LightBoxState.Closed) return;
        if (_openProgress <= 0.001f) return;

        // 计算缩放动画
        float scale = CalculateScale();
        float alpha = CalculateAlpha();

        Rectangle drawRect = BoxRect;
        if (scale < 1f)
        {
            int cx = BoxRect.Center.X;
            int cy = BoxRect.Center.Y;
            int w = (int)(BoxRect.Width * scale);
            int h = (int)(BoxRect.Height * scale);
            drawRect = new Rectangle(cx - w / 2, cy - h / 2, w, h);
        }

        // 切换到 NonPremultiplied 混合模式
        DrawHelper.BeginNonPremultiplied(sb);

        // 绘制背景（无边框）
        DrawRoundedRect(sb, drawRect, BoxBg * alpha, CornerRadius);

        // 绘制内容区域（无标题栏，整个矩形都是内容区）
        var contentRect = GetContentRect(drawRect);

        // 绘制所有子面板
        foreach (var subPanel in SubPanels)
        {
            subPanel.Draw(sb, contentRect, alpha);
        }

        DrawHelper.EndNonPremultiplied(sb);
    }

    // ==================== 辅助方法 ====================
    /// <summary>
    /// 获取内容区域（无标题栏，四周留 8px 内边距）
    /// </summary>
    private Rectangle GetContentRect(Rectangle rect)
    {
        int padding = 8;
        return new Rectangle(rect.X + padding, rect.Y + padding, rect.Width - padding * 2, rect.Height - padding * 2);
    }

    /// <summary>
    /// 计算缩放动画值
    /// </summary>
    private float CalculateScale()
    {
        return 0.85f + 0.15f * _openProgress;
    }

    /// <summary>
    /// 计算透明度动画值
    /// </summary>
    private float CalculateAlpha()
    {
        return _openProgress;
    }

    // ==================== 动画工具 ====================
    private static void AnimateFloat(ref float current, ref float velocity, float target)
    {
        float diff = target - current;
        velocity += diff * AnimSpeed;
        velocity *= (1f - AnimDamping);
        current += velocity;

        if (Math.Abs(diff) < 0.001f && Math.Abs(velocity) < 0.001f)
        {
            current = target;
            velocity = 0f;
        }
    }

    // ==================== 绘制工具 ====================
    /// <summary>
    /// 绘制圆角矩形（无边框）
    /// </summary>
    private static void DrawRoundedRect(SpriteBatch sb, Rectangle rect, Color bgColor, int radius)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        if (rect.Width <= radius * 2 || rect.Height <= radius * 2)
        {
            sb.Draw(pixel, rect, bgColor);
            return;
        }

        // 中心填充
        sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Y + radius, rect.Width - radius * 2, rect.Height - radius * 2), bgColor);

        // 四条边
        sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, radius), bgColor);
        sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Bottom - radius, rect.Width - radius * 2, radius), bgColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + radius, radius, rect.Height - radius * 2), bgColor);
        sb.Draw(pixel, new Rectangle(rect.Right - radius, rect.Y + radius, radius, rect.Height - radius * 2), bgColor);

        // 四个角
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, radius, radius), bgColor);
        sb.Draw(pixel, new Rectangle(rect.Right - radius, rect.Y, radius, radius), bgColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - radius, radius, radius), bgColor);
        sb.Draw(pixel, new Rectangle(rect.Right - radius, rect.Bottom - radius, radius, radius), bgColor);
    }

    /// <summary>
    /// 绘制纯色矩形
    /// </summary>
    private static void DrawFilledRect(SpriteBatch sb, Rectangle rect, Color color)
    {
        sb.Draw(TextureAssets.MagicPixel.Value, rect, color);
    }

    /// <summary>
    /// 绘制居中文字
    /// </summary>
    private static void DrawTextCentered(SpriteBatch sb, string text, Rectangle rect, Color color, float scale)
    {
        var font = FontAssets.MouseText.Value;
        var size = font.MeasureString(text) * scale;
        var pos = new Vector2(rect.Center.X - size.X / 2f, rect.Center.Y - size.Y / 2f);
        Utils.DrawBorderString(sb, text, pos, color, scale);
    }

    /// <summary>
    /// SpriteBatch 辅助类（与 SimplePanel 一致）
    /// </summary>
    private static class DrawHelper
    {
        public static void BeginNonPremultiplied(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        public static void EndNonPremultiplied(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }
    }
}
