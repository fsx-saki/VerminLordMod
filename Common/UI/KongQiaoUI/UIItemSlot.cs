using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 可交互物品槽位 — 现代化扁平风格
    /// </summary>
    public class UIItemSlot : UIElement
    {
        private Item _item;
        private readonly Color _emptyColor;
        private readonly Color _filledColor;
        private readonly Color _highlightColor;
        private bool _highlighted;
        private bool _showStack;

        public Item StoredItem => _item;
        public bool IsEmpty => _item == null || _item.type <= ItemID.None;
        public int SlotIndex { get; set; }
        public Action<int> OnSlotClick { get; set; }

        public UIItemSlot(Color? emptyColor = null, Color? filledColor = null)
        {
            _item = new Item();
            _item.TurnToAir();
            _emptyColor = emptyColor ?? new Color(38, 38, 55, 180);
            _filledColor = filledColor ?? new Color(45, 48, 70, 200);
            _highlightColor = new Color(70, 100, 170, 220);
            _showStack = true;

            Width.Set(48f, 0f);
            Height.Set(48f, 0f);

            OnLeftClick += (evt, listener) => OnSlotClick?.Invoke(SlotIndex);
        }

        public void SetItem(Item item)
        {
            if (item == null || item.type <= ItemID.None)
            {
                _item = new Item();
                _item.TurnToAir();
            }
            else
            {
                _item = item.Clone();
            }
        }

        public void Clear()
        {
            _item = new Item();
            _item.TurnToAir();
        }

        public void SetHighlight(bool highlighted) => _highlighted = highlighted;
        public void SetShowStack(bool show) => _showStack = show;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            // 背景
            Color bgColor = _highlighted ? _highlightColor : (IsEmpty ? _emptyColor : _filledColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, bgColor);

            // 边框
            Color borderColor = _highlighted ? UIStyles.BorderAccent : (IsEmpty ? UIStyles.BorderLight : UIStyles.Border);
            int borderSize = _highlighted ? 2 : 1;
            UIRendering.DrawBorder(spriteBatch, rect, borderSize, borderColor);

            // 物品图标
            if (!IsEmpty && _item.type < TextureAssets.Item.Length && _item.type > 0)
            {
                UIHelper.DrawItemIcon(spriteBatch, _item, rect, _showStack);
            }
        }
    }

    /// <summary>
    /// 可点击的类别标签按钮 — 扁平风格
    /// </summary>
    public class UICategoryButton : UIElement
    {
        private readonly string _text;
        private readonly Color _normalColor;
        private readonly Color _selectedColor;
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        public string CategoryKey { get; set; }

        public UICategoryButton(string text, string categoryKey)
        {
            _text = text;
            CategoryKey = categoryKey;
            _normalColor = UIStyles.BtnSecondary;
            _selectedColor = UIStyles.BtnSelected;

            Width.Set(-1f, 1f);
            Height.Set(26f, 0f);
            PaddingLeft = 8f;
            PaddingRight = 8f;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            Color bgColor = _isSelected ? _selectedColor : _normalColor;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, bgColor);

            // 底部高亮线（选中时）
            if (_isSelected)
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), UIStyles.BorderAccent);
            }

            // 文本
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(_text);
            Vector2 textPos = dims.Center() - textSize / 2f;
            Utils.DrawBorderString(spriteBatch, _text, textPos,
                _isSelected ? UIStyles.TextMain : UIStyles.TextSecondary, 0.7f);
        }
    }
}
