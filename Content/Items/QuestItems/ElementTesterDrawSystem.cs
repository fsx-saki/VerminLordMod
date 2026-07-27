// ============================================================
// ElementTesterDrawSystem - 元素测试武器菜单绘制系统
//
// 为什么需要这个类？
// ElementTester.HoldItem 运行在 更新阶段（Update phase），
// 此时 Main.spriteBatch 可能尚未 Begin() 或已经 End()。
// 如果在 HoldItem 中直接调用 sb.Draw(...)，会触发
// "Begin/Draw call order" 运行时错误。
//
// 解决方案：
// 通过 ModifyInterfaceLayers 在绘制阶段注入自定义绘制层，
// 此时 SpriteBatch 处于有效的 Begin/Draw/End 生命周期内。
//
// 数据流：
// 1. ElementTester.HoldItem（更新阶段）→ 右键切换 _showMenu
// 2. ElementTesterDrawSystem（绘制阶段）→ 绘制菜单 + 处理左键点击
//
// 菜单位置锁定：
// 菜单打开时，ElementTester 会基于鼠标位置计算一次位置
// 并存储在 _menuPosition 中。此后菜单固定在原位，
// 不会跟随鼠标移动，确保用户可以点击到菜单项。
//
// 点击检测在绘制阶段进行：
// 左键点击菜单项在 DrawMenu 中处理，因为绘制阶段的
// LegacyGameInterfaceLayer 可以可靠地检测鼠标点击事件，
// 不受 player.mouseInterface 状态影响。
// ============================================================
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using InnoVault;

namespace VerminLordMod.Content.Items.QuestItems
{
    /// <summary>
    /// 客户端专属 ModSystem，负责在绘制阶段渲染 ElementTester 的选择菜单
    /// 并处理左键点击菜单项事件。
    /// 仅当玩家手持 ElementTester 且菜单打开时才会绘制。
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class ElementTesterDrawSystem : ModSystem
    {
        // 上一帧左键状态（用于检测点击事件）
        private bool _prevLeft;

        // ============================================================
        // 颜色常量
        // ============================================================
        private static readonly Color BgColor = new(20, 18, 25, 240);       // 菜单背景
        private static readonly Color SelectedBg = new(50, 60, 80);         // 选中项背景
        private static readonly Color HoverBg = new(40, 40, 50);            // 悬停项背景
        private static readonly Color NormalBg = new(25, 25, 35);           // 普通项背景
        private static readonly Color SelectedText = Color.Gold;            // 选中项文字
        private static readonly Color HoverText = Color.White;              // 悬停项文字
        private static readonly Color NormalText = new(160, 170, 180);      // 普通项文字

        // ============================================================
        // 绘制层注入
        //
        // 在 "Vanilla: Mouse Text" 层之前插入自定义绘制层，
        // 确保菜单绘制在正确的 Z 顺序上。
        // ============================================================
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 找到鼠标文字层的索引，在其之前插入菜单绘制层
            int mouseTextIndex = layers.FindIndex(
                layer => layer.Name == "Vanilla: Mouse Text");

            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Element Tester Menu",
                    DrawMenu,
                    InterfaceScaleType.UI));
            }
        }

        // ============================================================
        // 菜单绘制 + 点击处理入口
        //
        // 返回值：始终返回 true，表示不阻断后续层的绘制。
        // ============================================================
        private bool DrawMenu()
        {
            // --- 前置条件检查 ---
            // 只有手持 ElementTester 且菜单打开时才绘制
            var player = Main.LocalPlayer;
            if (player?.HeldItem?.ModItem is not ElementTester tester)
                return true;
            if (!tester._showMenu)
                return true;

            var projectiles = ElementTester.GetProjectileEntries();
            var sb = Main.spriteBatch;

            // ============================================================
            // 使用 ElementTester 中锁定的菜单位置
            // ============================================================
            var (gridWidth, gridHeight) = ElementTester.GetMenuSize(projectiles.Length);
            var menuPos = tester._menuPosition;
            var menuRect = new Rectangle(menuPos.X, menuPos.Y, gridWidth, gridHeight);

            // ============================================================
            // 检测左键点击菜单项
            //
            // 使用帧间状态比较检测"按下"事件（而非 mouseLeftRelease），
            // 因为绘制阶段中 mouseLeftRelease 的语义可能不可靠。
            // ============================================================
            bool leftClick = Main.mouseLeft && !_prevLeft;
            _prevLeft = Main.mouseLeft;

            if (leftClick)
            {
                for (int i = 0; i < projectiles.Length; i++)
                {
                    var slotRect = ElementTester.GetSlotRect(i, projectiles.Length, menuPos);
                    if (slotRect.Contains(Main.MouseScreen.ToPoint()))
                    {
                        tester._selectedIndex = i;
                        tester._showMenu = false;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        break;
                    }
                }
            }

            // ============================================================
            // 绘制菜单背景
            // ============================================================
            Utils.DrawInvBG(sb, menuRect, BgColor);

            // ============================================================
            // 绘制每个弹幕选项
            // ============================================================
            for (int i = 0; i < projectiles.Length; i++)
            {
                var slotRect = ElementTester.GetSlotRect(i, projectiles.Length, menuPos);

                // 检测鼠标悬停
                bool hover = slotRect.Contains(Main.MouseScreen.ToPoint());
                bool selected = i == tester._selectedIndex;

                // --- 绘制选项背景 ---
                Color slotBg = selected ? SelectedBg : (hover ? HoverBg : NormalBg);
                sb.Draw(VaultAsset.placeholder2.Value, slotRect, slotBg);

                // --- 绘制选项文字 ---
                Color textColor = selected ? SelectedText : (hover ? HoverText : NormalText);
                Utils.DrawBorderStringFourWay(
                    sb,
                    FontAssets.MouseText.Value,
                    projectiles[i].DisplayName,
                    slotRect.X + 8,
                    slotRect.Y + 4,
                    textColor,
                    Color.Black,
                    Vector2.Zero,
                    0.85f);
            }

            return true;
        }
    }
}
