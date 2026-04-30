// ============================================================
// DeepLootUI - 尸体战利品 UI（轻量版）
// 使用 UIManager 框架，显示在尸体上方
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI
{
    /// <summary>
    /// 尸体战利品 UI — 轻量版
    /// 
    /// 设计原则：
    /// - 使用 UIManager 框架（BaseUI/BasePanel），不依赖 UIState/UserInterface
    /// - 显示在尸体上方，跟随尸体位置
    /// - 纯像素绘制，无纹理依赖
    /// - 仅在有尸体打开时显示
    /// </summary>
    public class CorpseLootUI
    {
        // ===== 布局常量 =====
        private const int SlotSize = 40;
        private const int SlotPadding = 4;
        private const int MaxSlotsPerRow = 6;
        private const int PanelPadding = 8;
        private const int TitleHeight = 20;
        private const int TakeAllBtnHeight = 22;
        private const int UIOffsetY = 30; // UI 在尸体上方偏移量

        // ===== 状态 =====
        private static CorpseLootUI _instance;
        public static CorpseLootUI Instance => _instance ??= new CorpseLootUI();

        private BaseUI _baseUI;
        private BasePanel _mainPanel;
        private List<Item> _cachedItems = new();
        private int _corpseWhoAmI = -1;
        private bool _isOpen;

        // 按钮区域定义（用于点击检测）
        private Rectangle _takeAllRect;
        private readonly List<Rectangle> _slotRects = new();
        private readonly List<int> _slotIndices = new();

        private CorpseLootUI() { }

        /// <summary>
        /// 初始化 UI（由 LootSystem 在加载时调用）
        /// </summary>
        public void Initialize()
        {
            if (_baseUI != null) return;

            _baseUI = UIManager.NewUI("CorpseLootUI");
            if (_baseUI == null)
            {
                // 已存在同名 UI，查找并复用
                _baseUI = UIManager.FindUI("CorpseLootUI");
                if (_baseUI == null) return;
            }

            _mainPanel = _baseUI.NewPanel(
                texture: null,
                PanleRot: 0f,
                Pos: Vector2.Zero,
                Size: Vector2.One,
                Visible: true,
                CanDraw: true,
                FullScreen: false,
                PanelName: "CorpseLootMainPanel",
                DrawColor: Color.White,
                Draw: () => { }
            );

            if (_mainPanel != null)
            {
                _mainPanel.DrawAct = DrawPanel;
                _mainPanel.Update = UpdatePanel;
            }
        }

        /// <summary>
        /// 打开尸体 UI
        /// </summary>
        public void Open(NpcCorpse corpse)
        {
            if (corpse == null || !corpse.Projectile.active) return;
            _isOpen = true;
            _corpseWhoAmI = corpse.Projectile.whoAmI;
            _cachedItems = new List<Item>(corpse.RemainingItems);
            if (_mainPanel != null)
            {
                _mainPanel.Visible = true;
                _mainPanel.CanDraw = true;
            }
        }

        /// <summary>
        /// 关闭尸体 UI
        /// </summary>
        public void Close()
        {
            _isOpen = false;
            _corpseWhoAmI = -1;
            _cachedItems.Clear();
            _slotRects.Clear();
            _slotIndices.Clear();
            if (_mainPanel != null)
            {
                _mainPanel.Visible = false;
                _mainPanel.CanDraw = false;
            }
        }

        /// <summary>
        /// 每帧更新面板状态
        /// </summary>
        private void UpdatePanel()
        {
            if (!_isOpen) return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
            {
                Close();
                return;
            }

            var lootSystem = ModContent.GetInstance<LootSystem>();
            var corpse = lootSystem.GetOpenLootCorpse(player.whoAmI);

            if (corpse == null || !corpse.Projectile.active)
            {
                Close();
                return;
            }

            // 检查玩家是否还在交互范围内
            if (!corpse.IsPlayerNearby(player, LootSystem.CloseUIRange))
            {
                lootSystem.CloseLootUI(player);
                Close();
                return;
            }

            // 更新物品列表
            var items = corpse.RemainingItems;
            var validItems = new List<Item>();
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null && !item.IsAir)
                        validItems.Add(item);
                }
            }

            // 缓存变化时更新
            if (_cachedItems.Count != validItems.Count || _corpseWhoAmI != corpse.Projectile.whoAmI)
            {
                _cachedItems = validItems;
                _corpseWhoAmI = corpse.Projectile.whoAmI;
            }

            // 如果物品被取完，由 LootSystem.CloseLootUI 负责关闭 UI
            if (_cachedItems.Count == 0)
                return;

            // ===== 点击检测 =====
            if (UIManager.LeftClicked)
            {
                Point mouse = new(Main.mouseX, Main.mouseY);

                // 检测"全部拾取"按钮
                if (_takeAllRect.Contains(mouse))
                {
                    lootSystem.TakeAllFromCorpse(player);
                    return;
                }

                // 检测物品槽位
                for (int i = 0; i < _slotRects.Count; i++)
                {
                    if (_slotRects[i].Contains(mouse))
                    {
                        lootSystem.TakeItemFromCorpse(player, _slotIndices[i]);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 绘制面板（在尸体上方）
        /// </summary>
        private void DrawPanel()
        {
            if (!_isOpen) return;

            // 获取尸体位置
            Projectile corpseProj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].whoAmI == _corpseWhoAmI)
                {
                    corpseProj = Main.projectile[i];
                    break;
                }
            }

            if (corpseProj == null)
            {
                Close();
                return;
            }

            SpriteBatch sb = Main.spriteBatch;
            int itemCount = _cachedItems.Count;
            if (itemCount == 0) return; // 无物品不绘制（由 LootSystem 负责关闭）

            int cols = System.Math.Min(itemCount, MaxSlotsPerRow);
            int rows = (itemCount + MaxSlotsPerRow - 1) / MaxSlotsPerRow;

            int panelWidth = cols * (SlotSize + SlotPadding) - SlotPadding + PanelPadding * 2;
            int panelHeight = TitleHeight + TakeAllBtnHeight + PanelPadding + rows * (SlotSize + SlotPadding) - SlotPadding + PanelPadding;

            // 计算面板位置：尸体中心上方
            Vector2 corpseScreenPos = corpseProj.Center - Main.screenPosition;
            float panelLeft = corpseScreenPos.X - panelWidth / 2f;
            float panelTop = corpseScreenPos.Y - corpseProj.height / 2f - panelHeight - UIOffsetY;

            // 限制在屏幕范围内
            panelLeft = System.Math.Clamp(panelLeft, 10, Main.screenWidth - panelWidth - 10);
            panelTop = System.Math.Clamp(panelTop, 10, Main.screenHeight - panelHeight - 10);

            Rectangle panelRect = new((int)panelLeft, (int)panelTop, panelWidth, panelHeight);

            // ===== 绘制面板背景（半透明深色） =====
            sb.Draw(TextureAssets.MagicPixel.Value, panelRect, new Color(22, 24, 32, 230));

            // ===== 绘制面板边框 =====
            UIHelper.DrawBorder(sb, panelRect, 1, new Color(55, 60, 75, 200));

            // ===== 绘制标题 =====
            string titleText = $"📦 战利品 ({itemCount})";
            Vector2 titlePos = new(panelLeft + PanelPadding, panelTop + PanelPadding);
            Utils.DrawBorderString(sb, titleText, titlePos, new Color(210, 212, 220), 0.7f);

            // ===== 绘制"全部拾取"按钮 =====
            Rectangle takeAllRect = new(
                (int)panelLeft + PanelPadding,
                (int)panelTop + PanelPadding + TitleHeight,
                panelWidth - PanelPadding * 2,
                TakeAllBtnHeight
            );
            _takeAllRect = takeAllRect;

            bool hoverTakeAll = takeAllRect.Contains(Main.mouseX, Main.mouseY);
            sb.Draw(TextureAssets.MagicPixel.Value, takeAllRect,
                hoverTakeAll ? new Color(65, 115, 80, 230) : new Color(50, 90, 65, 220));
            UIHelper.DrawBorder(sb, takeAllRect, 1, new Color(70, 130, 90, 200));

            string takeAllText = $"全部拾取 ({itemCount})";
            Vector2 takeAllSize = FontAssets.MouseText.Value.MeasureString(takeAllText);
            Vector2 takeAllPos = new(
                takeAllRect.Center.X - takeAllSize.X * 0.35f,
                takeAllRect.Center.Y - takeAllSize.Y * 0.35f
            );
            Utils.DrawBorderString(sb, takeAllText, takeAllPos, Color.White, 0.7f, 0.5f, 0.5f);

            // ===== 绘制物品槽位 =====
            _slotRects.Clear();
            _slotIndices.Clear();

            for (int i = 0; i < itemCount; i++)
            {
                int col = i % MaxSlotsPerRow;
                int row = i / MaxSlotsPerRow;
                int slotX = (int)panelLeft + PanelPadding + col * (SlotSize + SlotPadding);
                int slotY = (int)panelTop + PanelPadding + TitleHeight + TakeAllBtnHeight + PanelPadding + row * (SlotSize + SlotPadding);

                Rectangle slotRect = new(slotX, slotY, SlotSize, SlotSize);
                _slotRects.Add(slotRect);
                _slotIndices.Add(i);

                bool hoverSlot = slotRect.Contains(Main.mouseX, Main.mouseY);

                // 槽位背景
                sb.Draw(TextureAssets.MagicPixel.Value, slotRect,
                    hoverSlot ? new Color(45, 48, 62, 230) : new Color(32, 34, 44, 220));

                // 槽位边框
                UIHelper.DrawBorder(sb, slotRect, 1,
                    hoverSlot ? new Color(100, 150, 210, 220) : new Color(45, 48, 60, 160));

                // 物品图标
                var item = _cachedItems[i];
                if (item != null && !item.IsAir && item.type != ItemID.None && item.type < TextureAssets.Item.Length)
                {
                    UIHelper.DrawItemIcon(sb, item, slotRect);

                    // 悬停显示物品名称
                    if (hoverSlot)
                    {
                        string itemName = item.Name;
                        if (item.stack > 1)
                            itemName += $" x{item.stack}";
                        Utils.DrawBorderString(sb, itemName,
                            new Vector2(Main.mouseX + 12, Main.mouseY - 10),
                            UIStyles.GetRarityColor(item.rare), 0.8f);
                    }
                }
            }
        }

        /// <summary>
        /// 检查 UI 是否打开
        /// </summary>
        public bool IsOpen => _isOpen;
    }
}
