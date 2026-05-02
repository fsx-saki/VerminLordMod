// ============================================================
// SearchPromptUI - 搜索提示 UI
// 玩家靠近可搜索目标时，在目标上方显示"搜索"按钮
// 基于 SimpleLightBox 实现
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using VerminLordMod.Common.Search;
using VerminLordMod.Common.UI.SimpleUI;

namespace VerminLordMod.Common.UI.SearchUI;

/// <summary>
/// 搜索提示 UI — 靠近目标时显示"搜索"按钮
/// </summary>
public class SearchPromptUI
{
    // ===== 布局常量 =====
    private const int PromptWidth = 140;
    private const int PromptHeight = 50;
    private const int OffsetY = 30; // 目标上方偏移

    // ===== UI 组件 =====
    private SimpleLightBox? _lightBox;
    private bool _initialized;

    /// <summary> 当前提示关联的搜索目标 </summary>
    public ISearchable? CurrentTarget { get; private set; }

    /// <summary> 提示是否可见 </summary>
    public bool IsVisible => _lightBox != null && _lightBox.IsVisible;

    /// <summary> 搜索按钮被点击的回调 </summary>
    public System.Action? OnSearchClicked;

    // ============================================================
    // 初始化
    // ============================================================

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _lightBox = new SimpleLightBox(PromptWidth, PromptHeight);

        var subPanel = new SimpleSubPanel
        {
            RelativeX = 0f,
            RelativeY = 0f,
            RelativeWidth = 1f,
            RelativeHeight = 1f,
            Title = ""
        };

        var btnGroup = new SimpleFixedGroup { Spacing = 0 };

        btnGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Button,
            RelX = 0f,
            RelY = 0f,
            Width = 90,
            Height = 32,
            ButtonText = "搜索",
            ButtonClick = () =>
            {
                OnSearchClicked?.Invoke();
            }
        });

        btnGroup.CalculateSize();

        subPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.FixedGroup,
            RelX = 0.5f,
            RelY = 0.5f,
            AnchorX = ElementAnchor.Center,
            AnchorY = ElementAnchor.Center,
            Width = btnGroup.Width,
            Height = btnGroup.Height,
            FixedGroup = btnGroup
        });

        _lightBox.SubPanels.Add(subPanel);
    }

    // ============================================================
    // 生命周期
    // ============================================================

    /// <summary>
    /// 显示搜索提示
    /// </summary>
    public void Show(ISearchable target, Vector2 screenPos)
    {
        Initialize();

        CurrentTarget = target;

        // 定位在目标上方
        int x = (int)screenPos.X - PromptWidth / 2;
        int y = (int)screenPos.Y - PromptHeight - OffsetY;

        x = System.Math.Clamp(x, 10, Main.screenWidth - PromptWidth - 10);
        y = System.Math.Clamp(y, 10, Main.screenHeight - PromptHeight - 10);

        if (_lightBox != null)
        {
            _lightBox.BoxRect = new Rectangle(x, y, PromptWidth, PromptHeight);
            if (!_lightBox.IsVisible)
                _lightBox.Open();
        }
    }

    /// <summary>
    /// 隐藏搜索提示
    /// </summary>
    public void Hide()
    {
        if (_lightBox != null && _lightBox.IsVisible)
            _lightBox.Close();

        CurrentTarget = null;
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Update()
    {
        _lightBox?.Update();
    }

    /// <summary>
    /// 绘制
    /// </summary>
    public void Draw(SpriteBatch sb)
    {
        if (_lightBox != null && _lightBox.IsVisible)
            _lightBox.Draw(sb);
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Unload()
    {
        _lightBox = null;
        _initialized = false;
        CurrentTarget = null;
    }
}
