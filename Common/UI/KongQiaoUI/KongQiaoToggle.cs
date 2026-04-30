using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 空窍入口按钮 — 常驻可拖动UI，点击打开/关闭空窍面板
    /// 现代化扁平轻量风格
    /// </summary>
    public class KongQiaoToggle : UIState
    {
        private UIPanel _dragPanel;
        private UIText _label;
        private bool _dragging;
        private Vector2 _offset;

        // 保存位置用的key
        private const string PosXKey = "KongQiaoTogglePosX";
        private const string PosYKey = "KongQiaoTogglePosY";
        private const float DefaultX = 100f;
        private const float DefaultY = 400f;

        public override void OnInitialize()
        {
            _dragPanel = new UIPanel();
            _dragPanel.Width.Set(44f, 0f);
            _dragPanel.Height.Set(44f, 0f);
            _dragPanel.Left.Set(DefaultX, 0f);
            _dragPanel.Top.Set(DefaultY, 0f);
            _dragPanel.BackgroundColor = UIStyles.ToggleBg;
            _dragPanel.BorderColor = UIStyles.ToggleBorder;
            _dragPanel.SetPadding(0f);
            _dragPanel.OnLeftMouseDown += OnDragStart;
            _dragPanel.OnLeftMouseUp += OnDragEnd;
            _dragPanel.OnLeftClick += (evt, listener) => ToggleKongQiaoUI();
            Append(_dragPanel);

            // 文字 "窍" — 居中显示
            _label = new UIText("窍", 0.9f);
            _label.Left.Set(10f, 0f);
            _label.Top.Set(10f, 0f);
            _label.TextColor = UIStyles.TitleText;
            _dragPanel.Append(_label);
        }

        private void OnDragStart(UIMouseEvent evt, UIElement listener)
        {
            _dragging = true;
            _offset = new Vector2(evt.MousePosition.X - _dragPanel.Left.Pixels, evt.MousePosition.Y - _dragPanel.Top.Pixels);
        }

        private void OnDragEnd(UIMouseEvent evt, UIElement listener)
        {
            _dragging = false;
            SavePosition();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_dragging)
            {
                _dragPanel.Left.Set(Main.mouseX - _offset.X, 0f);
                _dragPanel.Top.Set(Main.mouseY - _offset.Y, 0f);

                // 限制在屏幕内
                _dragPanel.Left.Set(MathHelper.Clamp(_dragPanel.Left.Pixels, 0, Main.screenWidth - 44), 0f);
                _dragPanel.Top.Set(MathHelper.Clamp(_dragPanel.Top.Pixels, 0, Main.screenHeight - 44), 0f);
            }

            // 更新提示文字 — 显示空窍中的蛊虫数量
            var kongQiao = Main.LocalPlayer.GetModPlayer<KongQiaoPlayer>();
            _label.SetText($"{kongQiao.UsedSlots}");
        }

        private void ToggleKongQiaoUI()
        {
            ModContent.GetInstance<KongQiaoUISystem>().ToggleUI();
        }

        private void SavePosition()
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<KongQiaoToggleSavePlayer>();
            modPlayer.TogglePosX = (int)_dragPanel.Left.Pixels;
            modPlayer.TogglePosY = (int)_dragPanel.Top.Pixels;
        }

        public void LoadPosition(float x, float y)
        {
            _dragPanel.Left.Set(x, 0f);
            _dragPanel.Top.Set(y, 0f);
        }
    }

    /// <summary>
    /// 用于保存空窍按钮位置的 ModPlayer
    /// </summary>
    public class KongQiaoToggleSavePlayer : ModPlayer
    {
        public int TogglePosX = 100;
        public int TogglePosY = 400;

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["KTToggleX"] = TogglePosX;
            tag["KTToggleY"] = TogglePosY;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            TogglePosX = tag.GetInt("KTToggleX");
            TogglePosY = tag.GetInt("KTToggleY");
        }

        public override void OnEnterWorld()
        {
            ModContent.GetInstance<KongQiaoUISystem>().RestoreTogglePosition(TogglePosX, TogglePosY);
        }
    }
}
