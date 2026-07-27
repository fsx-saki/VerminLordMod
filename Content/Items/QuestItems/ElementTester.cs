// ============================================================
// ElementTester - 元素弹幕测试武器
// 功能：右键打开选择菜单，左键发射当前选中的元素弹幕
//
// 自动发现机制：
// 本类在首次访问时通过反射扫描当前 Mod 程序集中所有
// 标记了 [ElementInfo] 的弹幕类，自动构建选择列表。
// 新增弹幕只需添加 [ElementInfo] 属性，无需修改本文件。
//
// 重要设计说明：
// 本类只负责 物品属性 + 输入逻辑（HoldItem 中的鼠标检测）。
// 菜单的 绘制 由 ElementTesterDrawSystem 在绘制阶段完成，
// 因为 HoldItem 运行在更新阶段，此时 Main.spriteBatch 可能
// 尚未 Begin() 或已经 End()，直接绘制会导致
// "Begin/Draw call order" 运行时错误。
//
// 点击检测：
// 左键点击菜单项在 ElementTesterDrawSystem.DrawMenu 中处理，
// 因为绘制阶段可以可靠地检测鼠标点击事件。
// HoldItem 只负责右键切换菜单和阻止射击。
// ============================================================
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.QuestItems
{
    /// <summary>
    /// 元素弹幕测试武器 — 用于在游戏中快速测试不同元素弹幕的表现。
    /// 右键切换选择菜单，左键发射当前选中的弹幕。
    /// 弹幕列表通过反射自动发现，无需手动维护。
    /// </summary>
    public class ElementTester : ModItem
    {
        // ============================================================
        // 弹幕条目数据结构
        // ============================================================
        /// <summary>
        /// 描述一个可选择的弹幕。
        /// </summary>
        public readonly record struct ProjectileEntry(
            string Icon,        // 图标标识（如 "F", "W", "Wi"）
            string DisplayName, // 显示名称（如 "火焰弹"）
            int    ProjType     // 弹幕类型 ID
        );

        // ============================================================
        // 自动发现的弹幕列表（延迟初始化）
        // ============================================================
        private static ProjectileEntry[] _projectiles;

        /// <summary>
        /// 获取所有已注册的元素弹幕列表。
        /// 首次访问时通过反射扫描程序集，缓存结果。
        /// </summary>
        private static ProjectileEntry[] GetProjectiles()
        {
            if (_projectiles != null)
                return _projectiles;

            // 通过反射扫描所有标记了 [ElementInfo] 的弹幕类
            var entries = new List<ProjectileEntry>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                // 检查是否标记了 ElementInfo 属性
                var attr = type.GetCustomAttribute<ElementInfoAttribute>();
                if (attr == null)
                    continue;

                // 检查是否继承自 ModProjectile（确保是有效的弹幕类）
                if (!typeof(ModProjectile).IsAssignableFrom(type))
                    continue;

                // 通过反射调用 ModContent.ProjectileType<T>() 获取弹幕类型 ID
                var projectileTypeMethod = typeof(ModContent)
                    .GetMethod(nameof(ModContent.ProjectileType), 1, []);
                if (projectileTypeMethod == null)
                    continue;
                var genericMethod = projectileTypeMethod.MakeGenericMethod(type);
                int projType = (int)genericMethod.Invoke(null, null);
                if (projType <= 0)
                    continue;

                entries.Add(new ProjectileEntry(attr.Icon, attr.DisplayName, projType));
            }

            // 按显示名称排序，保证菜单顺序一致
            _projectiles = [.. entries.OrderBy(e => e.DisplayName)];
            return _projectiles;
        }

        /// <summary>当前选中的弹幕索引（默认 0）</summary>
        internal int _selectedIndex;
        /// <summary>菜单是否打开</summary>
        internal bool _showMenu;
        /// <summary>
        /// 菜单打开时锁定的屏幕位置（左上角）。
        /// 打开时基于鼠标位置计算一次，之后固定不动，
        /// 避免菜单跟随鼠标指针导致无法点击。
        /// </summary>
        internal Point _menuPosition;

        // ============================================================
        // 基础属性
        // ============================================================
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 28;
            Item.damage = 30;
            Item.DamageType = DamageClass.Magic;
            Item.knockBack = 5f;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15; Item.useAnimation = 15;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true; Item.noMelee = true;
            Item.shootSpeed = 10f;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.rare = ItemRarityID.Purple;
        }

        // ============================================================
        // 射击逻辑
        // 生成当前选中的弹幕，并阻止默认箭矢生成
        // ============================================================
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var projs = GetProjectiles();
            if (_selectedIndex < 0 || _selectedIndex >= projs.Length)
                return false;

            // 生成当前选中的弹幕
            Projectile.NewProjectile(source, position, velocity,
                projs[_selectedIndex].ProjType, damage, knockback, player.whoAmI);
            return false; // 阻止默认射击
        }

        // ============================================================
        // 手持输入逻辑（更新阶段）
        //
        // 注意：此处只做 右键切换菜单 和 阻止射击。
        // 左键点击菜单项在 ElementTesterDrawSystem.DrawMenu 中处理。
        // ============================================================
        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            var projs = GetProjectiles();

            // --- 右键切换菜单 ---
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                _showMenu = !_showMenu;

                // 打开菜单时锁定位置（基于当前鼠标位置计算一次）
                if (_showMenu)
                    _menuPosition = CalculateMenuPosition(projs.Length);
            }

            // 菜单打开时阻止物品使用（让玩家无法在菜单打开时射击）
            if (_showMenu)
                player.mouseInterface = true;
        }

        // ============================================================
        // 工具提示
        // ============================================================
        public override void ModifyTooltips(List<TooltipLine> tools)
        {
            var projs = GetProjectiles();
            string currentName = _selectedIndex >= 0 && _selectedIndex < projs.Length
                ? projs[_selectedIndex].DisplayName
                : "无";

            tools.Add(new TooltipLine(Mod, "Info",
                $"[c/87ceeb:测试武器 - 当前: {currentName}]"));
            tools.Add(new TooltipLine(Mod, "Hint",
                "[c/aaaaaa:左键发射 右键选择弹幕]"));
        }

        // ============================================================
        // 布局计算（供 ElementTesterDrawSystem 共用）
        // ============================================================

        private const int MenuColumns = 2;
        private const int SlotWidth = 150;
        private const int SlotHeight = 28;
        private const int MenuPadding = 16;
        private const int SlotPadding = 8;
        private const int CursorOffset = 20;

        /// <summary>
        /// 计算菜单左上角位置。
        /// 基于鼠标位置，自动避免超出屏幕边界。
        /// </summary>
        internal static Point CalculateMenuPosition(int itemCount)
        {
            int gridW = MenuColumns * SlotWidth + MenuPadding;
            int gridH = ((itemCount + MenuColumns - 1) / MenuColumns) * SlotHeight + MenuPadding;

            int mx = (int)Main.MouseScreen.X + CursorOffset;
            int my = (int)Main.MouseScreen.Y + CursorOffset;

            if (mx + gridW > Main.screenWidth)  mx = Main.screenWidth  - gridW - 10;
            if (my + gridH > Main.screenHeight) my = Main.screenHeight - gridH - 10;

            return new Point(mx, my);
        }

        /// <summary>
        /// 计算指定索引的选项矩形。
        /// </summary>
        internal static Rectangle GetSlotRect(int index, int itemCount, Point menuPos)
        {
            int row = index / MenuColumns;
            int col = index % MenuColumns;
            return new Rectangle(
                menuPos.X + SlotPadding + col * SlotWidth,
                menuPos.Y + SlotPadding + row * SlotHeight,
                SlotWidth,
                SlotHeight);
        }

        /// <summary>
        /// 获取菜单网格尺寸。
        /// </summary>
        internal static (int width, int height) GetMenuSize(int itemCount)
        {
            int w = MenuColumns * SlotWidth + MenuPadding;
            int h = ((itemCount + MenuColumns - 1) / MenuColumns) * SlotHeight + MenuPadding;
            return (w, h);
        }

        // ============================================================
        // 内部辅助属性（供 ElementTesterDrawSystem 使用）
        // ============================================================
        internal static ProjectileEntry[] GetProjectileEntries() => GetProjectiles();
    }
}
