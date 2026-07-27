using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using InnoVault;
using InnoVault.UIHandles;
using static InnoVault.VaultUtils;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Common.QuestSystem.Dialogue;
using VerminLordMod.Common.QuestSystem.Styles;
using VerminLordMod.Common.YuanHaiSystem;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.QuestSystem
{
    public enum QuestTab { Home, SystemChat, Quests, Character, Archive }

    public class QuestLog : UIHandle, ILocalizedModType
    {
        [VaultLoaden("VerminLordMod/Assets/Textures/QuestLog/QuestLogIcon")]
        public static Asset<Texture2D> QuestLogIcon;

        public static QuestLog Instance => UIHandleLoader.GetUIHandleOfType<QuestLog>();

        public override bool Active => visible || openScale > 0.01f || Main.playerInventory;

        internal bool visible;
        private float mainPanelAlpha;
        private float openScale;

        public IQuestLogStyle CurrentStyle { get; set; } = new DefaultQuestLogStyle();

        public QuestTab CurrentTab = QuestTab.SystemChat;
        private bool _isDraggingTab;
        private int _dragTabIndex = -1;
        private float _dragTabMouseStartX;

        private float zoom = 1f;
        private bool isDraggingMap;
        private Vector2 panOffset;
        private Vector2 dragStartMousePos;
        private Vector2 dragStartPanOffset;
        public Vector2 PanelPosition = new(100, 100);
        public Vector2 PanelSize = new(800, 600);
        private Rectangle panelRect => new((int)PanelPosition.X, (int)PanelPosition.Y, (int)PanelSize.X, (int)PanelSize.Y);
        private bool _isDraggingPanel;
        private bool _isResizing;
        private Vector2 _dragPanelStart;
        private Vector2 _dragPanelStartMouse;
        private Vector2 _resizeStartSize;
        private const int DragHandleSize = 20;
        private const int ResizeHandleSize = 16;
        private int oldScrollWheelValue;

        private QuestNode selectedNode;
        private bool showDetailPanel;
        private float detailPanelAlpha;
        private Rectangle detailPanelRect;

        private QuestNode hoveredNode;

        private QuestLogLauncher launcher;
        public Vector2 LauncherPosition;
        private bool isDraggingLauncher;
        private Vector2 dragStartLauncherPos;
        private Vector2 dragStartMousePosForLauncher;

        public static LocalizedText ObjectiveText;
        public static LocalizedText RewardText;
        public static LocalizedText ReceiveAwardText;
        public static LocalizedText ProgressText;
        public static LocalizedText LauncherHoverText;
        public static LocalizedText ObjectiveTemplateDefeatNpc;
        public static LocalizedText ObjectiveTemplateObtainItem;
        public static LocalizedText ObjectiveTemplateCollectItem;

        public string LocalizationCategory => "UI";

        private static string GetTabName(QuestTab tab) => tab switch
        {
            QuestTab.Home => "主页",
            QuestTab.SystemChat => "系统",
            QuestTab.Quests => "任务",
            QuestTab.Character => "人物",
            QuestTab.Archive => "资料",
            _ => ""
        };

        public QuestLog()
        {
            launcher = new QuestLogLauncher();
            LauncherPosition = new Vector2(572, 108);
        }

        public override void SetStaticDefaults()
        {
            ObjectiveText = this.GetLocalization("ObjectiveText");
            RewardText = this.GetLocalization("RewardText");
            ReceiveAwardText = this.GetLocalization("ReceiveAwardText");
            ProgressText = this.GetLocalization("ProgressText");
            LauncherHoverText = this.GetLocalization("LauncherHoverText");
            ObjectiveTemplateDefeatNpc = this.GetLocalization("ObjectiveTemplateDefeatNpc");
            ObjectiveTemplateObtainItem = this.GetLocalization("ObjectiveTemplateObtainItem");
            ObjectiveTemplateCollectItem = this.GetLocalization("ObjectiveTemplateCollectItem");
        }

        public override void SaveUIData(TagCompound tag)
        {
            tag["LauncherPosX"] = LauncherPosition.X;
            tag["LauncherPosY"] = LauncherPosition.Y;
            tag["Zoom"] = zoom;
            tag["PanX"] = panOffset.X;
            tag["PanY"] = panOffset.Y;
            tag["Tab"] = (int)CurrentTab;
            tag["PanelPosX"] = PanelPosition.X;
            tag["PanelPosY"] = PanelPosition.Y;
            tag["PanelSizeX"] = PanelSize.X;
            tag["PanelSizeY"] = PanelSize.Y;
        }

        public override void LoadUIData(TagCompound tag)
        {
            if (tag.TryGet("LauncherPosX", out float lx)) LauncherPosition.X = lx;
            if (tag.TryGet("LauncherPosY", out float ly)) LauncherPosition.Y = ly;
            if (tag.TryGet("Zoom", out float z)) zoom = z;
            if (tag.TryGet("PanX", out float px)) panOffset.X = px;
            if (tag.TryGet("PanY", out float py)) panOffset.Y = py;
            if (tag.TryGet("Tab", out int tab)) CurrentTab = (QuestTab)MathHelper.Clamp(tab, 0, 4);
            if (tag.TryGet("PanelPosX", out float ppx)) PanelPosition.X = ppx;
            if (tag.TryGet("PanelPosY", out float ppy)) PanelPosition.Y = ppy;
            if (tag.TryGet("PanelSizeX", out float psx)) PanelSize.X = psx;
            if (tag.TryGet("PanelSizeY", out float psy)) PanelSize.Y = psy;
        }

        public override void LogicUpdate()
        {
            CurrentStyle?.UpdateStyle();

            // 确保资料库标签解锁（LogicUpdate始终运行）
            var dp = Main.LocalPlayer?.GetModPlayer<DialoguePlayer>();
            if (dp != null && dp.DialogueCompleted && !dp.VisibleTabs.Contains((int)QuestTab.Archive))
                dp.VisibleTabs.Add((int)QuestTab.Archive);
        }

        public override void Update()
        {
            if (visible)
            {
                openScale = MathHelper.Lerp(openScale, 1f, 0.14f);
                mainPanelAlpha = MathHelper.Lerp(mainPanelAlpha, 1f, 0.14f);
            }
            else
            {
                openScale = MathHelper.Lerp(openScale, 0f, 0.14f);
                mainPanelAlpha = MathHelper.Lerp(mainPanelAlpha, 0f, 0.14f);
            }

            if (showDetailPanel)
            {
                if (detailPanelAlpha < 1f) detailPanelAlpha += 0.1f;
            }
            else
            {
                if (detailPanelAlpha > 0f) detailPanelAlpha -= 0.1f;
            }

            // 面板位置（支持拖动）
            UIHitBox = panelRect;
            hoverInMainPage = UIHitBox.Intersects(MouseHitBox) && visible;

            if (Main.playerInventory)
            {
                if (launcher.IsHovered && !hoverInMainPage)
                {
                    if (keyLeftPressState == KeyPressState.Pressed)
                    {
                        visible = !visible;
                        if (!visible)
                        {
                            showDetailPanel = false;
                            selectedNode = null;
                        }
                        SoundEngine.PlaySound(visible ? SoundID.MenuOpen : SoundID.MenuClose);
                    }
                }
            }
            else
            {
                if (visible && Main.keyState.IsKeyDown(Keys.Escape) && Main.oldKeyState.IsKeyUp(Keys.Escape))
                {
                    if (showDetailPanel)
                    {
                        showDetailPanel = false;
                        selectedNode = null;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        visible = false;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                }
            }

            if (Main.playerInventory)
            {
                if (launcher.IsHovered)
                {
                    player.mouseInterface = true;
                    if (keyRightPressState == KeyPressState.Pressed && !isDraggingLauncher)
                    {
                        isDraggingLauncher = true;
                        dragStartLauncherPos = LauncherPosition;
                        dragStartMousePosForLauncher = Main.MouseScreen;
                    }
                }

                if (isDraggingLauncher)
                {
                    LauncherPosition = dragStartLauncherPos + Main.MouseScreen - dragStartMousePosForLauncher;
                    if (keyRightPressState == KeyPressState.Released)
                        isDraggingLauncher = false;
                }

                launcher.Update(LauncherPosition, visible);
            }

            if (openScale <= 0.01f && !visible) return;

            if (hoverInMainPage) player.mouseInterface = true;

            if (showDetailPanel && detailPanelAlpha > 0.5f && CurrentTab == QuestTab.Quests)
            {
                UpdateQuestDetail();
                return;
            }

            bool hoveredOther = false;
            var mainCloseRect = CurrentStyle.GetCloseButtonRect(panelRect);
            if (mainCloseRect.Contains(Main.MouseScreen.ToPoint()))
            {
                player.mouseInterface = true;
                hoveredOther = true;
                if (keyLeftPressState == KeyPressState.Pressed)
                {
                    visible = false;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }

            // 面板拖拽（左上角）
            var dragHandle = new Rectangle((int)PanelPosition.X, (int)PanelPosition.Y, DragHandleSize, DragHandleSize);
            if (dragHandle.Contains(Main.MouseScreen.ToPoint()))
            {
                player.mouseInterface = true;
                if (keyLeftPressState == KeyPressState.Pressed && !_isDraggingPanel && !_isResizing)
                {
                    _isDraggingPanel = true;
                    _dragPanelStart = PanelPosition;
                    _dragPanelStartMouse = Main.MouseScreen;
                }
            }
            if (_isDraggingPanel && keyLeftPressState == KeyPressState.Held)
                PanelPosition = _dragPanelStart + Main.MouseScreen - _dragPanelStartMouse;
            if (_isDraggingPanel && keyLeftPressState == KeyPressState.Released)
                _isDraggingPanel = false;

            // 面板缩放（右下角）
            var resizeHandle = new Rectangle(panelRect.Right - ResizeHandleSize, panelRect.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            if (resizeHandle.Contains(Main.MouseScreen.ToPoint()))
            {
                player.mouseInterface = true;
                if (keyLeftPressState == KeyPressState.Pressed && !_isResizing && !_isDraggingPanel)
                {
                    _isResizing = true;
                    _resizeStartSize = PanelSize;
                    _dragPanelStartMouse = Main.MouseScreen;
                }
            }
            if (_isResizing && keyLeftPressState == KeyPressState.Held)
            {
                Vector2 diff = Main.MouseScreen - _dragPanelStartMouse;
                PanelSize = new Vector2(
                    MathHelper.Max(400, _resizeStartSize.X + diff.X),
                    MathHelper.Max(300, _resizeStartSize.Y + diff.Y));
            }
            if (_isResizing && keyLeftPressState == KeyPressState.Released)
                _isResizing = false;

            // 更新可见标签页
            var dlgP = Main.LocalPlayer.GetModPlayer<DialoguePlayer>();
            var yhFlags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
            if (dlgP != null)
            {
                var tabs = dlgP.VisibleTabs;
                if (!tabs.Contains((int)QuestTab.SystemChat)) tabs.Add((int)QuestTab.SystemChat);
                if (!tabs.Contains((int)QuestTab.Quests) && QuestNode.AllQuests.Count > 0)
                    tabs.Add((int)QuestTab.Quests);
                if (!tabs.Contains((int)QuestTab.Character) && yhFlags != null && yhFlags.Activated)
                    tabs.Add((int)QuestTab.Character);
                if (!tabs.Contains((int)QuestTab.Archive) && (dlgP.DialogueCompleted || dlgP.IsDialogueComplete))
                    tabs.Add((int)QuestTab.Archive);
            }

            // 底部选项卡
            var tabList = dlgP?.VisibleTabs ?? [];
            int tabCount = tabList.Count;
            int tabW = tabCount > 0 ? panelRect.Width / tabCount : panelRect.Width;
            int tabH = 32;
            int tabY = panelRect.Bottom - tabH;
            for (int i = 0; i < tabCount; i++)
            {
                if (i >= tabList.Count) break;
                var tabRect = new Rectangle(panelRect.X + i * tabW, tabY, tabW, tabH);
                if (tabRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    player.mouseInterface = true;
                    hoveredOther = true;
                    if (keyLeftPressState == KeyPressState.Pressed)
                    {
                        if ((QuestTab)tabList[i] != CurrentTab)
                        {
                            CurrentTab = (QuestTab)tabList[i];
                            showDetailPanel = false;
                            selectedNode = null;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                    }
                    if (keyRightPressState == KeyPressState.Pressed && !_isDraggingTab)
                    {
                        _isDraggingTab = true;
                        _dragTabIndex = i;
                        _dragTabMouseStartX = Main.MouseScreen.X;
                    }
                }
            }

            // 标签拖拽
            if (_isDraggingTab && _dragTabIndex >= 0)
            {
                float dx = Main.MouseScreen.X - _dragTabMouseStartX;
                if (Math.Abs(dx) > tabW * 0.4f)
                {
                    int targetIdx = _dragTabIndex + (dx > 0 ? 1 : -1);
                    if (targetIdx >= 0 && targetIdx < tabCount && dlgP != null)
                    {
                        var item = tabList[_dragTabIndex];
                        dlgP.VisibleTabs.RemoveAt(_dragTabIndex);
                        dlgP.VisibleTabs.Insert(targetIdx, item);
                        _dragTabIndex = targetIdx;
                        _dragTabMouseStartX = Main.MouseScreen.X;
                    }
                }
                if (keyRightPressState == KeyPressState.Released)
                {
                    _isDraggingTab = false;
                    _dragTabIndex = -1;
                }
            }

            if (CurrentTab == QuestTab.Quests && hoverInMainPage && !hoveredOther)
                UpdateQuestMap();
        }

        private void UpdateQuestDetail()
        {
            detailPanelRect = new Rectangle(
                (Main.screenWidth - 500) / 2, (Main.screenHeight - 600) / 2, 500, 600);

            var closeBtn = CurrentStyle.GetCloseButtonRect(detailPanelRect);
            if (closeBtn.Contains(Main.MouseScreen.ToPoint()))
            {
                player.mouseInterface = true;
                if (keyLeftPressState == KeyPressState.Pressed)
                {
                    showDetailPanel = false;
                    selectedNode = null;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }

            if (selectedNode?.IsCompleted == true && selectedNode.Rewards.Exists(r => !r.Claimed))
            {
                var rewardBtn = CurrentStyle.GetRewardButtonRect(detailPanelRect);
                if (rewardBtn.Contains(Main.MouseScreen.ToPoint()))
                {
                    player.mouseInterface = true;
                    if (keyLeftPressState == KeyPressState.Pressed)
                    {
                        ClaimRewards(selectedNode);
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
            }
        }

        private void UpdateQuestMap()
        {
            int scroll = Mouse.GetState().ScrollWheelValue;
            if (scroll != oldScrollWheelValue)
            {
                float oldZoom = zoom;
                float newZoom = MathHelper.Clamp(zoom + (scroll > oldScrollWheelValue ? 0.1f : -0.1f), 0.4f, 2f);
                if (oldZoom != newZoom)
                {
                    Vector2 center = new(panelRect.X + panelRect.Width / 2, panelRect.Y + panelRect.Height / 2);
                    Vector2 rel = Main.MouseScreen - center;
                    panOffset = rel - (rel - panOffset) * (newZoom / oldZoom);
                    zoom = newZoom;
                }
                oldScrollWheelValue = scroll;
            }

            hoveredNode = null;
            foreach (var node in QuestNode.AllQuests)
            {
                Vector2 nodePos = GetNodeScreenPos(node.CalculatedPosition);
                float nodeSize = 24 * zoom;
                if (Vector2.Distance(Main.MouseScreen, nodePos) < nodeSize)
                {
                    hoveredNode = node;
                    break;
                }
            }

            if (keyLeftPressState == KeyPressState.Pressed)
            {
                if (hoveredNode != null)
                {
                    selectedNode = hoveredNode;
                    showDetailPanel = true;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else
                {
                    isDraggingMap = true;
                    dragStartMousePos = Main.MouseScreen;
                    dragStartPanOffset = panOffset;
                }
            }

            if (keyLeftPressState == KeyPressState.Held && isDraggingMap)
                panOffset = dragStartPanOffset + Main.MouseScreen - dragStartMousePos;

            if (keyLeftPressState == KeyPressState.Released)
                isDraggingMap = false;
        }

        private void ClaimRewards(QuestNode node)
        {
            if (node.Rewards == null) return;
            Player player = Main.LocalPlayer;
            foreach (var reward in node.Rewards)
            {
                if (!reward.Claimed)
                {
                    player.QuickSpawnItem(player.GetSource_GiftOrReward(), reward.ItemType, reward.Amount);
                    reward.Claimed = true;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory)
                launcher.Draw(spriteBatch, visible);

            if (openScale <= 0.01f && !visible) return;

            CurrentStyle.DrawBackground(spriteBatch, this, panelRect);

            // 标签栏底部
            var px = VaultAsset.placeholder2.Value;
            var tabList = Main.LocalPlayer.GetModPlayer<DialoguePlayer>()?.VisibleTabs ?? [];
            int tabCount = tabList.Count;
            int tabW = tabCount > 0 ? panelRect.Width / tabCount : panelRect.Width;
            int tabH = 32;
            int tabY = panelRect.Bottom - tabH;

            for (int i = 0; i < tabCount; i++)
            {
                if (i >= tabList.Count) break;
                var tabRect = new Rectangle(panelRect.X + i * tabW, tabY, tabW, tabH);
                bool selected = (QuestTab)tabList[i] == CurrentTab;
                bool hover = tabRect.Contains(Main.MouseScreen.ToPoint());
                Color bg = selected ? new Color(60, 50, 35) : (hover ? new Color(40, 35, 25) : new Color(25, 20, 15));
                spriteBatch.Draw(px, tabRect, bg * mainPanelAlpha);
                spriteBatch.Draw(px, new Rectangle(tabRect.X, tabRect.Y, tabRect.Width, 1), new Color(100, 80, 50) * (mainPanelAlpha * 0.5f));
                if (selected)
                    spriteBatch.Draw(px, new Rectangle(tabRect.X, tabRect.Y - 1, tabRect.Width, 2), new Color(180, 140, 60) * mainPanelAlpha);

                string tabLabel = GetTabName((QuestTab)tabList[i]);
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(tabLabel);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, tabLabel,
                    tabRect.X + tabRect.Width / 2 - textSize.X / 2, tabRect.Y + 6,
                    selected ? Color.White : Color.Lerp(Color.White, Color.Gray, 0.4f), Color.Black * mainPanelAlpha, Vector2.Zero, 0.8f);
            }

            // 内容区域（留出标签栏空间）
            var contentRect = new Rectangle(panelRect.X + 4, panelRect.Y + 4, panelRect.Width - 8, panelRect.Height - tabH - 8);

            if (CurrentTab == QuestTab.SystemChat)
                DrawSystemChatPanel(spriteBatch, contentRect);
            else if (CurrentTab == QuestTab.Quests)
                DrawQuestMapPanel(spriteBatch, contentRect);
            else if (CurrentTab == QuestTab.Character)
                DrawCharacterPanel(spriteBatch, contentRect);
            else if (CurrentTab == QuestTab.Archive)
                DrawArchivePanel(spriteBatch, contentRect);

            // 关闭按钮
            DrawMainCloseButton(spriteBatch);

            // 拖拽手柄（左上角）
            var dH = new Rectangle((int)PanelPosition.X, (int)PanelPosition.Y, DragHandleSize, DragHandleSize);
            bool hovDrag = dH.Contains(Main.MouseScreen.ToPoint());
            spriteBatch.Draw(VaultAsset.placeholder2.Value, dH, (hovDrag ? new Color(80, 80, 100) : new Color(40, 40, 55)) * mainPanelAlpha * 0.5f);
            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, "≡",
                dH.X + 3, dH.Y - 1, new Color(150, 150, 180) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 0.7f);

            // 缩放手柄（右下角）
            var rH = new Rectangle(panelRect.Right - ResizeHandleSize, panelRect.Bottom - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            bool hovRes = rH.Contains(Main.MouseScreen.ToPoint());
            spriteBatch.Draw(VaultAsset.placeholder2.Value, rH, (hovRes ? new Color(80, 80, 100) : new Color(40, 40, 55)) * mainPanelAlpha * 0.5f);
            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, "↙",
                rH.X + 2, rH.Y + 1, new Color(150, 150, 180) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 0.7f);
        }

        private void DrawReservedSlot(SpriteBatch sb, Rectangle rect, ref int cy, string title, string hint)
        {
            var px = VaultAsset.placeholder2.Value;
            int slotH = 50;
            var slotRect = new Rectangle(rect.X + 30, cy, rect.Width - 60, slotH);

            sb.Draw(px, slotRect, new Color(20, 18, 15) * (mainPanelAlpha * 0.5f));
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 1), new Color(60, 50, 35) * (mainPanelAlpha * 0.3f));

            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, title,
                slotRect.X + 10, slotRect.Y + 6, new Color(200, 180, 140) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 0.9f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, hint,
                slotRect.X + 10, slotRect.Y + 28, new Color(120, 110, 90) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 0.7f);

            cy += slotH + 8;
        }

        private void DrawPlaceholderPanel(SpriteBatch sb, Rectangle rect, string label)
        {
            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, label,
                rect.X + 20, rect.Y + 30, new Color(150, 140, 120) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 1f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "（未实装）",
                rect.X + 20, rect.Y + 70, new Color(100, 100, 100) * mainPanelAlpha, Color.Black * mainPanelAlpha, Vector2.Zero, 0.8f);
        }

        private void DrawCharacterPanel(SpriteBatch sb, Rectangle rect)
        {
            var flags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
            float alpha = mainPanelAlpha;
            var px = VaultAsset.placeholder2.Value;

            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, "◆ 人物状态",
                rect.X + 10, rect.Y + 8, new Color(180, 210, 120) * alpha, Color.Black * alpha, Vector2.Zero, 1f);

            if (flags == null || !flags.Activated)
            {
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "（元海未激活）",
                    rect.X + 30, rect.Y + 50, new Color(100, 100, 100) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);
                return;
            }

            int cy = rect.Y + 42;

            // 资质
            string gradeStr = flags.IsJiaGrade ? "甲等" : "未知";
            Color gradeColor = flags.IsJiaGrade ? new Color(180, 120, 255) : Color.Gray;
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "资质：" + gradeStr,
                rect.X + 20, cy, gradeColor * alpha, Color.Black * alpha, Vector2.Zero, 0.9f);
            cy += 30;

            // 真元条
            int barW = 200;
            int barH = 20;
            sb.Draw(px, new Rectangle(rect.X + 20, cy, barW, barH), new Color(20, 18, 25) * (alpha * 0.8f));
            float pct = flags.QiMax > 0 ? (float)flags.QiCurrent / flags.QiMax : 0f;
            int fillW = (int)((barW - 4) * MathHelper.Clamp(pct, 0f, 1f));
            if (fillW > 0)
                sb.Draw(px, new Rectangle(rect.X + 22, cy + 2, fillW, barH - 4), new Color(100, 180, 220) * (alpha * 0.9f));
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, $"真元 {flags.QiCurrent}/{flags.QiMax}",
                rect.X + 28, cy + 2, Color.White * alpha, Color.Black * alpha, Vector2.Zero, 0.75f);
            cy += 32;

            // 显示真元条开关
            var toggleRect = new Rectangle(rect.X + 20, cy, 28, 16);
            bool toggleHover = toggleRect.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, toggleRect, (QiHUD.ShowBar ? new Color(100, 180, 100) : new Color(60, 60, 60)) * alpha);
            if (Main.mouseLeft && Main.mouseLeftRelease && toggleHover)
            {
                QiHUD.ShowBar = !QiHUD.ShowBar;
                SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            }
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "战斗时显示真元条",
                rect.X + 56, cy - 1, new Color(160, 170, 180) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);
            cy += 30;

            // 预留位置
            DrawReservedSlot(sb, rect, ref cy, "■ 空窍状态", "已装备的蛊虫、空窍容量等");
            DrawReservedSlot(sb, rect, ref cy, "■ 修炼进度", "当前境界、突破所需资源等");

            // 打开元海/炼化按钮
            var yuanHaiBtn = new Rectangle(rect.X + 20, rect.Bottom - 40, 100, 28);
            bool hoverYH = yuanHaiBtn.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, yuanHaiBtn, (hoverYH ? new Color(50, 45, 60) : new Color(30, 25, 40)) * alpha);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "元海 (Y)",
                yuanHaiBtn.X + 12, yuanHaiBtn.Y + 5, new Color(180, 150, 220) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);
            if (Main.mouseLeft && Main.mouseLeftRelease && hoverYH)
                InnoVault.UIHandles.UIHandleLoader.GetUIHandleOfType<YuanHaiUI>()?.Toggle();

            var refineBtn = new Rectangle(rect.X + 128, rect.Bottom - 40, 100, 28);
            bool hoverRF = refineBtn.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, refineBtn, (hoverRF ? new Color(50, 45, 45) : new Color(30, 25, 25)) * alpha);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "炼化 (U)",
                refineBtn.X + 12, refineBtn.Y + 5, new Color(200, 180, 120) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);
            if (Main.mouseLeft && Main.mouseLeftRelease && hoverRF)
                InnoVault.UIHandles.UIHandleLoader.GetUIHandleOfType<RefiningUI>()?.Toggle();
        }

        private void DrawSystemChatPanel(SpriteBatch sb, Rectangle rect)
        {
            var dlgPlayer = Main.LocalPlayer.GetModPlayer<DialoguePlayer>();
            float alpha = mainPanelAlpha;
            var px = VaultAsset.placeholder2.Value;

            // 标题
            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, "◆ 系统通讯",
                rect.X + 10, rect.Y + 8, new Color(180, 210, 255) * alpha, Color.Black * alpha, Vector2.Zero, 1f);

            int chatTop = rect.Y + 42;
            int chatH = rect.Height - 80;

            // 聊天气泡区域
            var chatRect = new Rectangle(rect.X + 10, chatTop, rect.Width - 20, chatH);
            sb.Draw(px, chatRect, new Color(10, 10, 15) * (alpha * 0.6f));

            if (dlgPlayer.IsDialogueComplete)
            {
                DrawSystemChatCompleted(sb, rect, chatRect, alpha, px);
                return;
            }

            // 当前对话
            var node = dlgPlayer.CurrentNode;
            if (node == null) return;

            // 对话奖励物品（仅 activate_yuanhai 节点）
            bool showReward = node.Id == "activate_yuanhai";
            bool boxClaimed = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>()?.BoxClaimed ?? false;

            int textAreaH = chatRect.Height;

            int maxW = chatRect.Width - 24;
            // 计算内容总高度以判断是否需要滚动
            float contentH = 0;
            foreach (string line in node.SystemLines)
            {
                string prefix = line.StartsWith("[") ? "" : "莲 > ";
                string fullLine = prefix + line;
                string[] wrapped = VaultUtils.WrapTextArray(
                    System.Text.RegularExpressions.Regex.Replace(fullLine, @"\[item:[^\]]+\]", " [icon] "),
                    FontAssets.MouseText.Value, maxW);
                contentH += wrapped.Length * 26f;
            }
            if (!string.IsNullOrEmpty(node.ChoiceText))
                contentH += 36f;

            bool needScroll = contentH > textAreaH;
            float maxScroll = needScroll ? contentH - textAreaH : 0f;

            // 滚轮
            if (needScroll && chatRect.Contains(Main.MouseScreen.ToPoint()))
            {
                int scrollDelta = MouseScrollDelta;
                _chatScrollOffset -= scrollDelta * 0.3f;
                _chatScrollOffset = MathHelper.Clamp(_chatScrollOffset, 0f, maxScroll);
            }

            // 剪裁
            var rs = new RasterizerState { ScissorTestEnable = true };
            Vector2 clipPos = Vector2.Transform(new Vector2(chatRect.X, chatRect.Y), Main.UIScaleMatrix);
            Vector2 clipSize = Vector2.Transform(new Vector2(chatRect.Width, textAreaH), Main.UIScaleMatrix)
                - Vector2.Transform(Vector2.Zero, Main.UIScaleMatrix);
            var scissorRect = new Rectangle((int)clipPos.X, (int)clipPos.Y, (int)clipSize.X, (int)clipSize.Y);
            var origRect = sb.GraphicsDevice.ScissorRectangle;
            scissorRect = Rectangle.Intersect(scissorRect, sb.GraphicsDevice.Viewport.Bounds);

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = scissorRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, rs, null, Main.UIScaleMatrix);

            int sy = chatRect.Y + 10 - (int)_chatScrollOffset;
            foreach (string line in node.SystemLines)
            {
                string prefix = line.StartsWith("[") ? "" : "莲 > ";
                DrawDialogueLineWithItems(sb, prefix + line, chatRect.X + 12, ref sy, maxW, alpha);
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = origRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            // 后退按钮（小，位于前进上方）
            if (dlgPlayer.CanGoBack)
            {
                var backRect = new Rectangle(chatRect.X + 10, chatRect.Bottom - 82, 60, 24);
                bool hoverBack = backRect.Contains(Main.MouseScreen.ToPoint());
                sb.Draw(px, backRect, (hoverBack ? new Color(50, 50, 60) : new Color(30, 30, 40)) * alpha);
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "← 上一步",
                    backRect.X + 6, backRect.Y + 4, (hoverBack ? Color.White : new Color(160, 170, 180)) * alpha,
                    Color.Black * alpha, Vector2.Zero, 0.75f);
                if (Main.mouseLeft && Main.mouseLeftRelease && hoverBack)
                {
                    _chatScrollOffset = 0;
                    dlgPlayer.GoBack();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }

            // 前进按钮（宽，占据整个底部）
            if (!string.IsNullOrEmpty(node.ChoiceText))
            {
                var fwdRect = new Rectangle(chatRect.X + 10, chatRect.Bottom - 44, chatRect.Width - 20, 34);
                bool hoverFwd = fwdRect.Contains(Main.MouseScreen.ToPoint());
                sb.Draw(px, fwdRect, (hoverFwd ? new Color(50, 60, 75) : new Color(30, 35, 45)) * alpha);
                sb.Draw(px, new Rectangle(fwdRect.X, fwdRect.Y, fwdRect.Width, 1), new Color(80, 100, 130) * (alpha * 0.5f));
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "▸ " + node.ChoiceText,
                    fwdRect.X + 12, fwdRect.Y + 8, (hoverFwd ? Color.White : new Color(200, 220, 255)) * alpha,
                    Color.Black * alpha, Vector2.Zero, 0.85f);
                if (Main.mouseLeft && Main.mouseLeftRelease && hoverFwd)
                {
                    _chatScrollOffset = 0;
                    dlgPlayer.Advance();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }

            // 对话内嵌物品（在 activate_yuanhai 节点的文字中显示盒子）
            if (showReward && !boxClaimed)
            {
                DrawInlineItemSlot(sb, chatRect, alpha);
            }
        }

        private float _chatScrollOffset;

        private struct InlineItemInfo
        {
            public string ClassName;
            public string Description;
            public int ItemType;
            public Rectangle Bounds;
        }

        private readonly List<InlineItemInfo> _inlineItems = [];

        private void DrawDialogueLineWithItems(SpriteBatch sb, string fullLine, float x, ref int y, int maxW, float alpha)
        {
            var font = FontAssets.MouseText.Value;
            float scale = 0.85f;
            int itemSlotSize = 22;

            // Parse segments: text and [item:ClassName:desc]
            var segments = new List<(string text, string itemClass, string itemDesc)>();
            int searchStart = 0;
            while (true)
            {
                int itemStart = fullLine.IndexOf("[item:", searchStart);
                if (itemStart < 0)
                {
                    segments.Add((fullLine[searchStart..], null, null));
                    break;
                }
                if (itemStart > searchStart)
                    segments.Add((fullLine[searchStart..itemStart], null, null));

                int descStart = itemStart + 6; // after "[item:"
                int colonPos = fullLine.IndexOf(':', descStart);
                int closeBracket = fullLine.IndexOf(']', itemStart);
                if (colonPos > 0 && closeBracket > colonPos)
                {
                    string className = fullLine[descStart..colonPos];
                    string desc = fullLine[(colonPos + 1)..closeBracket];
                    segments.Add((null, className, desc));
                    searchStart = closeBracket + 1;
                }
                else
                {
                    segments.Add((fullLine[itemStart..(closeBracket + 1)], null, null));
                    searchStart = closeBracket + 1;
                }
            }

            float currentX = x;
            foreach (var seg in segments)
            {
                if (seg.text != null)
                {
                    // Text segment - check if it wraps
                    string[] wrapped = VaultUtils.WrapTextArray(seg.text, font, maxW - (int)(currentX - x));
                    for (int w = 0; w < wrapped.Length; w++)
                    {
                        if (w > 0) { currentX = x; y += 26; }
                        Utils.DrawBorderStringFourWay(sb, font, wrapped[w],
                            currentX, y, new Color(180, 210, 255) * alpha, Color.Black * alpha, Vector2.Zero, scale);
                        float textW = font.MeasureString(wrapped[w]).X * scale;
                        currentX += textW;
                    }
                }
                else if (seg.itemClass != null)
                {
                    // Item icon inline
                    int itemType = 0;
                    string displayName = seg.itemClass;
                    if (seg.itemClass == "YuanHaiBox")
                    {
                        itemType = ModContent.ItemType<Content.Items.QuestItems.YuanHaiBox>();
                        var item = new Item();
                        item.SetDefaults(itemType);
                        displayName = item.Name;
                    }

                    // Check if we need to wrap before this item
                    if (currentX + itemSlotSize + 4 > x + maxW)
                    {
                        currentX = x;
                        y += 26;
                    }

                    var slotRect = new Rectangle((int)currentX, y + 2, itemSlotSize, itemSlotSize);
                    var px = VaultAsset.placeholder2.Value;

                    sb.Draw(px, slotRect, new Color(30, 25, 20) * alpha);
                    sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 1), new Color(100, 80, 50) * alpha);
                    sb.Draw(px, new Rectangle(slotRect.X, slotRect.Bottom - 1, slotRect.Width, 1), new Color(40, 30, 20) * alpha);
                    sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, 1, slotRect.Height), new Color(100, 80, 50) * alpha);
                    sb.Draw(px, new Rectangle(slotRect.Right - 1, slotRect.Y, 1, slotRect.Height), new Color(40, 30, 20) * alpha);

                    if (itemType > 0)
                    {
                        Main.instance.LoadItem(itemType);
                        var itemTex = Terraria.GameContent.TextureAssets.Item[itemType].Value;
                        if (itemTex != null)
                        {
                            float sc = itemSlotSize * 0.7f / Math.Max(itemTex.Width, itemTex.Height);
                            sb.Draw(itemTex, new Vector2(slotRect.Center.X, slotRect.Center.Y), null, Color.White * alpha, 0f,
                                itemTex.Size() / 2f, sc, SpriteEffects.None, 0f);
                        }

                        // Hover tooltip
                        if (slotRect.Contains(Main.MouseScreen.ToPoint()))
                        {
                            var hoverItem = new Item();
                            hoverItem.SetDefaults(itemType);
                            Main.HoverItem = hoverItem.Clone();
                            Main.hoverItemName = displayName;

                            // Description tooltip
                            if (!string.IsNullOrEmpty(seg.itemDesc))
                            {
                                Main.instance.MouseText(seg.itemDesc, 0);
                            }

                            // Click to claim
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                var flags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
                                if (flags != null && !flags.BoxClaimed && itemType == ModContent.ItemType<Content.Items.QuestItems.YuanHaiBox>())
                                {
                                    flags.BoxClaimed = true;
                                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), itemType, 1);
                                    SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                                }
                            }
                        }
                    }

                    currentX += itemSlotSize + 6;
                }
            }
            y += 26;
        }

        private void DrawInlineItemSlot(SpriteBatch sb, Rectangle chatRect, float alpha)
        {
            var px = VaultAsset.placeholder2.Value;
            int itemType = ModContent.ItemType<Content.Items.QuestItems.YuanHaiBox>();
            int slotSize = 32;

            // 放在对话文字末尾行内："已经发放物品实体 [■]"
            int slotX = chatRect.X + 12;
            int slotY = chatRect.Y + 60;

            string prefix = "莲 > 指令已确认。已经发放物品实体 ";
            float textW = FontAssets.MouseText.Value.MeasureString(prefix).X * 0.85f;
            slotX = chatRect.X + 12 + (int)textW + 4;

            var slotRect = new Rectangle(slotX, slotY, slotSize, slotSize);
            sb.Draw(px, slotRect, new Color(30, 25, 20) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 1), new Color(100, 80, 50) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Bottom - 1, slotRect.Width, 1), new Color(40, 30, 20) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, 1, slotRect.Height), new Color(100, 80, 50) * alpha);
            sb.Draw(px, new Rectangle(slotRect.Right - 1, slotRect.Y, 1, slotRect.Height), new Color(40, 30, 20) * alpha);

            Main.instance.LoadItem(itemType);
            var itemTex = Terraria.GameContent.TextureAssets.Item[itemType].Value;
            if (itemTex != null)
            {
                float sc = slotSize * 0.7f / Math.Max(itemTex.Width, itemTex.Height);
                sb.Draw(itemTex, new Vector2(slotRect.Center.X, slotRect.Center.Y), null, Color.White * alpha, 0f,
                    itemTex.Size() / 2f, sc, SpriteEffects.None, 0f);
            }

            if (Main.mouseLeft && Main.mouseLeftRelease && slotRect.Contains(Main.MouseScreen.ToPoint()))
            {
                var flags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
                if (flags != null && !flags.BoxClaimed)
                {
                    flags.BoxClaimed = true;
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), itemType, 1);
                    SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                }
            }
        }

        private void DrawSystemChatCompleted(SpriteBatch sb, Rectangle rect, Rectangle chatRect, float alpha, Texture2D px)
        {
            var dlgPlayer = Main.LocalPlayer.GetModPlayer<DialoguePlayer>();

            // 确保资料库标签解锁
            if (dlgPlayer != null && dlgPlayer.DialogueCompleted && !dlgPlayer.VisibleTabs.Contains((int)QuestTab.Archive))
            {
                dlgPlayer.VisibleTabs.Add((int)QuestTab.Archive);
            }

            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "— 主线引导已完成 —",
                chatRect.X + chatRect.Width / 2 - 80, chatRect.Y + 20,
                new Color(100, 120, 140) * alpha, Color.Black * alpha, Vector2.Zero, 0.9f);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "暂无新主线。等待后续指令。",
                chatRect.X + chatRect.Width / 2 - 100, chatRect.Y + 50,
                new Color(100, 120, 140) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);

            // 物品分析槽（单独区域，可交互）
            int slotSize = 52;
            int slotY = chatRect.Bottom - 80;
            int slotX = chatRect.X + 20;
            var slotRect = new Rectangle(slotX, slotY, slotSize, slotSize);

            sb.Draw(px, slotRect, new Color(25, 20, 15) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, slotRect.Width, 1), new Color(80, 65, 40) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Bottom - 1, slotRect.Width, 1), new Color(30, 25, 15) * alpha);
            sb.Draw(px, new Rectangle(slotRect.X, slotRect.Y, 1, slotRect.Height), new Color(80, 65, 40) * alpha);
            sb.Draw(px, new Rectangle(slotRect.Right - 1, slotRect.Y, 1, slotRect.Height), new Color(30, 25, 15) * alpha);

            bool hasItem = dlgPlayer.AnalysisSlot != null && !dlgPlayer.AnalysisSlot.IsAir;

            if (hasItem)
            {
                var item = dlgPlayer.AnalysisSlot;
                Main.instance.LoadItem(item.type);
                var itemTex = Terraria.GameContent.TextureAssets.Item[item.type].Value;
                if (itemTex != null)
                {
                    float sc = slotSize * 0.7f / Math.Max(itemTex.Width, itemTex.Height);
                    sb.Draw(itemTex, new Vector2(slotRect.Center.X, slotRect.Center.Y), null, Color.White * alpha, 0f,
                        itemTex.Size() / 2f, sc, SpriteEffects.None, 0f);
                }

                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, item.Name,
                    slotRect.Right + 12, slotY + 4, new Color(200, 200, 200) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);

                // 分析按钮
                int btnY = slotY + slotSize - 30;
                var analyzeBtn = new Rectangle(chatRect.Right - 130, btnY, 120, 28);
                bool hoverBtn = analyzeBtn.Contains(Main.MouseScreen.ToPoint());
                sb.Draw(px, analyzeBtn, (hoverBtn ? new Color(50, 60, 75) : new Color(30, 35, 45)) * alpha);
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "▸ 这是什么玩意……你这也做的太粗糙了吧",
                    analyzeBtn.X + 10, analyzeBtn.Y + 5, (hoverBtn ? Color.White : new Color(200, 220, 255)) * alpha,
                    Color.Black * alpha, Vector2.Zero, 0.8f);

                if (Main.mouseLeft && Main.mouseLeftRelease && hoverBtn)
                {
                    ItemChatSystem.AnalyzeItem(item);
                    SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
                }

                if (Main.mouseLeft && Main.mouseLeftRelease && slotRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    if (hasItem && !dlgPlayer.AnalysisSlot.IsAir && Main.mouseItem.IsAir)
                    {
                        Main.mouseItem = dlgPlayer.AnalysisSlot.Clone();
                        Main.mouseItem.stack = 1;
                        dlgPlayer.AnalysisSlot.TurnToAir();
                        SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                    }
                }
            }
            else
            {
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "放入物品分析",
                    slotRect.Right + 12, slotY + 14, new Color(120, 120, 120) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);

                if (Main.mouseLeft && Main.mouseLeftRelease && slotRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    if (dlgPlayer.AnalysisSlot.IsAir)
                    {
                        Item cursorItem = Main.mouseItem;
                        if (cursorItem != null && !cursorItem.IsAir)
                        {
                            dlgPlayer.AnalysisSlot = cursorItem.Clone();
                            dlgPlayer.AnalysisSlot.stack = 1;
                            cursorItem.stack -= 1;
                            if (cursorItem.stack <= 0)
                                cursorItem.TurnToAir();
                            SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                        }
                    }
                }
            }

            // 箱子奖励（如有）
            if (!(Main.LocalPlayer.GetModPlayer<YuanHaiFlags>()?.BoxClaimed ?? true))
                DrawInlineItemSlot(sb, chatRect, alpha);
        }

        private string _selectedArchiveNodeId;
        private float _archiveScrollOffset;
        private int _oldArchiveScrollValue;
        private const float ArchiveLineH = 30f;
        private bool _archiveCategoryExpanded = true;
        private bool _archiveSubExpanded = true;

        private void DrawArchivePanel(SpriteBatch sb, Rectangle rect)
        {
            var dlgPlayer = Main.LocalPlayer.GetModPlayer<DialoguePlayer>();
            var tree = DialogueLoader.LoadedTree;
            float alpha = mainPanelAlpha;
            var px = VaultAsset.placeholder2.Value;

            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value, "◆ 资料库",
                rect.X + 10, rect.Y + 8, new Color(180, 210, 120) * alpha, Color.Black * alpha, Vector2.Zero, 1f);

            if (!Main.LocalPlayer.GetModPlayer<DialoguePlayer>().IsDialogueComplete)
            {
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "（主线引导完成后开放）",
                    rect.X + 30, rect.Y + 50, new Color(100, 100, 100) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);
                return;
            }

            if (dlgPlayer.UnlockedNodeIds.Count == 0)
            {
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "（暂无资料，请先与系统进行通讯）",
                    rect.X + 30, rect.Y + 50, new Color(100, 100, 100) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);
                return;
            }

            if (_selectedArchiveNodeId != null)
            {
                DrawArchiveDetail(sb, rect, alpha);
                return;
            }

            // 目录层级：主线 > 初入
            int catY = rect.Y + 42;
            int indent = 0;

            // 主线
            var mainCatRect = new Rectangle(rect.X + 10, catY, rect.Width - 30, 26);
            bool hoverMain = mainCatRect.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, mainCatRect, (hoverMain ? new Color(50, 45, 35) : new Color(35, 30, 20)) * alpha);
            if (Main.mouseLeft && Main.mouseLeftRelease && hoverMain)
            {
                _archiveCategoryExpanded = !_archiveCategoryExpanded;
                _archiveScrollOffset = 0;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value,
                (_archiveCategoryExpanded ? "▼" : "▶") + " 主线",
                mainCatRect.X + 8, mainCatRect.Y + 3, new Color(200, 180, 120) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);

            if (!_archiveCategoryExpanded) return;

            // 初入（子目录）
            catY += 28;
            var subCatRect = new Rectangle(rect.X + 24, catY, rect.Width - 44, 26);
            bool hoverSub = subCatRect.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, subCatRect, (hoverSub ? new Color(45, 40, 30) : new Color(30, 25, 18)) * alpha);
            if (Main.mouseLeft && Main.mouseLeftRelease && hoverSub)
            {
                _archiveSubExpanded = !_archiveSubExpanded;
                _archiveScrollOffset = 0;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value,
                (_archiveSubExpanded ? "▼" : "▶") + " 初入",
                subCatRect.X + 8, subCatRect.Y + 3, new Color(180, 170, 100) * alpha, Color.Black * alpha, Vector2.Zero, 0.8f);

            if (!_archiveSubExpanded) return;

            // List view
            int listX = rect.X + 24;
            int listY = catY + 28;
            int listW = rect.Width - 30;
            int listH = rect.Bottom - listY - 10;
            int itemCount = dlgPlayer.UnlockedNodeIds.Count;
            float contentH = itemCount * ArchiveLineH;
            bool hasScroll = contentH > listH;
            float maxScroll = hasScroll ? contentH - listH : 0f;

            // Mouse wheel
            int scrollDelta = 0;
            int currentScroll = Mouse.GetState().ScrollWheelValue;
            if (_oldArchiveScrollValue != 0)
                scrollDelta = _oldArchiveScrollValue - currentScroll;
            _oldArchiveScrollValue = currentScroll;
            if (hasScroll && new Rectangle(listX, listY, listW, listH).Contains(Main.MouseScreen.ToPoint()))
            {
                _archiveScrollOffset -= scrollDelta * 0.5f;
                _archiveScrollOffset = MathHelper.Clamp(_archiveScrollOffset, 0f, maxScroll);
            }

            // Clipping
            var rs = new RasterizerState { ScissorTestEnable = true };
            Vector2 clipPos = Vector2.Transform(new Vector2(listX, listY), Main.UIScaleMatrix);
            Vector2 clipSize = Vector2.Transform(new Vector2(listW, listH), Main.UIScaleMatrix)
                - Vector2.Transform(Vector2.Zero, Main.UIScaleMatrix);
            var scissorRect = new Rectangle((int)clipPos.X, (int)clipPos.Y, (int)clipSize.X, (int)clipSize.Y);
            var origRect = sb.GraphicsDevice.ScissorRectangle;
            scissorRect = Rectangle.Intersect(scissorRect, sb.GraphicsDevice.Viewport.Bounds);

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = scissorRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, rs, null, Main.UIScaleMatrix);

            int ay = listY - (int)_archiveScrollOffset;
            foreach (string id in dlgPlayer.UnlockedNodeIds)
            {
                var node = tree?.Get(id);
                if (node == null) continue;

                int idx = tree.GetIndex(id);
                string preview = node.SystemLines.Count > 0
                    ? (node.SystemLines[0].Length > 40 ? node.SystemLines[0][..40] + "…" : node.SystemLines[0])
                    : id;
                string label = $"  #{idx + 1}  {preview}";
                var entryRect = new Rectangle(listX + 8, ay, listW - 8, (int)ArchiveLineH);

                bool hover = entryRect.Contains(Main.MouseScreen.ToPoint());
                if (hover)
                {
                    sb.Draw(px, entryRect, new Color(40, 40, 50) * alpha);
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        _selectedArchiveNodeId = id;
                        _archiveScrollOffset = 0;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }

                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, label,
                    entryRect.X + 8, entryRect.Y + 4, hover ? Color.White : new Color(160, 170, 180) * alpha,
                    Color.Black * alpha, Vector2.Zero, 0.8f);

                ay += (int)ArchiveLineH;
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = origRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            // Scrollbar
            if (hasScroll)
            {
                int sbX = rect.Right - 16;
                int sbY = listY;
                int sbW = 6;
                int sbH = listH;

                // Track
                sb.Draw(px, new Rectangle(sbX, sbY, sbW, sbH), new Color(20, 20, 30) * (alpha * 0.6f));

                // Thumb
                float viewRatio = listH / contentH;
                int thumbH = Math.Max(20, (int)(sbH * viewRatio));
                float scrollRatio = maxScroll > 0f ? _archiveScrollOffset / maxScroll : 0f;
                int thumbY = sbY + (int)((sbH - thumbH) * scrollRatio);
                bool hoverSb = new Rectangle(sbX, sbY, sbW, sbH).Contains(Main.MouseScreen.ToPoint());
                sb.Draw(px, new Rectangle(sbX, thumbY, sbW, thumbH),
                    (hoverSb ? new Color(120, 130, 160) : new Color(70, 80, 100)) * alpha);
            }
        }

        private void DrawArchiveDetail(SpriteBatch sb, Rectangle rect, float alpha)
        {
            var dlgPlayer = Main.LocalPlayer.GetModPlayer<DialoguePlayer>();
            var tree = DialogueLoader.LoadedTree;
            var node = tree?.Get(_selectedArchiveNodeId);
            if (node == null) return;

            var px = VaultAsset.placeholder2.Value;

            // Back button
            var backRect = new Rectangle(rect.X + 10, rect.Y + 6, 60, 24);
            bool hoverBack = backRect.Contains(Main.MouseScreen.ToPoint());
            sb.Draw(px, backRect, (hoverBack ? new Color(50, 50, 60) : new Color(30, 30, 40)) * alpha);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "← 返回",
                backRect.X + 8, backRect.Y + 4, (hoverBack ? Color.White : new Color(160, 170, 180)) * alpha,
                Color.Black * alpha, Vector2.Zero, 0.75f);
            if (Main.mouseLeft && Main.mouseLeftRelease && hoverBack)
            {
                _selectedArchiveNodeId = null;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            // Node title
            Utils.DrawBorderStringFourWay(sb, FontAssets.ItemStack.Value,
                "#" + (tree.GetIndex(_selectedArchiveNodeId) + 1) + " " + (node.SystemLines.Count > 0 ? node.SystemLines[0] : ""),
                rect.X + 20, rect.Y + 40, new Color(200, 220, 255) * alpha, Color.Black * alpha, Vector2.Zero, 0.9f);

            // Full dialogue content with wrap
            int maxW = rect.Width - 40;
            int cy = rect.Y + 76;
            foreach (string line in node.SystemLines)
            {
                string prefix = line.StartsWith("[") ? "" : "莲 > ";
                string fullLine = prefix + line;
                string[] wrapped = VaultUtils.WrapTextArray(fullLine, FontAssets.MouseText.Value, maxW);
                foreach (string wl in wrapped)
                {
                    Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, wl,
                        rect.X + 20, cy, new Color(180, 210, 255) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);
                    cy += 26;
                }
            }

            // Player choice
            if (!string.IsNullOrEmpty(node.ChoiceText))
            {
                cy += 8;
                Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "▸ " + node.ChoiceText,
                    rect.X + 20, cy, new Color(140, 200, 140) * alpha, Color.Black * alpha, Vector2.Zero, 0.85f);
            }
        }

        private void DrawQuestMapPanel(SpriteBatch sb, Rectangle rect)
        {
            var rs = new RasterizerState { ScissorTestEnable = true };
            Vector2 pos = Vector2.Transform(new Vector2(rect.X, rect.Y), Main.UIScaleMatrix);
            Vector2 size = Vector2.Transform(new Vector2(rect.Width, rect.Height), Main.UIScaleMatrix)
                - Vector2.Transform(Vector2.Zero, Main.UIScaleMatrix);
            var scissorRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
            var origRect = sb.GraphicsDevice.ScissorRectangle;
            scissorRect = Rectangle.Intersect(scissorRect, sb.GraphicsDevice.Viewport.Bounds);

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = scissorRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, rs, null, Main.UIScaleMatrix);

            foreach (var node in QuestNode.AllQuests)
            {
                foreach (var parentId in node.ParentIDs)
                {
                    var parent = QuestNode.GetQuest(parentId);
                    if (parent != null)
                    {
                        Vector2 s = GetNodeScreenPos(parent.CalculatedPosition);
                        Vector2 e = GetNodeScreenPos(node.CalculatedPosition);
                        CurrentStyle.DrawConnection(sb, s, e, node.IsUnlocked, mainPanelAlpha);
                    }
                }
            }

            foreach (var node in QuestNode.AllQuests)
            {
                Vector2 np = GetNodeScreenPos(node.CalculatedPosition);
                bool hov = hoveredNode == node;
                if (node.PreDraw(sb, np, zoom, hov, mainPanelAlpha))
                    CurrentStyle.DrawNode(sb, node, np, zoom, hov, mainPanelAlpha);
                node.PostDraw(sb, np, zoom, hov, mainPanelAlpha);
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = origRect;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            if (showDetailPanel || detailPanelAlpha > 0.01f)
            {
                if (selectedNode != null)
                {
                    CurrentStyle.DrawQuestDetail(sb, selectedNode, detailPanelRect, detailPanelAlpha);
                    DrawCloseButton(sb);
                }
            }

            CurrentStyle.DrawProgressBar(sb, this, panelRect);
        }

        private void DrawMainCloseButton(SpriteBatch sb)
        {
            var rect = CurrentStyle.GetCloseButtonRect(panelRect);
            bool hovered = rect.Contains(Main.MouseScreen.ToPoint());
            var px = VaultAsset.placeholder2.Value;

            Color bgC = hovered ? new Color(80, 40, 40) * (mainPanelAlpha * 0.4f)
                : new Color(10, 10, 10) * (mainPanelAlpha * 0.35f);
            sb.Draw(px, rect, bgC);

            Color xC = hovered ? new Color(255, 100, 100) * mainPanelAlpha
                : new Color(180, 180, 180) * (mainPanelAlpha * 0.6f);
            float cx = rect.X + rect.Width / 2f;
            float cy2 = rect.Y + rect.Height / 2f;
            float xSize = rect.Width * 0.22f;
            sb.Draw(px, new Vector2(cx, cy2), null, xC,
                MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xSize * 2f, 1.5f), SpriteEffects.None, 0f);
            sb.Draw(px, new Vector2(cx, cy2), null, xC,
                -MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xSize * 2f, 1.5f), SpriteEffects.None, 0f);
        }

        private void DrawCloseButton(SpriteBatch sb)
        {
            var rect = CurrentStyle.GetCloseButtonRect(detailPanelRect);
            bool hovered = rect.Contains(Main.MouseScreen.ToPoint());
            var px = VaultAsset.placeholder2.Value;

            Color bgC = hovered ? new Color(80, 40, 40) * (detailPanelAlpha * 0.4f)
                : new Color(10, 10, 10) * (detailPanelAlpha * 0.35f);
            sb.Draw(px, rect, bgC);

            Color xC = hovered ? new Color(255, 100, 100) * detailPanelAlpha
                : new Color(180, 180, 180) * (detailPanelAlpha * 0.6f);
            float cx = rect.X + rect.Width / 2f;
            float cy2 = rect.Y + rect.Height / 2f;
            float xSize = rect.Width * 0.22f;
            sb.Draw(px, new Vector2(cx, cy2), null, xC,
                MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xSize * 2f, 1.5f), SpriteEffects.None, 0f);
            sb.Draw(px, new Vector2(cx, cy2), null, xC,
                -MathHelper.PiOver4, new Vector2(0.5f), new Vector2(xSize * 2f, 1.5f), SpriteEffects.None, 0f);
        }

        private Vector2 GetNodeScreenPos(Vector2 nodePos)
        {
            return new Vector2(panelRect.Center.X, panelRect.Center.Y) + (nodePos + panOffset) * zoom;
        }
    }
}
