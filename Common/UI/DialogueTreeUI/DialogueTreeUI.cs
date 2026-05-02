// ============================================================
// DialogueTreeUI - 对话树自定义UI面板
// 使用 SimplePanel 框架，展示NPC文本和所有选项按钮
// 突破原版2按钮限制，支持任意数量的选项
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.DialogueTreeUI;

/// <summary>
/// 对话树UI — 自定义对话界面，支持任意数量选项
/// 当对话树激活时，替代原版2按钮对话系统
/// </summary>
public class DialogueTreeUI
{
    // ===== 单例 =====
    public static DialogueTreeUI Instance { get; } = new();

    // ===== 状态 =====
    private bool _isOpen;
    private string _npcText = "";
    private List<DialogueOption> _options = new();
    private string _npcName = "";
    private int _npcHeadType = -1;

    // ===== 布局 =====
    private Rectangle _panelRect;
    private Rectangle _textRect;
    private readonly List<OptionButton> _optionButtons = new();

    // ===== 滚动 =====
    private float _scrollOffset;
    private const float ScrollSpeed = 40f;

    // ===== 颜色主题（青绿色，与SimplePanel的淡紫色区分） =====
    private static readonly Color PanelBg = new(30, 40, 60, 230);
    private static readonly Color TitleBarBg = new(50, 70, 100, 240);
    private static readonly Color BorderColor = new(80, 120, 180, 200);
    private static readonly Color TextBg = new(20, 30, 50, 200);
    private static readonly Color TextColor = new(220, 230, 255, 255);
    private static readonly Color OptionBgNormal = new(50, 80, 120, 200);
    private static readonly Color OptionBgHover = new(70, 110, 160, 220);
    private static readonly Color OptionTextColor = new(230, 240, 255, 255);
    private static readonly Color ScrollbarBg = new(40, 50, 70, 180);
    private static readonly Color ScrollbarThumb = new(80, 120, 180, 200);
    private static readonly Color CloseBtnColor = new(200, 80, 80, 200);
    private static readonly Color CloseBtnHover = new(255, 100, 100, 240);

    // ===== 尺寸常量 =====
    private const int PanelWidth = 480;
    private const int PanelHeight = 420;
    private const int TitleBarHeight = 32;
    private const int TextAreaHeight = 120;
    private const int OptionHeight = 36;
    private const int OptionSpacing = 4;
    private const int Padding = 10;
    private const int CloseBtnSize = 22;
    private const int CloseBtnMargin = 6;

    // ===== 鼠标状态 =====
    private bool _lastMouseLeft;
    private bool _closeBtnHovered;
    private bool _closeBtnPressed;
    private bool _isDragging;
    private Vector2 _dragOffset;

    // ===== 滚轮状态 =====
    private int _lastScrollValue;

    // ===== ESC检测 =====
    private bool _escapeWasDown;

    // ============================================================
    // 打开/关闭
    // ============================================================

    /// <summary>
    /// 打开对话树UI
    /// </summary>
    public void Open(string npcName, int npcHeadType, string text, List<DialogueOption> options)
    {
        _npcName = npcName;
        _npcHeadType = npcHeadType;
        _npcText = text;
        _options = options ?? new List<DialogueOption>();
        _scrollOffset = 0;
        _lastScrollValue = Mouse.GetState().ScrollWheelValue;
        _escapeWasDown = false;
        _isOpen = true;

        // 计算面板位置（屏幕居中）
        int x = (Main.screenWidth - PanelWidth) / 2;
        int y = (Main.screenHeight - PanelHeight) / 2;
        _panelRect = new Rectangle(x, y, PanelWidth, PanelHeight);

        // 计算文本区域
        _textRect = new Rectangle(
            _panelRect.X + Padding,
            _panelRect.Y + TitleBarHeight + Padding,
            _panelRect.Width - Padding * 2,
            TextAreaHeight
        );

        RebuildOptionButtons();
    }

    /// <summary>
    /// 关闭对话树UI
    /// </summary>
    public void Close()
    {
        _isOpen = false;
        _options.Clear();
        _optionButtons.Clear();
        _npcText = "";
    }

    /// <summary>
    /// 刷新UI内容（选择选项后调用）
    /// </summary>
    public void Refresh(string text, List<DialogueOption> options)
    {
        _npcText = text ?? "";
        _options = options ?? new List<DialogueOption>();
        _scrollOffset = 0;
        RebuildOptionButtons();

        // 如果没有可见选项，自动结束对话
        if (_options.Count == 0 && _optionButtons.Count == 0)
        {
            Main.NewText("[对话树] 没有可用选项，对话结束", Color.Yellow);
            CloseDialogue();
        }
    }

    /// <summary>
    /// UI是否打开
    /// </summary>
    public bool IsOpen => _isOpen;

    // ============================================================
    // 选项按钮重建
    // ============================================================

    private void RebuildOptionButtons()
    {
        _optionButtons.Clear();

        int startY = _textRect.Bottom + Padding;
        int btnWidth = _panelRect.Width - Padding * 2 - 16; // 留出滚动条空间

        for (int i = 0; i < _options.Count; i++)
        {
            var option = _options[i];
            int y = startY + i * (OptionHeight + OptionSpacing);
            var rect = new Rectangle(
                _panelRect.X + Padding,
                y,
                btnWidth,
                OptionHeight
            );

            string typeTag = GetOptionTypeTag(option.OptionType);
            string displayText = string.IsNullOrEmpty(typeTag)
                ? option.Text
                : $"[{typeTag}] {option.Text}";

            _optionButtons.Add(new OptionButton
            {
                Rect = rect,
                Text = displayText,
                Tooltip = option.Tooltip,
                Index = i,
                OptionType = option.OptionType
            });
        }
    }

    private static string GetOptionTypeTag(DialogueOptionType type)
    {
        return type switch
        {
            DialogueOptionType.Informative => "信息",
            DialogueOptionType.Risky => "冒险",
            DialogueOptionType.Trade => "交易",
            DialogueOptionType.Special => "特殊",
            DialogueOptionType.Exit => "退出",
            DialogueOptionType.Combat => "战斗",
            DialogueOptionType.Barter => "议价",
            DialogueOptionType.Quest => "任务",
            DialogueOptionType.Social => "社交",
            DialogueOptionType.Craft => "炼制",
            DialogueOptionType.Teach => "教学",
            DialogueOptionType.Steal => "偷窃",
            DialogueOptionType.Deceive => "欺骗",
            DialogueOptionType.Ally => "结盟",
            DialogueOptionType.Betray => "背叛",
            _ => ""
        };
    }

    /// <summary>
    /// 获取选项类型对应的颜色
    /// </summary>
    private static Color GetOptionTypeColor(DialogueOptionType type)
    {
        return type switch
        {
            DialogueOptionType.Normal => new Color(230, 240, 255),       // 白色
            DialogueOptionType.Informative => new Color(100, 180, 255),  // 蓝色
            DialogueOptionType.Risky => new Color(255, 120, 100),       // 红色
            DialogueOptionType.Trade => new Color(255, 215, 0),         // 金色
            DialogueOptionType.Special => new Color(200, 130, 255),     // 紫色
            DialogueOptionType.Exit => new Color(160, 160, 160),        // 灰色
            DialogueOptionType.Combat => new Color(200, 50, 50),        // 深红
            DialogueOptionType.Barter => new Color(255, 165, 0),        // 橙色
            DialogueOptionType.Quest => new Color(0, 255, 200),         // 青色
            DialogueOptionType.Social => new Color(255, 150, 200),      // 粉色
            DialogueOptionType.Craft => new Color(100, 255, 100),       // 绿色
            DialogueOptionType.Teach => new Color(100, 200, 255),       // 亮蓝
            DialogueOptionType.Steal => new Color(140, 80, 200),        // 暗紫
            DialogueOptionType.Deceive => new Color(180, 60, 60),       // 暗红
            DialogueOptionType.Ally => new Color(255, 215, 0),          // 亮金
            DialogueOptionType.Betray => new Color(180, 150, 50),       // 暗金
            _ => new Color(230, 240, 255)
        };
    }

    /// <summary>
    /// 获取选项类型对应的背景色
    /// </summary>
    private static Color GetOptionBgColor(DialogueOptionType type, bool hovered)
    {
        Color baseColor = type switch
        {
            DialogueOptionType.Combat => new Color(80, 30, 30),
            DialogueOptionType.Barter => new Color(70, 50, 20),
            DialogueOptionType.Quest => new Color(20, 60, 50),
            DialogueOptionType.Social => new Color(60, 30, 50),
            DialogueOptionType.Craft => new Color(30, 60, 30),
            DialogueOptionType.Teach => new Color(20, 50, 70),
            DialogueOptionType.Steal => new Color(50, 30, 70),
            DialogueOptionType.Deceive => new Color(70, 30, 30),
            DialogueOptionType.Ally => new Color(60, 50, 20),
            DialogueOptionType.Betray => new Color(60, 50, 20),
            _ => new Color(50, 80, 120)
        };

        return hovered
            ? new Color(
                System.Math.Min(baseColor.R + 30, 255),
                System.Math.Min(baseColor.G + 30, 255),
                System.Math.Min(baseColor.B + 30, 255),
                220)
            : new Color(baseColor.R, baseColor.G, baseColor.B, 200);
    }

    // ============================================================
    // 更新
    // ============================================================

    public void Update()
    {
        if (!_isOpen) return;

        var mousePos = Main.MouseScreen;
        bool currentLeft = Main.mouseLeft;
        bool leftClick = currentLeft && !_lastMouseLeft;

        // 标记鼠标被UI使用，防止游戏隐藏鼠标指针
        Main.LocalPlayer.mouseInterface = true;

        // 检测ESC关闭
        if (Main.keyState.IsKeyDown(Keys.Escape) && !_escapeWasDown)
        {
            _escapeWasDown = true;
            CloseDialogue();
            _lastMouseLeft = currentLeft;
            return;
        }
        _escapeWasDown = Main.keyState.IsKeyDown(Keys.Escape);

        // 检查关闭按钮
        var closeRect = GetCloseButtonRect();
        _closeBtnHovered = closeRect.Contains(mousePos.ToPoint());

        if (_closeBtnHovered && leftClick)
        {
            _closeBtnPressed = true;
            CloseDialogue();
            _lastMouseLeft = currentLeft;
            return;
        }
        _closeBtnPressed = false;

        // 拖动标题栏
        UpdateDragging(mousePos, currentLeft, leftClick);

        // 滚轮滚动
        if (_panelRect.Contains(mousePos.ToPoint()))
        {
            int currentScroll = Mouse.GetState().ScrollWheelValue;
            int scrollDelta = currentScroll - _lastScrollValue;
            _lastScrollValue = currentScroll;
            if (scrollDelta != 0)
            {
                float maxScroll = Math.Max(0, _options.Count * (OptionHeight + OptionSpacing) - GetOptionsAreaHeight());
                _scrollOffset = Math.Clamp(_scrollOffset - Math.Sign(scrollDelta) * ScrollSpeed, 0, maxScroll);
            }
        }

        // 选项按钮点击
        foreach (var btn in _optionButtons)
        {
            var btnRect = btn.Rect;
            btnRect.Y -= (int)_scrollOffset;

            if (btnRect.Contains(mousePos.ToPoint()) && leftClick)
            {
                OnOptionClicked(btn.Index);
                break;
            }
        }

        _lastMouseLeft = currentLeft;
    }

    private void UpdateDragging(Vector2 mousePos, bool currentLeft, bool leftClick)
    {
        var titleRect = GetTitleBarRect();

        if (currentLeft)
        {
            if (!_isDragging)
            {
                if (titleRect.Contains(mousePos.ToPoint()) && !GetCloseButtonRect().Contains(mousePos.ToPoint()))
                {
                    _isDragging = true;
                    _dragOffset = new Vector2(_panelRect.X - mousePos.X, _panelRect.Y - mousePos.Y);
                }
            }
            else
            {
                int newX = (int)(mousePos.X + _dragOffset.X);
                int newY = (int)(mousePos.Y + _dragOffset.Y);
                newX = Math.Clamp(newX, -_panelRect.Width + 50, Main.screenWidth - 50);
                newY = Math.Clamp(newY, 0, Main.screenHeight - 50);
                _panelRect = new Rectangle(newX, newY, _panelRect.Width, _panelRect.Height);
                RecalculateRects();
            }
        }
        else
        {
            _isDragging = false;
        }
    }

    private void RecalculateRects()
    {
        _textRect = new Rectangle(
            _panelRect.X + Padding,
            _panelRect.Y + TitleBarHeight + Padding,
            _panelRect.Width - Padding * 2,
            TextAreaHeight
        );
        RebuildOptionButtons();
    }

    private void OnOptionClicked(int index)
    {
        try
        {
            if (index < 0 || index >= _options.Count) return;

            var manager = DialogueTreeManager.Instance;
            if (!manager.CurrentSession.IsActive) return;

            var option = _options[index];

            // 处理商店：关闭自定义UI，原版对话仍然打开，设置商店名即可
            if (option.OpensShop != null)
            {
                var npc = manager.CurrentSession.CurrentNPC;
                if (npc != null && npc.active)
                {
                    Close();
                    Main.npcChatText = option.OpensShop;
                }
                return;
            }

            // 执行选项选择
            manager.SelectOption(index);

            // 检查对话是否结束
            if (!manager.CurrentSession.IsActive)
            {
                Close();
                // 对话结束，关闭原版对话
                if (Main.LocalPlayer != null)
                {
                    Main.LocalPlayer.SetTalkNPC(-1);
                }
                return;
            }

            // 刷新UI
            string newText = manager.GetCurrentNPCText();
            var newOptions = manager.GetCurrentOptions();
            Refresh(newText, newOptions);
        }
        catch (Exception ex)
        {
            Main.NewText($"[DialogueTreeUI Option Error] {ex.Message}", Color.Red);
        }
    }

    private void CloseDialogue()
    {
        DialogueTreeManager.Instance.EndDialogue();
        Close();
        // 完全关闭对话界面，确保玩家可以重新右键点击NPC
        // 注意：只调用 SetTalkNPC(-1) 来关闭原版对话
        // 不要调用 CloseSign()，它是用于关闭告示牌/箱子的，会干扰NPC交互
        Main.npcChatText = "";
        if (Main.LocalPlayer != null)
        {
            Main.LocalPlayer.SetTalkNPC(-1);
        }
    }

    // ============================================================
    // 绘制
    // ============================================================

    public void Draw(SpriteBatch sb)
    {
        if (!_isOpen) return;

        try
        {
            var pixel = UIRendering.Pixel;

            // ---- 面板背景 ----
            sb.Draw(pixel, _panelRect, PanelBg);

            // ---- 标题栏 ----
            var titleRect = GetTitleBarRect();
            sb.Draw(pixel, titleRect, TitleBarBg);

            // NPC名称
            Utils.DrawBorderString(sb, _npcName, new Vector2(_panelRect.X + 10, titleRect.Y + 4), Color.White, 0.85f);

            // ---- NPC文本区域 ----
            sb.Draw(pixel, _textRect, TextBg);

            // NPC文本（简单绘制，不换行）
            Utils.DrawBorderString(sb, _npcText, new Vector2(_textRect.X + 4, _textRect.Y + 4), TextColor, 0.8f);

            // ---- 选项按钮（使用与Update一致的坐标） ----
            foreach (var btn in _optionButtons)
            {
                var drawRect = btn.Rect;
                drawRect.Y -= (int)_scrollOffset;

                // 只绘制在可见区域内的按钮
                if (drawRect.Bottom < _textRect.Bottom + Padding || drawRect.Top > _panelRect.Bottom)
                    continue;

                // 检测悬停
                bool hovered = drawRect.Contains(Main.MouseScreen.ToPoint());
                Color btnColor = GetOptionBgColor(btn.OptionType, hovered);
                Color textColor = GetOptionTypeColor(btn.OptionType);

                sb.Draw(pixel, drawRect, btnColor);
                Utils.DrawBorderString(sb, btn.Text, new Vector2(drawRect.X + 4, drawRect.Y + 4), textColor, 0.8f);
            }

            // ---- 关闭按钮 ----
            DrawCloseButton(sb);

            // ---- 滚动条（选项过多时显示） ----
            int totalOptionHeight = _options.Count * (OptionHeight + OptionSpacing);
            int optionsAreaHeight = GetOptionsAreaHeight();
            if (totalOptionHeight > optionsAreaHeight)
            {
                var scrollArea = new Rectangle(
                    _panelRect.X + Padding,
                    _textRect.Bottom + Padding,
                    _panelRect.Width - Padding * 2,
                    optionsAreaHeight
                );
                UIRendering.DrawScrollbar(sb, scrollArea, _scrollOffset, totalOptionHeight, ScrollbarBg, ScrollbarThumb);
            }
        }
        catch (Exception ex)
        {
            Main.NewText($"[DialogueTreeUI Draw Error] {ex.Message}", Color.Red);
        }
    }

    // ============================================================
    // 辅助方法
    // ============================================================

    private Rectangle GetTitleBarRect()
    {
        return new Rectangle(_panelRect.X, _panelRect.Y, _panelRect.Width, TitleBarHeight);
    }

    private Rectangle GetCloseButtonRect()
    {
        int btnX = _panelRect.Right - CloseBtnSize - CloseBtnMargin;
        int btnY = _panelRect.Y + (TitleBarHeight - CloseBtnSize) / 2;
        return new Rectangle(btnX, btnY, CloseBtnSize, CloseBtnSize);
    }

    private int GetOptionsAreaHeight()
    {
        return _panelRect.Height - TitleBarHeight - Padding - TextAreaHeight - Padding * 2;
    }

    private void DrawCloseButton(SpriteBatch sb)
    {
        var rect = GetCloseButtonRect();
        Color bgColor = _closeBtnPressed ? CloseBtnHover : (_closeBtnHovered ? CloseBtnHover : CloseBtnColor);
        UIRendering.DrawCloseButton(sb, rect, bgColor, Color.White * 0.9f);
    }

    // ============================================================
    // 内部类
    // ============================================================

    private class OptionButton
    {
        public Rectangle Rect;
        public string Text;
        public string Tooltip;
        public int Index;
        public DialogueOptionType OptionType;
    }
}
