// ============================================================
// SearchChairHandler - 搜索椅 UI 管理器
// 管理信息提示框、搜索面板和动画格子行的生命周期
// 由 SearchChairTile.MouseOver 触发
// ============================================================
#nullable enable
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using VerminLordMod.Common.UI.SimpleUI;

namespace VerminLordMod.Content.Items.Debuggers.SearchChair;

/// <summary>
/// 搜索状态
/// </summary>
public enum SearchState
{
    /// <summary> 未开始搜索 </summary>
    NotStarted,
    /// <summary> 搜索中（动画播放中） </summary>
    Searching,
    /// <summary> 搜索完成 </summary>
    Complete
}

/// <summary>
/// 搜索椅 UI 管理器 — 静态持有所有 UI 实例
/// </summary>
public static class SearchChairHandler
{
    /// <summary> 轻量级搜索提示框实例（无标题栏） </summary>
    private static SimpleLightBox? _searchPrompt;

    /// <summary> 搜索面板实例 </summary>
    private static SimplePanel? _searchPanel;

    /// <summary> 动画格子行实例 </summary>
    private static SimpleAnimSlotRow? _animSlotRow;

    /// <summary> 是否已初始化 </summary>
    private static bool _initialized;

    /// <summary> 上次触发 OnPlayerNearChair 的时间（毫秒） </summary>
    private static int _lastTriggerTime;

    /// <summary> 当前搜索状态 </summary>
    public static SearchState CurrentState { get; private set; } = SearchState.NotStarted;

    /// <summary> 椅子在屏幕上的位置（用于 UI 定位） </summary>
    private static Vector2 _chairScreenPos;

    /// <summary> 搜索提示框的尺寸 </summary>
    private const int PromptWidth = 160;
    private const int PromptHeight = 60;

    /// <summary> 搜索面板的尺寸 </summary>
    private const int PanelWidth = 320;
    private const int PanelHeight = 200;

    /// <summary> 离开超时时间（毫秒） </summary>
    private const int LeaveTimeout = 300;

    /// <summary>
    /// 初始化 UI 实例
    /// </summary>
    private static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // ===== 搜索提示框（轻量级，无标题栏，只有一个"搜索"按钮） =====
        _searchPrompt = new SimpleLightBox(PromptWidth, PromptHeight);

        var promptSubPanel = new SimpleSubPanel
        {
            RelativeX = 0f,
            RelativeY = 0f,
            RelativeWidth = 1f,
            RelativeHeight = 1f,
            Title = ""
        };

        // "搜索"按钮，整体居中
        var btnFixedGroup = new SimpleFixedGroup
        {
            Spacing = 0
        };

        btnFixedGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Button,
            RelX = 0f,
            RelY = 0f,
            Width = 100,
            Height = 36,
            ButtonText = "搜索",
            ButtonClick = () =>
            {
                // 点击搜索按钮：关闭提示框，打开搜索面板
                _searchPrompt?.Close();
                _searchPanel?.Open();
                _animSlotRow?.Play();
                CurrentState = SearchState.Searching;
                Main.NewText("[搜索椅] 开始搜索...", new Color(200, 160, 255));
            }
        });

        btnFixedGroup.CalculateSize();

        promptSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.FixedGroup,
            RelX = 0.5f,
            RelY = 0.5f,
            AnchorX = ElementAnchor.Center,
            AnchorY = ElementAnchor.Center,
            Width = btnFixedGroup.Width,
            Height = btnFixedGroup.Height,
            FixedGroup = btnFixedGroup
        });

        _searchPrompt.SubPanels.Add(promptSubPanel);

        // ===== 搜索面板（播放搜索动画） =====
        _animSlotRow = new SimpleAnimSlotRow();
        _animSlotRow.OnComplete = () =>
        {
            CurrentState = SearchState.Complete;
            Main.NewText("[搜索椅] 搜索完成！", new Color(200, 160, 255));
        };

        _searchPanel = new SimplePanel(PanelWidth, PanelHeight)
        {
            Title = "搜索中..."
        };

        var searchSubPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.05f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.9f,
            Title = ""
        };

        // 动画格子行
        searchSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.AnimSlotRow,
            RelX = 0f,
            RelY = 0f,
            Width = 1,
            Height = 1,
            AnimRow = _animSlotRow
        });

        _searchPanel.SubPanels.Add(searchSubPanel);
    }

    /// <summary>
    /// 玩家靠近椅子时调用（由 SearchChairTile.MouseOver 触发）
    /// </summary>
    /// <param name="tileScreenPos">椅子 Tile 在屏幕上的坐标</param>
    public static void OnPlayerNearChair(Vector2 tileScreenPos)
    {
        if (Main.dedServ) return;

        Initialize();

        // 更新椅子屏幕位置
        _chairScreenPos = tileScreenPos;

        // 记录触发时间
        _lastTriggerTime = (int)(Main.gameTimeCache.TotalGameTime.TotalMilliseconds);

        // 根据状态展示对应 UI
        switch (CurrentState)
        {
            case SearchState.NotStarted:
                // 未开始搜索 → 显示搜索提示框
                PositionAboveTile(_searchPrompt, PromptWidth, PromptHeight);
                if (_searchPrompt != null && !_searchPrompt.IsVisible)
                {
                    _searchPrompt.Open();
                }
                break;

            case SearchState.Searching:
                // 搜索中 → 显示搜索面板
                PositionAboveTile(_searchPanel, PanelWidth, PanelHeight);
                if (_searchPanel != null && !_searchPanel.IsVisible)
                {
                    _searchPanel.Open();
                }
                break;

            case SearchState.Complete:
                // 搜索完成 → 显示搜索面板（结果）
                PositionAboveTile(_searchPanel, PanelWidth, PanelHeight);
                if (_searchPanel != null && !_searchPanel.IsVisible)
                {
                    _searchPanel.Open();
                }
                break;
        }
    }

    /// <summary>
    /// 将 UI 窗口定位在椅子 Tile 上方
    /// </summary>
    private static void PositionAboveTile(object uiObj, int width, int height)
    {
        // 椅子 Tile 上方 20px 处居中
        int x = (int)_chairScreenPos.X - width / 2;
        int y = (int)_chairScreenPos.Y - height - 20;

        // 确保不超出屏幕边界
        x = Math.Clamp(x, 10, Main.screenWidth - width - 10);
        y = Math.Clamp(y, 10, Main.screenHeight - height - 10);

        if (uiObj is SimpleLightBox lightBox)
        {
            lightBox.BoxRect = new Rectangle(x, y, width, height);
        }
        else if (uiObj is SimplePanel panel)
        {
            panel.PanelRect = new Rectangle(x, y, width, height);
        }
    }

    /// <summary>
    /// 每帧更新（由 SimpleUISystem 调用）
    /// </summary>
    public static void Update()
    {
        if (!_initialized) return;

        _searchPrompt?.Update();
        _searchPanel?.Update();

        // 检查玩家是否远离椅子
        int now = (int)(Main.gameTimeCache.TotalGameTime.TotalMilliseconds);
        bool playerLeft = (now - _lastTriggerTime) > LeaveTimeout;

        if (playerLeft)
        {
            // 玩家远离 → 打断搜索，隐藏所有窗体
            if (_searchPrompt != null && _searchPrompt.IsVisible)
            {
                _searchPrompt.Close();
            }

            if (_searchPanel != null && _searchPanel.IsVisible)
            {
                _searchPanel.Close();
            }

            // 如果正在搜索中，打断搜索（重置动画）
            if (CurrentState == SearchState.Searching)
            {
                _animSlotRow?.Reset();
                CurrentState = SearchState.NotStarted;
            }
        }
    }

    /// <summary>
    /// 每帧绘制（由 SimpleUISystem 调用）
    /// </summary>
    public static void Draw(SpriteBatch sb)
    {
        if (!_initialized) return;

        if (_searchPrompt != null && _searchPrompt.IsVisible)
        {
            _searchPrompt.Draw(sb);
        }

        if (_searchPanel != null && _searchPanel.IsVisible)
        {
            _searchPanel.Draw(sb);
        }
    }

    /// <summary>
    /// 清理静态引用
    /// </summary>
    public static void Unload()
    {
        _searchPrompt = null;
        _searchPanel = null;
        _animSlotRow = null;
        _initialized = false;
        CurrentState = SearchState.NotStarted;
    }
}
