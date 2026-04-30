// ============================================================
// SimpleItemSlot - 完全独立设计的UI物品格子
// 不依赖任何现有UI系统
// 功能：
// - 显示物品图标和数量
// - 鼠标悬停高亮
// - 点击取走物品
// ============================================================
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 简单物品格子 — 可存放一个物品，点击取走
/// </summary>
public class SimpleItemSlot
{
    // ==================== 属性 ====================
    /// <summary> 格子中存放的物品（null 表示空） </summary>
    public Item? StoredItem { get; private set; }

    /// <summary> 点击取走物品时的回调 </summary>
    public Action<Item>? OnTakeItem { get; set; }

    /// <summary> 格子尺寸 </summary>
    public int SlotSize { get; set; } = 52;

    // ==================== 颜色 ====================
    private static readonly Color SlotBgNormal = new(60, 40, 80, 180);       // 空格子背景（深紫灰）
    private static readonly Color SlotBgHover = new(80, 60, 100, 200);       // 悬停背景
    private static readonly Color SlotBgFilled = new(80, 50, 110, 200);      // 有物品时背景
    private static readonly Color SlotBorderNormal = new(140, 100, 180, 200); // 边框
    private static readonly Color SlotBorderHover = new(200, 160, 240, 220);  // 悬停边框
    private static readonly Color SlotBorderFilled = new(180, 140, 220, 220); // 有物品时边框

    // ==================== 动画状态 ====================
    private float _hoverAnim;
    private float _hoverVelocity;
    private bool _wasMouseLeft;

    private const float AnimSpeed = 0.15f;
    private const float AnimDamping = 0.6f;
    private const float HoverScale = 1.06f;

    // ==================== 物品操作 ====================
    /// <summary>
    /// 放入物品到格子
    /// </summary>
    public void PutItem(Item item)
    {
        StoredItem = item;
    }

    /// <summary>
    /// 清空格子
    /// </summary>
    public void ClearSlot()
    {
        StoredItem = null;
    }

    /// <summary>
    /// 格子是否为空
    /// </summary>
    public bool IsEmpty => StoredItem == null || StoredItem.IsAir;

    // ==================== 更新 ====================
    /// <summary>
    /// 更新格子交互逻辑
    /// </summary>
    /// <param name="rect">格子在屏幕上的矩形区域</param>
    public void Update(Rectangle rect)
    {
        var mousePos = Main.MouseScreen;
        bool isHovered = rect.Contains(mousePos.ToPoint());

        // 悬停动画
        AnimateFloat(ref _hoverAnim, ref _hoverVelocity, isHovered ? 1f : 0f);

        // 点击检测（边沿触发）
        bool currentLeft = Main.mouseLeft;
        if (isHovered && currentLeft && !_wasMouseLeft)
        {
            // 点击格子
            Main.LocalPlayer.mouseInterface = true;

            if (!IsEmpty && StoredItem != null)
            {
                // 格子有物品 → 取走
                var player = Main.LocalPlayer;
                var takeItem = StoredItem.Clone();

                // 生成到玩家身上（自动放入背包或掉落）
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), takeItem);

                // 清空格子
                ClearSlot();

                // 调用回调
                OnTakeItem?.Invoke(takeItem);

                Main.NewText("[SimpleUI] 从格子取走了物品", new Color(200, 160, 255));
            }
        }

        _wasMouseLeft = currentLeft;

        // 阻止鼠标穿透
        if (isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制物品格子
    /// </summary>
    /// <param name="sb">SpriteBatch</param>
    /// <param name="rect">格子矩形区域</param>
    /// <param name="parentAlpha">父面板透明度</param>
    public void Draw(SpriteBatch sb, Rectangle rect, float parentAlpha)
    {
        // 计算悬停缩放
        float scaleFactor = 1f + (HoverScale - 1f) * _hoverAnim;

        Rectangle drawRect = rect;
        if (Math.Abs(scaleFactor - 1f) > 0.001f)
        {
            int cx = rect.Center.X;
            int cy = rect.Center.Y;
            int w = (int)(rect.Width * scaleFactor);
            int h = (int)(rect.Height * scaleFactor);
            drawRect = new Rectangle(cx - w / 2, cy - h / 2, w, h);
        }

        bool isHovered = _hoverAnim > 0.1f;
        bool hasItem = !IsEmpty;

        // 选择颜色
        Color bgColor;
        Color borderColor;

        if (hasItem)
        {
            bgColor = SlotBgFilled;
            borderColor = isHovered ? SlotBorderHover : SlotBorderFilled;
        }
        else
        {
            bgColor = isHovered ? SlotBgHover : SlotBgNormal;
            borderColor = isHovered ? SlotBorderHover : SlotBorderNormal;
        }

        bgColor *= parentAlpha;
        borderColor *= parentAlpha;

        var pixel = TextureAssets.MagicPixel.Value;

        // 绘制格子背景
        DrawSlotBackground(sb, drawRect, bgColor, borderColor);

        // 绘制物品图标
        if (hasItem && StoredItem != null)
        {
            DrawItemIcon(sb, StoredItem, drawRect, parentAlpha);
        }
    }

    /// <summary>
    /// 绘制格子背景（带内发光效果）
    /// </summary>
    private static void DrawSlotBackground(SpriteBatch sb, Rectangle rect, Color bgColor, Color borderColor)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        // 背景
        sb.Draw(pixel, rect, bgColor);

        // 边框（2像素宽）
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), borderColor);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + 2, 2, rect.Height - 4), borderColor);
        sb.Draw(pixel, new Rectangle(rect.Right - 2, rect.Y + 2, 2, rect.Height - 4), borderColor);

        // 内发光（左上角亮边）
        Color innerGlow = new(255, 255, 255, 30);
        sb.Draw(pixel, new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, 1), innerGlow);
        sb.Draw(pixel, new Rectangle(rect.X + 3, rect.Y + 3, 1, rect.Height - 6), innerGlow);
    }

    /// <summary>
    /// 绘制物品图标（带数量）
    /// </summary>
    private static void DrawItemIcon(SpriteBatch sb, Item item, Rectangle rect, float parentAlpha)
    {
        // 计算物品图标绘制区域（内边距4像素）
        int padding = 4;
        var iconRect = new Rectangle(rect.X + padding, rect.Y + padding, rect.Width - padding * 2, rect.Height - padding * 2);

        // 获取物品纹理
        Texture2D itemTexture = TextureAssets.Item[item.type].Value;

        // 计算缩放以适配格子
        float scale = 1f;
        if (itemTexture.Width > iconRect.Width || itemTexture.Height > iconRect.Height)
        {
            float scaleX = (float)iconRect.Width / itemTexture.Width;
            float scaleY = (float)iconRect.Height / itemTexture.Height;
            scale = Math.Min(scaleX, scaleY);
        }

        int drawW = (int)(itemTexture.Width * scale);
        int drawH = (int)(itemTexture.Height * scale);
        var drawPos = new Vector2(iconRect.Center.X - drawW / 2f, iconRect.Center.Y - drawH / 2f);

        // 绘制物品纹理
        sb.Draw(itemTexture, new Rectangle((int)drawPos.X, (int)drawPos.Y, drawW, drawH), item.GetAlpha(Color.White * parentAlpha));

        // 绘制物品边框光效
        if (item.rare > 0)
        {
            Color rareColor = item.GetAlpha(Lighting.GetColor((int)Main.MouseScreen.X / 16, (int)Main.MouseScreen.Y / 16));
            // 简单稀有度边框
        }

        // 绘制数量（始终显示在右下角，不超出格子边界）
        {
            string stackText = item.stack.ToString();
            var font = FontAssets.MouseText.Value;

            // 根据格子大小自适应字号，确保不超出格子
            float fontSize = Math.Min(rect.Width, rect.Height) / 22f;
            fontSize = Math.Clamp(fontSize, 0.5f, 0.85f);

            var textSize = font.MeasureString(stackText) * fontSize;

            // 限制文本宽度不超过格子宽度的 40%，高度不超过格子高度的 30%
            float maxTextW = rect.Width * 0.4f;
            float maxTextH = rect.Height * 0.3f;
            if (textSize.X > maxTextW || textSize.Y > maxTextH)
            {
                float scaleW = maxTextW / textSize.X;
                float scaleH = maxTextH / textSize.Y;
                fontSize *= Math.Min(scaleW, scaleH);
                textSize = font.MeasureString(stackText) * fontSize;
            }

            // 右下角位置，留 2px 内边距
            var textPos = new Vector2(rect.Right - textSize.X - 2, rect.Bottom - textSize.Y - 1);

            // 绘制带阴影的数字，确保可读性
            Utils.DrawBorderString(sb, stackText, textPos + Vector2.One, new Color(0, 0, 0, 180) * parentAlpha, fontSize);
            Utils.DrawBorderString(sb, stackText, textPos, Color.White * parentAlpha, fontSize);
        }
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
}
