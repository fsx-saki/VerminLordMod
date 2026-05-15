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

    // ===== 颜色主题（使用 UIStyles 统一风格） =====
    private static readonly Color PanelBg = UIStyles.PanelBg;
    private static readonly Color TitleBarBg = UIStyles.TitleBarBg;
    private static readonly Color BorderColor = UIStyles.Border;
    private static readonly Color TextBg = new(22, 24, 32, 200);
    private static readonly Color TextColor = UIStyles.TextMain;
    private static readonly Color OptionBgNormal = UIStyles.BtnDefault;
    private static readonly Color OptionBgHover = UIStyles.ListItemHover;
    private static readonly Color OptionTextColor = UIStyles.TextMain;
    private static readonly Color ScrollbarBg = UIStyles.ScrollbarBg;
    private static readonly Color ScrollbarThumb = UIStyles.ScrollbarThumb;
    private static readonly Color CloseBtnColor = UIStyles.BtnDanger;
    private static readonly Color CloseBtnHover = UIStyles.HoverOver(UIStyles.BtnDanger);

    // ===== 尺寸常量 =====
    private const int PanelWidth = 520;
    private const int PanelHeight = 500;
    private const int TitleBarHeight = 32;
    private const int TextAreaHeight = 140;
    private const int OptionHeight = 40;
    private const int OptionSpacing = 4;
    private const int Padding = 10;
    private const int CloseBtnSize = 22;
    private const int CloseBtnMargin = 6;
    private const float TextScale = 0.75f;
    private const float OptionTextScale = 0.7f;
    private const int TextPaddingX = 6;
    private const int TextPaddingY = 4;
    private const int LineSpacing = 2;

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

    // ===== 键盘导航 =====
    private int _keyboardSelectedIndex = -1;
    private Keys[] _lastKeys = Array.Empty<Keys>();

    // ===== 动画 =====
    private float _openProgress;
    private float _openVelocity;
    private const float AnimSpeed = 0.15f;
    private const float AnimDamping = 0.6f;

    // ===== 文本换行缓存 =====
    private List<string> _wrappedTextLines = new();
    private string _lastWrappedText = "";

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
        _keyboardSelectedIndex = -1;
        _openProgress = 0f;
        _openVelocity = 0f;
        _isOpen = true;

        int x = (Main.screenWidth - PanelWidth) / 2;
        int y = (Main.screenHeight - PanelHeight) / 2;
        _panelRect = new Rectangle(x, y, PanelWidth, PanelHeight);

        _textRect = new Rectangle(
            _panelRect.X + Padding,
            _panelRect.Y + TitleBarHeight + Padding,
            _panelRect.Width - Padding * 2,
            TextAreaHeight
        );

        WrapNPCText();
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
        WrapNPCText();
        RebuildOptionButtons();

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
    // 文本换行
    // ============================================================

    private void WrapNPCText()
    {
        _wrappedTextLines.Clear();
        if (string.IsNullOrEmpty(_npcText))
        {
            _lastWrappedText = "";
            return;
        }

        _lastWrappedText = _npcText;

        var font = FontAssets.MouseText.Value;
        float maxWidth = _textRect.Width - TextPaddingX * 2;

        string[] paragraphs = _npcText.Split('\n');
        foreach (string paragraph in paragraphs)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                _wrappedTextLines.Add("");
                continue;
            }

            string[] words = paragraph.Split(' ');
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                float testWidth = font.MeasureString(testLine).X * TextScale;

                if (testWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
                {
                    _wrappedTextLines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                _wrappedTextLines.Add(currentLine);
        }
    }

    private string TruncateOptionText(string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var font = FontAssets.MouseText.Value;
        float textWidth = font.MeasureString(text).X * OptionTextScale;

        if (textWidth <= maxWidth) return text;

        for (int i = text.Length - 1; i > 0; i--)
        {
            string truncated = text.Substring(0, i) + "...";
            if (font.MeasureString(truncated).X * OptionTextScale <= maxWidth)
                return truncated;
        }

        return "...";
    }

    // ============================================================
    // 选项按钮重建
    // ============================================================

    private void RebuildOptionButtons()
    {
        _optionButtons.Clear();

        int startY = _textRect.Bottom + Padding;
        int btnWidth = _panelRect.Width - Padding * 2 - 16;
        float maxTextWidth = btnWidth - TextPaddingX * 2;

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
            string fullText = string.IsNullOrEmpty(typeTag)
                ? option.Text
                : $"[{typeTag}] {option.Text}";

            string displayText = TruncateOptionText(fullText, maxTextWidth);

            _optionButtons.Add(new OptionButton
            {
                Rect = rect,
                Text = displayText,
                FullText = fullText,
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

        UpdateAnimation();
        if (_openProgress < 0.95f) return;

        var mousePos = Main.MouseScreen;
        bool currentLeft = Main.mouseLeft;
        bool leftClick = currentLeft && !_lastMouseLeft;

        Main.LocalPlayer.mouseInterface = true;

        if (Main.keyState.IsKeyDown(Keys.Escape) && !_escapeWasDown)
        {
            _escapeWasDown = true;
            CloseDialogue();
            _lastMouseLeft = currentLeft;
            return;
        }
        _escapeWasDown = Main.keyState.IsKeyDown(Keys.Escape);

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

        UpdateDragging(mousePos, currentLeft, leftClick);

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

        UpdateKeyboardNavigation();

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

    private void UpdateAnimation()
    {
        float target = 1f;
        float diff = target - _openProgress;
        _openVelocity += diff * AnimSpeed;
        _openVelocity *= (1f - AnimDamping);
        _openProgress += _openVelocity;

        if (Math.Abs(diff) < 0.001f && Math.Abs(_openVelocity) < 0.001f)
        {
            _openProgress = target;
            _openVelocity = 0f;
        }
    }

    private void UpdateKeyboardNavigation()
    {
        Keys[] currentKeys = Main.keyState.GetPressedKeys();

        bool KeyPressed(Keys k) => Array.IndexOf(currentKeys, k) >= 0 && Array.IndexOf(_lastKeys, k) < 0;

        int selectedIndex = -1;
        if (KeyPressed(Keys.D1)) selectedIndex = 0;
        else if (KeyPressed(Keys.D2)) selectedIndex = 1;
        else if (KeyPressed(Keys.D3)) selectedIndex = 2;
        else if (KeyPressed(Keys.D4)) selectedIndex = 3;
        else if (KeyPressed(Keys.D5)) selectedIndex = 4;
        else if (KeyPressed(Keys.D6)) selectedIndex = 5;
        else if (KeyPressed(Keys.D7)) selectedIndex = 6;
        else if (KeyPressed(Keys.D8)) selectedIndex = 7;
        else if (KeyPressed(Keys.D9)) selectedIndex = 8;

        if (selectedIndex >= 0 && selectedIndex < _options.Count)
        {
            OnOptionClicked(selectedIndex);
        }

        _lastKeys = currentKeys;
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

            float scale = 0.8f + _openProgress * 0.2f;
            float alpha = _openProgress;

            Rectangle drawPanel = _panelRect;
            if (_openProgress < 1f)
            {
                int cx = _panelRect.Center.X;
                int cy = _panelRect.Center.Y;
                int w = (int)(_panelRect.Width * scale);
                int h = (int)(_panelRect.Height * scale);
                drawPanel = new Rectangle(cx - w / 2, cy - h / 2, w, h);
            }

            sb.Draw(pixel, drawPanel, PanelBg * alpha);
            UIRendering.DrawBorder(sb, drawPanel, 1, BorderColor * alpha);

            var titleRect = new Rectangle(drawPanel.X, drawPanel.Y, drawPanel.Width, TitleBarHeight);
            sb.Draw(pixel, titleRect, TitleBarBg * alpha);

            DrawNPCHead(sb, drawPanel, alpha);

            Utils.DrawBorderString(sb, _npcName,
                new Vector2(drawPanel.X + 48, titleRect.Y + 4),
                UIStyles.TitleText * alpha, 0.85f);

            var textRect = new Rectangle(
                drawPanel.X + Padding,
                drawPanel.Y + TitleBarHeight + Padding,
                drawPanel.Width - Padding * 2,
                TextAreaHeight
            );

            sb.Draw(pixel, textRect, TextBg * alpha);
            UIRendering.DrawBorder(sb, textRect, 1, UIStyles.BorderLight * alpha);

            for (int i = 0; i < _wrappedTextLines.Count; i++)
            {
                float lineY = textRect.Y + TextPaddingY + i * (FontAssets.MouseText.Value.MeasureString("A").Y * TextScale + LineSpacing);
                if (lineY + FontAssets.MouseText.Value.MeasureString("A").Y * TextScale > textRect.Bottom - TextPaddingY)
                    break;

                Utils.DrawBorderString(sb, _wrappedTextLines[i],
                    new Vector2(textRect.X + TextPaddingX, lineY),
                    TextColor * alpha, TextScale);
            }

            int optionsStartY = textRect.Bottom + Padding;
            for (int idx = 0; idx < _optionButtons.Count; idx++)
            {
                var btn = _optionButtons[idx];
                var drawRect = btn.Rect;
                drawRect.Y -= (int)_scrollOffset;

                drawRect = new Rectangle(
                    drawPanel.X + Padding,
                    optionsStartY + idx * (OptionHeight + OptionSpacing) - (int)_scrollOffset,
                    drawPanel.Width - Padding * 2,
                    OptionHeight
                );

                if (drawRect.Bottom < textRect.Bottom + Padding || drawRect.Top > drawPanel.Bottom)
                    continue;

                bool hovered = drawRect.Contains(Main.MouseScreen.ToPoint());
                Color btnColor = GetOptionBgColor(btn.OptionType, hovered) * alpha;
                Color textColor = GetOptionTypeColor(btn.OptionType) * alpha;

                sb.Draw(pixel, drawRect, btnColor);
                UIRendering.DrawBorder(sb, drawRect, 1, (hovered ? UIStyles.BorderAccent : UIStyles.BorderLight) * alpha);

                string keyHint = idx < 9 ? $"[{idx + 1}] " : "";
                string btnText = keyHint + btn.Text;
                var font = FontAssets.MouseText.Value;
                var textSize = font.MeasureString(btnText) * OptionTextScale;
                float textY = drawRect.Y + (drawRect.Height - textSize.Y) / 2f;
                Utils.DrawBorderString(sb, btnText,
                    new Vector2(drawRect.X + TextPaddingX, textY),
                    textColor, OptionTextScale);

                if (hovered && !string.IsNullOrEmpty(btn.Tooltip))
                {
                    DrawTooltip(sb, btn.Tooltip);
                }
            }

            DrawCloseButton(sb, drawPanel, alpha);

            int totalOptionHeight = _options.Count * (OptionHeight + OptionSpacing);
            int optionsAreaHeight = drawPanel.Height - TitleBarHeight - Padding - TextAreaHeight - Padding * 2;
            if (totalOptionHeight > optionsAreaHeight)
            {
                var scrollArea = new Rectangle(
                    drawPanel.X + Padding,
                    textRect.Bottom + Padding,
                    drawPanel.Width - Padding * 2,
                    optionsAreaHeight
                );
                UIRendering.DrawScrollbar(sb, scrollArea, _scrollOffset, totalOptionHeight,
                    ScrollbarBg * alpha, ScrollbarThumb * alpha);
            }
        }
        catch (Exception ex)
        {
            Main.NewText($"[DialogueTreeUI Draw Error] {ex.Message}", Color.Red);
        }
    }

    private void DrawNPCHead(SpriteBatch sb, Rectangle panelRect, float alpha)
    {
        if (_npcHeadType < 0 || _npcHeadType >= TextureAssets.NpcHead.Length)
            return;

        var headTex = TextureAssets.NpcHead[_npcHeadType].Value;
        if (headTex == null) return;

        int headSize = 32;
        var headRect = new Rectangle(
            panelRect.X + 8,
            panelRect.Y + (TitleBarHeight - headSize) / 2,
            headSize,
            headSize
        );

        sb.Draw(headTex, headRect, Color.White * alpha);
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

    private void DrawCloseButton(SpriteBatch sb, Rectangle panelRect, float alpha)
    {
        int btnX = panelRect.Right - CloseBtnSize - CloseBtnMargin;
        int btnY = panelRect.Y + (TitleBarHeight - CloseBtnSize) / 2;
        var rect = new Rectangle(btnX, btnY, CloseBtnSize, CloseBtnSize);
        Color bgColor = (_closeBtnPressed ? CloseBtnHover : (_closeBtnHovered ? CloseBtnHover : CloseBtnColor)) * alpha;
        UIRendering.DrawCloseButton(sb, rect, bgColor, Color.White * 0.9f * alpha);
    }

    private void DrawTooltip(SpriteBatch sb, string tooltip)
    {
        if (string.IsNullOrEmpty(tooltip)) return;

        var font = FontAssets.MouseText.Value;
        float scale = 0.65f;
        var textSize = font.MeasureString(tooltip) * scale;
        var mousePos = Main.MouseScreen;

        int padding = 6;
        int tooltipWidth = (int)textSize.X + padding * 2;
        int tooltipHeight = (int)textSize.Y + padding * 2;

        int tooltipX = (int)mousePos.X + 16;
        int tooltipY = (int)mousePos.Y + 16;

        if (tooltipX + tooltipWidth > Main.screenWidth)
            tooltipX = Main.screenWidth - tooltipWidth - 4;
        if (tooltipY + tooltipHeight > Main.screenHeight)
            tooltipY = (int)mousePos.Y - tooltipHeight - 4;

        var tooltipRect = new Rectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
        sb.Draw(UIRendering.Pixel, tooltipRect, UIStyles.PanelBg);
        UIRendering.DrawBorder(sb, tooltipRect, 1, UIStyles.BorderAccent);

        Utils.DrawBorderString(sb, tooltip,
            new Vector2(tooltipX + padding, tooltipY + padding),
            UIStyles.TextMain, scale);
    }

    // ============================================================
    // 内部类
    // ============================================================

    private class OptionButton
    {
        public Rectangle Rect;
        public string Text;
        public string FullText;
        public string Tooltip;
        public int Index;
        public DialogueOptionType OptionType;
    }
}
