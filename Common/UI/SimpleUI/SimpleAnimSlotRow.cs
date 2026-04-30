// ============================================================
// SimpleAnimSlotRow - 动画格子行
// 功能：
// - 播放：依次产生 1→2→3→4 个物品栏（土块）
// - 复位：清空所有格子
// - 每个新格子从右侧放大入场，把左侧格子向左挤
// - 整体始终保持居中
// - 点击格子可取走其中的物品
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// 单个动画格子的状态
/// </summary>
internal class AnimSlotState
{
    /// <summary> 格子中的物品数量（1~4块土） </summary>
    public int StackCount { get; set; }

    /// <summary> 入场动画进度 0~1 </summary>
    public float EnterProgress { get; set; }

    /// <summary> 入场动画速度 </summary>
    public float EnterVelocity { get; set; }

    /// <summary> 是否已完成入场 </summary>
    public bool IsEntered => EnterProgress >= 1f;

    /// <summary> 缩放值（从0放大到1） </summary>
    public float Scale => EnterProgress;

    /// <summary> 水平偏移（从右侧飞入，初始偏移 = +容器半宽，最终 = 0） </summary>
    public float OffsetX { get; set; }
}

/// <summary>
/// 动画格子行 — 管理一组带入场动画的物品格子
/// </summary>
public class SimpleAnimSlotRow
{
    // ==================== 常量 ====================
    private const int SlotSize = 48;
    private const int SlotSpacing = 8;
    private const float AnimSpeed = 0.08f;
    private const float AnimDamping = 0.55f;
    private const float EnterDelay = 20f; // 每个格子入场延迟（帧数）

    // ==================== 状态 ====================
    private readonly List<AnimSlotState> _slots = new();
    private int _targetCount; // 目标格子数量
    private int _currentSpawnIndex; // 当前正在生成的格子索引
    private float _spawnTimer; // 生成计时器

    /// <summary> 鼠标左键上一帧状态（用于边缘触发检测） </summary>
    private bool _lastMouseLeft;

    /// <summary> 当前容器的矩形（用于点击检测） </summary>
    private Rectangle _currentContainerRect;

    /// <summary> 是否正在播放动画 </summary>
    public bool IsPlaying { get; private set; }

    /// <summary> 是否已完成所有格子的入场 </summary>
    public bool IsComplete => _slots.Count >= _targetCount && _slots.TrueForAll(s => s.IsEntered);

    /// <summary> 播放完成回调 </summary>
    public Action? OnComplete { get; set; }

    // ==================== 控制方法 ====================
    /// <summary>
    /// 开始播放：依次产生 1→2→3→4 个格子
    /// </summary>
    public void Play()
    {
        Reset();
        _targetCount = 4;
        _currentSpawnIndex = 0;
        _spawnTimer = 0f;
        IsPlaying = true;
    }

    /// <summary>
    /// 复位：清空所有格子
    /// </summary>
    public void Reset()
    {
        _slots.Clear();
        _targetCount = 0;
        _currentSpawnIndex = 0;
        _spawnTimer = 0f;
        IsPlaying = false;
    }

    // ==================== 更新 ====================
    /// <summary>
    /// 每帧更新动画状态和鼠标交互
    /// </summary>
    public void Update()
    {
        if (!IsPlaying && _slots.Count == 0) return;

        // 生成新格子
        if (IsPlaying && _currentSpawnIndex < _targetCount)
        {
            _spawnTimer++;
            if (_spawnTimer >= EnterDelay)
            {
                _spawnTimer = 0f;

                // 新格子初始偏移：从右侧 200px 外飞入
                var newSlot = new AnimSlotState
                {
                    StackCount = _currentSpawnIndex + 1, // 1, 2, 3, 4
                    EnterProgress = 0f,
                    EnterVelocity = 0f,
                    OffsetX = 200f
                };
                _slots.Add(newSlot);
                _currentSpawnIndex++;

                if (_currentSpawnIndex >= _targetCount)
                {
                    IsPlaying = false;
                }
            }
        }

        // 更新每个格子的入场动画
        bool allEntered = true;
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (!slot.IsEntered)
            {
                // 内联动画逻辑（避免 ref 参数问题）
                float diff = 1f - slot.EnterProgress;
                slot.EnterVelocity += diff * AnimSpeed;
                slot.EnterVelocity *= (1f - AnimDamping);
                slot.EnterProgress += slot.EnterVelocity;

                if (Math.Abs(1f - slot.EnterProgress) < 0.001f && Math.Abs(slot.EnterVelocity) < 0.001f)
                {
                    slot.EnterProgress = 1f;
                    slot.EnterVelocity = 0f;
                }

                if (!slot.IsEntered) allEntered = false;
            }
        }

        // 全部入场完成时触发回调
        if (allEntered && _slots.Count >= _targetCount)
        {
            OnComplete?.Invoke();
        }

        // ===== 鼠标点击检测：点击格子取走物品 =====
        bool currentMouseLeft = Main.mouseLeft;
        bool clicked = currentMouseLeft && !_lastMouseLeft;
        _lastMouseLeft = currentMouseLeft;

        if (clicked && _slots.Count > 0 && _currentContainerRect.Width > 0)
        {
            var mousePos = Main.MouseScreen;
            var containerRect = _currentContainerRect;

            // 计算格子位置（与 Draw 中的计算一致）
            int totalW = _slots.Count * SlotSize + (_slots.Count - 1) * SlotSpacing;
            int startX = containerRect.Center.X - totalW / 2;
            int centerY = containerRect.Center.Y;

            for (int i = _slots.Count - 1; i >= 0; i--) // 从后往前检测（上层格子优先）
            {
                var slot = _slots[i];
                if (!slot.IsEntered) continue; // 未入场完成的格子不可点击

                float slotScale = slot.Scale;
                int leftW = i * SlotSize + i * SlotSpacing;
                int slotCenterX = startX + leftW + SlotSize / 2;
                float offsetX = slot.OffsetX * (1f - slot.EnterProgress);
                float drawCenterX = slotCenterX + offsetX;

                int drawW = (int)(SlotSize * slotScale);
                int drawH = (int)(SlotSize * slotScale);

                var slotRect = new Rectangle(
                    (int)(drawCenterX - drawW / 2f),
                    centerY - drawH / 2,
                    drawW,
                    drawH
                );

                if (slotRect.Contains(mousePos.ToPoint()))
                {
                    // 点击格子：给玩家物品，移除格子
                    var dirt = new Item();
                    dirt.SetDefaults(ItemID.DirtBlock);
                    dirt.stack = slot.StackCount;
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), dirt);
                    Main.NewText($"[搜索椅] 取走了 {slot.StackCount} 块土", new Color(200, 160, 255));

                    _slots.RemoveAt(i);
                    Main.LocalPlayer.mouseInterface = true;
                    break;
                }
            }
        }
    }

    // ==================== 绘制 ====================
    /// <summary>
    /// 绘制动画格子行
    /// </summary>
    /// <param name="sb">SpriteBatch</param>
    /// <param name="containerRect">容器矩形（用于居中计算）</param>
    /// <param name="parentAlpha">父透明度</param>
    public void Draw(SpriteBatch sb, Rectangle containerRect, float parentAlpha)
    {
        // 保存容器矩形供点击检测使用
        _currentContainerRect = containerRect;

        if (_slots.Count == 0) return;

        // 计算总宽度
        int totalW = _slots.Count * SlotSize + (_slots.Count - 1) * SlotSpacing;

        // 计算起始 X（居中）
        int startX = containerRect.Center.X - totalW / 2;
        int centerY = containerRect.Center.Y;

        var pixel = TextureAssets.MagicPixel.Value;

        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];

            // 计算该格子的位置（考虑入场偏移）
            float slotScale = slot.Scale;

            // 该格子左侧所有格子的总宽度（用于计算该格子应该在哪）
            int leftW = i * SlotSize + i * SlotSpacing;
            int slotCenterX = startX + leftW + SlotSize / 2;

            // 入场偏移随进度衰减到0（从右侧飞入）
            float offsetX = slot.OffsetX * (1f - slot.EnterProgress);

            // 应用入场偏移（新格子从右侧飞入）
            float drawCenterX = slotCenterX + offsetX;

            // 缩放后的尺寸
            int drawW = (int)(SlotSize * slotScale);
            int drawH = (int)(SlotSize * slotScale);

            // 如果缩放极小则跳过绘制
            if (drawW < 2 || drawH < 2) continue;

            var drawRect = new Rectangle(
                (int)(drawCenterX - drawW / 2f),
                centerY - drawH / 2,
                drawW,
                drawH
            );

            // 格子背景
            var bgColor = new Color(80, 50, 110, 200) * parentAlpha;
            var borderColor = new Color(180, 140, 220, 220) * parentAlpha;

            sb.Draw(pixel, drawRect, bgColor);

            // 边框
            sb.Draw(pixel, new Rectangle(drawRect.X, drawRect.Y, drawRect.Width, 2), borderColor);
            sb.Draw(pixel, new Rectangle(drawRect.X, drawRect.Bottom - 2, drawRect.Width, 2), borderColor);
            sb.Draw(pixel, new Rectangle(drawRect.X, drawRect.Y + 2, 2, drawRect.Height - 4), borderColor);
            sb.Draw(pixel, new Rectangle(drawRect.Right - 2, drawRect.Y + 2, 2, drawRect.Height - 4), borderColor);

            // 内发光
            var innerGlow = new Color(255, 255, 255, 30) * parentAlpha;
            sb.Draw(pixel, new Rectangle(drawRect.X + 3, drawRect.Y + 3, drawRect.Width - 6, 1), innerGlow);
            sb.Draw(pixel, new Rectangle(drawRect.X + 3, drawRect.Y + 3, 1, drawRect.Height - 6), innerGlow);

            // 绘制土块图标
            DrawDirtIcon(sb, drawRect, slot.StackCount, parentAlpha);
        }
    }

    /// <summary>
    /// 绘制土块图标和数量
    /// </summary>
    private static void DrawDirtIcon(SpriteBatch sb, Rectangle rect, int stackCount, float parentAlpha)
    {
        // 获取土块纹理
        var itemTexture = TextureAssets.Item[ItemID.DirtBlock].Value;

        int padding = 4;
        int iconW = rect.Width - padding * 2;
        int iconH = rect.Height - padding * 2;

        if (iconW <= 0 || iconH <= 0) return;

        // 缩放适配
        float scale = 1f;
        if (itemTexture.Width > iconW || itemTexture.Height > iconH)
        {
            float sx = (float)iconW / itemTexture.Width;
            float sy = (float)iconH / itemTexture.Height;
            scale = Math.Min(sx, sy);
        }

        int drawW = (int)(itemTexture.Width * scale);
        int drawH = (int)(itemTexture.Height * scale);
        var drawPos = new Vector2(
            rect.Center.X - drawW / 2f,
            rect.Center.Y - drawH / 2f
        );

        sb.Draw(itemTexture, new Rectangle((int)drawPos.X, (int)drawPos.Y, drawW, drawH), Color.White * parentAlpha);

        // 绘制数量（右下角）
        string stackText = stackCount.ToString();
        var font = FontAssets.MouseText.Value;

        float fontSize = Math.Min(rect.Width, rect.Height) / 22f;
        fontSize = Math.Clamp(fontSize, 0.5f, 0.85f);

        var textSize = font.MeasureString(stackText) * fontSize;

        float maxTextW = rect.Width * 0.4f;
        float maxTextH = rect.Height * 0.3f;
        if (textSize.X > maxTextW || textSize.Y > maxTextH)
        {
            float scaleW = maxTextW / textSize.X;
            float scaleH = maxTextH / textSize.Y;
            fontSize *= Math.Min(scaleW, scaleH);
            textSize = font.MeasureString(stackText) * fontSize;
        }

        var textPos = new Vector2(rect.Right - textSize.X - 2, rect.Bottom - textSize.Y - 1);

        Utils.DrawBorderString(sb, stackText, textPos + Vector2.One, new Color(0, 0, 0, 180) * parentAlpha, fontSize);
        Utils.DrawBorderString(sb, stackText, textPos, Color.White * parentAlpha, fontSize);
    }

    // ==================== 动画工具 ====================
    // 注：不再使用 AnimateFloat(ref, ref, float)，改为内联以避免 List<T> 索引器的 ref 限制
}
