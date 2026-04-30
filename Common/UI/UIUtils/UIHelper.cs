// ============================================================
// UIHelper - UI 辅助工具类（现代化扁平轻量风格）
// 提供面板创建、居中定位、颜色定义、圆角绘制等
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// UI 辅助工具类 — 现代化扁平轻量风格
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// 创建主面板（使用 UIStyles 统一风格）
        /// </summary>
        public static UIPanel CreatePanel(float width, float height,
            Color? bgColor = null, Color? borderColor = null)
        {
            var panel = new UIPanel
            {
                Width = { Pixels = width },
                Height = { Pixels = height },
                Left = { Pixels = Main.screenWidth / 2f - width / 2f },
                Top = { Pixels = Main.screenHeight / 2f - height / 2f },
                BackgroundColor = bgColor ?? UIStyles.PanelBg,
                BorderColor = borderColor ?? UIStyles.Border
            };
            panel.SetPadding(0f);
            return panel;
        }

        /// <summary>
        /// 更新面板居中位置
        /// </summary>
        public static void UpdatePanelCenter(UIElement panel, float width, float height)
        {
            panel.Left.Set(Main.screenWidth / 2f - width / 2f, 0f);
            panel.Top.Set(Main.screenHeight / 2f - height / 2f, 0f);
        }

        /// <summary>
        /// 创建带内边距的子面板
        /// </summary>
        public static UIPanel CreateSubPanel(float width, float height,
            Color? bgColor = null, Color? borderColor = null, float padding = 6f)
        {
            var panel = new UIPanel
            {
                Width = { Pixels = width },
                Height = { Pixels = height },
                BackgroundColor = bgColor ?? UIStyles.PanelBgLight,
                BorderColor = borderColor ?? UIStyles.BorderLight
            };
            panel.SetPadding(padding);
            return panel;
        }

        /// <summary>
        /// 创建标题文本
        /// </summary>
        public static UIText CreateTitle(string text, float left, float top,
            Color? color = null, float scale = 1.1f)
        {
            return new UIText(text, scale)
            {
                Left = { Pixels = left },
                Top = { Pixels = top },
                TextColor = color ?? UIStyles.TitleText
            };
        }

        /// <summary>
        /// 创建普通文本
        /// </summary>
        public static UIText CreateText(string text, float left, float top,
            Color? color = null, float scale = 0.8f)
        {
            return new UIText(text, scale)
            {
                Left = { Pixels = left },
                Top = { Pixels = top },
                TextColor = color ?? UIStyles.TextMain
            };
        }

        /// <summary>
        /// 创建按钮（扁平风格）
        /// </summary>
        public static UITextPanel<string> CreateButton(string text, float width, float height,
            float left, float top, Color? bgColor = null, float textScale = 0.85f)
        {
            return new UITextPanel<string>(text, textScale)
            {
                Width = { Pixels = width },
                Height = { Pixels = height },
                Left = { Pixels = left },
                Top = { Pixels = top },
                BackgroundColor = bgColor ?? UIStyles.BtnDefault,
                BorderColor = UIStyles.BorderLight
            };
        }

        /// <summary>
        /// 创建关闭按钮（右上角小叉）
        /// </summary>
        public static UITextPanel<string> CreateCloseButton(float panelWidth, float top = 5f,
            Color? bgColor = null)
        {
            return new UITextPanel<string>("✕", 0.8f)
            {
                Width = { Pixels = 28f },
                Height = { Pixels = 26f },
                Left = { Pixels = panelWidth - 42f },
                Top = { Pixels = top },
                BackgroundColor = bgColor ?? UIStyles.BtnDanger,
                BorderColor = UIStyles.BorderLight
            };
        }

        /// <summary>
        /// 创建标题栏（顶部横条）
        /// </summary>
        public static UIPanel CreateTitleBar(float width, float height = 36f,
            Color? bgColor = null)
        {
            var bar = new UIPanel
            {
                Width = { Pixels = width },
                Height = { Pixels = height },
                Left = { Pixels = 0f },
                Top = { Pixels = 0f },
                BackgroundColor = bgColor ?? UIStyles.TitleBarBg,
                BorderColor = UIStyles.BorderLight
            };
            bar.SetPadding(0f);
            return bar;
        }

        /// <summary>
        /// 创建滚动条（扁平风格）
        /// </summary>
        public static UIScrollbar CreateScrollbar(float left, float top, float height, float width = 10f)
        {
            return new UIScrollbar
            {
                Left = { Pixels = left },
                Top = { Pixels = top },
                Width = { Pixels = width },
                Height = { Pixels = height }
            };
        }

        /// <summary>
        /// 创建列表（扁平风格）
        /// </summary>
        public static UIList CreateUIList(float left, float top, float width, float height)
        {
            return new UIList
            {
                Left = { Pixels = left },
                Top = { Pixels = top },
                Width = { Pixels = width },
                Height = { Pixels = height }
            };
        }

        /// <summary>
        /// 绘制圆角矩形边框（使用 9-patch 模拟圆角）
        /// </summary>
        public static void DrawRoundedRect(SpriteBatch sb, Rectangle rect, Color color, int radius = 4)
        {
            var pixel = TextureAssets.MagicPixel.Value;

            // 四个角
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, radius, radius), color);
            sb.Draw(pixel, new Rectangle(rect.Right - radius, rect.Y, radius, radius), color);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - radius, radius, radius), color);
            sb.Draw(pixel, new Rectangle(rect.Right - radius, rect.Bottom - radius, radius, radius), color);

            // 四条边
            sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, 1), color);
            sb.Draw(pixel, new Rectangle(rect.X + radius, rect.Bottom - 1, rect.Width - radius * 2, 1), color);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y + radius, 1, rect.Height - radius * 2), color);
            sb.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y + radius, 1, rect.Height - radius * 2), color);
        }

        /// <summary>
        /// 绘制矩形边框（简单版）
        /// </summary>
        public static void DrawBorder(SpriteBatch sb, Rectangle rect, int thickness, Color color)
        {
            var pixel = TextureAssets.MagicPixel.Value;
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            sb.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        /// <summary>
        /// 绘制物品图标（通用方法）
        /// </summary>
        public static void DrawItemIcon(SpriteBatch sb, Item item, Rectangle rect, bool showStack = true)
        {
            if (item == null || item.IsAir || item.type <= ItemID.None || item.type >= TextureAssets.Item.Length)
                return;

            var tex = TextureAssets.Item[item.type].Value;
            var frame = Main.itemAnimations[item.type]?.GetFrame(tex) ?? tex.Frame();
            float scale = System.Math.Min(rect.Width * 0.85f / frame.Width, rect.Height * 0.85f / frame.Height);
            var pos = new Vector2(rect.Center.X, rect.Center.Y) - frame.Size() * scale / 2f;

            sb.Draw(tex, pos, frame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            if (showStack && item.stack > 1)
            {
                var stackText = item.stack.ToString();
                var textSize = FontAssets.ItemStack.Value.MeasureString(stackText);
                Utils.DrawBorderString(sb, stackText,
                    new Vector2(rect.Right - textSize.X - 3, rect.Bottom - textSize.Y - 1),
                    Color.White, 0.7f);
            }
        }

        /// <summary>
        /// 检测 ESC 键释放（边沿触发）
        /// </summary>
        public static bool CheckEscapeReleased(ref bool wasDown)
        {
            bool isDown = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
            bool released = !isDown && wasDown;
            wasDown = isDown;
            return released;
        }
    }
}
