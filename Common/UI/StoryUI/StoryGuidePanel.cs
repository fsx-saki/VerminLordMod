using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.StoryUI
{
    /// <summary>
    /// 主线引导面板 — 显示当前剧情阶段、任务目标、下一步指引
    /// 灵动设计：渐入渐出动画、阶段进度条、目标追踪
    /// </summary>
    public class StoryGuidePanel
    {
        // ==================== 状态 ====================
        public bool IsVisible { get; private set; }
        public float Opacity { get; private set; }

        private float _targetOpacity;
        private const float FadeSpeed = 0.08f;

        // ==================== 布局 ====================
        private Rectangle _panelRect;
        private const int PanelWidth = 320;
        private const int PanelHeight = 260;

        // ==================== 颜色（使用UIStyles） ====================
        private static readonly Color BgColor = UIStyles.PanelBg;
        private static readonly Color TitleColor = UIStyles.TitleText;
        private static readonly Color TextColor = UIStyles.TextMain;
        private static readonly Color DimColor = UIStyles.TextSecondary;
        private static readonly Color AccentColor = UIStyles.TextInfo;
        private static readonly Color ProgressBg = new(22, 24, 38, 210);
        private static readonly Color ProgressFill = new(110, 175, 220, 230);
        private static readonly Color ProgressFillGold = new(200, 175, 90, 230);

        // ==================== 阶段进度数据 ====================
        private static readonly Dictionary<int, string> StageNames = new()
        {
            { 1, "一转·蛊师入门" }, { 2, "二转·家族争锋" },
            { 3, "三转·南疆流浪" }, { 4, "四转·义天山大战" },
            { 5, "五转·北原争霸" }, { 6, "六转·宿命大战" },
            { 7, "七转·蛊仙之路" }
        };

        private static readonly Dictionary<StoryPhase, string> PhaseDescriptions = new()
        {
            { StoryPhase.NotEntered, "你还未踏入蛊道……" },
            { StoryPhase.Arrival, "初入古月山寨，一切从这里开始。" },
            { StoryPhase.AwakeningCeremony, "参加开窍仪式，测试你的资质。" },
            { StoryPhase.SchoolTraining, "进入学堂，学习蛊师基础知识。" },
            { StoryPhase.MedicineRequest, "药堂家老需要帮助，这是获得认可的机会。" },
            { StoryPhase.JiaJinShengDeath, "贾金生之死，你需要做出选择……" },
            { StoryPhase.HuaJiuInheritance, "花酒行者的传承，隐藏在青茅山深处。" },
            { StoryPhase.FamilyRecognition, "获得古月家族的正式认可。" },
            { StoryPhase.PreTournament, "三寨大比即将开始，展示你的实力！" },
            { StoryPhase.TournamentBegin, "三寨大比进行中！" },
            { StoryPhase.TournamentFinal, "决赛——面对白凝冰！" },
            { StoryPhase.TianHeAttack, "天鹤上人来袭！保卫山寨！" },
            { StoryPhase.BaiNingBingIceSeal, "白凝冰的空窍……她做出了抉择。" },
            { StoryPhase.BloodSacrifice, "血祭之夜，方源的真面目……" },
            { StoryPhase.LeftQingMao, "离开青茅山，踏入更广阔的世界。" },
            { StoryPhase.SouthBorderArrival, "到达南疆，散修营地等待着你。" },
            { StoryPhase.ShangXinCiMeet, "遇见商心慈，了解南疆的局势。" },
            { StoryPhase.ThreeKingsInheritance, "三王传承——获得真传力量。" },
            { StoryPhase.ChunQiuChanFragment, "春秋蝉残影——时间回溯之力。" },
            { StoryPhase.SanXiuCampComplete, "散修营地主线完成。" },
            { StoryPhase.YiTianShanAppears, "义天山异变——各方势力云集！" },
            { StoryPhase.YiTianShanDungeon, "深入义天山副本。" },
            { StoryPhase.DaTongFeng, "大同风——生存挑战！" },
            { StoryPhase.FangYuanReveal, "方源暴露真面目！" },
            { StoryPhase.YiTianShanComplete, "义天山事件完成。" },
            { StoryPhase.NorthDesertArrival, "到达北原冰原。" },
            { StoryPhase.WangTingAlly, "与黑楼兰王庭结盟。" },
            { StoryPhase.ChangShengTianContact, "接触长生天势力。" },
            { StoryPhase.TaiBaiYunShengDeath, "太白云生——生死抉择。" },
            { StoryPhase.ImmortalZombieChoice, "仙僵的选择——生与死的边界。" },
            { StoryPhase.HeavenPrelude, "天庭前奏——风暴将至。" },
            { StoryPhase.DestinyWarBegin, "宿命大战开始！" },
            { StoryPhase.FourPillarsDown, "四柱已破——天庭降临！" },
            { StoryPhase.FactionChoice, "阵营选择——正道、魔道、中立。" },
            { StoryPhase.LongGongPhase1, "龙公之战——宿命的考验。" },
            { StoryPhase.ChunQiuRebirth, "春秋蝉回溯——逆转时间！" },
            { StoryPhase.LongGongPhase2, "再战龙公——宿命可改！" },
            { StoryPhase.DestinyShattered, "宿命碎裂——新的时代来临。" },
            { StoryPhase.Ascension, "升仙——踏入蛊仙之境！" },
            { StoryPhase.SevenTurnBegin, "七转——蛊仙之路开启。" },
            { StoryPhase.ApertureBuilt, "仙窍建设完成。" },
            { StoryPhase.EightTurnBegin, "八转——道主争夺。" },
            { StoryPhase.DaoLordChallenge, "挑战道主——争夺道系至尊。" },
            { StoryPhase.NineTurnBegin, "九转——尊者之战。" },
            { StoryPhase.VenerableBattle, "尊者之战——巅峰对决。" },
            { StoryPhase.TenTurnFinale, "十转终局——永生之门。" },
            { StoryPhase.EndingChosen, "结局已定。" },
        };

        // ==================== 方法 ====================

        public void Show() { IsVisible = true; _targetOpacity = 1f; }
        public void Hide() { _targetOpacity = 0f; }
        public void Toggle() { if (IsVisible && Opacity > 0.5f) Hide(); else Show(); }

        public void Update()
        {
            // 渐入渐出
            if (Opacity < _targetOpacity)
                Opacity = Math.Min(Opacity + FadeSpeed, _targetOpacity);
            else if (Opacity > _targetOpacity)
            {
                Opacity = Math.Max(Opacity - FadeSpeed, _targetOpacity);
                if (Opacity <= 0f) IsVisible = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsVisible || Opacity <= 0f) return;

            Player player = Main.LocalPlayer;
            var progress = StoryManager.Instance.GetProgress(player);
            int stage = progress.GetCurrentStage();
            string stageName = progress.GetStageDisplayName();
            string phaseDesc = PhaseDescriptions.TryGetValue(progress.CurrentPhase, out var d) ? d : "";

            // 面板位置（右上角）
            int px = Main.screenWidth - PanelWidth - 20;
            int py = 80;
            _panelRect = new Rectangle(px, py, PanelWidth, PanelHeight);

            // 绘制背景
            DrawPanelBackground(sb, _panelRect);

            float y = py + 12;
            float x = px + 16;
            float alpha = Opacity;

            // 标题：当前阶段
            Utils.DrawBorderString(sb, $"[ 剧情进度 ]", new Vector2(x, y), TitleColor * alpha, UIStyles.TitleScale);
            y += 28;

            // 阶段名称
            Color stageColor = UIStyles.GetGuLevelColor(stage);
            Utils.DrawBorderString(sb, stageName, new Vector2(x, y), stageColor * alpha, UIStyles.SubTitleScale);
            y += 24;

            // 阶段进度条（7个阶段）
            DrawStageProgressBar(sb, x, y, stage, alpha);
            y += 28;

            // 分隔线
            DrawSeparator(sb, px + 12, y, PanelWidth - 24, alpha);
            y += 10;

            // 当前阶段描述
            Utils.DrawBorderString(sb, phaseDesc, new Vector2(x, y), TextColor * alpha, UIStyles.BodyScale);
            y += 22;

            // 下一步指引
            string nextHint = GetNextHint(progress.CurrentPhase);
            if (!string.IsNullOrEmpty(nextHint))
            {
                Utils.DrawBorderString(sb, "→ " + nextHint, new Vector2(x, y), AccentColor * alpha, UIStyles.BodyScale);
                y += 22;
            }

            // 关键选择记录
            y += 4;
            DrawSeparator(sb, px + 12, y, PanelWidth - 24, alpha);
            y += 10;
            Utils.DrawBorderString(sb, "[ 关键抉择 ]", new Vector2(x, y), DimColor * alpha, UIStyles.SmallScale);
            y += 18;

            var choices = ChoiceTrackerSystem.GetAllChoices();
            int shown = 0;
            foreach (var kvp in choices)
            {
                if (shown >= 4) break; // 最多显示4个
                string desc = ChoiceTrackerSystem.GetChoiceDescription(kvp.Key);
                Utils.DrawBorderString(sb, $"· {desc}", new Vector2(x + 4, y), DimColor * alpha, UIStyles.SmallScale);
                y += 16;
                shown++;
            }
            if (choices.Count == 0)
            {
                Utils.DrawBorderString(sb, "暂无关键抉择", new Vector2(x + 4, y), UIStyles.TextDim * alpha, UIStyles.SmallScale);
            }
        }

        private void DrawPanelBackground(SpriteBatch sb, Rectangle rect)
        {
            float a = Opacity;
            // 主背景
            sb.Draw(TextureAssets.BlackTile.Value, rect, BgColor * a);
            // 边框
            DrawBorder(sb, rect, UIStyles.Border * a);
            // 顶部装饰线
            sb.Draw(TextureAssets.BlackTile.Value,
                new Rectangle(rect.X, rect.Y, rect.Width, 3),
                UIStyles.BorderAccent * a * 0.6f);
        }

        private void DrawStageProgressBar(SpriteBatch sb, float x, float y, int currentStage, float alpha)
        {
            float barWidth = PanelWidth - 40;
            float segWidth = barWidth / 7f;

            for (int i = 1; i <= 7; i++)
            {
                Rectangle seg = new((int)(x + (i - 1) * segWidth), (int)y, (int)segWidth - 2, 12);

                if (i < currentStage)
                    sb.Draw(TextureAssets.BlackTile.Value, seg, ProgressFill * alpha * 0.7f);
                else if (i == currentStage)
                    sb.Draw(TextureAssets.BlackTile.Value, seg, ProgressFillGold * alpha);
                else
                    sb.Draw(TextureAssets.BlackTile.Value, seg, ProgressBg * alpha);

                // 阶段编号
                string num = i.ToString();
                Vector2 numPos = new(seg.Center.X - 4, seg.Bottom + 2);
                Color numColor = i <= currentStage ? TextColor * alpha : DimColor * alpha;
                Utils.DrawBorderString(sb, num, numPos, numColor, UIStyles.TinyScale);
            }
        }

        private void DrawSeparator(SpriteBatch sb, float x, float y, float width, float alpha)
        {
            sb.Draw(TextureAssets.BlackTile.Value,
                new Rectangle((int)x, (int)y, (int)width, 1),
                UIStyles.BorderLight * alpha * 0.5f);
        }

        private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color)
        {
            int t = 1;
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
            sb.Draw(TextureAssets.BlackTile.Value, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
        }

        private string GetNextHint(StoryPhase phase)
        {
            return phase switch
            {
                StoryPhase.NotEntered => "前往古月山寨",
                StoryPhase.Arrival => "与守门蛊师对话",
                StoryPhase.AwakeningCeremony => "前往学堂学习",
                StoryPhase.SchoolTraining => "帮助药堂采集药材",
                StoryPhase.MedicineRequest => "调查贾金生之死",
                StoryPhase.JiaJinShengDeath => "探索花酒行者传承",
                StoryPhase.HuaJiuInheritance => "获得家族认可",
                StoryPhase.FamilyRecognition => "准备三寨大比",
                StoryPhase.PreTournament => "参加三寨大比",
                StoryPhase.TournamentBegin => "赢得比赛",
                StoryPhase.TournamentFinal => "击败决赛对手",
                StoryPhase.TianHeAttack => "抵御天鹤上人",
                StoryPhase.BaiNingBingIceSeal => "面对白凝冰的抉择",
                StoryPhase.BloodSacrifice => "击败地脉守护者",
                StoryPhase.LeftQingMao => "前往南疆",
                StoryPhase.SouthBorderArrival => "与散修营地NPC交流",
                StoryPhase.ShangXinCiMeet => "帮助商心慈",
                StoryPhase.ThreeKingsInheritance => "完成三王传承副本",
                StoryPhase.ChunQiuChanFragment => "完成散修营地主线",
                StoryPhase.SanXiuCampComplete => "等待义天山异变",
                StoryPhase.YiTianShanAppears => "进入义天山副本",
                StoryPhase.YiTianShanDungeon => "深入副本",
                StoryPhase.DaTongFeng => "在大同风中存活",
                StoryPhase.FangYuanReveal => "面对方源",
                StoryPhase.YiTianShanComplete => "前往北原",
                StoryPhase.NorthDesertArrival => "与黑楼兰对话",
                StoryPhase.WangTingAlly => "接触长生天",
                StoryPhase.ChangShengTianContact => "面对太白云生之死",
                StoryPhase.TaiBaiYunShengDeath => "做出仙僵选择",
                StoryPhase.ImmortalZombieChoice => "等待天庭前奏",
                StoryPhase.HeavenPrelude => "准备宿命大战",
                StoryPhase.DestinyWarBegin => "击败四柱",
                StoryPhase.FourPillarsDown => "选择阵营",
                StoryPhase.FactionChoice => "挑战龙公",
                StoryPhase.LongGongPhase1 => "春秋蝉回溯",
                StoryPhase.ChunQiuRebirth => "再战龙公",
                StoryPhase.LongGongPhase2 => "见证宿命碎裂",
                StoryPhase.DestinyShattered => "通过升仙天劫",
                StoryPhase.Ascension => "建设仙窍",
                StoryPhase.SevenTurnBegin => "提升修为",
                StoryPhase.ApertureBuilt => "突破八转",
                StoryPhase.EightTurnBegin => "挑战道主",
                StoryPhase.DaoLordChallenge => "突破九转",
                StoryPhase.NineTurnBegin => "挑战尊者",
                StoryPhase.VenerableBattle => "面对终局",
                StoryPhase.TenTurnFinale => "选择结局",
                StoryPhase.EndingChosen => "旅途已终",
                _ => ""
            };
        }
    }
}
