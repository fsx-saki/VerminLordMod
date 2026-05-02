// ============================================================
// SearchProgressUI - 搜索进度 UI
// 搜索过程中显示进度条/动画
// 基于 SimplePanel + SimpleAnimSlotRow 实现
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using VerminLordMod.Common.Search;
using VerminLordMod.Common.UI.SimpleUI;

namespace VerminLordMod.Common.UI.SearchUI;

/// <summary>
/// 搜索进度 UI — 搜索中显示进度动画
/// </summary>
public class SearchProgressUI
{
    // ===== 布局常量 =====
    private const int PanelWidth = 280;
    private const int PanelHeight = 80;

    // ===== UI 组件 =====
    private SimplePanel? _panel;
    private SimpleAnimSlotRow? _animSlotRow;
    private bool _initialized;

    /// <summary> 进度 UI 是否可见 </summary>
    public bool IsVisible => _panel != null && _panel.IsVisible;

    // ============================================================
    // 初始化
    // ============================================================

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _animSlotRow = new SimpleAnimSlotRow();

        _panel = new SimplePanel(PanelWidth, PanelHeight)
        {
            Title = "搜索中..."
        };

        var subPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.05f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.9f,
            Title = ""
        };

        subPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.AnimSlotRow,
            RelX = 0f,
            RelY = 0f,
            Width = 1,
            Height = 1,
            AnimRow = _animSlotRow
        });

        _panel.SubPanels.Add(subPanel);
    }

    // ============================================================
    // 生命周期
    // ============================================================

    /// <summary>
    /// 显示搜索进度 UI
    /// </summary>
    public void Show(Vector2 screenPos)
    {
        Initialize();

        int x = (int)screenPos.X - PanelWidth / 2;
        int y = (int)screenPos.Y - PanelHeight - 40;

        x = System.Math.Clamp(x, 10, Main.screenWidth - PanelWidth - 10);
        y = System.Math.Clamp(y, 10, Main.screenHeight - PanelHeight - 10);

        if (_panel != null)
        {
            _panel.PanelRect = new Rectangle(x, y, PanelWidth, PanelHeight);
            if (!_panel.IsVisible)
            {
                _panel.Open();
                _animSlotRow?.Play();
            }
        }
    }

    /// <summary>
    /// 隐藏搜索进度 UI
    /// </summary>
    public void Hide()
    {
        if (_panel != null && _panel.IsVisible)
            _panel.Close();

        _animSlotRow?.Reset();
    }

    /// <summary>
    /// 更新进度显示
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (_panel != null && _panel.IsVisible)
        {
            _panel.Title = $"搜索中... {(int)(progress * 100)}%";
        }
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Update()
    {
        _panel?.Update();
    }

    /// <summary>
    /// 绘制
    /// </summary>
    public void Draw(SpriteBatch sb)
    {
        if (_panel != null && _panel.IsVisible)
            _panel.Draw(sb);
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Unload()
    {
        _panel = null;
        _animSlotRow = null;
        _initialized = false;
    }
}
