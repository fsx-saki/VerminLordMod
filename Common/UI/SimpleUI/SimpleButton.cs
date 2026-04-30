// ============================================================
// SimpleButton - 完全独立设计的UI按钮
// 不依赖任何现有UI系统
// 功能：
// - 始终居中于父容器
// - 鼠标悬停微微变大
// - 点击时有按下效果
// - 点击后在聊天栏发送信息
// ============================================================
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 简单按钮 - 弹性居中，悬停放大，有点击效果
/// </summary>
public class SimpleButton
{
    // ==================== 属性 ====================
    /// <summary> 按钮文字 </summary>
    public string Text { get; set; }

    /// <summary> 点击回调 </summary>
    public Action? OnClickCallback { get; set; }

    // ==================== 颜色 ====================
    private static readonly Color BgNormal = new(200, 160, 240, 200);       // 淡紫色
    private static readonly Color BgHover = new(220, 180, 255, 220);        // 悬停亮紫
    private static readonly Color BgPressed = new(160, 120, 200, 240);      // 按下深紫
    private static readonly Color BorderNormal = new(220, 190, 255, 200);   // 边框
    private static readonly Color BorderHover = new(240, 210, 255, 220);    // 悬停边框
    private static readonly Color TextColor = new(255, 255, 255, 220);      // 文字

    // ==================== 动画状态 ====================
    private float _hoverAnim;       // 0~1 悬停动画
    private float _hoverVelocity;
    private float _pressAnim;       // 0~1 按下动画
    private float _pressVelocity;
    private bool _wasPressed;       // 上一帧是否按下（边沿检测用）

    private const float HoverScale = 1.08f;     // 悬停放大到108%
    private const float PressScale = 0.92f;     // 按下缩小到92%
    private const float AnimSpeed = 0.15f;
    private const float AnimDamping = 0.6f;
    private const int CornerRadius = 6;

    // ==================== 构造 ====================
    public SimpleButton(string text, Action? onClick = null)
    {
        Text = text;
        OnClickCallback = onClick;
    }

    // ==================== 点击触发 ====================
    /// <summary>
    /// 按钮被点击时调用
    /// </summary>
    public void OnClick()
    {
        // 在聊天栏发送信息
        string message = $"[SimpleUI] 按钮被点击了！";
        Main.NewText(message, new Color(200, 160, 255));

        // 调用外部回调
        OnClickCallback?.Invoke();
    }

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制按钮
    /// </summary>
    /// <param name="sb">SpriteBatch</param>
    /// <param name="rect">按钮区域（由父面板计算居中位置）</param>
    /// <param name="parentAlpha">父面板透明度</param>
    public void Draw(SpriteBatch sb, Rectangle rect, float parentAlpha)
    {
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());
        bool isPressed = isHovered && Main.mouseLeft;

        // 更新动画
        UpdateAnimation(isHovered, isPressed);

        // 计算最终缩放
        float hoverScale = 1f + (HoverScale - 1f) * _hoverAnim;
        float pressScale = 1f - (1f - PressScale) * _pressAnim;
        float finalScale = hoverScale * pressScale;

        // 计算缩放后的绘制矩形
        Rectangle drawRect = rect;
        if (Math.Abs(finalScale - 1f) > 0.001f)
        {
            int cx = rect.Center.X;
            int cy = rect.Center.Y;
            int w = (int)(rect.Width * finalScale);
            int h = (int)(rect.Height * finalScale);
            drawRect = new Rectangle(cx - w / 2, cy - h / 2, w, h);
        }

        // 选择颜色
        Color bgColor;
        Color borderColor;
        if (isPressed)
        {
            bgColor = BgPressed;
            borderColor = BorderNormal;
        }
        else if (isHovered)
        {
            bgColor = BgHover;
            borderColor = BorderHover;
        }
        else
        {
            bgColor = BgNormal;
            borderColor = BorderNormal;
        }

        bgColor *= parentAlpha;
        borderColor *= parentAlpha;

        var pixel = TextureAssets.MagicPixel.Value;

        // 绘制按钮背景（圆角矩形）
        DrawRoundedRect(sb, drawRect, bgColor, borderColor, CornerRadius);

        // 绘制文字
        var font = FontAssets.MouseText.Value;
        var textSize = font.MeasureString(Text) * 0.85f;
        var textPos = new Vector2(drawRect.Center.X - textSize.X / 2f, drawRect.Center.Y - textSize.Y / 2f);
        Utils.DrawBorderString(sb, Text, textPos, TextColor * parentAlpha, 0.85f);

        // 阻止鼠标穿透
        if (isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    /// <summary>
    /// 更新悬停和按下动画
    /// </summary>
    private void UpdateAnimation(bool isHovered, bool isPressed)
    {
        // 悬停动画
        AnimateFloat(ref _hoverAnim, ref _hoverVelocity, isHovered ? 1f : 0f);

        // 按下动画（边沿触发，按下瞬间弹一下）
        if (isPressed && !_wasPressed)
        {
            _pressAnim = 1f;
            _pressVelocity = 0f;
        }
        else if (!isPressed && _wasPressed)
        {
            // 释放时弹回
            _pressAnim = 0f;
            _pressVelocity = 0f;
        }
        else if (isPressed)
        {
            // 保持按下状态
            AnimateFloat(ref _pressAnim, ref _pressVelocity, 1f);
        }
        else
        {
            // 弹回
            AnimateFloat(ref _pressAnim, ref _pressVelocity, 0f);
        }

        _wasPressed = isPressed;
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
    private static void DrawRoundedRect(SpriteBatch sb, Rectangle rect, Color bgColor, Color borderColor, int radius)
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

        // 边框
        sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Bottom - 1, rect.Width - radius * 2, 1), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + radius, 1, rect.Height - radius * 2), borderColor);
        sb.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y + radius, 1, rect.Height - radius * 2), borderColor);
    }
}
