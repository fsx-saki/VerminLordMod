using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 空窍面板 UI — 现代化扁平轻量风格
    /// </summary>
    public class KongQiaoUI : UIState
    {
        private UIPanel _mainPanel;
        private UIText _titleText;
        private UIText _infoText;
        private UITextPanel<string> _closeButton;
        private UIScrollbar _scrollbar;
        private UIList _slotList;

        public override void OnInitialize()
        {
            _mainPanel = UIHelper.CreatePanel(500f, 460f);
            _mainPanel.BackgroundColor = UIStyles.PanelBg;
            _mainPanel.BorderColor = UIStyles.Border;
            Append(_mainPanel);

            // 标题栏
            var titleBar = UIHelper.CreateTitleBar(490f);
            titleBar.Left.Set(5f, 0f);
            titleBar.Top.Set(5f, 0f);
            _mainPanel.Append(titleBar);

            _titleText = UIHelper.CreateTitle("空窍", 12f, 6f);
            titleBar.Append(_titleText);

            // 信息文本
            _infoText = UIHelper.CreateText("", 80f, 8f, UIStyles.TextSecondary, 0.75f);
            titleBar.Append(_infoText);

            // 合炼按钮
            var craftBtn = UIHelper.CreateButton("合炼", 50f, 26f, 330f, 5f, UIStyles.BtnPrimary, 0.75f);
            craftBtn.OnLeftClick += (evt, listener) => OpenCraftUI();
            titleBar.Append(craftBtn);

            // 关闭按钮
            _closeButton = UIHelper.CreateCloseButton(490f);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            titleBar.Append(_closeButton);

            // 滚动条
            _scrollbar = UIHelper.CreateScrollbar(480f, 48f, 400f, 10f);
            _mainPanel.Append(_scrollbar);

            // 格子列表
            _slotList = UIHelper.CreateUIList(8f, 48f, 472f, 400f);
            _slotList.SetScrollbar(_scrollbar);
            _mainPanel.Append(_slotList);
        }

        public void Refresh()
        {
            var kongQiao = Main.LocalPlayer.GetModPlayer<KongQiaoPlayer>();
            var qiResource = Main.LocalPlayer.GetModPlayer<QiResourcePlayer>();

            _infoText.SetText($"格子: {kongQiao.UsedSlots}/{kongQiao.MaxSlots}  |  真元: {kongQiao.GetTotalQiOccupation()}/{qiResource.QiAvailable}");

            _slotList.Clear();

            if (kongQiao.KongQiao.Count == 0)
            {
                var empty = UIHelper.CreateText("空窍空空如也... 右键蛊虫炼化入窍", 8f, 8f, UIStyles.TextDim, 0.85f);
                _slotList.Add(empty);
                return;
            }

            for (int i = 0; i < kongQiao.KongQiao.Count; i++)
            {
                var slot = kongQiao.KongQiao[i];
                var item = new KongQiaoSlotUI(i, slot, OnToggleActive, OnExtract);
                _slotList.Add(item);
            }
        }

        private void OnToggleActive(int slotIndex)
        {
            var kongQiao = Main.LocalPlayer.GetModPlayer<KongQiaoPlayer>();
            var slot = kongQiao.KongQiao[slotIndex];
            kongQiao.SetGuActive(slotIndex, !slot.IsActive);
            Refresh();
        }

        private void OnExtract(int slotIndex)
        {
            var kongQiao = Main.LocalPlayer.GetModPlayer<KongQiaoPlayer>();
            kongQiao.TryExtractGu(slotIndex);
            Refresh();
        }

        private void CloseUI() => ModContent.GetInstance<KongQiaoUISystem>().ToggleUI();
        private void OpenCraftUI() => ModContent.GetInstance<KongQiaoUISystem>().ToggleGuCraftUI();

        private bool _escapeWasDown = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (UIHelper.CheckEscapeReleased(ref _escapeWasDown))
                CloseUI();
            UIHelper.UpdatePanelCenter(_mainPanel, 500f, 460f);
        }
    }

    /// <summary>
    /// 单个空窍格子的UI元素 — 扁平风格
    /// </summary>
    public class KongQiaoSlotUI : UIPanel
    {
        private int _slotIndex;
        private KongQiaoSlot _slot;
        private Action<int> _onToggleActive;
        private Action<int> _onExtract;

        public KongQiaoSlotUI(int slotIndex, KongQiaoSlot slot, Action<int> onToggleActive, Action<int> onExtract)
        {
            _slotIndex = slotIndex;
            _slot = slot;
            _onToggleActive = onToggleActive;
            _onExtract = onExtract;

            Width.Set(460f, 0f);
            Height.Set(56f, 0f);
            MarginTop = 3f;

            BackgroundColor = slot.IsActive
                ? new Color(35, 55, 42, 210)
                : new Color(42, 35, 35, 210);
            BorderColor = slot.IsMainGu
                ? UIStyles.BorderHighlight
                : UIStyles.BorderLight;

            // 蛊虫图标
            Texture2D iconTex;
            if (slot.GuItem.type < TextureAssets.Item.Length && slot.GuItem.type > 0)
                iconTex = TextureAssets.Item[slot.GuItem.type].Value;
            else
                iconTex = ModContent.Request<Texture2D>("Terraria/Images/Item_0").Value;

            var icon = new UIImage(iconTex);
            icon.Left.Set(5f, 0f);
            icon.Top.Set(4f, 0f);
            icon.Width.Set(48f, 0f);
            icon.Height.Set(48f, 0f);
            icon.ScaleToFit = true;
            Append(icon);

            // 蛊虫名称
            string nameText = slot.GuItem.Name;
            if (slot.IsMainGu) nameText += " [本命]";
            var name = new UIText(nameText, 0.85f);
            name.Left.Set(58f, 0f);
            name.Top.Set(5f, 0f);
            name.TextColor = slot.IsMainGu ? UIStyles.TitleText : UIStyles.TextMain;
            Append(name);

            // 状态信息
            string statusText = slot.IsActive ? "已启用" : "已休眠";
            statusText += $"  |  忠诚: {slot.Loyalty:F1}%  |  占窍: {slot.QiOccupation}";
            var status = new UIText(statusText, 0.65f);
            status.Left.Set(58f, 0f);
            status.Top.Set(30f, 0f);
            status.TextColor = slot.IsActive ? UIStyles.TextSuccess : UIStyles.TextDim;
            Append(status);

            // 切换按钮
            var toggleBtn = new UITextPanel<string>(slot.IsActive ? "休眠" : "启用", 0.65f);
            toggleBtn.Width.Set(46f, 0f);
            toggleBtn.Height.Set(24f, 0f);
            toggleBtn.Left.Set(320f, 0f);
            toggleBtn.Top.Set(5f, 0f);
            toggleBtn.BackgroundColor = slot.IsActive ? UIStyles.BtnDanger : UIStyles.BtnPrimary;
            toggleBtn.BorderColor = UIStyles.BorderLight;
            int capturedIndex = slotIndex;
            toggleBtn.OnLeftClick += (evt, listener) => _onToggleActive?.Invoke(capturedIndex);
            Append(toggleBtn);

            // 取出按钮
            if (!slot.IsMainGu)
            {
                var extractBtn = new UITextPanel<string>("取出", 0.65f);
                extractBtn.Width.Set(46f, 0f);
                extractBtn.Height.Set(24f, 0f);
                extractBtn.Left.Set(372f, 0f);
                extractBtn.Top.Set(5f, 0f);
                extractBtn.BackgroundColor = UIStyles.BtnSecondary;
                extractBtn.BorderColor = UIStyles.BorderLight;
                extractBtn.OnLeftClick += (evt, listener) => _onExtract?.Invoke(capturedIndex);
                Append(extractBtn);
            }
        }
    }
}
