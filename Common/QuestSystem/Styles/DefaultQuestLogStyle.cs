using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using InnoVault;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.QuestSystem.Core;

namespace VerminLordMod.Common.QuestSystem.Styles
{
    public class DefaultQuestLogStyle : IQuestLogStyle
    {
        private float flowTimer;
        private float pulseTimer;
        private static Texture2D _pixel;

        private static Texture2D Pixel => _pixel ??= TextureAssets.MagicPixel.Value;

        public void UpdateStyle()
        {
            flowTimer += 0.025f;
            if (flowTimer > MathHelper.TwoPi) flowTimer -= MathHelper.TwoPi;
            pulseTimer += 0.025f;
            if (pulseTimer > MathHelper.TwoPi) pulseTimer -= MathHelper.TwoPi;
        }

        public void DrawBackground(SpriteBatch sb, QuestLog log, Rectangle rect)
        {
            float alpha = 1f;
            Color top = new(28, 18, 10);
            Color mid = new(18, 10, 6);
            Color bot = new(10, 6, 4);

            int segs = 20;
            for (int i = 0; i < segs; i++)
            {
                float t = i / (float)segs;
                float t2 = (i + 1f) / segs;
                int y1 = rect.Y + (int)(t * rect.Height);
                int y2 = rect.Y + (int)(t2 * rect.Height);
                Color c = t < 0.5f
                    ? Color.Lerp(top, mid, t * 2f)
                    : Color.Lerp(mid, bot, (t - 0.5f) * 2f);
                sb.Draw(Pixel, new Rectangle(rect.X, y1, rect.Width, Math.Max(1, y2 - y1)), c * alpha);
            }

            // 扫描线
            Color scanC = new(30, 18, 8);
            for (int y = rect.Y; y < rect.Bottom; y += 3)
                sb.Draw(Pixel, new Rectangle(rect.X + 2, y, rect.Width - 4, 1), scanC * (alpha * 0.08f));

            // 暗角
            int vigW = 30;
            for (int v = 0; v < vigW; v += 3)
            {
                float fade = (1f - v / (float)vigW);
                fade *= fade;
                Color vc = Color.Black * (alpha * 0.18f * fade);
                sb.Draw(Pixel, new Rectangle(rect.X + v, rect.Y, 2, rect.Height), vc);
                sb.Draw(Pixel, new Rectangle(rect.Right - v - 2, rect.Y, 2, rect.Height), vc);
            }
            for (int v = 0; v < 20; v += 3)
            {
                float fade = (1f - v / 20f);
                fade *= fade;
                Color vc = Color.Black * (alpha * 0.22f * fade);
                sb.Draw(Pixel, new Rectangle(rect.X, rect.Y + v, rect.Width, 2), vc);
                sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - v - 2, rect.Width, 2), vc);
            }

            // 脉冲光覆盖
            float pulse = MathF.Sin(pulseTimer * 2f) * 0.5f + 0.5f;
            Color pulseC = new(160, 70, 30);
            sb.Draw(Pixel, rect, pulseC * (0.03f * pulse * alpha));

            // 扫掠光带
            float scanY = rect.Y + (flowTimer * 0.055f % 1f) * rect.Height;
            for (int dy = -5; dy <= 5; dy++)
            {
                int py = (int)scanY + dy;
                if (py < rect.Y || py >= rect.Bottom) continue;
                float f = 1f - Math.Abs(dy) / 6f;
                Color sc = new(120, 55, 18);
                sb.Draw(Pixel, new Rectangle(rect.X + 2, py, rect.Width - 4, 1), sc * (alpha * 0.08f * f * f));
            }
        }

        public void DrawNode(SpriteBatch sb, QuestNode node, Vector2 pos, float scale, bool hovered, float alpha)
        {
            int size = (int)(48 * scale);
            int half = size / 2;
            float glowPulse = MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.5f + 0.5f;

            bool hasUnclaimed = node.HasUnclaimedRewards;
            Color core, glow, edge;

            if (node.IsCompleted && !hasUnclaimed)
            {
                core = new(45, 130, 65); glow = new(70, 200, 90); edge = new(130, 255, 155);
            }
            else if (hasUnclaimed)
            {
                core = new(160, 130, 30); glow = new(245, 210, 60); edge = new(255, 240, 120);
            }
            else if (node.IsUnlocked)
            {
                core = new(155, 85, 35); glow = new(220, 140, 55); edge = new(255, 180, 90);
            }
            else
            {
                core = new(45, 45, 55); glow = new(70, 70, 85); edge = new(110, 110, 130);
            }

            if (hovered)
            {
                core = Color.Lerp(core, Color.White, 0.25f);
                glow = Color.Lerp(glow, Color.White, 0.4f);
                edge = Color.White;
            }

            // 光晕
            if (node.IsUnlocked || node.IsCompleted)
            {
                for (int layer = 3; layer >= 1; layer--)
                {
                    int expand = layer * 5 + (hovered ? 3 : 0);
                    float la = (0.06f + glowPulse * 0.04f) / layer;
                    if (hasUnclaimed) la *= 1.6f;
                    sb.Draw(Pixel, new Rectangle((int)pos.X - half - expand, (int)pos.Y - half - expand,
                        size + expand * 2, size + expand * 2), glow * (la * alpha));
                }
            }

            // 阴影
            sb.Draw(Pixel, new Rectangle((int)pos.X - half + 3, (int)pos.Y - half + 4, size, size),
                Color.Black * (0.45f * alpha));

            // 主体渐变
            var nodeRect = new Rectangle((int)pos.X - half, (int)pos.Y - half, size, size);
            int grad = 6;
            for (int g = 0; g < grad; g++)
            {
                float gt = g / (float)grad;
                float gt2 = (g + 1f) / grad;
                int gy1 = nodeRect.Y + (int)(gt * nodeRect.Height);
                int gy2 = nodeRect.Y + (int)(gt2 * nodeRect.Height);
                float lf = 1f - gt * 0.6f;
                Color gc = new((int)(core.R * lf), (int)(core.G * lf), (int)(core.B * lf));
                sb.Draw(Pixel, new Rectangle(nodeRect.X, gy1, nodeRect.Width, Math.Max(1, gy2 - gy1)), gc * alpha);
            }

            // 顶部高光
            sb.Draw(Pixel, new Rectangle(nodeRect.X + 3, nodeRect.Y + 1, nodeRect.Width - 6, 1),
                edge * (0.3f * alpha));
            sb.Draw(Pixel, new Rectangle(nodeRect.X + 5, nodeRect.Y + 2, nodeRect.Width - 10, 1),
                edge * (0.15f * alpha));

            // 边光
            Color hl = edge * (0.4f * alpha);
            Color sh = Color.Black * (0.5f * alpha);
            int bw = hovered ? 2 : 1;
            sb.Draw(Pixel, new Rectangle(nodeRect.X, nodeRect.Y, bw, nodeRect.Height), hl);
            sb.Draw(Pixel, new Rectangle(nodeRect.X, nodeRect.Y, nodeRect.Width, bw), hl);
            sb.Draw(Pixel, new Rectangle(nodeRect.Right - bw, nodeRect.Y, bw, nodeRect.Height), sh);
            sb.Draw(Pixel, new Rectangle(nodeRect.X, nodeRect.Bottom - bw, nodeRect.Width, bw), sh);

            // 图标
            var tex = node.GetIconTexture();
            if (tex != null)
            {
                var srcRect = node.GetIconSourceRect(tex);
                var src = srcRect ?? tex.Frame();
                float iconSize = size * 0.6f;
                float sc2 = iconSize / Math.Max(src.Width, src.Height);
                sb.Draw(tex, pos, src, Color.White * alpha, 0f, src.Size() / 2f, sc2, SpriteEffects.None, 0f);
            }

            // 名称
            string name = node.DisplayName?.Value ?? node.Name;
            Color textColor;
            if (node.IsCompleted && !hasUnclaimed) textColor = new(120, 230, 145);
            else if (hasUnclaimed) textColor = new(255, 230, 100);
            else if (node.IsUnlocked) textColor = new(235, 185, 125);
            else textColor = new(125, 125, 140);
            if (hovered) textColor = Color.White;
            var nameSize = FontAssets.MouseText.Value.MeasureString(name) * 0.75f;
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, name,
                pos.X, pos.Y + half + 8, textColor * alpha, Color.Black * alpha, nameSize / 2f, 0.75f);
        }

        public void DrawConnection(SpriteBatch sb, Vector2 start, Vector2 end, bool unlocked, float alpha)
        {
            var diff = end - start;
            float len = diff.Length();
            float rot = diff.ToRotation();
            if (len < 2f) return;

            int lineW = 6;

            // 阴影
            sb.Draw(Pixel, start + new Vector2(2, 3).RotatedBy(rot),
                new Rectangle(0, 0, (int)len, lineW), Color.Black * (0.35f * alpha),
                rot, new Vector2(0, lineW / 2f), 1f, SpriteEffects.None, 0f);

            if (unlocked)
            {
                // 管道底层
                sb.Draw(Pixel, start, new Rectangle(0, 0, (int)len, lineW),
                    new Color(55, 35, 18) * (0.85f * alpha), rot, new Vector2(0, lineW / 2f), 1f,
                    SpriteEffects.None, 0f);

                // 流动渐变
                int segs = Math.Max((int)(len / 10f), 3);
                float flow = (flowTimer * 0.2f) % 1f;
                for (int i = 0; i < segs; i++)
                {
                    float t = i / (float)segs;
                    float dist = t * len;
                    var p = start + new Vector2(dist, 0).RotatedBy(rot);
                    float wave = MathF.Sin((t - flow) * MathHelper.TwoPi * 2f);
                    float bright = wave * 0.5f + 0.5f;
                    Color c = Color.Lerp(new Color(130, 65, 25), new Color(255, 170, 65), bright);
                    int segLen = (int)(len / segs) + 1;

                    sb.Draw(Pixel, p, new Rectangle(0, 0, segLen, lineW),
                        c * (0.5f * alpha), rot, new Vector2(0, lineW / 2f), 1f, SpriteEffects.None, 0f);
                    sb.Draw(Pixel, p, new Rectangle(0, 0, segLen, lineW / 2),
                        c * (0.7f * alpha * bright), rot, new Vector2(0, lineW / 4f), 1f, SpriteEffects.None, 0f);
                }

                // 外辉光
                int glowW = lineW + 8;
                sb.Draw(Pixel, start, new Rectangle(0, 0, (int)len, glowW),
                    new Color(255, 130, 50) * (0.12f * alpha), rot, new Vector2(0, glowW / 2f), 1f,
                    SpriteEffects.None, 0f);

                // 能量脉冲点
                int pulseCount = Math.Max((int)(len / 55f), 2);
                for (int i = 0; i < pulseCount; i++)
                {
                    float t = ((flowTimer * 0.5f + i * (1f / pulseCount)) % 1f);
                    var p = Vector2.Lerp(start, end, t);
                    float sz = 3f + MathF.Sin(flowTimer * 5f + i) * 1.5f;
                    sb.Draw(Pixel, p, new Rectangle(0, 0, 1, 1),
                        new Color(255, 220, 160) * (0.8f * alpha), 0f,
                        new Vector2(0.5f, 0.5f), new Vector2(sz * 2f, sz), SpriteEffects.None, 0f);
                }
            }
            else
            {
                // 虚线
                int dashLen = 12;
                int gapLen = 8;
                int total = dashLen + gapLen;
                int dashCount = (int)(len / total);
                for (int i = 0; i < dashCount; i++)
                {
                    float ds = i * total;
                    var p = start + new Vector2(ds, 0).RotatedBy(rot);
                    sb.Draw(Pixel, p, new Rectangle(0, 0, dashLen, lineW - 2),
                        new Color(60, 60, 72) * (0.5f * alpha), rot,
                        new Vector2(0, (lineW - 2) / 2f), 1f, SpriteEffects.None, 0f);
                }
            }
        }

        public Vector4 GetPadding() => new(15, 35, 15, 15);

        public Rectangle GetCloseButtonRect(Rectangle rect) =>
            new(rect.Right - 40, rect.Y + 10, 30, 30);

        public Rectangle GetRewardButtonRect(Rectangle rect) =>
            new(rect.X + rect.Width / 2 - 60, rect.Bottom - 60, 120, 35);

        public void DrawQuestDetail(SpriteBatch sb, QuestNode node, Rectangle rect, float alpha)
        {
            // 全屏遮罩
            sb.Draw(Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (0.6f * alpha));

            // 面板背景
            DrawBackground(sb, null, rect);

            int pad = 20;
            int cy = rect.Y + pad;

            // 标题
            Color titleColor = node.IsCompleted ? new(115, 220, 135) : new(225, 175, 115);
            Utils.DrawBorderString(sb, node.DisplayName?.Value ?? "",
                new Vector2(rect.X + pad, cy), titleColor * alpha, 1.2f);
            cy += (int)(FontAssets.MouseText.Value.MeasureString(node.DisplayName?.Value ?? " ").Y * 1.2f) + 8;

            // 分隔线
            int lw = rect.Width - pad * 2;
            sb.Draw(Pixel, new Rectangle(rect.X + pad, cy, lw, 1), new Color(8, 5, 2) * (alpha * 0.8f));
            sb.Draw(Pixel, new Rectangle(rect.X + pad, cy + 1, lw, 1), new Color(170, 105, 45) * (alpha * 0.35f));
            cy += 14;

            // 描述
            string desc = !string.IsNullOrEmpty(node.DetailedDescription?.Value)
                ? node.DetailedDescription.Value : node.Description?.Value;
            if (!string.IsNullOrEmpty(desc))
            {
                int maxW = rect.Width - pad * 2;
                string[] lines = VaultUtils.WrapTextArray(desc, FontAssets.MouseText.Value, (int)(maxW / 0.85f));
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    Utils.DrawBorderString(sb, line.TrimEnd('-', ' '),
                        new Vector2(rect.X + pad, cy), Color.White * alpha, 0.85f);
                    cy += (int)(FontAssets.MouseText.Value.MeasureString(line).Y * 0.85f) + 4;
                }
                cy += 10;
            }

            // 目标
            if (node.Objectives.Count > 0)
            {
                Utils.DrawBorderString(sb, QuestLog.ObjectiveText.Value + ":",
                    new Vector2(rect.X + pad, cy), new Color(215, 165, 105) * alpha, 0.9f);
                cy += 25;

                foreach (var obj in node.Objectives)
                {
                    string objText = $"• {obj.GetDisplayText()} ({obj.CurrentProgress}/{obj.RequiredProgress})";
                    Color objColor = obj.IsCompleted ? new(140, 255, 160) : Color.White;
                    Utils.DrawBorderString(sb, objText,
                        new Vector2(rect.X + pad + 10, cy), objColor * alpha, 0.8f);
                    cy += 22;
                }
                cy += 10;
            }

            // 奖励
            if (node.Rewards.Count > 0)
            {
                Utils.DrawBorderString(sb, QuestLog.RewardText.Value + ":",
                    new Vector2(rect.X + pad, cy), new Color(215, 165, 105) * alpha, 0.9f);
                cy += 25;

                int rx = rect.X + pad + 10;
                foreach (var reward in node.Rewards)
                {
                    var rewardRect = new Rectangle(rx, cy, 32, 32);
                    Color rewardColor = reward.Claimed ? new(100, 100, 110) : new(255, 200, 120);
                    sb.Draw(Pixel, rewardRect, rewardColor * (alpha * 0.3f));

                    Main.instance.LoadItem(reward.ItemType);
                    var itemTex = TextureAssets.Item[reward.ItemType].Value;
                    if (itemTex != null)
                    {
                        var frame = Main.itemAnimations[reward.ItemType] != null
                            ? Main.itemAnimations[reward.ItemType].GetFrame(itemTex)
                            : itemTex.Frame();
                        float sc = 1f;
                        if (frame.Width > 32 || frame.Height > 32)
                            sc = 32f / Math.Max(frame.Width, frame.Height);
                        sb.Draw(itemTex, new Vector2(rewardRect.X + 16, rewardRect.Y + 16),
                            frame, Color.White * alpha, 0f, frame.Size() / 2f, sc, SpriteEffects.None, 0f);
                    }

                    Utils.DrawBorderString(sb, $"x{reward.Amount}",
                        new Vector2(rx + 36, cy + 8), Color.White * alpha, 0.75f);

                    rx += 100;
                    if (rx > rect.Right - pad - 100)
                    {
                        rx = rect.X + pad + 10;
                        cy += 40;
                    }
                }
                cy += 50;
            }

            // 领取按钮
            if (node.IsCompleted && node.Rewards.Exists(r => !r.Claimed))
            {
                var btnRect = GetRewardButtonRect(rect);
                bool hover = btnRect.Contains(Main.MouseScreen.ToPoint());
                DrawMetallicButton(sb, btnRect, QuestLog.ReceiveAwardText.Value, hover, alpha);
            }
        }

        public void DrawProgressBar(SpriteBatch sb, QuestLog log, Rectangle panelRect)
        {
            int total = 0, completed = 0;
            foreach (var node in QuestNode.AllQuests)
            {
                total++;
                if (node.IsCompleted) completed++;
            }
            if (total == 0) return;
            float progress = (float)completed / total;

            int barH = 22;
            int barW = panelRect.Width - 40;
            int barX = panelRect.X + 20;
            int barY = panelRect.Y + panelRect.Height - 30;

            // 背景
            sb.Draw(Pixel, new Rectangle(barX, barY, barW, barH), new Color(20, 15, 10) * 0.9f);
            // 边框
            sb.Draw(Pixel, new Rectangle(barX, barY, barW, 1), new Color(100, 65, 30) * 0.6f);
            sb.Draw(Pixel, new Rectangle(barX, barY + barH - 1, barW, 1), new Color(40, 25, 10) * 0.6f);

            // 进度
            if (progress > 0)
            {
                int fillW = (int)((barW - 4) * progress);
                if (fillW > 0)
                {
                    for (int g = 0; g < 4; g++)
                    {
                        float gt = g / 4f;
                        float gt2 = (g + 1f) / 4f;
                        int fy1 = barY + 2 + (int)(gt * (barH - 4));
                        int fy2 = barY + 2 + (int)(gt2 * (barH - 4));
                        float lf = 1f - gt * 0.4f;
                        Color fc = new((int)(200 * lf), (int)(160 * lf), (int)(60 * lf));
                        sb.Draw(Pixel, new Rectangle(barX + 2, fy1, fillW, Math.Max(1, fy2 - fy1)), fc * 0.9f);
                    }
                }
            }

            string text = $"{QuestLog.ProgressText?.Value ?? "Progress"} {completed}/{total}";
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, text,
                barX + 8, barY + 3, Color.White, Color.Black, Vector2.Zero, 0.7f);
        }

        public Rectangle GetClaimAllButtonRect(Rectangle panelRect) =>
            new(panelRect.X + 20, panelRect.Y + panelRect.Height - 56, 120, 24);

        public Rectangle GetResetViewButtonRect(Rectangle panelRect) =>
            new(panelRect.X + 20, panelRect.Y + 10, 80, 24);

        public Rectangle GetStyleSwitchButtonRect(Rectangle panelRect) =>
            new(panelRect.X + 110, panelRect.Y + 10, 80, 24);

        private void DrawMetallicButton(SpriteBatch sb, Rectangle rect, string text, bool hover, float alpha)
        {
            Color bg = hover ? new(200, 160, 60) : new(140, 110, 40);
            Color border = hover ? new(255, 210, 80) : new(180, 140, 50);

            sb.Draw(Pixel, rect, bg * alpha);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), border * (alpha * 0.8f));
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), Color.Black * (alpha * 0.5f));
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), border * (alpha * 0.8f));
            sb.Draw(Pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), Color.Black * (alpha * 0.5f));

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
            Vector2 textPos = new(rect.X + rect.Width / 2 - textSize.X / 2, rect.Y + 4);
            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, text,
                textPos.X, textPos.Y, hover ? Color.White : Color.Lerp(Color.White, Color.Gray, 0.3f),
                Color.Black * alpha, Vector2.Zero, 0.8f);
        }


    }
}
