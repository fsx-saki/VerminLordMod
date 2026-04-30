using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 可交互物品槽位 — 支持显示物品图标、数量、边框高亮
    /// 用于合炼台的原料槽和结果槽
    /// </summary>
    public class UIItemSlot : UIElement
    {
        private Item _item;
        private readonly Color _emptyColor;
        private readonly Color _filledColor;
        private readonly Color _highlightColor;
        private bool _highlighted;
        private bool _showStack;

        /// <summary>当前槽位中的物品</summary>
        public Item StoredItem => _item;

        /// <summary>槽位是否为空</summary>
        public bool IsEmpty => _item == null || _item.type <= ItemID.None;

        /// <summary>槽位索引（用于事件回调）</summary>
        public int SlotIndex { get; set; }

        /// <summary>点击回调</summary>
        public Action<int> OnSlotClick { get; set; }

        public UIItemSlot(Color? emptyColor = null, Color? filledColor = null)
        {
            _item = new Item();
            _item.TurnToAir();
            _emptyColor = emptyColor ?? new Color(40, 40, 60, 180);
            _filledColor = filledColor ?? new Color(50, 50, 80, 200);
            _highlightColor = new Color(80, 120, 200, 220);
            _showStack = true;

            Width.Set(48f, 0f);
            Height.Set(48f, 0f);

            OnLeftClick += (evt, listener) =>
            {
                OnSlotClick?.Invoke(SlotIndex);
            };
        }

        /// <summary>
        /// 设置槽位中的物品
        /// </summary>
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

        /// <summary>
        /// 清空槽位
        /// </summary>
        public void Clear()
        {
            _item = new Item();
            _item.TurnToAir();
        }

        /// <summary>
        /// 设置高亮状态
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            _highlighted = highlighted;
        }

        /// <summary>
        /// 设置是否显示堆叠数量
        /// </summary>
        public void SetShowStack(bool show) => _showStack = show;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            // 背景
            Color bgColor = _highlighted ? _highlightColor : (IsEmpty ? _emptyColor : _filledColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, bgColor);

            // 边框
            Color borderColor = _highlighted ? Color.Cyan : (IsEmpty ? new Color(60, 60, 80) : new Color(100, 120, 180));
            int borderSize = _highlighted ? 2 : 1;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, rect.Width, borderSize), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y + rect.Height - borderSize, rect.Width, borderSize), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, borderSize, rect.Height), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X + rect.Width - borderSize, rect.Y, borderSize, rect.Height), borderColor);

            // 绘制物品
            if (!IsEmpty && _item.type < TextureAssets.Item.Length && _item.type > 0)
            {
                Texture2D itemTex = TextureAssets.Item[_item.type].Value;
                if (itemTex != null)
                {
                    Rectangle? sourceRect = null;
                    if (_item.type < Main.itemAnimations.Length && Main.itemAnimations[_item.type] != null)
                        sourceRect = Main.itemAnimations[_item.type].GetFrame(itemTex);

                    float scale = Math.Min(40f / itemTex.Width, 40f / itemTex.Height);
                    Vector2 pos = dims.Center() - (sourceRect?.Size() ?? itemTex.Size()) * scale / 2f;

                    spriteBatch.Draw(itemTex, pos, sourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                    // 堆叠数量
                    if (_showStack && _item.stack > 1)
                    {
                        string stackText = _item.stack.ToString();
                        Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(stackText);
                        Vector2 textPos = new Vector2(
                            dims.X + dims.Width - textSize.X - 4f,
                            dims.Y + dims.Height - textSize.Y - 2f
                        );
                        Utils.DrawBorderString(spriteBatch, stackText, textPos, Color.White, 0.75f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 可点击的类别标签按钮
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
            _normalColor = new Color(50, 50, 70, 200);
            _selectedColor = new Color(60, 100, 180, 220);

            Width.Set(-1f, 1f); // 自适应宽度
            Height.Set(28f, 0f);
            PaddingLeft = 8f;
            PaddingRight = 8f;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dims = GetDimensions();
            var rect = dims.ToRectangle();

            Color bgColor = _isSelected ? _selectedColor : _normalColor;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, bgColor);

            // 边框
            Color borderColor = _isSelected ? Color.CornflowerBlue : new Color(60, 60, 80);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, rect.Width, 1), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), borderColor);

            // 文本
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(_text);
            Vector2 textPos = dims.Center() - textSize / 2f;
            Utils.DrawBorderString(spriteBatch, _text, textPos, _isSelected ? Color.White : Color.LightGray, 0.75f);
        }
    }
}
