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

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 空窍面板 UI — 显示所有空窍格子、蛊虫图标、启用/休眠切换、取出按钮
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
            // 主面板
            _mainPanel = new UIPanel();
            _mainPanel.Width.Set(520f, 0f);
            _mainPanel.Height.Set(480f, 0f);
            _mainPanel.Left.Set(Main.screenWidth / 2f - 260f, 0f);
            _mainPanel.Top.Set(Main.screenHeight / 2f - 240f, 0f);
            _mainPanel.BackgroundColor = new Color(20, 30, 50, 230);
            _mainPanel.BorderColor = new Color(60, 100, 180, 255);
            Append(_mainPanel);

            // 标题
            _titleText = new UIText("空窍", 1.3f);
            _titleText.Left.Set(20f, 0f);
            _titleText.Top.Set(10f, 0f);
            _titleText.TextColor = Color.Gold;
            _mainPanel.Append(_titleText);

            // 信息文本（格子数/真元占用）
            _infoText = new UIText("", 0.8f);
            _infoText.Left.Set(20f, 0f);
            _infoText.Top.Set(40f, 0f);
            _infoText.TextColor = Color.LightGray;
            _mainPanel.Append(_infoText);

            // 关闭按钮
            _closeButton = new UITextPanel<string>("关闭 [ESC]", 0.9f);
            _closeButton.Width.Set(120f, 0f);
            _closeButton.Height.Set(30f, 0f);
            _closeButton.Left.Set(380f, 0f);
            _closeButton.Top.Set(10f, 0f);
            _closeButton.BackgroundColor = new Color(80, 30, 30);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            _mainPanel.Append(_closeButton);

            // 合炼台按钮
            var craftButton = new UITextPanel<string>("合炼", 0.8f);
            craftButton.Width.Set(60f, 0f);
            craftButton.Height.Set(30f, 0f);
            craftButton.Left.Set(260f, 0f);
            craftButton.Top.Set(10f, 0f);
            craftButton.BackgroundColor = new Color(40, 60, 40);
            craftButton.OnLeftClick += (evt, listener) => OpenCraftUI();
            _mainPanel.Append(craftButton);

            // 滚动条
            _scrollbar = new UIScrollbar();
            _scrollbar.Left.Set(490f, 0f);
            _scrollbar.Top.Set(80f, 0f);
            _scrollbar.Height.Set(380f, 0f);
            _scrollbar.Width.Set(16f, 0f);
            _mainPanel.Append(_scrollbar);

            // 格子列表
            _slotList = new UIList();
            _slotList.Left.Set(10f, 0f);
            _slotList.Top.Set(80f, 0f);
            _slotList.Width.Set(480f, 0f);
            _slotList.Height.Set(380f, 0f);
            _slotList.SetScrollbar(_scrollbar);
            _mainPanel.Append(_slotList);
        }

        /// <summary>
        /// 刷新空窍面板内容
        /// </summary>
        public void Refresh()
        {
            var kongQiao = Main.LocalPlayer.GetModPlayer<KongQiaoPlayer>();
            var qiResource = Main.LocalPlayer.GetModPlayer<QiResourcePlayer>();

            _infoText.SetText($"格子: {kongQiao.UsedSlots}/{kongQiao.MaxSlots}  |  真元占用: {kongQiao.GetTotalQiOccupation()}  |  可用真元: {qiResource.QiAvailable}");

            _slotList.Clear();

            if (kongQiao.KongQiao.Count == 0)
            {
                _slotList.Add(new UIText("空窍空空如也... 右键蛊虫炼化入窍", 0.9f));
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

        private void CloseUI()
        {
            ModContent.GetInstance<KongQiaoUISystem>().ToggleUI();
        }

        private void OpenCraftUI()
        {
            ModContent.GetInstance<KongQiaoUISystem>().ToggleGuCraftUI();
        }

        private bool _escapeWasDown = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool escapeDown = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
            if (!escapeDown && _escapeWasDown)
            {
                CloseUI();
            }
            _escapeWasDown = escapeDown;

            // 跟随屏幕
            _mainPanel.Left.Set(Main.screenWidth / 2f - 260f, 0f);
            _mainPanel.Top.Set(Main.screenHeight / 2f - 240f, 0f);
        }
    }

    /// <summary>
    /// 单个空窍格子的UI元素
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
            Height.Set(60f, 0f);
            MarginTop = 4f;

            BackgroundColor = slot.IsActive
                ? new Color(30, 60, 40, 200)
                : new Color(40, 30, 30, 200);
            BorderColor = slot.IsMainGu
                ? new Color(255, 215, 0, 200)
                : new Color(60, 60, 80, 200);

            // === 在构造函数中直接创建子元素（不依赖 OnInitialize） ===

            // 蛊虫图标（物品图标）
            Texture2D iconTex;
            if (slot.GuItem.type < TextureAssets.Item.Length && slot.GuItem.type > 0)
                iconTex = TextureAssets.Item[slot.GuItem.type].Value;
            else
                iconTex = ModContent.Request<Texture2D>("Terraria/Images/Item_0").Value;

            var icon = new UIImage(iconTex);
            icon.Left.Set(6f, 0f);
            icon.Top.Set(6f, 0f);
            icon.Width.Set(48f, 0f);
            icon.Height.Set(48f, 0f);
            icon.ScaleToFit = true;
            Append(icon);

            // 蛊虫名称
            string nameText = slot.GuItem.Name;
            if (slot.IsMainGu) nameText += " [本命]";
            var name = new UIText(nameText, 0.9f);
            name.Left.Set(60f, 0f);
            name.Top.Set(6f, 0f);
            name.TextColor = slot.IsMainGu ? Color.Gold : Color.White;
            Append(name);

            // 状态信息
            string statusText = slot.IsActive ? "已启用" : "已休眠";
            statusText += $"  |  忠诚: {slot.Loyalty:F1}%  |  占窍: {slot.QiOccupation}";
            var status = new UIText(statusText, 0.7f);
            status.Left.Set(60f, 0f);
            status.Top.Set(30f, 0f);
            status.TextColor = slot.IsActive ? Color.LightGreen : Color.Gray;
            Append(status);

            // 启用/休眠切换按钮
            var toggleBtn = new UITextPanel<string>(slot.IsActive ? "休眠" : "启用", 0.7f);
            toggleBtn.Width.Set(50f, 0f);
            toggleBtn.Height.Set(26f, 0f);
            toggleBtn.Left.Set(320f, 0f);
            toggleBtn.Top.Set(6f, 0f);
            toggleBtn.BackgroundColor = slot.IsActive
                ? new Color(80, 40, 40)
                : new Color(40, 80, 40);
            int capturedIndex = slotIndex;
            toggleBtn.OnLeftClick += (evt, listener) => _onToggleActive?.Invoke(capturedIndex);
            Append(toggleBtn);

            // 取出按钮（本命蛊不可取出）
            if (!slot.IsMainGu)
            {
                var extractBtn = new UITextPanel<string>("取出", 0.7f);
                extractBtn.Width.Set(50f, 0f);
                extractBtn.Height.Set(26f, 0f);
                extractBtn.Left.Set(380f, 0f);
                extractBtn.Top.Set(6f, 0f);
                extractBtn.BackgroundColor = new Color(40, 40, 80);
                extractBtn.OnLeftClick += (evt, listener) => _onExtract?.Invoke(capturedIndex);
                Append(extractBtn);
            }
        }
    }
}
