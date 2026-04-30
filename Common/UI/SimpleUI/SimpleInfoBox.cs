// ============================================================
// SimpleInfoBox - 信息提示框（与 SimplePanel 风格一致）
// 功能：
// - 淡紫色半透明背景（无边框）
// - 有标题栏但不可移动
// - 无关闭按钮
// - 无调整大小手柄
// - 支持嵌套子面板（SubPanels）
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 信息提示框状态
/// </summary>
public enum InfoBoxState
{
    Closed,
    Opening,
    Open,
    Closing
}

/// <summary>
/// 信息提示框 — 淡紫色主题，无边框/无关闭/无调整大小/标题栏不可移动
/// </summary>
public class SimpleInfoBox
{
    // ==================== 属性 ====================
    /// <summary> 提示框矩形（位置和尺寸） </summary>
    public Rectangle BoxRect { get; private set; }

    /// <summary> 是否可见 </summary>
    public bool IsVisible { get; private set; }

    /// <summary> 状态 </summary>
    public InfoBoxState State { get; private set; } = InfoBoxState.Closed;

    /// <summary> 标题文字 </summary>
    public string Title { get; set; } = "";

    // ==================== 颜色主题（淡紫色，与 SimplePanel 一致） ====================
    private static readonly Color BoxBg = new(180, 140, 220, 200);            // 淡紫色半透明背景
    private static readonly Color TitleBarBg = new(140, 100, 190, 220);       // 深紫色标题栏
    private static readonly Color TitleBarHover = new(160, 120, 210, 220);    // 标题栏悬停
    private static readonly Color TitleTextColor = new(255, 255, 255, 220);   // 标题文字

    // ==================== 尺寸常量 ====================
    private const int TitleBarHeight = 32;
    private const int CornerRadius = 8;
    private const float AnimSpeed = 0.12f;
    private const float AnimDamping = 0.65f;

    // ==================== 动画状态 ====================
    private float _openProgress;
    private float _openVelocity;
    private float _titleHoverAnim;
    private float _titleHoverVelocity;

    // ==================== 子面板列表 ====================
    /// <summary> 提示框内的嵌套子面板列表 </summary>
    public List<SimpleSubPanel> SubPanels { get; set; } = new();

    // ==================== 构造 ====================
    public SimpleInfoBox(int width, int height)
    {
        int x = (Main.screenWidth - width) / 2;
        int y = (Main.screenHeight - height) / 2;
        BoxRect = new Rectangle(x, y, width, height);
        Title = "信息提示";
    }

    // ==================== 打开/关闭 ====================
    /// <summary>
    /// 打开提示框
    /// </summary>
    public void Open()
    {
        if (IsVisible) return;

        IsVisible = true;
        State = InfoBoxState.Opening;
        _openProgress = 0f;
        _openVelocity = 0f;
    }

    /// <summary>
    /// 关闭提示框
    /// </summary>
    public void Close()
    {
        if (!IsVisible) return;

        State = InfoBoxState.Closing;
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
        if (!IsVisible && State == InfoBoxState.Closed) return;

        // 更新动画
        UpdateAnimation();

        // 更新交互（无拖动/无调整大小/无关闭按钮）
        if (State == InfoBoxState.Open)
        {
            UpdateHoverAnimations();
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
            case InfoBoxState.Opening:
                AnimateFloat(ref _openProgress, ref _openVelocity, 1f);
                if (Math.Abs(_openProgress - 1f) < 0.001f)
                {
                    _openProgress = 1f;
                    State = InfoBoxState.Open;
                }
                break;

            case InfoBoxState.Closing:
                AnimateFloat(ref _openProgress, ref _openVelocity, 0f);
                if (Math.Abs(_openProgress) < 0.001f)
                {
                    _openProgress = 0f;
                    State = InfoBoxState.Closed;
                    IsVisible = false;
                }
                break;
        }
    }

    /// <summary>
    /// 更新悬停动画
    /// </summary>
    private void UpdateHoverAnimations()
    {
        var mousePos = Main.MouseScreen;

        // 标题栏悬停（仅视觉效果）
        bool hoverTitle = GetTitleBarRect().Contains(mousePos.ToPoint());
        AnimateFloat(ref _titleHoverAnim, ref _titleHoverVelocity, hoverTitle ? 1f : 0f);
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
        if (!IsVisible && State == InfoBoxState.Closed) return;
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

        // 绘制标题栏
        var titleRect = new Rectangle(drawRect.X, drawRect.Y, drawRect.Width, TitleBarHeight);
        Color titleColor = Color.Lerp(TitleBarBg, TitleBarHover, _titleHoverAnim) * alpha;
        DrawFilledRect(sb, titleRect, titleColor);

        // 标题栏底部装饰线（淡色分隔线，非边框）
        DrawFilledRect(sb, new Rectangle(drawRect.X, drawRect.Y + TitleBarHeight - 1, drawRect.Width, 1), new Color(200, 170, 240, 120) * alpha);

        // 标题文字
        if (!string.IsNullOrEmpty(Title))
        {
            var titleTextRect = new Rectangle(drawRect.X + 8, drawRect.Y, drawRect.Width - 16, TitleBarHeight);
            DrawTextCentered(sb, Title, titleTextRect, TitleTextColor * alpha, 0.9f);
        }

        // 绘制内容区域
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
    /// 获取标题栏矩形
    /// </summary>
    private Rectangle GetTitleBarRect()
    {
        return new Rectangle(BoxRect.X, BoxRect.Y, BoxRect.Width, TitleBarHeight);
    }

    /// <summary>
    /// 获取内容区域（无调整大小手柄，底部留白较少）
    /// </summary>
    private Rectangle GetContentRect(Rectangle rect)
    {
        int top = rect.Y + TitleBarHeight + 8;
        int left = rect.X + 8;
        int width = rect.Width - 16;
        int height = rect.Height - TitleBarHeight - 16;
        return new Rectangle(left, top, width, height);
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
