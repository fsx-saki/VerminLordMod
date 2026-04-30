// ============================================================
// SimpleFixedGroup - 固定框（独立逻辑）
// 包含一组子元素，整体弹性居中定位，
// 内部元素使用固定像素间距，不随面板缩放变化。
// 组本身不能调整大小，尺寸由内部元素自动计算。
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 固定框 — 包含一组子元素，整体弹性居中定位，
/// 内部元素使用固定像素间距，不随面板缩放变化。
/// </summary>
public class SimpleFixedGroup
{
    /// <summary> 组内子元素列表 </summary>
    public List<SubPanelElement> Elements { get; set; } = new();

    /// <summary> 内部子元素之间的垂直间距（像素） </summary>
    public int Spacing { get; set; } = 8;

    /// <summary> 组的自动计算宽度（像素） </summary>
    public int Width { get; private set; } = 80;

    /// <summary> 组的自动计算高度（像素） </summary>
    public int Height { get; private set; } = 30;

    /// <summary>
    /// 计算组的自动尺寸
    /// 宽度 = 最大子元素宽度
    /// 高度 = 子元素高度之和 + 间距之和
    /// </summary>
    public void CalculateSize()
    {
        if (Elements.Count == 0)
        {
            Width = 80;
            Height = 30;
            return;
        }

        int maxW = 0;
        int totalH = 0;
        for (int i = 0; i < Elements.Count; i++)
        {
            var child = Elements[i];
            if (child.Width > maxW) maxW = child.Width;
            totalH += child.Height;
            if (i < Elements.Count - 1)
                totalH += Spacing;
        }

        Width = maxW;
        Height = totalH;
    }

    /// <summary>
    /// 更新组内子元素的交互
    /// </summary>
    /// <param name="groupRect">组在屏幕上的像素矩形</param>
    /// <param name="lastMouseLeft">上一帧鼠标左键状态</param>
    public void Update(Rectangle groupRect, bool lastMouseLeft)
    {
        int currentY = groupRect.Y;
        for (int i = 0; i < Elements.Count; i++)
        {
            var child = Elements[i];
            int cx = groupRect.X + (int)child.RelX;
            int cy = currentY;
            var childRect = new Rectangle(cx, cy, child.Width, child.Height);

            switch (child.Type)
            {
                case SubPanelElementType.Button:
                    UpdateButton(child, childRect, lastMouseLeft);
                    break;
                case SubPanelElementType.ItemSlot:
                    child.ItemSlot?.Update(childRect);
                    break;
                case SubPanelElementType.Label:
                    // 标签无需交互
                    break;
            }

            currentY += child.Height + Spacing;
        }
    }

    /// <summary>
    /// 绘制组内子元素
    /// </summary>
    public void Draw(SpriteBatch sb, Rectangle groupRect, float parentAlpha)
    {
        int currentY = groupRect.Y;
        for (int i = 0; i < Elements.Count; i++)
        {
            var child = Elements[i];
            int cx = groupRect.X + (int)child.RelX;
            int cy = currentY;
            var childRect = new Rectangle(cx, cy, child.Width, child.Height);

            switch (child.Type)
            {
                case SubPanelElementType.Button:
                    DrawButton(sb, child, childRect, parentAlpha, i);
                    break;
                case SubPanelElementType.ItemSlot:
                    child.ItemSlot?.Draw(sb, childRect, parentAlpha);
                    break;
                case SubPanelElementType.Label:
                    DrawLabel(sb, child, childRect, parentAlpha);
                    break;
            }

            currentY += child.Height + Spacing;
        }
    }

    /// <summary>
    /// 获取组内所有子元素占用的最大右边界（相对于组左上角）
    /// </summary>
    public int GetContentRight()
    {
        int maxRight = 0;
        foreach (var child in Elements)
        {
            int right = (int)child.RelX + child.Width;
            if (right > maxRight) maxRight = right;
        }
        return maxRight;
    }

    /// <summary>
    /// 获取组内所有子元素占用的最大下边界（相对于组左上角）
    /// </summary>
    public int GetContentBottom()
    {
        int totalH = 0;
        for (int i = 0; i < Elements.Count; i++)
        {
            totalH += Elements[i].Height;
            if (i < Elements.Count - 1)
                totalH += Spacing;
        }
        return totalH;
    }

    // ==================== 内部更新/绘制方法 ====================

    private static void UpdateButton(SubPanelElement elem, Rectangle rect, bool lastMouseLeft)
    {
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());

        if (isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;

            bool currentLeft = Main.mouseLeft;
            if (currentLeft && !lastMouseLeft)
            {
                elem.ButtonClick?.Invoke();
            }
        }
    }

    private static void DrawButton(SpriteBatch sb, SubPanelElement elem, Rectangle rect, float parentAlpha, int index)
    {
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());
        bool isPressed = isHovered && Main.mouseLeft;

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

        sb.Draw(pixel, rect, bgColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 2), borderColor);
        sb.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y + 1, 1, rect.Height - 2), borderColor);

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

    private static void DrawLabel(SpriteBatch sb, SubPanelElement elem, Rectangle rect, float parentAlpha)
    {
        if (string.IsNullOrEmpty(elem.LabelText)) return;

        var font = FontAssets.MouseText.Value;
        var textSize = font.MeasureString(elem.LabelText) * elem.LabelScale;
        var textPos = new Vector2(rect.X, rect.Y);
        Utils.DrawBorderString(sb, elem.LabelText, textPos, elem.LabelColor * parentAlpha, elem.LabelScale);
    }
}
