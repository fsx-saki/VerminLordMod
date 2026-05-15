// ============================================================
// SimplePanel - 完全独立设计的UI面板
// 不依赖任何现有UI系统（ModernUI等），纯手工打造
// 功能：
// - 淡紫色半透明背景
// - 按住标题栏拖动
// - 按住右下角调整大小
// - 右上角 X 关闭按钮
// - 内容区域弹性居中一个按钮
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 简单面板状态
/// </summary>
public enum PanelState
{
    Closed,
    Opening,
    Open,
    Closing
}

/// <summary>
/// 完全独立设计的UI面板
/// 淡紫色主题，可拖动，可调整大小
/// </summary>
public class SimplePanel
{
    // ==================== 面板属性 ====================
    /// <summary> 面板矩形（位置和尺寸） </summary>
    public Rectangle PanelRect { get; set; }

    /// <summary> 面板是否可见 </summary>
    public bool IsVisible { get; private set; }

    /// <summary> 面板状态 </summary>
    public PanelState State { get; private set; } = PanelState.Closed;

    /// <summary> 标题文字 </summary>
    public string Title { get; set; } = "";

    // ==================== 颜色主题（淡紫色） ====================
    private static readonly Color PanelBg = new(180, 140, 220, 200);          // 淡紫色半透明背景
    private static readonly Color TitleBarBg = new(140, 100, 190, 220);       // 深紫色标题栏
    private static readonly Color TitleBarHover = new(160, 120, 210, 220);    // 标题栏悬停
    private static readonly Color BorderColor = new(200, 170, 240, 220);      // 边框
    private static readonly Color ResizeHandleColor = new(200, 170, 240, 180); // 调整大小手柄
    private static readonly Color ResizeHandleHover = new(220, 200, 255, 220); // 手柄悬停
    private static readonly Color TitleTextColor = new(255, 255, 255, 220);   // 标题文字
    private static readonly Color CloseBtnNormal = new(200, 80, 80, 200);     // 关闭按钮默认（淡红）
    private static readonly Color CloseBtnHover = new(255, 100, 100, 240);    // 关闭按钮悬停（亮红）
    private static readonly Color CloseBtnPressed = new(180, 60, 60, 240);    // 关闭按钮按下（暗红）
    private static readonly Color CloseSymbolColor = new(255, 255, 255, 220); // X 符号颜色

    // ==================== 尺寸常量 ====================
    private const int TitleBarHeight = 32;
    private const int ResizeHandleSize = 16;
    private const int MinWidth = 200;
    private const int MinHeight = 150;
    private const int CornerRadius = 8;
    private const float AnimSpeed = 0.12f;
    private const float AnimDamping = 0.65f;
    private const int CloseBtnSize = 22;
    private const int CloseBtnRightMargin = 6;

    // ==================== 拖动状态 ====================
    private bool _isDragging;
    private Vector2 _dragOffset;

    // ==================== 调整大小状态 ====================
    private bool _isResizing;
    private Vector2 _resizeStartMouse;
    private Rectangle _resizeStartRect;

    // ==================== 动画状态 ====================
    private float _openProgress;
    private float _openVelocity;
    private float _titleHoverAnim;
    private float _titleHoverVelocity;
    private float _resizeHoverAnim;
    private float _resizeHoverVelocity;
    private float _closeBtnHoverAnim;
    private float _closeBtnHoverVelocity;

    // ==================== 鼠标状态 ====================
    private bool _lastMouseLeft;
    private bool _closeBtnPressed;

    // ==================== 子面板列表 ====================
    /// <summary> 面板内的嵌套子面板列表 </summary>
    public List<SimpleSubPanel> SubPanels { get; set; } = new();

    // ==================== 构造 ====================
    public SimplePanel(int width, int height)
    {
        int x = (Main.screenWidth - width) / 2;
        int y = (Main.screenHeight - height) / 2;
        PanelRect = new Rectangle(x, y, width, height);
        Title = "简单面板";
    }

    // ==================== 打开/关闭 ====================
    /// <summary>
    /// 打开面板
    /// </summary>
    public void Open()
    {
        if (IsVisible) return;

        IsVisible = true;
        State = PanelState.Opening;
        _openProgress = 0f;
        _openVelocity = 0f;
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public void Close()
    {
        if (!IsVisible) return;

        State = PanelState.Closing;
        _openProgress = 1f;
        _openVelocity = 0f;
    }

    /// <summary>
    /// 切换面板
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
    /// 每帧更新面板
    /// </summary>
    public void Update()
    {
        if (!IsVisible && State == PanelState.Closed) return;

        // 更新动画
        UpdateAnimation();

        // 更新交互
        if (State == PanelState.Open)
        {
            UpdateCloseButtonClick();
            UpdateDragging();
            UpdateResizing();
            UpdateHoverAnimations();
            UpdateSubPanels();
        }

        // 更新鼠标状态
        _lastMouseLeft = Main.mouseLeft;
    }

    /// <summary>
    /// 更新打开/关闭动画
    /// </summary>
    private void UpdateAnimation()
    {
        switch (State)
        {
            case PanelState.Opening:
                AnimateFloat(ref _openProgress, ref _openVelocity, 1f);
                if (Math.Abs(_openProgress - 1f) < 0.001f)
                {
                    _openProgress = 1f;
                    State = PanelState.Open;
                }
                break;

            case PanelState.Closing:
                AnimateFloat(ref _openProgress, ref _openVelocity, 0f);
                if (Math.Abs(_openProgress) < 0.001f)
                {
                    _openProgress = 0f;
                    State = PanelState.Closed;
                    IsVisible = false;
                }
                break;
        }
    }

    /// <summary>
    /// 更新关闭按钮点击检测
    /// </summary>
    private void UpdateCloseButtonClick()
    {
        var mousePos = Main.MouseScreen;
        var closeRect = GetCloseButtonRect();

        bool hoverClose = closeRect.Contains(mousePos.ToPoint());
        bool currentLeft = Main.mouseLeft;

        if (hoverClose)
        {
            Main.LocalPlayer.mouseInterface = true;

            if (currentLeft && !_lastMouseLeft)
            {
                _closeBtnPressed = true;
                Close();
                Main.NewText("[SimpleUI] 面板已关闭", new Color(200, 160, 255));
            }
        }

        if (!currentLeft)
        {
            _closeBtnPressed = false;
        }
    }

    /// <summary>
    /// 更新拖动逻辑
    /// </summary>
    private void UpdateDragging()
    {
        var mousePos = Main.MouseScreen;

        if (Main.mouseLeft)
        {
            if (!_isDragging)
            {
                // 检查是否在标题栏上按下（排除关闭按钮区域）
                var titleRect = GetTitleBarRect();
                var closeRect = GetCloseButtonRect();
                if (titleRect.Contains(mousePos.ToPoint()) && !closeRect.Contains(mousePos.ToPoint()))
                {
                    _isDragging = true;
                    _dragOffset = new Vector2(PanelRect.X - mousePos.X, PanelRect.Y - mousePos.Y);
                    Main.LocalPlayer.mouseInterface = true;
                }
            }
            else
            {
                // 拖动中
                Main.LocalPlayer.mouseInterface = true;
                int newX = (int)(mousePos.X + _dragOffset.X);
                int newY = (int)(mousePos.Y + _dragOffset.Y);

                // 限制不能完全拖出屏幕
                newX = Math.Clamp(newX, -PanelRect.Width + 50, Main.screenWidth - 50);
                newY = Math.Clamp(newY, 0, Main.screenHeight - 50);

                PanelRect = new Rectangle(newX, newY, PanelRect.Width, PanelRect.Height);
            }
        }
        else
        {
            _isDragging = false;
        }
    }

    /// <summary>
    /// 更新调整大小逻辑
    /// </summary>
    private void UpdateResizing()
    {
        var mousePos = Main.MouseScreen;

        if (Main.mouseLeft)
        {
            if (!_isResizing)
            {
                // 检查是否在手柄上按下
                var handleRect = GetResizeHandleRect();
                if (handleRect.Contains(mousePos.ToPoint()))
                {
                    _isResizing = true;
                    _resizeStartMouse = mousePos;
                    _resizeStartRect = PanelRect;
                    Main.LocalPlayer.mouseInterface = true;
                }
            }
            else
            {
                // 调整大小中
                Main.LocalPlayer.mouseInterface = true;

                int deltaX = (int)(mousePos.X - _resizeStartMouse.X);
                int deltaY = (int)(mousePos.Y - _resizeStartMouse.Y);

                int newWidth = Math.Max(MinWidth, _resizeStartRect.Width + deltaX);
                int newHeight = Math.Max(MinHeight, _resizeStartRect.Height + deltaY);

                // 最大尺寸限制
                int maxW = (int)(Main.screenWidth * 0.9f);
                int maxH = (int)(Main.screenHeight * 0.9f);
                newWidth = Math.Min(maxW, newWidth);
                newHeight = Math.Min(maxH, newHeight);

                PanelRect = new Rectangle(PanelRect.X, PanelRect.Y, newWidth, newHeight);
            }
        }
        else
        {
            _isResizing = false;
        }
    }

    /// <summary>
    /// 更新悬停动画
    /// </summary>
    private void UpdateHoverAnimations()
    {
        var mousePos = Main.MouseScreen;

        // 标题栏悬停
        bool hoverTitle = GetTitleBarRect().Contains(mousePos.ToPoint());
        AnimateFloat(ref _titleHoverAnim, ref _titleHoverVelocity, hoverTitle ? 1f : 0f);

        // 关闭按钮悬停
        bool hoverClose = GetCloseButtonRect().Contains(mousePos.ToPoint());
        AnimateFloat(ref _closeBtnHoverAnim, ref _closeBtnHoverVelocity, hoverClose ? 1f : 0f);

        // 调整大小手柄悬停
        bool hoverResize = GetResizeHandleRect().Contains(mousePos.ToPoint());
        AnimateFloat(ref _resizeHoverAnim, ref _resizeHoverVelocity, hoverResize ? 1f : 0f);

        // 更新光标
        if (hoverResize)
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.cursorOverride = 7; // 调整大小光标
        }
    }

    /// <summary>
    /// 更新所有子面板
    /// </summary>
    private void UpdateSubPanels()
    {
        var contentRect = GetContentRect(PanelRect);
        foreach (var subPanel in SubPanels)
        {
            subPanel.Update(contentRect);
        }
    }

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制面板
    /// </summary>
    public void Draw(SpriteBatch sb)
    {
        if (!IsVisible && State == PanelState.Closed) return;
        if (_openProgress <= 0.001f) return;

        // 计算缩放动画
        float scale = CalculateScale();
        float alpha = CalculateAlpha();

        Rectangle drawRect = PanelRect;
        if (scale < 1f)
        {
            int cx = PanelRect.Center.X;
            int cy = PanelRect.Center.Y;
            int w = (int)(PanelRect.Width * scale);
            int h = (int)(PanelRect.Height * scale);
            drawRect = new Rectangle(cx - w / 2, cy - h / 2, w, h);
        }

        // 切换到 NonPremultiplied 混合模式
        DrawHelper.BeginNonPremultiplied(sb);

        UIRendering.DrawRoundedRect(sb, drawRect, PanelBg * alpha, BorderColor * alpha, CornerRadius);

        var titleRect = new Rectangle(drawRect.X, drawRect.Y, drawRect.Width, TitleBarHeight);
        Color titleColor = Color.Lerp(TitleBarBg, TitleBarHover, _titleHoverAnim) * alpha;
        UIRendering.DrawFilledRect(sb, titleRect, titleColor);

        UIRendering.DrawFilledRect(sb, new Rectangle(drawRect.X, drawRect.Y + TitleBarHeight - 1, drawRect.Width, 1), BorderColor * alpha * 0.5f);

        if (!string.IsNullOrEmpty(Title))
        {
            var titleTextRect = new Rectangle(drawRect.X + 8, drawRect.Y, drawRect.Width - CloseBtnSize - CloseBtnRightMargin - 8, TitleBarHeight);
            UIRendering.DrawTextCentered(sb, Title, titleTextRect, TitleTextColor * alpha, 0.9f);
        }

        var closeRect = GetCloseButtonRect(drawRect);
        DrawCloseButton(sb, closeRect, alpha);

        var contentRect = GetContentRect(drawRect);

        foreach (var subPanel in SubPanels)
        {
            subPanel.Draw(sb, contentRect, alpha);
        }

        var handleRect = GetResizeHandleRect(drawRect);
        Color handleColor = Color.Lerp(ResizeHandleColor, ResizeHandleHover, _resizeHoverAnim) * alpha;
        DrawResizeHandle(sb, handleRect, handleColor);

        DrawHelper.EndNonPremultiplied(sb);
    }

    // ==================== 辅助方法 ====================
    /// <summary>
    /// 获取标题栏矩形
    /// </summary>
    private Rectangle GetTitleBarRect()
    {
        return new Rectangle(PanelRect.X, PanelRect.Y, PanelRect.Width, TitleBarHeight);
    }

    /// <summary>
    /// 获取关闭按钮矩形（标题栏右上角）
    /// </summary>
    private Rectangle GetCloseButtonRect()
    {
        return GetCloseButtonRect(PanelRect);
    }

    private Rectangle GetCloseButtonRect(Rectangle rect)
    {
        int btnX = rect.Right - CloseBtnSize - CloseBtnRightMargin;
        int btnY = rect.Y + (TitleBarHeight - CloseBtnSize) / 2;
        return new Rectangle(btnX, btnY, CloseBtnSize, CloseBtnSize);
    }

    /// <summary>
    /// 获取调整大小手柄矩形
    /// </summary>
    private Rectangle GetResizeHandleRect()
    {
        return GetResizeHandleRect(PanelRect);
    }

    private Rectangle GetResizeHandleRect(Rectangle rect)
    {
        return new Rectangle(rect.Right - ResizeHandleSize, rect.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
    }

    /// <summary>
    /// 获取内容区域
    /// </summary>
    private Rectangle GetContentRect(Rectangle rect)
    {
        int top = rect.Y + TitleBarHeight + 8;
        int left = rect.X + 8;
        int width = rect.Width - 16;
        int height = rect.Height - TitleBarHeight - 16 - ResizeHandleSize;
        return new Rectangle(left, top, width, height);
    }


    /// <summary>
    /// 计算缩放动画值
    /// </summary>
    private float CalculateScale()
    {
        // 打开时从0.85放大到1.0，关闭时从1.0缩小到0.85
        if (State == PanelState.Opening || State == PanelState.Open)
        {
            return 0.85f + 0.15f * _openProgress;
        }
        else // Closing
        {
            return 0.85f + 0.15f * _openProgress;
        }
    }

    /// <summary>
    /// 计算透明度动画值
    /// </summary>
    private float CalculateAlpha()
    {
        if (State == PanelState.Opening || State == PanelState.Open)
        {
            return _openProgress;
        }
        else // Closing
        {
            return _openProgress;
        }
    }

    // ==================== 动画工具 ====================
    /// <summary>
    /// 弹性动画插值
    /// </summary>
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
    /// 绘制关闭按钮（X 符号）
    /// </summary>
    private void DrawCloseButton(SpriteBatch sb, Rectangle rect, float alpha)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        bool hover = rect.Contains(Main.MouseScreen.ToPoint());

        // 按钮背景色
        Color bgColor;
        if (_closeBtnPressed && hover)
            bgColor = CloseBtnPressed;
        else if (hover)
            bgColor = CloseBtnHover;
        else
            bgColor = CloseBtnNormal;

        bgColor *= alpha;

        UIRendering.DrawFilledRect(sb, rect, bgColor);

        // 绘制 X 符号（两条交叉线）
        int cx = rect.Center.X;
        int cy = rect.Center.Y;
        int len = 5;
        int thickness = 2;

        // 左上到右下斜线
        for (int i = -len; i <= len; i++)
        {
            sb.Draw(pixel, new Rectangle(cx + i, cy + i, thickness, 1), CloseSymbolColor * alpha);
        }
        // 右上到左下斜线
        for (int i = -len; i <= len; i++)
        {
            sb.Draw(pixel, new Rectangle(cx + i, cy - i, thickness, 1), CloseSymbolColor * alpha);
        }
    }

    /// <summary>
    /// 绘制调整大小手柄（右下角小三角）
    /// </summary>
    private static void DrawResizeHandle(SpriteBatch sb, Rectangle rect, Color color)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        // 手柄背景
        sb.Draw(pixel, rect, color);

        // 装饰斜线
        int cx = rect.X;
        int cy = rect.Y;
        int size = ResizeHandleSize;

        // 三条斜线
        for (int i = 0; i < 3; i++)
        {
            int lineY = cy + 4 + i * 4;
            int lineLen = size - 4 - i * 4;
            if (lineLen > 0)
            {
                sb.Draw(pixel, new Rectangle(cx + size - lineLen - 2, lineY, lineLen, 1), Color.White * 0.4f);
            }
        }
    }

    /// <summary>
    /// SpriteBatch 辅助类（内联，避免依赖外部）
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
