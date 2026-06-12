using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.StoryUI
{
    /// <summary>
    /// 关键选择面板 — 在剧情关键节点弹出，让玩家做出影响后续的选择
    /// 灵动设计：居中弹出、选项渐入、悬停高亮、选择后渐出
    /// </summary>
    public class StoryChoicePanel
    {
        public bool IsVisible { get; private set; }
        public float Opacity { get; private set; }

        private float _targetOpacity;
        private const float FadeSpeed = 0.06f;

        private string _choiceID = "";
        private string _title = "";
        private string _description = "";
        private List<ChoiceOption> _options = new();
        private int _hoveredIndex = -1;
        private bool _choiceMade;

        public bool IsChoiceActive => IsVisible && !_choiceMade;

        // ==================== 颜色 ====================
        private static readonly Color OverlayColor = new(0, 0, 0, 160);
        private static readonly Color PanelBg = UIStyles.PanelBg;
        private static readonly Color TitleColor = UIStyles.TitleText;
        private static readonly Color DescColor = UIStyles.TextMain;
        private static readonly Color OptionNormal = UIStyles.BtnDefault;
        private static readonly Color OptionHover = UIStyles.ListItemHover;
        private static readonly Color OptionSelected = UIStyles.BtnSelected;
        private static readonly Color OptionText = UIStyles.TextMain;

        // ==================== 数据类 ====================
        public class ChoiceOption
        {
            public string Text;
            public string Tooltip;
            public int Value;
            public Color AccentColor;
        }

        // ==================== 方法 ====================

        public void ShowChoice(string choiceID, string title, string description, List<ChoiceOption> options)
        {
            _choiceID = choiceID;
            _title = title;
            _description = description;
            _options = options;
            _choiceMade = false;
            _hoveredIndex = -1;
            IsVisible = true;
            _targetOpacity = 1f;
            Opacity = 0f;
        }

        public void Update()
        {
            if (Opacity < _targetOpacity)
                Opacity = Math.Min(Opacity + FadeSpeed, _targetOpacity);
            else if (Opacity > _targetOpacity)
            {
                Opacity = Math.Max(Opacity - FadeSpeed, _targetOpacity);
                if (Opacity <= 0f) IsVisible = false;
            }

            if (IsVisible && !_choiceMade)
            {
                UpdateHover();
            }
        }

        private void UpdateHover()
        {
            _hoveredIndex = -1;
            if (_options == null) return;

            var ms = Main.MouseScreen;
            int panelW = 500;
            int panelH = 60 + _options.Count * 50 + 80;
            int px = (Main.screenWidth - panelW) / 2;
            int py = (Main.screenHeight - panelH) / 2;
            int optStartY = py + 80;

            for (int i = 0; i < _options.Count; i++)
            {
                var optRect = new Rectangle(px + 30, optStartY + i * 50, panelW - 60, 40);
                if (optRect.Contains(ms.ToPoint()))
                {
                    _hoveredIndex = i;
                    break;
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsVisible || Opacity <= 0f) return;
            float a = Opacity;

            // 全屏遮罩
            sb.Draw(TextureAssets.BlackTile.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                OverlayColor * a);

            int panelW = 500;
            int panelH = 60 + _options.Count * 50 + 80;
            int px = (Main.screenWidth - panelW) / 2;
            int py = (Main.screenHeight - panelH) / 2;

            // 面板背景
            sb.Draw(TextureAssets.BlackTile.Value,
                new Rectangle(px, py, panelW, panelH),
                PanelBg * a);
            DrawBorder(sb, new Rectangle(px, py, panelW, panelH), UIStyles.BorderAccent * a * 0.6f);

            // 标题
            Utils.DrawBorderString(sb, _title, new Vector2(px + 24, py + 16), TitleColor * a, UIStyles.TitleScale);

            // 描述
            Utils.DrawBorderString(sb, _description, new Vector2(px + 24, py + 44), DescColor * a, UIStyles.BodyScale);

            // 选项
            int optStartY = py + 80;
            for (int i = 0; i < _options.Count; i++)
            {
                var opt = _options[i];
                var optRect = new Rectangle(px + 30, optStartY + i * 50, panelW - 60, 40);
                bool hovered = i == _hoveredIndex;
                bool selected = _choiceMade && ChoiceTrackerSystem.GetChoice(_choiceID) == opt.Value;

                Color bgColor = selected ? OptionSelected * a :
                                hovered ? OptionHover * a :
                                OptionNormal * a;

                // 选项背景
                sb.Draw(TextureAssets.BlackTile.Value, optRect, bgColor);

                // 左侧装饰条
                sb.Draw(TextureAssets.BlackTile.Value,
                    new Rectangle(optRect.X, optRect.Y, 4, optRect.Height),
                    opt.AccentColor * a);

                // 选项文字
                Utils.DrawBorderString(sb, opt.Text,
                    new Vector2(optRect.X + 16, optRect.Y + 10),
                    OptionText * a, UIStyles.BodyScale);

                // 悬停时显示提示
                if (hovered && !string.IsNullOrEmpty(opt.Tooltip))
                {
                    Utils.DrawBorderString(sb, opt.Tooltip,
                        new Vector2(optRect.X + 16, optRect.Y + 26),
                        UIStyles.TextSecondary * a, UIStyles.TinyScale);
                }
            }
        }

        public void HandleClick()
        {
            if (!IsVisible || _choiceMade || _hoveredIndex < 0) return;
            if (_hoveredIndex >= _options.Count) return;

            var opt = _options[_hoveredIndex];
            ChoiceTrackerSystem.MakeChoice(_choiceID, opt.Value);
            _choiceMade = true;
            _targetOpacity = 0f;
        }

        private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color)
        {
            int t = 2;
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
        }
    }
}
