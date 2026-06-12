using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Systems;
using Terraria.UI;

namespace VerminLordMod.Common.UI.StoryUI
{
    /// <summary>
    /// 主线UI系统 — 管理StoryGuidePanel和StoryChoicePanel
    /// 处理键盘快捷键、鼠标点击、EventBus订阅
    /// </summary>
    public class StoryUISystem : ModSystem
    {
        public static StoryUISystem Instance => ModContent.GetInstance<StoryUISystem>();

        public StoryGuidePanel GuidePanel { get; } = new();
        public StoryChoicePanel ChoicePanel { get; } = new();

        // Toast通知队列
        private readonly List<StoryToast> _toasts = new();

        // 上一次的阶段（用于检测变化）
        private StoryPhase _lastPhase;

        private bool _subscribed = false;

        public override void PostUpdateWorld()
        {
            if (!_subscribed)
            {
                EventBus.Subscribe<StoryPhaseAdvancedEvent>(OnPhaseAdvanced);
                EventBus.Subscribe<ChoiceMadeEvent>(OnChoiceMade);
                _subscribed = true;
            }
        }

        public override void OnWorldUnload()
        {
            _subscribed = false;
            _toasts.Clear();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            GuidePanel.Update();
            ChoicePanel.Update();

            // 更新Toast
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                _toasts[i].Update();
                if (_toasts[i].IsExpired)
                    _toasts.RemoveAt(i);
            }

            // 检测阶段变化
            if (Main.LocalPlayer != null)
            {
                var currentPhase = StoryManager.Instance.GetPhase(Main.LocalPlayer);
                if (currentPhase != _lastPhase && _lastPhase != StoryPhase.NotEntered)
                {
                    OnPhaseChanged(_lastPhase, currentPhase);
                }
                _lastPhase = currentPhase;
            }

            // 快捷键：J键切换引导面板
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J) &&
                !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J) &&
                Main.drawingPlayerChat == false)
            {
                GuidePanel.Toggle();
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(l => l.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex >= 0)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: StoryUI",
                    () =>
                    {
                        var sb = Main.spriteBatch;
                        GuidePanel.Draw(sb);
                        ChoicePanel.Draw(sb);
                        DrawToasts(sb);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        // ==================== Toast系统 ====================

        public void ShowToast(string text, Color color, float duration = 5f)
        {
            _toasts.Add(new StoryToast(text, color, duration));
            if (_toasts.Count > 5)
                _toasts.RemoveAt(0);
        }

        private void DrawToasts(SpriteBatch sb)
        {
            float y = Main.screenHeight - 200;
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                _toasts[i].Draw(sb, y);
                y -= 40;
            }
        }

        // ==================== 事件处理 ====================

        private void OnPhaseAdvanced(StoryPhaseAdvancedEvent e)
        {
            if (Main.LocalPlayer == null || Main.LocalPlayer.whoAmI != e.PlayerID) return;

            string phaseName = ((StoryPhase)e.NewPhase).ToString();
            ShowToast($"剧情推进：{phaseName}", new Color(110, 175, 220), 6f);
        }

        private void OnChoiceMade(ChoiceMadeEvent e)
        {
            string desc = ChoiceTrackerSystem.GetChoiceDescription(e.ChoiceID);
            ShowToast($"抉择已定：{desc}", new Color(200, 175, 90), 5f);
        }

        private void OnPhaseChanged(StoryPhase oldPhase, StoryPhase newPhase)
        {
            // 阶段变化时自动显示引导面板
            GuidePanel.Show();
        }

        // ==================== 鼠标处理 ====================

        public void HandleMouseClick()
        {
            if (ChoicePanel.IsChoiceActive)
            {
                ChoicePanel.HandleClick();
            }
        }

        // ==================== Toast内部类 ====================

        private class StoryToast
        {
            private readonly string _text;
            private readonly Color _color;
            private readonly float _duration;
            private float _timer;
            private float _opacity;

            public bool IsExpired => _timer >= _duration;

            public StoryToast(string text, Color color, float duration)
            {
                _text = text;
                _color = color;
                _duration = duration;
                _timer = 0f;
                _opacity = 0f;
            }

            public void Update()
            {
                _timer += 1f / 60f;
                if (_timer < 0.3f)
                    _opacity = _timer / 0.3f;
                else if (_timer > _duration - 0.5f)
                    _opacity = Math.Max(0, (_duration - _timer) / 0.5f);
                else
                    _opacity = 1f;
            }

            public void Draw(SpriteBatch sb, float y)
            {
                if (_opacity <= 0f) return;

                var font = Terraria.GameContent.FontAssets.MouseText.Value;
                var size = font.MeasureString(_text);
                int padding = 12;
                int w = (int)size.X + padding * 2;
                int h = (int)size.Y + padding;
                int x = (Main.screenWidth - w) / 2;

                // 背景
                sb.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value,
                    new Rectangle(x, (int)y, w, h),
                    new Color(28, 30, 38, (int)(220 * _opacity)));

                // 左侧装饰条
                sb.Draw(Terraria.GameContent.TextureAssets.BlackTile.Value,
                    new Rectangle(x, (int)y, 3, h),
                    _color * _opacity);

                // 文字
                Utils.DrawBorderString(sb, _text,
                    new Vector2(x + padding, (int)y + padding / 2),
                    _color * _opacity, 0.85f);
            }
        }
    }
}
