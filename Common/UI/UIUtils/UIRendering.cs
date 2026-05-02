// ============================================================
// UIRendering - UI 渲染工具类
// 提供通用的绘制方法：填充矩形、圆角矩形、关闭按钮、滚动条等
// 合并了 DialogueTreeUI、SimplePanel、UIHelper 中的重复绘制逻辑
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace VerminLordMod.Common.UI.UIUtils;

/// <summary>
/// UI 渲染工具类 — 提供通用的绘制方法
/// </summary>
public static class UIRendering
{
    /// <summary>
    /// 获取 MagicPixel 纹理的快捷方式
    /// </summary>
    public static Texture2D Pixel => TextureAssets.MagicPixel.Value;

    /// <summary>
    /// 绘制填充矩形
    /// </summary>
    public static void DrawFilledRect(SpriteBatch sb, Rectangle rect, Color color)
    {
        sb.Draw(Pixel, rect, color);
    }

    /// <summary>
    /// 绘制圆角矩形（填充 + 边框）
    /// </summary>
    public static void DrawRoundedRect(SpriteBatch sb, Rectangle rect, Color bgColor, Color borderColor, int radius)
    {
        if (rect.Width <= radius * 2 || rect.Height <= radius * 2)
        {
            sb.Draw(Pixel, rect, bgColor);
            return;
        }

        // 中心填充
        sb.Draw(Pixel, new Rectangle(rect.X + radius, rect.Y + radius, rect.Width - radius * 2, rect.Height - radius * 2), bgColor);
        // 上下边
        sb.Draw(Pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, radius), bgColor);
        sb.Draw(Pixel, new Rectangle(rect.X + radius, rect.Bottom - radius, rect.Width - radius * 2, radius), bgColor);
        // 左右边
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y + radius, radius, rect.Height - radius * 2), bgColor);
        sb.Draw(Pixel, new Rectangle(rect.Right - radius, rect.Y + radius, radius, rect.Height - radius * 2), bgColor);
        // 四个角
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, radius, radius), bgColor);
        sb.Draw(Pixel, new Rectangle(rect.Right - radius, rect.Y, radius, radius), bgColor);
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - radius, radius, radius), bgColor);
        sb.Draw(Pixel, new Rectangle(rect.Right - radius, rect.Bottom - radius, radius, radius), bgColor);

        // 边框
        sb.Draw(Pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, 1), borderColor);
        sb.Draw(Pixel, new Rectangle(rect.X + radius, rect.Bottom - 1, rect.Width - radius * 2, 1), borderColor);
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y + radius, 1, rect.Height - radius * 2), borderColor);
        sb.Draw(Pixel, new Rectangle(rect.Right - 1, rect.Y + radius, 1, rect.Height - radius * 2), borderColor);
    }

    /// <summary>
    /// 绘制圆角矩形（仅填充，无边框）
    /// </summary>
    public static void DrawRoundedRect(SpriteBatch sb, Rectangle rect, Color color, int radius)
    {
        DrawRoundedRect(sb, rect, color, color, radius);
    }

    /// <summary>
    /// 绘制矩形边框
    /// </summary>
    public static void DrawBorder(SpriteBatch sb, Rectangle rect, int thickness, Color color)
    {
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        sb.Draw(Pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    /// <summary>
    /// 绘制关闭按钮（X 符号）
    /// </summary>
    public static void DrawCloseButton(SpriteBatch sb, Rectangle rect, Color bgColor, Color symbolColor, int symbolSize = 5, int thickness = 2)
    {
        DrawFilledRect(sb, rect, bgColor);

        // X 符号
        int cx = rect.Center.X;
        int cy = rect.Center.Y;
        for (int i = -symbolSize; i <= symbolSize; i++)
        {
            sb.Draw(Pixel, new Rectangle(cx + i, cy + i, thickness, 1), symbolColor);
        }
        for (int i = -symbolSize; i <= symbolSize; i++)
        {
            sb.Draw(Pixel, new Rectangle(cx + i, cy - i, thickness, 1), symbolColor);
        }
    }

    /// <summary>
    /// 绘制垂直滚动条
    /// </summary>
    public static void DrawScrollbar(SpriteBatch sb, Rectangle area, float scrollOffset, float contentHeight, Color trackColor, Color thumbColor)
    {
        // 滚动条轨道（右侧窄条）
        int scrollbarX = area.Right - 8;
        var trackRect = new Rectangle(scrollbarX, area.Y, 6, area.Height);
        DrawFilledRect(sb, trackRect, trackColor);

        // 滚动条滑块
        float visibleRatio = area.Height / contentHeight;
        float thumbHeight = MathHelper.Max(20, area.Height * visibleRatio);
        float maxScroll = contentHeight - area.Height;
        float scrollRatio = maxScroll > 0 ? scrollOffset / maxScroll : 0;
        float thumbY = area.Y + scrollRatio * (area.Height - thumbHeight);

        var thumbRect = new Rectangle(scrollbarX, (int)thumbY, 6, (int)thumbHeight);
        DrawFilledRect(sb, thumbRect, thumbColor);
    }

    /// <summary>
    /// 绘制带边框的文本（文字阴影效果）
    /// </summary>
    public static void DrawBorderStringWithShadow(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f)
    {
        // 阴影
        Utils.DrawBorderString(sb, text, pos + Vector2.One, Color.Black * 0.5f, scale);
        // 前景
        Utils.DrawBorderString(sb, text, pos, color, scale);
    }

    /// <summary>
    /// 在矩形区域内居中绘制文本
    /// </summary>
    public static void DrawTextCentered(SpriteBatch sb, string text, Rectangle rect, Color color, float scale = 1f)
    {
        var font = FontAssets.MouseText.Value;
        var textSize = font.MeasureString(text) * scale;
        var pos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2,
            rect.Y + (rect.Height - textSize.Y) / 2
        );
        Utils.DrawBorderString(sb, text, pos, color, scale);
    }
}
