// ============================================================
// UIStyles - 统一 UI 风格定义（现代化扁平轻量风格）
// 提供所有 UI 共享的颜色、字体、布局常量
// ============================================================
using Microsoft.Xna.Framework;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// 统一 UI 风格定义 — 现代化扁平轻量风格
    /// 特点：低饱和度、柔和阴影、清晰排版、干净简洁
    /// </summary>
    public static class UIStyles
    {
        // ==================== 基础色板 ====================
        // 采用低饱和度、柔和色调，减少视觉疲劳

        /// <summary> 面板背景 — 柔和深灰 </summary>
        public static readonly Color PanelBg = new(28, 30, 38, 240);
        /// <summary> 面板背景浅色 </summary>
        public static readonly Color PanelBgLight = new(35, 38, 48, 235);
        /// <summary> 面板背景更浅 </summary>
        public static readonly Color PanelBgLighter = new(42, 45, 55, 230);
        /// <summary> 标题栏背景 </summary>
        public static readonly Color TitleBarBg = new(38, 42, 55, 235);
        /// <summary> 列表项背景 </summary>
        public static readonly Color ListItemBg = new(32, 34, 44, 220);
        /// <summary> 列表项悬停 </summary>
        public static readonly Color ListItemHover = new(45, 48, 62, 230);
        /// <summary> 列表项选中 </summary>
        public static readonly Color ListItemSelected = new(50, 65, 90, 230);

        // ==================== 边框 ====================
        /// <summary> 主边框 — 柔和蓝灰 </summary>
        public static readonly Color Border = new(55, 60, 75, 200);
        /// <summary> 浅边框 </summary>
        public static readonly Color BorderLight = new(45, 48, 60, 160);
        /// <summary> 高亮边框 </summary>
        public static readonly Color BorderAccent = new(100, 150, 210, 220);
        /// <summary> 金色高亮边框（本命蛊等） </summary>
        public static readonly Color BorderHighlight = new(200, 175, 90, 220);
        /// <summary> 输入框边框 </summary>
        public static readonly Color BorderInput = new(55, 60, 80, 180);

        // ==================== 文字 ====================
        /// <summary> 标题文字 — 暖白 </summary>
        public static readonly Color TitleText = new(235, 225, 200);
        /// <summary> 主文字 </summary>
        public static readonly Color TextMain = new(210, 212, 220);
        /// <summary> 次要文字 </summary>
        public static readonly Color TextSecondary = new(155, 158, 170);
        /// <summary> 暗淡文字 </summary>
        public static readonly Color TextDim = new(100, 102, 115);
        /// <summary> 成功 </summary>
        public static readonly Color TextSuccess = new(120, 200, 140);
        /// <summary> 警告 </summary>
        public static readonly Color TextWarning = new(220, 190, 90);
        /// <summary> 危险 </summary>
        public static readonly Color TextDanger = new(210, 90, 90);
        /// <summary> 信息 </summary>
        public static readonly Color TextInfo = new(110, 175, 220);

        // ==================== 按钮 ====================
        /// <summary> 默认按钮 </summary>
        public static readonly Color BtnDefault = new(50, 54, 68, 230);
        /// <summary> 主要按钮（合炼/拾取） </summary>
        public static readonly Color BtnPrimary = new(55, 100, 70, 230);
        /// <summary> 危险按钮（关闭） </summary>
        public static readonly Color BtnDanger = new(90, 50, 50, 230);
        /// <summary> 次要按钮 </summary>
        public static readonly Color BtnSecondary = new(50, 55, 80, 230);
        /// <summary> 选中按钮 </summary>
        public static readonly Color BtnSelected = new(60, 85, 130, 230);
        /// <summary> 禁用按钮 </summary>
        public static readonly Color BtnDisabled = new(55, 45, 45, 200);

        // ==================== 滚动条 ====================
        public static readonly Color ScrollbarBg = new(22, 24, 32, 200);
        public static readonly Color ScrollbarThumb = new(55, 60, 80, 220);
        public static readonly Color ScrollbarThumbHover = new(70, 78, 100, 230);

        // ==================== 特殊UI ====================
        /// <summary> 空窍入口按钮 </summary>
        public static readonly Color ToggleBg = new(50, 35, 70, 230);
        public static readonly Color ToggleBorder = new(100, 70, 140, 230);

        /// <summary> 真元条 </summary>
        public static readonly Color QiBarBg = new(22, 24, 38, 210);
        public static readonly Color QiBarBorder = new(50, 55, 85, 200);

        /// <summary> 狼潮条 </summary>
        public static readonly Color WolfBarBg = new(38, 22, 22, 210);
        public static readonly Color WolfBarBorder = new(85, 50, 50, 200);

        /// <summary> 战利品面板 </summary>
        public static readonly Color LootPanelBg = new(28, 28, 38, 235);

        /// <summary> 分类面板 </summary>
        public static readonly Color CategoryPanelBg = new(30, 32, 42, 220);

        // ==================== 布局常量 ====================
        public const float PanelPadding = 8f;
        public const float TitleBarHeight = 36f;
        public const float ButtonHeight = 28f;
        public const float ListItemHeight = 32f;
        public const int SlotSize = 48;
        public const int SlotPadding = 6;
        public const int MaxSlotsPerRow = 8;

        // ==================== 字体缩放 ====================
        public const float TitleScale = 1.1f;
        public const float SubTitleScale = 0.9f;
        public const float BodyScale = 0.8f;
        public const float SmallScale = 0.7f;
        public const float TinyScale = 0.6f;

        // ==================== 辅助方法 ====================

        /// <summary> 蛊虫等级颜色 </summary>
        public static Color GetGuLevelColor(int level) => level switch
        {
            1 => new Color(130, 215, 130),
            2 => new Color(130, 185, 235),
            3 => new Color(130, 215, 215),
            4 => new Color(225, 210, 90),
            5 => new Color(235, 165, 70),
            6 => new Color(215, 90, 90),
            7 => new Color(185, 110, 220),
            8 => new Color(90, 165, 205),
            9 => new Color(250, 185, 60),
            >= 10 => new Color(250, 220, 105),
            _ => new Color(145, 145, 160),
        };

        /// <summary> 稀有度颜色 </summary>
        public static Color GetRarityColor(int rare) => rare switch
        {
            -1 => new Color(130, 130, 130),
            0 => new Color(255, 255, 255),
            1 => new Color(150, 150, 255),
            2 => new Color(150, 255, 150),
            3 => new Color(255, 255, 150),
            4 => new Color(255, 150, 255),
            5 => new Color(255, 150, 50),
            6 => new Color(255, 50, 50),
            7 => new Color(210, 160, 255),
            8 => new Color(255, 255, 50),
            9 => new Color(100, 255, 100),
            _ => new Color(255, 255, 255),
        };

        /// <summary> 悬停变亮 </summary>
        public static Color HoverOver(Color c) => new(
            (byte)System.Math.Min(c.R + 25, 255),
            (byte)System.Math.Min(c.G + 25, 255),
            (byte)System.Math.Min(c.B + 25, 255), c.A);

        /// <summary> 按下变暗 </summary>
        public static Color PressDown(Color c) => new(
            (byte)System.Math.Max(c.R - 20, 0),
            (byte)System.Math.Max(c.G - 20, 0),
            (byte)System.Math.Max(c.B - 20, 0), c.A);
    }
}
