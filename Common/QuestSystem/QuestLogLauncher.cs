using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using VerminLordMod.Common.QuestSystem.Core;

namespace VerminLordMod.Common.QuestSystem
{
    public class QuestLogLauncher
    {
        public Rectangle IconRect;
        public bool IsHovered;
        private float animTimer;
        private float pulseTimer;

        public void Update(Vector2 position, bool isOpen)
        {
            int iconSize = 48;
            IconRect = new Rectangle((int)position.X, (int)position.Y, iconSize, iconSize);
            IsHovered = IconRect.Contains(Main.MouseScreen.ToPoint());

            animTimer += 0.05f;
            if (animTimer > MathHelper.TwoPi) animTimer -= MathHelper.TwoPi;
            pulseTimer += 0.04f;
            if (pulseTimer > MathHelper.TwoPi) pulseTimer -= MathHelper.TwoPi;
        }

        public void Draw(SpriteBatch spriteBatch, bool isOpen)
        {
            var texAsset = QuestLog.QuestLogIcon;
            if (texAsset?.Value == null)
                return;

            var tex = texAsset.Value;
            int frameH = tex.Height / 3;
            int frameIndex = !isOpen ? 0 : (IsHovered ? 2 : 1);
            var srcRect = new Rectangle(0, frameH * frameIndex, tex.Width, frameH);

            float breathe = IsHovered ? (float)Math.Sin(animTimer * 2f) * 0.05f + 1f : 1f;
            var drawPos = new Vector2(IconRect.X + IconRect.Width / 2f, IconRect.Y + IconRect.Height / 2f);

            // 阴影
            spriteBatch.Draw(tex, new Vector2(IconRect.X + 3, IconRect.Y + 3),
                srcRect, Color.Black * 0.6f, 0f, Vector2.Zero,
                new Vector2(IconRect.Width / (float)tex.Width, IconRect.Height / (float)frameH),
                SpriteEffects.None, 0f);

            // 发光效果（打开且悬停时）
            if (isOpen && IsHovered)
            {
                for (int i = 0; i < 3; i++)
                {
                    float layerScale = breathe * (1.2f + i * 0.15f);
                    float layerAlpha = 0.4f - i * 0.1f;
                    float pulse = (float)Math.Sin(pulseTimer + i * 0.5f) * 0.5f + 0.5f;
                    layerAlpha *= pulse;
                    Color glowColor = new Color(255, 180, 100) * layerAlpha;
                    spriteBatch.Draw(tex, drawPos, srcRect, glowColor, 0f,
                        new Vector2(tex.Width / 2f, frameH / 2f), layerScale, SpriteEffects.None, 0f);
                }
            }

            // 主图标
            spriteBatch.Draw(tex, drawPos, srcRect, Color.White, 0f,
                new Vector2(tex.Width / 2f, frameH / 2f), breathe, SpriteEffects.None, 0f);

            // 通知红点
            DrawNotificationBadge(spriteBatch);

            if (IsHovered)
            {
                string text = QuestLog.LauncherHoverText?.Value ?? "Quest Log";
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text,
                    IconRect.X + IconRect.Width + 8, IconRect.Y + 8, Color.White, Color.Black, Vector2.Zero, 1f);
            }
        }

        private void DrawNotificationBadge(SpriteBatch spriteBatch)
        {
            int count = 0;
            foreach (var quest in QuestNode.AllQuests)
            {
                if (quest.HasUnclaimedRewards)
                    count++;
            }
            if (count == 0) return;

            string text = count > 99 ? "99+" : count.ToString();
            var textSize = FontAssets.MouseText.Value.MeasureString(text) * 0.75f;
            float maxDim = Math.Max(textSize.X, textSize.Y);
            float bgSize = Math.Max(20, maxDim + 8);

            var center = new Vector2(IconRect.Right - 4, IconRect.Top + 4);

            // 外层辉光
            float glowSize = bgSize * 1.6f;
            var glowRect = new Rectangle(
                (int)(center.X - glowSize / 2), (int)(center.Y - glowSize / 2),
                (int)glowSize, (int)glowSize);
            float pulse = MathF.Sin(pulseTimer * 2f) * 0.5f + 0.5f;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, glowRect,
                new Color(200, 30, 20) * (0.2f + pulse * 0.1f));

            // 主体红点
            var mainRect = new Rectangle(
                (int)(center.X - bgSize / 2), (int)(center.Y - bgSize / 2),
                (int)bgSize, (int)bgSize);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, mainRect, new Color(220, 40, 30));

            // 数字
            var textPos = new Vector2(
                mainRect.X + mainRect.Width / 2 - textSize.X / 2,
                mainRect.Y + mainRect.Height / 2 - textSize.Y / 2);
            Utils.DrawBorderString(spriteBatch, text, textPos, Color.White, 0.75f);
        }
    }
}
