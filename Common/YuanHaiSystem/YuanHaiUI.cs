using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using InnoVault;
using InnoVault.UIHandles;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public class YuanHaiUI : UIHandle
    {
        public static YuanHaiUI Instance => UIHandleLoader.GetUIHandleOfType<YuanHaiUI>();
        public override bool Active => true;

        public bool Visible;
        private float zoom = 1f;
        private Vector2 panOffset;
        private Vector2 dragStartMouse;
        private Vector2 dragStartPan;
        private bool isDraggingMap;
        private int dragGuIndex = -1;
        private bool isDraggingGu;

        public Vector2 PanelPosition = new(100, 100);
        public Vector2 PanelSize = new(800, 600);
        private bool _isDraggingPanel, _isResizing;
        private Vector2 _dragPanelStart, _dragPanelStartMouse, _resizeStartSize;
        private const int DragHandleSize = 20, ResizeHandleSize = 16;

        private Rectangle PanelRect => new((int)PanelPosition.X, (int)PanelPosition.Y, (int)PanelSize.X, (int)PanelSize.Y);

        private readonly List<QiParticle> _particles = [];

        public void Toggle() => Visible = !Visible;

        public override void Update()
        {
            if (!Visible) return;

            var player = Main.LocalPlayer;
            var yhPlayer = player.GetModPlayer<YuanHaiPlayer>();
            bool leftPressed = keyLeftPressState == KeyPressState.Pressed;
            bool leftHeld = keyLeftPressState == KeyPressState.Held;
            bool leftReleased = keyLeftPressState == KeyPressState.Released;
            bool rightPressed = keyRightPressState == KeyPressState.Pressed;

            var pr = PanelRect;
            bool inPanel = pr.Contains(Main.MouseScreen.ToPoint());
            if (inPanel) player.mouseInterface = true;

            // Drag panel (top-left handle)
            var dh = new Rectangle(pr.X, pr.Y, DragHandleSize, DragHandleSize);
            if (dh.Contains(Main.MouseScreen.ToPoint()) && leftPressed && !_isDraggingPanel && !_isResizing)
            { _isDraggingPanel = true; _dragPanelStart = PanelPosition; _dragPanelStartMouse = Main.MouseScreen; }
            if (_isDraggingPanel && leftHeld)
                PanelPosition = _dragPanelStart + Main.MouseScreen - _dragPanelStartMouse;
            if (_isDraggingPanel && leftReleased) _isDraggingPanel = false;

            // Resize (bottom-right handle)
            var rh = new Rectangle(pr.Right - ResizeHandleSize, pr.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            if (rh.Contains(Main.MouseScreen.ToPoint()) && leftPressed && !_isResizing && !_isDraggingPanel)
            { _isResizing = true; _resizeStartSize = PanelSize; _dragPanelStartMouse = Main.MouseScreen; }
            if (_isResizing && leftHeld)
                PanelSize = new Vector2(MathHelper.Max(400, _resizeStartSize.X + Main.MouseScreen.X - _dragPanelStartMouse.X),
                                MathHelper.Max(300, _resizeStartSize.Y + Main.MouseScreen.Y - _dragPanelStartMouse.Y));
            if (_isResizing && leftReleased) _isResizing = false;

            // Particles
            if (Main.rand != null && _particles.Count < 20 && Main.rand.NextBool(3))
                _particles.Add(new QiParticle
                {
                    Position = new(Main.rand.Next(-250, 251), Main.rand.Next(-250, 251)),
                    Velocity = new Vector2((float)Main.rand.NextDouble() * 2 - 1, (float)Main.rand.NextDouble() * 2 - 1) * 0.5f,
                    Life = 120 + Main.rand.Next(60), MaxLife = 180, Size = Main.rand.NextFloat(2, 6)
                });
            for (int i = _particles.Count - 1; i >= 0; i--)
            { _particles[i].Update(); if (_particles[i].Life <= 0) _particles.RemoveAt(i); }

            // Gu movement
            if (yhPlayer != null)
                foreach (var gu in yhPlayer.GuCollection) gu.UpdateMovement();

            // Scroll zoom
            int scroll = MouseScrollDelta;
            if (scroll != 0 && pr.Contains(Main.MouseScreen.ToPoint()))
            {
                float old = zoom;
                zoom = MathHelper.Clamp(zoom + (scroll > 0 ? 0.1f : -0.1f), 0.3f, 3f);
                if (old != zoom)
                {
                    Vector2 c = new(pr.Center.X, pr.Center.Y);
                    Vector2 rel = Main.MouseScreen - c;
                    panOffset = rel - (rel - panOffset) * (zoom / old);
                }
            }

            // Right-click to extract Gu (independent of left-click)
            if (rightPressed && yhPlayer != null && !_isDraggingPanel && !_isResizing)
            {
                Vector2 c = new(pr.Center.X, pr.Center.Y);
                for (int i = yhPlayer.GuCollection.Count - 1; i >= 0; i--)
                {
                    var gu = yhPlayer.GuCollection[i];
                    Vector2 screenPos = c + (gu.Position + panOffset) * zoom;
                    if (Vector2.Distance(Main.MouseScreen, screenPos) < 24 * zoom)
                    {
                        int itemType = gu.ItemType;
                        yhPlayer.GuCollection.RemoveAt(i);
                        int idx = Item.NewItem(player.GetSource_GiftOrReward(), player.Center, Vector2.Zero, itemType, 1);
                        if (idx >= 0 && idx < Main.item.Length)
                        {
                            var mi = Main.item[idx].ModItem as global::VerminLordMod.Common.Items.GuBaseItem;
                            if (mi != null) mi.IsRefined = true;
                        }
                        SoundEngine.PlaySound(SoundID.Grab);
                        break;
                    }
                }
            }

            // Map drag (only when not dragging panel/resize/gu)
            if (!_isDraggingPanel && !_isResizing && !isDraggingGu)
            {
                bool inContent = new Rectangle(pr.X + DragHandleSize, pr.Y, pr.Width - DragHandleSize - ResizeHandleSize, pr.Height - ResizeHandleSize).Contains(Main.MouseScreen.ToPoint());
                if (inContent && leftPressed && !isDraggingMap)
                {
                    // Check if clicking a Gu
                    bool hitGu = false;
                    if (yhPlayer != null)
                    {
                        Vector2 c = new(pr.Center.X, pr.Center.Y);
                        for (int i = yhPlayer.GuCollection.Count - 1; i >= 0; i--)
                        {
                            var gu = yhPlayer.GuCollection[i];
                            Vector2 screenPos = c + (gu.Position + panOffset) * zoom;
                            if (Vector2.Distance(Main.MouseScreen, screenPos) < 24 * zoom)
                            {
                                dragGuIndex = i; isDraggingGu = true; hitGu = true;
                                break;
                            }
                        }
                    }
                    if (!hitGu) { isDraggingMap = true; dragStartMouse = Main.MouseScreen; dragStartPan = panOffset; }
                }
                if (isDraggingMap && leftHeld)
                    panOffset = dragStartPan + (Main.MouseScreen - dragStartMouse) / zoom;
                if (isDraggingMap && leftReleased) isDraggingMap = false;
            }

            // Dragging Gu (left-click drag to reposition)
            if (isDraggingGu && dragGuIndex >= 0 && yhPlayer != null && dragGuIndex < yhPlayer.GuCollection.Count)
            {
                Vector2 c = new(pr.Center.X, pr.Center.Y);
                yhPlayer.GuCollection[dragGuIndex].Position = (Main.MouseScreen - c) / zoom - panOffset;
            }

            if (leftReleased && !isDraggingMap) { dragGuIndex = -1; isDraggingGu = false; }
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!Visible) return;
            var px = VaultAsset.placeholder2.Value;
            var light = VaultAsset.Light?.Value ?? px;
            var pr = PanelRect;
            int x = pr.X, y = pr.Y, w = pr.Width, h = pr.Height;

            // Background
            Utils.DrawInvBG(sb, pr, new Color(10, 8, 20, 220));
            sb.Draw(px, new Rectangle(x, y, w, 2), new Color(60, 50, 80) * 0.5f);
            sb.Draw(px, new Rectangle(x, pr.Bottom - 2, w, 2), new Color(20, 15, 35) * 0.5f);

            // Title
            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, "◆ 元海", x + 30, y + 8, new Color(180, 150, 220), Color.Black, Vector2.Zero, 1f);

            // Content area with scissor
            var contentRect = new Rectangle(x + DragHandleSize, y + 36, w - DragHandleSize - ResizeHandleSize, h - 70);
            var rs = new RasterizerState { ScissorTestEnable = true };
            Vector2 cp = Vector2.Transform(new Vector2(contentRect.X, contentRect.Y), Main.UIScaleMatrix);
            Vector2 cs = Vector2.Transform(new Vector2(contentRect.Width, contentRect.Height), Main.UIScaleMatrix) - Vector2.Transform(Vector2.Zero, Main.UIScaleMatrix);
            var sr = new Rectangle((int)cp.X, (int)cp.Y, (int)cs.X, (int)cs.Y);
            var or = sb.GraphicsDevice.ScissorRectangle;
            sr = Rectangle.Intersect(sr, sb.GraphicsDevice.Viewport.Bounds);
            sb.End();
            sb.GraphicsDevice.ScissorRectangle = sr;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rs, null, Main.UIScaleMatrix);

            Vector2 center = new(contentRect.Center.X, contentRect.Center.Y);

            // Qi particles
            foreach (var p in _particles)
            {
                float a = p.Life / (float)p.MaxLife;
                Vector2 dp = center + (p.Position + panOffset) * zoom;
                sb.Draw(light, dp, null, new Color(100, 180, 255) * a * 0.4f, 0f, light.Size() / 2f, p.Size * zoom * 0.1f, SpriteEffects.None, 0f);
            }

            // Gu entities
            var yhPlayer = Main.LocalPlayer.GetModPlayer<YuanHaiPlayer>();
            if (yhPlayer != null)
            {
                foreach (var gu in yhPlayer.GuCollection)
                {
                    Vector2 gp = center + (gu.Position + panOffset) * zoom;
                    float sz = 32 * zoom;
                    sb.Draw(light, gp, null, new Color(80, 150, 220, 0) * 0.3f, 0f, light.Size() / 2f, sz / light.Width * 2f, SpriteEffects.None, 0f);
                    Main.instance.LoadItem(gu.ItemType);
                    var tex = TextureAssets.Item[gu.ItemType].Value;
                    if (tex != null)
                    {
                        float sc = sz * 0.7f / Math.Max(tex.Width, tex.Height);
                        sb.Draw(tex, gp, null, Color.White, gu.Rotation, tex.Size() / 2f, sc, SpriteEffects.None, 0f);
                    }
                }
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = or;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            // Gather button
            int bs = 36;
            var gr = new Rectangle(pr.Right - bs - 20, pr.Y + 8, bs, bs);
            bool hg = gr.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, gr, (hg ? new Color(60, 60, 80) : new Color(40, 40, 55)) * 0.8f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "集", gr.X + 10, gr.Y + 8, Color.White, Color.Black, Vector2.Zero, 0.8f);
            if (hg && keyLeftPressState == KeyPressState.Pressed && yhPlayer != null)
            { foreach (var g in yhPlayer.GuCollection) g.Position *= 0.3f; SoundEngine.PlaySound(SoundID.MenuTick); }

            // Close button
            int cs2 = 30;
            var cr = new Rectangle(pr.Right - cs2 - 8, pr.Y + 6, cs2, cs2);
            bool hc = cr.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, cr, (hc ? new Color(80, 40, 40) : new Color(10, 10, 10)) * 0.35f);
            Color xc = hc ? new Color(255, 100, 100) : new Color(180, 180, 180) * 0.6f;
            float cx = cr.X + cr.Width / 2f, cy = cr.Y + cr.Height / 2f, xs2 = cr.Width * 0.22f;
            sb.Draw(px, new Vector2(cx, cy), null, xc, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xs2 * 2f, 1.5f), SpriteEffects.None, 0f);
            sb.Draw(px, new Vector2(cx, cy), null, xc, -MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xs2 * 2f, 1.5f), SpriteEffects.None, 0f);
            if (hc && keyLeftPressState == KeyPressState.Pressed) { Visible = false; SoundEngine.PlaySound(SoundID.MenuClose); }

            // Drag handle
            var dH = new Rectangle(x, y, DragHandleSize, DragHandleSize);
            sb.Draw(px, dH, (dH.Contains(Main.MouseScreen.ToPoint()) ? new Color(80, 80, 100) : new Color(40, 40, 55)) * 0.5f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "≡", x + 3, y - 1, new Color(150, 150, 180), Color.Black, Vector2.Zero, 0.7f);

            // Resize handle
            var rH = new Rectangle(pr.Right - ResizeHandleSize, pr.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            sb.Draw(px, rH, (rH.Contains(Main.MouseScreen.ToPoint()) ? new Color(80, 80, 100) : new Color(40, 40, 55)) * 0.5f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "↙", rH.X + 2, rH.Y + 1, new Color(150, 150, 180), Color.Black, Vector2.Zero, 0.7f);
        }

        private class QiParticle
        {
            public Vector2 Position, Velocity;
            public int Life, MaxLife;
            public float Size;
            public void Update() { Position += Velocity; Velocity += new Vector2((float)Math.Sin(Life * 0.05f) * 0.01f, (float)Math.Cos(Life * 0.07f) * 0.01f); Life--; }
        }
    }
}
