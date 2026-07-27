using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using InnoVault;
using InnoVault.UIHandles;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public class RefiningUI : UIHandle
    {
        public static RefiningUI Instance => UIHandleLoader.GetUIHandleOfType<RefiningUI>();
        public override bool Active => true;

        public bool Visible;
        private Item _slotItem = new();
        private int _yuanShiDeposited;
        private int _progress;
        private bool _isRefining;

        public Vector2 PanelPosition = new(300, 200);
        public Vector2 PanelSize = new(340, 280);
        private bool _isDraggingPanel, _isResizing;
        private Vector2 _dragPanelStart, _dragPanelStartMouse, _resizeStartSize;
        private const int DragHandleSize = 20, ResizeHandleSize = 16, SlotSize = 52;
        private Rectangle PanelRect => new((int)PanelPosition.X, (int)PanelPosition.Y, (int)PanelSize.X, (int)PanelSize.Y);

        public void Toggle() => Visible = !Visible;

        public void ReturnStoredItems()
        {
            if (_slotItem?.IsAir == false)
            {
                Main.LocalPlayer?.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), _slotItem, _slotItem.stack);
                _slotItem.TurnToAir();
            }
            if (_yuanShiDeposited > 0)
            {
                Main.LocalPlayer?.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ModContent.ItemType<Content.Items.QuestItems.YuanShi>(), _yuanShiDeposited);
                _yuanShiDeposited = 0;
            }
            _progress = 0; _isRefining = false;
        }

        public override void Update()
        {
            if (!Visible) return;

            var yh = Main.LocalPlayer.GetModPlayer<YuanHaiPlayer>();
            if (yh == null) return;

            if (_isRefining)
            {
                _progress++;
                if (_progress >= 120)
                {
                    _isRefining = false; _progress = 0;
                    if (_slotItem.type > 0)
                    {
                        var mi = _slotItem.ModItem as global::VerminLordMod.Common.Items.GuBaseItem;
                        if (mi != null) mi.IsRefined = true;
                        yh.AddGu(_slotItem.type);
                        _slotItem.TurnToAir();
                        _yuanShiDeposited -= 10;
                    }
                    SoundEngine.PlaySound(SoundID.Item4);
                }
            }

            // Drag
            var dh = new Rectangle((int)PanelPosition.X, (int)PanelPosition.Y, DragHandleSize, DragHandleSize);
            if (dh.Contains(Main.MouseScreen.ToPoint()) && keyLeftPressState == KeyPressState.Pressed && !_isDraggingPanel && !_isResizing)
            { _isDraggingPanel = true; _dragPanelStart = PanelPosition; _dragPanelStartMouse = Main.MouseScreen; }
            if (_isDraggingPanel && keyLeftPressState == KeyPressState.Held)
                PanelPosition = _dragPanelStart + Main.MouseScreen - _dragPanelStartMouse;
            if (_isDraggingPanel && keyLeftPressState == KeyPressState.Released) _isDraggingPanel = false;

            // Resize
            var rh = new Rectangle(PanelRect.Right - ResizeHandleSize, PanelRect.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            if (rh.Contains(Main.MouseScreen.ToPoint()) && keyLeftPressState == KeyPressState.Pressed && !_isResizing && !_isDraggingPanel)
            { _isResizing = true; _resizeStartSize = PanelSize; _dragPanelStartMouse = Main.MouseScreen; }
            if (_isResizing && keyLeftPressState == KeyPressState.Held)
                PanelSize = new(MathHelper.Max(300, _resizeStartSize.X + Main.MouseScreen.X - _dragPanelStartMouse.X),
                                MathHelper.Max(240, _resizeStartSize.Y + Main.MouseScreen.Y - _dragPanelStartMouse.Y));
            if (_isResizing && keyLeftPressState == KeyPressState.Released) _isResizing = false;

            // Gu slot interaction (once per press)
            if (keyLeftPressState == KeyPressState.Pressed && !_isRefining)
            {
                var pr = PanelRect;
                var sr = new Rectangle(pr.X + 20, pr.Y + 50, SlotSize, SlotSize);
                var ysR2 = new Rectangle(pr.X + 20 + SlotSize + 20, pr.Y + 50, SlotSize, SlotSize);

                // Gu slot: place item from cursor (only unrefined Gu)
                if (sr.Contains(Main.MouseScreen.ToPoint()) && _slotItem.IsAir && !Main.mouseItem.IsAir)
                {
                    var mi = Main.mouseItem.ModItem as global::VerminLordMod.Common.Items.GuBaseItem;
                    if (mi != null && !mi.IsRefined)
                    {
                        _slotItem = Main.mouseItem.Clone(); _slotItem.stack = 1;
                        Main.mouseItem.stack -= 1;
                        if (Main.mouseItem.stack <= 0) Main.mouseItem = new Item();
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
                // Gu slot: take item to cursor
                else if (sr.Contains(Main.MouseScreen.ToPoint()) && !_slotItem.IsAir && Main.mouseItem.IsAir)
                { Main.mouseItem = _slotItem.Clone(); _slotItem.TurnToAir(); SoundEngine.PlaySound(SoundID.Grab); }
                // YuanShi slot: deposit all from cursor
                else if (ysR2.Contains(Main.MouseScreen.ToPoint()) && !Main.mouseItem.IsAir && Main.mouseItem.type == ModContent.ItemType<Content.Items.QuestItems.YuanShi>())
                { _yuanShiDeposited += Main.mouseItem.stack; Main.mouseItem = new Item(); SoundEngine.PlaySound(SoundID.Grab); }
                // YuanShi slot: withdraw all
                else if (ysR2.Contains(Main.MouseScreen.ToPoint()) && _yuanShiDeposited > 0 && Main.mouseItem.IsAir)
                { Main.mouseItem = new Item(ModContent.ItemType<Content.Items.QuestItems.YuanShi>(), _yuanShiDeposited); _yuanShiDeposited = 0; SoundEngine.PlaySound(SoundID.Grab); }
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!Visible) return;
            var px = VaultAsset.placeholder2.Value;
            var r = PanelRect;
            int x = r.X, y = r.Y, w = r.Width, h = r.Height;

            Utils.DrawInvBG(sb, r, new Color(20, 18, 25, 220));

            // Drag handle
            var dH = new Rectangle(x, y, DragHandleSize, DragHandleSize);
            sb.Draw(px, dH, (dH.Contains(Main.MouseScreen.ToPoint()) ? new Color(80, 80, 100) : new Color(40, 40, 55)) * 0.5f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "≡", x + 3, y - 1, new Color(150, 150, 180), Color.Black, Vector2.Zero, 0.7f);

            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, "◆ 炼化", x + 30, y + 8, new Color(200, 180, 120), Color.Black, Vector2.Zero, 1f);

            // Gu slot (visual only - interaction in Update)
            var sr = new Rectangle(x + 20, y + 50, SlotSize, SlotSize);
            sb.Draw(px, sr, new Color(30, 25, 20)); DrawSlotBorder(sb, sr, Color.Gray * 0.5f);
            if (_slotItem.type > 0)
            {
                Main.instance.LoadItem(_slotItem.type);
                var tex = TextureAssets.Item[_slotItem.type].Value;
                if (tex != null)
                {
                    float sc = SlotSize * 0.7f / Math.Max(tex.Width, tex.Height);
                    sb.Draw(tex, sr.Center.ToVector2(), null, Color.White, 0f, tex.Size() / 2f, sc, SpriteEffects.None, 0f);
                }
            }

            // YuanShi slot (visual only - interaction in Update)
            var ysR = new Rectangle(x + 20 + SlotSize + 20, y + 50, SlotSize, SlotSize);
            sb.Draw(px, ysR, new Color(30, 25, 20)); DrawSlotBorder(sb, ysR, new Color(180, 150, 80) * 0.5f);
            if (_yuanShiDeposited > 0)
            {
                int ysType = ModContent.ItemType<Content.Items.QuestItems.YuanShi>();
                Main.instance.LoadItem(ysType);
                var ysTex = TextureAssets.Item[ysType].Value;
                if (ysTex != null)
                {
                    float sc = SlotSize * 0.7f / Math.Max(ysTex.Width, ysTex.Height);
                    sb.Draw(ysTex, ysR.Center.ToVector2(), null, Color.White, 0f, ysTex.Size() / 2f, sc, SpriteEffects.None, 0f);
                }
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, $"x{_yuanShiDeposited}",
                    ysR.Right + 8, ysR.Y + 16, new Color(180, 150, 80), Color.Black, Vector2.Zero, 0.8f);
            }
            else
            {
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "元石",
                    ysR.Right + 12, ysR.Y + 16, new Color(100, 90, 60), Color.Black, Vector2.Zero, 0.75f);
            }

            // Progress bar
            int barY = y + 120, barW = w - 40;
            sb.Draw(px, new Rectangle(x + 20, barY, barW, 16), new Color(20, 18, 25));
            float pct = _isRefining ? _progress / 120f : (_yuanShiDeposited >= 10 ? 1f : _yuanShiDeposited / 10f);
            if (pct > 0) sb.Draw(px, new Rectangle(x + 22, barY + 2, (int)((barW - 4) * pct), 12), _isRefining ? new Color(100, 180, 220) : new Color(180, 150, 80));
            string bt = _isRefining ? $"炼化中... {_progress * 100 / 120}%" : $"元石: {_yuanShiDeposited}/10";
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, bt, x + 28, barY + 1, Color.White, Color.Black, Vector2.Zero, 0.7f);

            // Start
            int btnY2 = barY + 30;
            var btnR = new Rectangle(x + w / 2 - 60, btnY2, 120, 30);
            bool hovBtn = btnR.Contains(Main.MouseScreen.ToPoint());
            Color btnC = (_yuanShiDeposited >= 10 && !_isRefining && _slotItem.type > 0) ? (hovBtn ? new Color(60, 80, 60) : new Color(40, 60, 40)) : new Color(40, 40, 40);
            sb.Draw(px, btnR, btnC);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "开始炼化", btnR.X + 20, btnR.Y + 6, Color.White, Color.Black, Vector2.Zero, 0.8f);
            if (hovBtn && keyLeftPressState == KeyPressState.Pressed && _yuanShiDeposited >= 10 && !_isRefining && _slotItem.type > 0)
            { _isRefining = true; _progress = 0; SoundEngine.PlaySound(SoundID.Item4); }

            // Close
            var clR = new Rectangle(x + w - 36, y + 6, 30, 30);
            bool hovCl = clR.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, clR, hovCl ? new Color(80, 40, 40) * 0.4f : new Color(10, 10, 10) * 0.35f);
            if (hovCl && keyLeftPressState == KeyPressState.Pressed) { ReturnStoredItems(); Visible = false; SoundEngine.PlaySound(SoundID.MenuClose); }

            // Resize handle
            var rH = new Rectangle(r.Right - ResizeHandleSize, r.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            sb.Draw(px, rH, (rH.Contains(Main.MouseScreen.ToPoint()) ? new Color(80, 80, 100) : new Color(40, 40, 55)) * 0.5f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "↙", rH.X + 2, rH.Y + 1, new Color(150, 150, 180), Color.Black, Vector2.Zero, 0.7f);
        }

        private static void DrawSlotBorder(SpriteBatch sb, Rectangle rect, Color color)
        {
            var px = VaultAsset.placeholder2.Value;
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
        }
    }
}
