// ============================================================
// DeepLootUI - 尸体战利品 UI（轻量版）
// 独立绘制，通过 SimpleUISystem 注册到游戏界面层
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
    /// - 独立绘制，不依赖 UIManager/UIState/SimplePanel 框架
    /// - 通过 SimpleUISystem.ModifyInterfaceLayers 注册绘制层
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

        private List<Item> _cachedItems = new();
        private int _corpseWhoAmI = -1;
        private bool _isOpen;

        // 按钮区域定义（用于点击检测）
        private Rectangle _takeAllRect;
        private readonly List<Rectangle> _slotRects = new();
        private readonly List<int> _slotIndices = new();

        // 鼠标状态追踪（上升沿检测）
        private bool _lastMouseLeftRelease = true;

        private CorpseLootUI() { }

        /// <summary>
        /// 打开尸体 UI
        /// </summary>
        public void Open(NpcCorpse corpse)
        {
            if (corpse == null || !corpse.Projectile.active) return;
            _isOpen = true;
            _corpseWhoAmI = corpse.Projectile.whoAmI;
            _cachedItems = new List<Item>(corpse.RemainingItems);
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
        }

        /// <summary>
        /// 每帧更新（由 SimpleUISystem 在 UpdateUI 中调用）
        /// </summary>
        public void Update()
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

            // ===== 点击检测（上升沿检测） =====
            bool leftClicked = DetectLeftClick();
            if (leftClicked)
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
        /// 鼠标左键上升沿检测
        /// </summary>
        private bool DetectLeftClick()
        {
            bool currentRelease = Main.mouseLeftRelease;
            bool clicked = !currentRelease && _lastMouseLeftRelease;
            _lastMouseLeftRelease = currentRelease;
            return clicked;
        }

        /// <summary>
        /// 绘制面板（在尸体上方）
        /// 由 SimpleUISystem 在 ModifyInterfaceLayers 中调用
        /// </summary>
        public void Draw(SpriteBatch sb)
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
            sb.Draw(TextureAssets.MagicPixel.Value, panelRect, UIStyles.PanelBg);

            // ===== 绘制面板边框 =====
            UIRendering.DrawBorder(sb, panelRect, 1, UIStyles.Border);

            // ===== 绘制标题 =====
            string titleText = $"战利品 ({itemCount})";
            Vector2 titlePos = new(panelLeft + PanelPadding, panelTop + PanelPadding);
            Utils.DrawBorderString(sb, titleText, titlePos, UIStyles.TextMain, 0.7f);

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
                hoverTakeAll ? UIStyles.HoverOver(UIStyles.BtnPrimary) : UIStyles.BtnPrimary);
            UIRendering.DrawBorder(sb, takeAllRect, 1, UIStyles.BorderAccent);

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
                    hoverSlot ? UIStyles.ListItemHover : UIStyles.ListItemBg);

                // 槽位边框
                UIRendering.DrawBorder(sb, slotRect, 1,
                    hoverSlot ? UIStyles.BorderAccent : UIStyles.BorderLight);

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
