// ============================================================
// SimpleSubPanel - 完全独立设计的嵌套子面板（分组框）
// 不依赖任何现有UI系统
// 功能：
// - 深紫色细边框，用于分割整理主面板内容
// - 弹性定位：通过百分比定义在主面板内容区中的位置
// - 内部可包含按钮、物品格子等子元素
// - 内容超出时出现垂直/水平滚动条
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 子面板中的元素类型
/// </summary>
public enum SubPanelElementType
{
    Button,
    ItemSlot,
    Label,
    /// <summary>
    /// 固定框：包含一组子元素，整体弹性居中定位，
    /// 内部元素使用固定像素间距，不随面板缩放变化。
    /// 组本身不能调整大小，尺寸由内部元素自动计算。
    /// </summary>
    FixedGroup,
    /// <summary>
    /// 动画格子行：播放/复位控制，格子从右侧放大入场
    /// </summary>
    AnimSlotRow
}

/// <summary>
/// 元素水平对齐方式
/// </summary>
public enum ElementAnchor
{
    /// <summary> RelX 表示元素左边缘位置 </summary>
    Left,
    /// <summary> RelX 表示元素中心位置（自动偏移宽度的一半） </summary>
    Center,
    /// <summary> RelX 表示元素右边缘位置 </summary>
    Right
}

/// <summary>
/// 子面板中的元素描述
/// </summary>
public class SubPanelElement
{
    /// <summary> 元素类型 </summary>
    public SubPanelElementType Type { get; set; }

    /// <summary> 在子面板内容区中的相对 X 位置 (0~1)，或像素 X（当 UsePixelPosition=true） </summary>
    public float RelX { get; set; }

    /// <summary> 在子面板内容区中的相对 Y 位置 (0~1)，或像素 Y（当 UsePixelPosition=true） </summary>
    public float RelY { get; set; }

    /// <summary> 水平对齐方式 </summary>
    public ElementAnchor AnchorX { get; set; } = ElementAnchor.Left;

    /// <summary> 垂直对齐方式 </summary>
    public ElementAnchor AnchorY { get; set; } = ElementAnchor.Left;

    /// <summary> 元素宽度（像素） </summary>
    public int Width { get; set; } = 80;

    /// <summary> 元素高度（像素） </summary>
    public int Height { get; set; } = 30;

    /// <summary>
    /// 是否使用像素定位（RelX/RelY 直接作为像素值，而非百分比）
    /// 用于固定间距布局，避免缩小时元素重叠
    /// </summary>
    public bool UsePixelPosition { get; set; }

    // ===== 按钮相关 =====
    /// <summary> 按钮文字 </summary>
    public string? ButtonText { get; set; }

    /// <summary> 按钮点击回调 </summary>
    public Action? ButtonClick { get; set; }

    // ===== 物品格子相关 =====
    /// <summary> 物品格子引用 </summary>
    public SimpleItemSlot? ItemSlot { get; set; }

    // ===== 标签相关 =====
    /// <summary> 标签文字 </summary>
    public string? LabelText { get; set; }

    /// <summary> 标签颜色 </summary>
    public Color LabelColor { get; set; } = Color.White;

    /// <summary> 标签缩放 </summary>
    public float LabelScale { get; set; } = 0.8f;

    // ===== 固定框相关 =====
    /// <summary>
    /// 固定框实例引用（仅当 Type==FixedGroup 时有效）
    /// </summary>
    public SimpleFixedGroup? FixedGroup { get; set; }

    // ===== 动画格子行相关 =====
    /// <summary>
    /// 动画格子行实例引用（仅当 Type==AnimSlotRow 时有效）
    /// </summary>
    public SimpleAnimSlotRow? AnimRow { get; set; }

    /// <summary>
    /// 获取元素在子面板内容区中的像素矩形（考虑对齐方式）
    /// 如果 UsePixelPosition=true，RelX/RelY 直接作为像素坐标
    /// 对于 FixedGroup 类型，Width/Height 由 SimpleFixedGroup.CalculateSize() 自动计算
    /// </summary>
    public Rectangle GetRect(Rectangle subContentRect)
    {
        int x;
        if (UsePixelPosition)
        {
            // 像素定位：RelX/RelY 直接作为子面板内容区内的像素偏移
            switch (AnchorX)
            {
                case ElementAnchor.Center:
                    x = subContentRect.X + (int)RelX - Width / 2;
                    break;
                case ElementAnchor.Right:
                    x = subContentRect.X + (int)RelX - Width;
                    break;
                default:
                    x = subContentRect.X + (int)RelX;
                    break;
            }

            int y;
            switch (AnchorY)
            {
                case ElementAnchor.Center:
                    y = subContentRect.Y + (int)RelY - Height / 2;
                    break;
                case ElementAnchor.Right:
                    y = subContentRect.Y + (int)RelY - Height;
                    break;
                default:
                    y = subContentRect.Y + (int)RelY;
                    break;
            }
            return new Rectangle(x, y, Width, Height);
        }
        else
        {
            // 百分比定位
            switch (AnchorX)
            {
                case ElementAnchor.Center:
                    x = subContentRect.X + (int)(RelX * subContentRect.Width) - Width / 2;
                    break;
                case ElementAnchor.Right:
                    x = subContentRect.X + (int)(RelX * subContentRect.Width) - Width;
                    break;
                default:
                    x = subContentRect.X + (int)(RelX * subContentRect.Width);
                    break;
            }

            int y;
            switch (AnchorY)
            {
                case ElementAnchor.Center:
                    y = subContentRect.Y + (int)(RelY * subContentRect.Height) - Height / 2;
                    break;
                case ElementAnchor.Right:
                    y = subContentRect.Y + (int)(RelY * subContentRect.Height) - Height;
                    break;
                default:
                    y = subContentRect.Y + (int)(RelY * subContentRect.Height);
                    break;
            }
            return new Rectangle(x, y, Width, Height);
        }
    }
}

/// <summary>
/// 简单嵌套子面板 — 深紫色细边框，弹性定位，可滚动
/// </summary>
public class SimpleSubPanel
{
    // ==================== 定位属性（百分比，0.0~1.0） ====================
    /// <summary> 在主面板内容区中的相对 X 位置 </summary>
    public float RelativeX { get; set; } = 0f;

    /// <summary> 在主面板内容区中的相对 Y 位置 </summary>
    public float RelativeY { get; set; } = 0f;

    /// <summary> 相对宽度（占主面板内容区的比例） </summary>
    public float RelativeWidth { get; set; } = 0.5f;

    /// <summary> 相对高度（占主面板内容区的比例） </summary>
    public float RelativeHeight { get; set; } = 0.5f;

    /// <summary> 子面板标题（可选，显示在左上角） </summary>
    public string? Title { get; set; }

    /// <summary> 子面板中的元素列表 </summary>
    public List<SubPanelElement> Elements { get; set; } = new();

    // ==================== 颜色主题（深紫色细边框） ====================
    private static readonly Color SubPanelBg = new(30, 15, 50, 60);           // 极淡深紫背景
    private static readonly Color SubPanelBorder = new(80, 40, 120, 200);     // 深紫色细边框
    private static readonly Color SubPanelBorderHover = new(120, 60, 160, 220); // 悬停时边框
    private static readonly Color SubPanelTitleColor = new(160, 120, 200, 220); // 标题文字
    private static readonly Color ScrollbarTrack = new(40, 20, 60, 150);      // 滚动条轨道
    private static readonly Color ScrollbarThumb = new(100, 60, 140, 200);    // 滚动条滑块
    private static readonly Color ScrollbarThumbHover = new(140, 100, 180, 220); // 滑块悬停

    // ==================== 尺寸常量 ====================
    private const int BorderWidth = 1;
    private const int TitleHeight = 18;
    private const int ScrollbarSize = 10;
    private const int Padding = 4;
    private const int ScrollMargin = 20; // 滚动条底部/右侧的余空间（像素）
    private const int MinSubPanelWidth = 60;  // 子面板最小宽度
    private const int MinSubPanelHeight = 40; // 子面板最小高度

    // ==================== 滚动状态 ====================
    private float _scrollX;          // 水平滚动偏移
    private float _scrollY;          // 垂直滚动偏移
    private float _scrollXVelocity;
    private float _scrollYVelocity;
    private bool _isDraggingScrollH; // 是否正在拖动水平滑块
    private bool _isDraggingScrollV; // 是否正在拖动垂直滑块
    private float _scrollDragOffset; // 拖动偏移量
    private bool _lastMouseLeft;
    private bool _isHovered;
    private int _lastScrollValue;

    // ==================== 动画 ====================
    private float _hoverAnim;
    private float _hoverVelocity;
    private const float AnimSpeed = 0.12f;
    private const float AnimDamping = 0.65f;

    // ==================== 内部按钮缓存（避免每帧 new） ====================
    private readonly Dictionary<int, SimpleButton> _cachedButtons = new();

    // ==================== 计算矩形 ====================
    /// <summary>
    /// 获取子面板在主面板内容区中的像素矩形
    /// 应用最小尺寸限制，避免缩小时元素重叠
    /// </summary>
    public Rectangle GetRect(Rectangle parentContentRect)
    {
        int x = parentContentRect.X + (int)(RelativeX * parentContentRect.Width);
        int y = parentContentRect.Y + (int)(RelativeY * parentContentRect.Height);
        int w = Math.Max(MinSubPanelWidth, (int)(RelativeWidth * parentContentRect.Width));
        int h = Math.Max(MinSubPanelHeight, (int)(RelativeHeight * parentContentRect.Height));
        return new Rectangle(x, y, w, h);
    }

    /// <summary>
    /// 计算元素的实际右边界（考虑对齐方式）
    /// 对于 FixedGroup，Width 已由 SimpleFixedGroup.CalculateSize() 自动计算
    /// </summary>
    private static int GetElemRightEdge(SubPanelElement elem, int baseW)
    {
        float relPixel = elem.UsePixelPosition ? elem.RelX : elem.RelX * baseW;
        switch (elem.AnchorX)
        {
            case ElementAnchor.Center:
                return (int)(relPixel + elem.Width / 2f);
            case ElementAnchor.Right:
                return (int)relPixel;
            default:
                return (int)(relPixel + elem.Width);
        }
    }

    /// <summary>
    /// 计算元素的实际下边界（考虑对齐方式）
    /// 对于 FixedGroup，Height 已由 SimpleFixedGroup.CalculateSize() 自动计算
    /// </summary>
    private static int GetElemBottomEdge(SubPanelElement elem, int baseH)
    {
        float relPixel = elem.UsePixelPosition ? elem.RelY : elem.RelY * baseH;
        switch (elem.AnchorY)
        {
            case ElementAnchor.Center:
                return (int)(relPixel + elem.Height / 2f);
            case ElementAnchor.Right:
                return (int)relPixel;
            default:
                return (int)(relPixel + elem.Height);
        }
    }

    /// <summary>
    /// 获取子面板内部内容区域（扣除边框、标题、滚动条空间）
    /// 使用固定的"基础内容尺寸"作为 RelX/RelY 的基准，
    /// 确保滚动条出现/消失不会导致元素位置偏移。
    /// </summary>
    private Rectangle GetContentRect(Rectangle subRect)
    {
        int topOffset = BorderWidth;
        if (!string.IsNullOrEmpty(Title))
            topOffset += TitleHeight;

        // 基础内容尺寸（不含滚动条空间）— 用作 RelX/RelY 的基准
        int baseContentW = subRect.Width - BorderWidth * 2 - Padding * 2;
        int baseContentH = subRect.Height - topOffset - BorderWidth - Padding;

        // 用基础尺寸检查是否需要滚动条（考虑对齐方式）
        bool needHScroll = false;
        bool needVScroll = false;
        foreach (var elem in Elements)
        {
            int elemRight = GetElemRightEdge(elem, baseContentW);
            int elemBottom = GetElemBottomEdge(elem, baseContentH);
            if (elemRight > baseContentW) needHScroll = true;
            if (elemBottom > baseContentH) needVScroll = true;
        }

        // 最终内容尺寸（扣除滚动条空间）
        int contentW = baseContentW;
        int contentH = baseContentH;
        if (needVScroll) contentW -= ScrollbarSize;
        if (needHScroll) contentH -= ScrollbarSize;

        int cx = subRect.X + BorderWidth + Padding;
        int cy = subRect.Y + topOffset + Padding;
        return new Rectangle(cx, cy, Math.Max(1, contentW), Math.Max(1, contentH));
    }

    /// <summary>
    /// 获取虚拟内容总尺寸（用于计算滚动范围）
    /// 使用与 GetContentRect 相同的基础尺寸作为 RelX/RelY 基准。
    /// 在元素边界基础上加 ScrollMargin 余量，避免信息贴边。
    /// </summary>
    private Point GetVirtualContentSize(Rectangle contentRect, Rectangle subRect)
    {
        int topOffset = BorderWidth;
        if (!string.IsNullOrEmpty(Title))
            topOffset += TitleHeight;

        // 使用与 GetContentRect 相同的基础宽度
        int baseContentW = subRect.Width - BorderWidth * 2 - Padding * 2;
        int baseContentH = subRect.Height - topOffset - BorderWidth - Padding;

        int maxRight = contentRect.Width;
        int maxBottom = contentRect.Height;
        foreach (var elem in Elements)
        {
            // 用基础尺寸计算元素边界（考虑对齐方式）
            int elemRight = GetElemRightEdge(elem, baseContentW);
            int elemBottom = GetElemBottomEdge(elem, baseContentH);
            if (elemRight > maxRight) maxRight = elemRight;
            if (elemBottom > maxBottom) maxBottom = elemBottom;
        }

        // 加余量，避免滚动到底部时信息贴边
        maxRight += ScrollMargin;
        maxBottom += ScrollMargin;

        // 当内容超出可视区域时，确保虚拟尺寸至少为可视区域的 2 倍，
        // 这样滚动范围 >= 可视区域大小，滑块大小合理，拖动顺畅。
        // 如果内容未超出，保持原值（不触发滚动条）。
        if (maxRight > contentRect.Width)
        {
            int minVirtualW = contentRect.Width * 2;
            if (maxRight < minVirtualW) maxRight = minVirtualW;
        }
        if (maxBottom > contentRect.Height)
        {
            int minVirtualH = contentRect.Height * 2;
            if (maxBottom < minVirtualH) maxBottom = minVirtualH;
        }

        return new Point(maxRight, maxBottom);
    }

    // ==================== 更新 ====================
    /// <summary>
    /// 更新子面板交互
    /// </summary>
    public void Update(Rectangle parentContentRect)
    {
        var subRect = GetRect(parentContentRect);
        var mousePos = Main.MouseScreen;

        _isHovered = subRect.Contains(mousePos.ToPoint());

        // 悬停动画
        AnimateFloat(ref _hoverAnim, ref _hoverVelocity, _isHovered ? 1f : 0f);

        var contentRect = GetContentRect(subRect);
        var virtualSize = GetVirtualContentSize(contentRect, subRect);

        bool needHScroll = virtualSize.X > contentRect.Width;
        bool needVScroll = virtualSize.Y > contentRect.Height;

        // 滚动条拖动：放在悬停检查之前，这样即使鼠标移出面板仍能拉住滚动条
        UpdateScrollbarDrag(subRect, contentRect, virtualSize, needHScroll, needVScroll);

        // 鼠标滚轮滚动
        int currentScroll = Mouse.GetState().ScrollWheelValue;
        if (_isHovered)
        {
            int delta = _lastScrollValue - currentScroll;

            if (delta != 0)
            {
                if (needVScroll)
                {
                    _scrollY -= delta * 0.3f;
                }
                else if (needHScroll)
                {
                    _scrollX -= delta * 0.3f;
                }
            }
        }
        _lastScrollValue = currentScroll;

        // 限制滚动范围
        if (needHScroll)
            _scrollX = Math.Clamp(_scrollX, 0, virtualSize.X - contentRect.Width);
        else
            _scrollX = 0;

        if (needVScroll)
            _scrollY = Math.Clamp(_scrollY, 0, virtualSize.Y - contentRect.Height);
        else
            _scrollY = 0;

        // 更新子元素（仅在悬停时处理交互）
        if (_isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;
            UpdateElements(contentRect);
        }

        _lastMouseLeft = Main.mouseLeft;
    }

    /// <summary>
    /// 更新滚动条拖动
    /// </summary>
    private void UpdateScrollbarDrag(Rectangle subRect, Rectangle contentRect, Point virtualSize, bool needHScroll, bool needVScroll)
    {
        var mousePos = Main.MouseScreen;
        bool currentLeft = Main.mouseLeft;

        if (currentLeft && !_lastMouseLeft)
        {
            // 检查是否点击了滚动条滑块
            if (needVScroll)
            {
                var vTrack = GetVScrollTrackRect(subRect, contentRect);
                var vThumb = GetVScrollThumbRect(subRect, contentRect, virtualSize);
                if (vThumb.Contains(mousePos.ToPoint()))
                {
                    _isDraggingScrollV = true;
                    _scrollDragOffset = mousePos.Y - vThumb.Y;
                }
                else if (vTrack.Contains(mousePos.ToPoint()) && !vThumb.Contains(mousePos.ToPoint()))
                {
                    // 点击轨道，跳转到点击位置
                    float clickRel = (mousePos.Y - vTrack.Y) / vTrack.Height;
                    _scrollY = clickRel * (virtualSize.Y - contentRect.Height);
                }
            }

            if (needHScroll)
            {
                var hTrack = GetHScrollTrackRect(subRect, contentRect);
                var hThumb = GetHScrollThumbRect(subRect, contentRect, virtualSize);
                if (hThumb.Contains(mousePos.ToPoint()))
                {
                    _isDraggingScrollH = true;
                    _scrollDragOffset = mousePos.X - hThumb.X;
                }
                else if (hTrack.Contains(mousePos.ToPoint()) && !hThumb.Contains(mousePos.ToPoint()))
                {
                    float clickRel = (mousePos.X - hTrack.X) / hTrack.Width;
                    _scrollX = clickRel * (virtualSize.X - contentRect.Width);
                }
            }
        }

        // 拖动中
        if (_isDraggingScrollV && currentLeft)
        {
            var vTrack = GetVScrollTrackRect(subRect, contentRect);
            float trackHeight = vTrack.Height;
            if (trackHeight > 0)
            {
                float rel = (mousePos.Y - _scrollDragOffset - vTrack.Y) / trackHeight;
                _scrollY = rel * (virtualSize.Y - contentRect.Height);
            }
        }
        else
        {
            _isDraggingScrollV = false;
        }

        if (_isDraggingScrollH && currentLeft)
        {
            var hTrack = GetHScrollTrackRect(subRect, contentRect);
            float trackWidth = hTrack.Width;
            if (trackWidth > 0)
            {
                float rel = (mousePos.X - _scrollDragOffset - hTrack.X) / trackWidth;
                _scrollX = rel * (virtualSize.X - contentRect.Width);
            }
        }
        else
        {
            _isDraggingScrollH = false;
        }
    }

    /// <summary>
    /// 更新子面板中的元素
    /// </summary>
    private void UpdateElements(Rectangle contentRect)
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            var elem = Elements[i];
            var elemRect = elem.GetRect(contentRect);

            // 应用滚动偏移
            var scrolledRect = new Rectangle(
                elemRect.X - (int)_scrollX,
                elemRect.Y - (int)_scrollY,
                elemRect.Width,
                elemRect.Height
            );

            switch (elem.Type)
            {
                case SubPanelElementType.Button:
                    UpdateButtonElement(elem, scrolledRect, i);
                    break;
                case SubPanelElementType.ItemSlot:
                    elem.ItemSlot?.Update(scrolledRect);
                    break;
                case SubPanelElementType.Label:
                    // 标签无需交互
                    break;
                case SubPanelElementType.FixedGroup:
                    UpdateFixedGroup(elem, scrolledRect);
                    break;
                case SubPanelElementType.AnimSlotRow:
                    elem.AnimRow?.Update();
                    break;
            }
        }
    }

    /// <summary>
    /// 更新固定框内的子元素
    /// 委托给 SimpleFixedGroup.Update()
    /// </summary>
    private void UpdateFixedGroup(SubPanelElement groupElem, Rectangle groupRect)
    {
        groupElem.FixedGroup?.Update(groupRect, _lastMouseLeft);
    }

    /// <summary>
    /// 更新按钮元素
    /// </summary>
    private void UpdateButtonElement(SubPanelElement elem, Rectangle rect, int index)
    {
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());

        if (isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;

            bool currentLeft = Main.mouseLeft;
            if (currentLeft && !_lastMouseLeft)
            {
                elem.ButtonClick?.Invoke();
            }
        }
    }

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制子面板
    /// </summary>
    public void Draw(SpriteBatch sb, Rectangle parentContentRect, float parentAlpha)
    {
        var subRect = GetRect(parentContentRect);

        // 裁剪到子面板区域
        var pixel = TextureAssets.MagicPixel.Value;

        // 绘制背景
        sb.Draw(pixel, subRect, SubPanelBg * parentAlpha);

        // 绘制边框（深紫色细线）
        Color borderColor = Color.Lerp(SubPanelBorder, SubPanelBorderHover, _hoverAnim) * parentAlpha;
        sb.Draw(pixel, new Rectangle(subRect.X, subRect.Y, subRect.Width, BorderWidth), borderColor);
        sb.Draw(pixel, new Rectangle(subRect.X, subRect.Bottom - BorderWidth, subRect.Width, BorderWidth), borderColor);
        sb.Draw(pixel, new Rectangle(subRect.X, subRect.Y + BorderWidth, BorderWidth, subRect.Height - BorderWidth * 2), borderColor);
        sb.Draw(pixel, new Rectangle(subRect.Right - BorderWidth, subRect.Y + BorderWidth, BorderWidth, subRect.Height - BorderWidth * 2), borderColor);

        // 绘制标题（左上角）
        if (!string.IsNullOrEmpty(Title))
        {
            var titleRect = new Rectangle(subRect.X + BorderWidth + 4, subRect.Y + BorderWidth, subRect.Width - BorderWidth * 2 - 8, TitleHeight);
            DrawText(sb, Title, titleRect, SubPanelTitleColor * parentAlpha, 0.7f);
        }

        // 计算内容区域
        var contentRect = GetContentRect(subRect);
        var virtualSize = GetVirtualContentSize(contentRect, subRect);

        bool needHScroll = virtualSize.X > contentRect.Width;
        bool needVScroll = virtualSize.Y > contentRect.Height;

        // 启用 ScissorTest 裁剪：只有子面板边框内的部分可见，超出部分隐藏
        // 裁剪矩形 = 整个子面板区域（包括边框）
        // 注意：ScissorRectangle 使用物理像素坐标，而 subRect 是 UI 坐标（已应用 UIScaleMatrix）
        // 因此需要将 subRect 乘以 Main.UIScale 转换到屏幕像素坐标
        var prevScissorRect = sb.GraphicsDevice.ScissorRectangle;
        float uiScale = Main.UIScale;
        var scissorRect = new Rectangle(
            (int)(subRect.X * uiScale),
            (int)(subRect.Y * uiScale),
            (int)(subRect.Width * uiScale),
            (int)(subRect.Height * uiScale)
        );

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None,
            new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true },
            null, Main.UIScaleMatrix);

        sb.GraphicsDevice.ScissorRectangle = scissorRect;

        // 绘制子元素（超出 subRect 的部分被裁剪隐藏）
        DrawElements(sb, contentRect, parentAlpha);

        // 恢复非裁剪模式
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None,
            new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = false },
            null, Main.UIScaleMatrix);

        sb.GraphicsDevice.ScissorRectangle = prevScissorRect;

        // 绘制滚动条（在裁剪区域之上，不受裁剪影响）
        if (needVScroll)
            DrawVScrollbar(sb, subRect, contentRect, virtualSize, parentAlpha);
        if (needHScroll)
            DrawHScrollbar(sb, subRect, contentRect, virtualSize, parentAlpha);
    }

    /// <summary>
    /// 绘制子元素
    /// </summary>
    private void DrawElements(SpriteBatch sb, Rectangle contentRect, float parentAlpha)
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            var elem = Elements[i];
            var elemRect = elem.GetRect(contentRect);

            // 应用滚动偏移
            var drawRect = new Rectangle(
                elemRect.X - (int)_scrollX,
                elemRect.Y - (int)_scrollY,
                elemRect.Width,
                elemRect.Height
            );

            switch (elem.Type)
            {
                case SubPanelElementType.Button:
                    DrawButtonElement(sb, elem, drawRect, parentAlpha, i);
                    break;
                case SubPanelElementType.ItemSlot:
                    elem.ItemSlot?.Draw(sb, drawRect, parentAlpha);
                    break;
                case SubPanelElementType.Label:
                    DrawLabelElement(sb, elem, drawRect, parentAlpha);
                    break;
                case SubPanelElementType.FixedGroup:
                    DrawFixedGroup(sb, elem, drawRect, parentAlpha);
                    break;
                case SubPanelElementType.AnimSlotRow:
                    // AnimSlotRow 需要整个内容区作为容器来居中格子
                    elem.AnimRow?.Draw(sb, contentRect, parentAlpha);
                    break;
            }
        }
    }

    /// <summary>
    /// 绘制固定框及其内部子元素
    /// 委托给 SimpleFixedGroup.Draw()
    /// </summary>
    private void DrawFixedGroup(SpriteBatch sb, SubPanelElement groupElem, Rectangle groupRect, float parentAlpha)
    {
        groupElem.FixedGroup?.Draw(sb, groupRect, parentAlpha);
    }

    /// <summary>
    /// 绘制按钮元素
    /// </summary>
    private void DrawButtonElement(SpriteBatch sb, SubPanelElement elem, Rectangle rect, float parentAlpha, int index)
    {
        // 获取或创建缓存的 SimpleButton
        if (!_cachedButtons.TryGetValue(index, out var button))
        {
            button = new SimpleButton(elem.ButtonText ?? "按钮");
            _cachedButtons[index] = button;
        }

        // 更新按钮文字（可能变化）
        if (button.Text != elem.ButtonText)
            button.Text = elem.ButtonText ?? "按钮";

        // 使用 SimpleButton 的绘制逻辑
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());
        bool isPressed = isHovered && Main.mouseLeft;

        // 手动绘制按钮（复用 SimpleButton 的样式）
        var pixel = TextureAssets.MagicPixel.Value;

        Color bgColor, borderColor;
        if (isPressed)
        {
            bgColor = new Color(160, 120, 200, 240);
            borderColor = new Color(220, 190, 255, 200);
        }
        else if (isHovered)
        {
            bgColor = new Color(220, 180, 255, 220);
            borderColor = new Color(240, 210, 255, 220);
        }
        else
        {
            bgColor = new Color(200, 160, 240, 200);
            borderColor = new Color(220, 190, 255, 200);
        }

        bgColor *= parentAlpha;
        borderColor *= parentAlpha;

        // 背景
        sb.Draw(pixel, rect, bgColor);
        // 边框
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 2), borderColor);
        sb.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y + 1, 1, rect.Height - 2), borderColor);

        // 文字
        if (!string.IsNullOrEmpty(elem.ButtonText))
        {
            var font = FontAssets.MouseText.Value;
            var textSize = font.MeasureString(elem.ButtonText) * 0.8f;
            var textPos = new Vector2(rect.Center.X - textSize.X / 2f, rect.Center.Y - textSize.Y / 2f);
            Utils.DrawBorderString(sb, elem.ButtonText, textPos, Color.White * parentAlpha, 0.8f);
        }

        if (isHovered)
            Main.LocalPlayer.mouseInterface = true;
    }

    /// <summary>
    /// 绘制标签元素
    /// </summary>
    private static void DrawLabelElement(SpriteBatch sb, SubPanelElement elem, Rectangle rect, float parentAlpha)
    {
        if (string.IsNullOrEmpty(elem.LabelText)) return;

        var font = FontAssets.MouseText.Value;
        var textSize = font.MeasureString(elem.LabelText) * elem.LabelScale;
        var textPos = new Vector2(rect.X, rect.Y);
        Utils.DrawBorderString(sb, elem.LabelText, textPos, elem.LabelColor * parentAlpha, elem.LabelScale);
    }

    // ==================== 滚动条计算 ====================
    private Rectangle GetVScrollTrackRect(Rectangle subRect, Rectangle contentRect)
    {
        int top = subRect.Y + BorderWidth;
        if (!string.IsNullOrEmpty(Title))
            top += TitleHeight;
        top += Padding;

        int bottom = subRect.Bottom - BorderWidth - Padding;
        // 如果有水平滚动条，留出空间
        // 简单起见，总是预留
        bottom -= ScrollbarSize;

        int trackX = subRect.Right - BorderWidth - ScrollbarSize;
        return new Rectangle(trackX, top, ScrollbarSize, Math.Max(1, bottom - top));
    }

    private Rectangle GetVScrollThumbRect(Rectangle subRect, Rectangle contentRect, Point virtualSize)
    {
        var track = GetVScrollTrackRect(subRect, contentRect);
        float contentH = virtualSize.Y;
        float viewH = contentRect.Height;
        if (contentH <= viewH) return Rectangle.Empty;

        float thumbHeight = Math.Max(ScrollbarSize, track.Height * (viewH / contentH));
        float maxScroll = contentH - viewH;
        float scrollRatio = maxScroll > 0 ? _scrollY / maxScroll : 0;
        float thumbY = track.Y + scrollRatio * (track.Height - thumbHeight);

        return new Rectangle(track.X, (int)thumbY, ScrollbarSize, (int)thumbHeight);
    }

    private Rectangle GetHScrollTrackRect(Rectangle subRect, Rectangle contentRect)
    {
        int left = subRect.X + BorderWidth + Padding;
        int right = subRect.Right - BorderWidth - Padding;
        // 如果有垂直滚动条，留出空间
        right -= ScrollbarSize;

        int trackY = subRect.Bottom - BorderWidth - ScrollbarSize;
        return new Rectangle(left, trackY, Math.Max(1, right - left), ScrollbarSize);
    }

    private Rectangle GetHScrollThumbRect(Rectangle subRect, Rectangle contentRect, Point virtualSize)
    {
        var track = GetHScrollTrackRect(subRect, contentRect);
        float contentW = virtualSize.X;
        float viewW = contentRect.Width;
        if (contentW <= viewW) return Rectangle.Empty;

        float thumbWidth = Math.Max(ScrollbarSize, track.Width * (viewW / contentW));
        float maxScroll = contentW - viewW;
        float scrollRatio = maxScroll > 0 ? _scrollX / maxScroll : 0;
        float thumbX = track.X + scrollRatio * (track.Width - thumbWidth);

        return new Rectangle((int)thumbX, track.Y, (int)thumbWidth, ScrollbarSize);
    }

    // ==================== 滚动条绘制 ====================
    private void DrawVScrollbar(SpriteBatch sb, Rectangle subRect, Rectangle contentRect, Point virtualSize, float parentAlpha)
    {
        var track = GetVScrollTrackRect(subRect, contentRect);
        var thumb = GetVScrollThumbRect(subRect, contentRect, virtualSize);

        var pixel = TextureAssets.MagicPixel.Value;

        // 轨道
        sb.Draw(pixel, track, ScrollbarTrack * parentAlpha);

        // 滑块
        if (!thumb.IsEmpty)
        {
            bool hoverThumb = thumb.Contains(Main.MouseScreen.ToPoint());
            Color thumbColor = hoverThumb ? ScrollbarThumbHover : ScrollbarThumb;
            sb.Draw(pixel, thumb, thumbColor * parentAlpha);
        }
    }

    private void DrawHScrollbar(SpriteBatch sb, Rectangle subRect, Rectangle contentRect, Point virtualSize, float parentAlpha)
    {
        var track = GetHScrollTrackRect(subRect, contentRect);
        var thumb = GetHScrollThumbRect(subRect, contentRect, virtualSize);

        var pixel = TextureAssets.MagicPixel.Value;

        // 轨道
        sb.Draw(pixel, track, ScrollbarTrack * parentAlpha);

        // 滑块
        if (!thumb.IsEmpty)
        {
            bool hoverThumb = thumb.Contains(Main.MouseScreen.ToPoint());
            Color thumbColor = hoverThumb ? ScrollbarThumbHover : ScrollbarThumb;
            sb.Draw(pixel, thumb, thumbColor * parentAlpha);
        }
    }

    // ==================== 工具方法 ====================
    private static void DrawText(SpriteBatch sb, string text, Rectangle rect, Color color, float scale)
    {
        var font = FontAssets.MouseText.Value;
        var size = font.MeasureString(text) * scale;
        var pos = new Vector2(rect.X, rect.Center.Y - size.Y / 2f);
        Utils.DrawBorderString(sb, text, pos, color, scale);
    }

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
}
