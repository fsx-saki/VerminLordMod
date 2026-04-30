using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.UI
{
    /// <summary>
    /// 深度搜尸进度条 UI
    /// 
    /// MVA 简化：屏幕中央进度条
    /// 当玩家正在深度搜尸时显示
    /// 支持怪物尸体和玩家尸体
    /// 
    /// P1 扩展：
    /// - 更精美的圆形进度条
    /// - 显示尸体剩余物品数量
    /// - 搜尸完成动画
    /// </summary>
    public class DeepLootUI : UIState
    {
        // ===== 单例 =====
        private static DeepLootUI _instance;
        public static DeepLootUI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DeepLootUI();
                return _instance;
            }
        }

        // ===== 布局常量 =====
        private const int BarWidth = 200;
        private const int BarHeight = 20;
        private const int BarYOffset = 60; // 从屏幕中心向下偏移

        // ===== 颜色 =====
        private static readonly Color BackgroundColor = new Color(30, 30, 30, 200);
        private static readonly Color ProgressColor = new Color(255, 200, 50, 220);
        private static readonly Color BorderColor = new Color(100, 100, 100, 200);
        private static readonly Color TextColor = Color.White;

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 只在单人模式下工作（MVA 简化）
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient && Main.myPlayer == -1)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;

            // 检查玩家是否正在深度搜尸
            var lootSystem = ModContent.GetInstance<LootSystem>();
            var state = lootSystem.GetDeepLootState(player.whoAmI);
            if (state == null)
                return;

            // 计算屏幕位置（屏幕中央偏下）
            int screenWidth = Main.screenWidth;
            int screenHeight = Main.screenHeight;
            Vector2 barPos = new Vector2(
                (screenWidth - BarWidth) / 2f,
                screenHeight / 2f + BarYOffset
            );

            // 绘制背景
            Rectangle bgRect = new Rectangle(
                (int)barPos.X,
                (int)barPos.Y,
                BarWidth,
                BarHeight
            );
            spriteBatch.Draw(
                Terraria.GameContent.TextureAssets.MagicPixel.Value,
                bgRect,
                null,
                BackgroundColor
            );

            // 绘制进度
            if (state.Duration > 0)
            {
                float progress = (float)state.Elapsed / state.Duration;
                int progressWidth = (int)(BarWidth * progress);
                if (progressWidth > 0)
                {
                    Rectangle progressRect = new Rectangle(
                        (int)barPos.X,
                        (int)barPos.Y,
                        progressWidth,
                        BarHeight
                    );
                    spriteBatch.Draw(
                        Terraria.GameContent.TextureAssets.MagicPixel.Value,
                        progressRect,
                        null,
                        ProgressColor
                    );
                }
            }

            // 绘制边框
            Rectangle borderRect = new Rectangle(
                (int)barPos.X - 1,
                (int)barPos.Y - 1,
                BarWidth + 2,
                BarHeight + 2
            );
            // 使用边框绘制（上下左右四条线）
            DrawBorder(spriteBatch, borderRect, 1, BorderColor);

            // 绘制尸体名称
            string corpseName = state.Corpse?.OwnerName ?? "尸体";
            string title = $"搜刮 {corpseName}... {state.Progress:F0}%";
            Vector2 textSize = Terraria.Utils.DrawBorderString(
                spriteBatch,
                title,
                new Vector2(barPos.X + BarWidth / 2f, barPos.Y - 10),
                TextColor,
                0.8f,
                0.5f,
                1f
            );

            // 绘制提示文字
            string hint = "移动或攻击将中断搜尸";
            Terraria.Utils.DrawBorderString(
                spriteBatch,
                hint,
                new Vector2(barPos.X + BarWidth / 2f, barPos.Y + BarHeight + 5),
                Color.Gray,
                0.6f,
                0.5f,
                1f
            );
        }

        /// <summary>
        /// 绘制矩形边框
        /// </summary>
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            // 上
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), null, color);
            // 下
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), null, color);
            // 左
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), null, color);
            // 右
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), null, color);
        }
    }

    /// <summary>
    /// 深度搜尸 UI 系统（负责注册和更新）
    /// </summary>
    public class DeepLootUISystem : ModSystem
    {
        private UserInterface _deepLootUI;

        public override void Load()
        {
            _deepLootUI = new UserInterface();
            _deepLootUI.SetState(DeepLootUI.Instance);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _deepLootUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Deep Loot UI",
                    delegate
                    {
                        _deepLootUI?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }
    }
}
