// ============================================================
// SearchResultUI - 搜索结果 UI
// 搜索完成后显示获得的物品列表
// 复用 CorpseLootUI 的格子布局风格
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Search;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.SearchUI;

/// <summary>
/// 搜索结果 UI — 显示搜索获得的物品
/// </summary>
public class SearchResultUI
{
    // ===== 布局常量 =====
    private const int SlotSize = 40;
    private const int SlotPadding = 4;
    private const int MaxSlotsPerRow = 6;
    private const int PanelPadding = 8;
    private const int TitleHeight = 20;
    private const int TakeAllBtnHeight = 22;
    private const int OffsetY = 30;

    // ===== 状态 =====
    private List<Item> _cachedItems = new();
    private bool _isOpen;
    private Vector2 _worldPosition;

    // 按钮区域定义（用于点击检测）
    private Rectangle _takeAllRect;
    private readonly List<Rectangle> _slotRects = new();
    private readonly List<int> _slotIndices = new();

    // 鼠标状态追踪（上升沿检测）
    private bool _lastMouseLeftRelease = true;

    /// <summary> 全部拾取回调 </summary>
    public System.Action<List<Item>>? OnTakeAll;

    /// <summary> 拾取单个物品回调（参数：物品索引） </summary>
    public System.Action<int>? OnTakeItem;

    /// <summary> UI 是否打开 </summary>
    public bool IsOpen => _isOpen;

    // ============================================================
    // 生命周期
    // ============================================================

    /// <summary>
    /// 显示搜索结果
    /// </summary>
    public void Show(SearchResult result, Vector2 worldPos)
    {
        _isOpen = true;
        _worldPosition = worldPos;
        _cachedItems = new List<Item>(result.Loot);
        _slotRects.Clear();
        _slotIndices.Clear();
    }

    /// <summary>
    /// 关闭搜索结果 UI
    /// </summary>
    public void Close()
    {
        _isOpen = false;
        _cachedItems.Clear();
        _slotRects.Clear();
        _slotIndices.Clear();
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Update()
    {
        if (!_isOpen) return;

        // 如果没有物品了，自动关闭
        if (_cachedItems.Count == 0)
        {
            Close();
            return;
        }

        // 点击检测（上升沿）
        bool currentRelease = Main.mouseLeftRelease;
        bool leftClicked = !currentRelease && _lastMouseLeftRelease;
        _lastMouseLeftRelease = currentRelease;

        if (leftClicked)
        {
            Point mouse = new(Main.mouseX, Main.mouseY);

            // 检测"全部拾取"按钮
            if (_takeAllRect.Contains(mouse))
            {
                OnTakeAll?.Invoke(new List<Item>(_cachedItems));
                Close();
                return;
            }

            // 检测物品槽位
            for (int i = 0; i < _slotRects.Count; i++)
            {
                if (_slotRects[i].Contains(mouse))
                {
                    OnTakeItem?.Invoke(_slotIndices[i]);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 绘制
    /// </summary>
    public void Draw(SpriteBatch sb)
    {
        if (!_isOpen) return;

        int itemCount = _cachedItems.Count;
        if (itemCount == 0) return;

        int cols = System.Math.Min(itemCount, MaxSlotsPerRow);
        int rows = (itemCount + MaxSlotsPerRow - 1) / MaxSlotsPerRow;

        int panelWidth = cols * (SlotSize + SlotPadding) - SlotPadding + PanelPadding * 2;
        int panelHeight = TitleHeight + TakeAllBtnHeight + PanelPadding + rows * (SlotSize + SlotPadding) - SlotPadding + PanelPadding;

        // 计算面板位置：目标中心上方
        Vector2 screenPos = _worldPosition - Main.screenPosition;
        float panelLeft = screenPos.X - panelWidth / 2f;
        float panelTop = screenPos.Y - panelHeight - OffsetY;

        panelLeft = System.Math.Clamp(panelLeft, 10, Main.screenWidth - panelWidth - 10);
        panelTop = System.Math.Clamp(panelTop, 10, Main.screenHeight - panelHeight - 10);

        Rectangle panelRect = new((int)panelLeft, (int)panelTop, panelWidth, panelHeight);

        // 面板背景
        sb.Draw(TextureAssets.MagicPixel.Value, panelRect, UIStyles.PanelBg);

        // 面板边框
        UIRendering.DrawBorder(sb, panelRect, 1, UIStyles.Border);

        // 标题
        string titleText = $"搜索收获 ({itemCount})";
        Vector2 titlePos = new(panelLeft + PanelPadding, panelTop + PanelPadding);
        Utils.DrawBorderString(sb, titleText, titlePos, UIStyles.TextMain, 0.7f);

        // "全部拾取"按钮
        Rectangle takeAllRect = new(
            (int)panelLeft + PanelPadding,
            (int)panelTop + PanelPadding + TitleHeight,
            panelWidth - PanelPadding * 2,
            TakeAllBtnHeight
        );
        _takeAllRect = takeAllRect;

        bool hoverTakeAll = takeAllRect.Contains(Main.mouseX, Main.mouseY);
        sb.Draw(TextureAssets.MagicPixel.Value, takeAllRect,
            hoverTakeAll ? UIStyles.HoverOver(UIStyles.BtnPrimary) : UIStyles.BtnPrimary);
        UIRendering.DrawBorder(sb, takeAllRect, 1, UIStyles.BorderAccent);

        string takeAllText = $"全部拾取 ({itemCount})";
        Vector2 takeAllSize = FontAssets.MouseText.Value.MeasureString(takeAllText);
        Vector2 takeAllPos = new(
            takeAllRect.Center.X - takeAllSize.X * 0.35f,
            takeAllRect.Center.Y - takeAllSize.Y * 0.35f
        );
        Utils.DrawBorderString(sb, takeAllText, takeAllPos, Color.White, 0.7f, 0.5f, 0.5f);

        // 物品槽位
        _slotRects.Clear();
        _slotIndices.Clear();

        for (int i = 0; i < itemCount; i++)
        {
            int col = i % MaxSlotsPerRow;
            int row = i / MaxSlotsPerRow;
            int slotX = (int)panelLeft + PanelPadding + col * (SlotSize + SlotPadding);
            int slotY = (int)panelTop + PanelPadding + TitleHeight + TakeAllBtnHeight + PanelPadding + row * (SlotSize + SlotPadding);

            Rectangle slotRect = new(slotX, slotY, SlotSize, SlotSize);
            _slotRects.Add(slotRect);
            _slotIndices.Add(i);

            bool hoverSlot = slotRect.Contains(Main.mouseX, Main.mouseY);

            // 槽位背景
            sb.Draw(TextureAssets.MagicPixel.Value, slotRect,
                hoverSlot ? UIStyles.ListItemHover : UIStyles.ListItemBg);

            // 槽位边框
            UIRendering.DrawBorder(sb, slotRect, 1,
                hoverSlot ? UIStyles.BorderAccent : UIStyles.BorderLight);

            // 物品图标
            var item = _cachedItems[i];
            if (item != null && !item.IsAir && item.type != ItemID.None && item.type < TextureAssets.Item.Length)
            {
                UIHelper.DrawItemIcon(sb, item, slotRect);

                // 悬停显示物品名称
                if (hoverSlot)
                {
                    string itemName = item.Name;
                    if (item.stack > 1)
                        itemName += $" x{item.stack}";
                    Utils.DrawBorderString(sb, itemName,
                        new Vector2(Main.mouseX + 12, Main.mouseY - 10),
                        UIStyles.GetRarityColor(item.rare), 0.8f);
                }
            }
        }
    }

    /// <summary>
    /// 从列表中移除指定索引的物品（拾取后调用）
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _cachedItems.Count)
        {
            _cachedItems.RemoveAt(index);
            if (_cachedItems.Count == 0)
                Close();
        }
    }
}
