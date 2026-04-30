// ============================================================
// DaosUI - 道痕面板 UI（现代化扁平轻量风格）
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.DaosUI
{
    /// <summary>
    /// 道痕面板 UI — 显示玩家已领悟的道痕
    /// </summary>
    public class DaosUI : UIState
    {
        private UIPanel _mainPanel;
        private UIText _titleText;
        private UITextPanel<string> _closeButton;
        private UIList _daosList;
        private bool _escapeWasDown;

        public override void OnInitialize()
        {
            _mainPanel = UIHelper.CreatePanel(380f, 280f);
            _mainPanel.BackgroundColor = UIStyles.PanelBg;
            _mainPanel.BorderColor = UIStyles.Border;
            Append(_mainPanel);

            // 标题栏
            var titleBar = UIHelper.CreateTitleBar(370f);
            titleBar.Left.Set(5f, 0f);
            titleBar.Top.Set(5f, 0f);
            _mainPanel.Append(titleBar);

            _titleText = UIHelper.CreateTitle("道痕", 12f, 6f);
            titleBar.Append(_titleText);

            // 关闭按钮
            _closeButton = UIHelper.CreateCloseButton(370f);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            titleBar.Append(_closeButton);

            // 道痕列表
            _daosList = UIHelper.CreateUIList(10f, 48f, 360f, 220f);
            _mainPanel.Append(_daosList);

            // 占位内容
            var placeholder = UIHelper.CreateText("道痕系统开发中...", 10f, 10f, UIStyles.TextDim, 0.85f);
            _daosList.Add(placeholder);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UIHelper.UpdatePanelCenter(_mainPanel, 380f, 280f);
            if (UIHelper.CheckEscapeReleased(ref _escapeWasDown))
                CloseUI();
        }

        private void CloseUI()
        {
            ModContent.GetInstance<DaosUISystem>().ToggleUI();
        }
    }

    public class DaosUISystem : ModSystem
    {
        private UserInterface _daosUI;
        public static DaosUI DaosUIInstance;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _daosUI = new UserInterface();
                DaosUIInstance = new DaosUI();
                DaosUIInstance.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _daosUI?.Update(gameTime);
        }

        public void ToggleUI()
        {
            if (_daosUI.CurrentState == null)
                _daosUI.SetState(DaosUIInstance);
            else
                _daosUI.SetState(null);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Daos UI",
                    () =>
                    {
                        if (_daosUI?.CurrentState != null)
                            _daosUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
