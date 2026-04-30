using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.ReputationUI
{
    /// <summary>
    /// 势力声望 UI — 现代化扁平轻量风格
    /// 固定在屏幕右上角，点击标题栏展开/折叠
    /// </summary>
    public class ReputationUI : UIState
    {
        private UIPanel _mainPanel;
        private UIText _titleText;
        private UIList _factionList;
        private bool _isExpanded = false;

        public override void OnInitialize()
        {
            _mainPanel = new UIPanel();
            _mainPanel.Width.Set(260f, 0f);
            _mainPanel.Height.Set(36f, 0f);
            _mainPanel.Left.Set(-270f, 1f);
            _mainPanel.Top.Set(100f, 0f);
            _mainPanel.BackgroundColor = UIStyles.PanelBg;
            _mainPanel.BorderColor = UIStyles.Border;
            _mainPanel.SetPadding(6f);
            Append(_mainPanel);

            // 标题栏（可点击展开/折叠）
            _titleText = new UIText("势力声望 [点击展开]", 0.85f);
            _titleText.Left.Set(8f, 0f);
            _titleText.Top.Set(6f, 0f);
            _titleText.TextColor = UIStyles.TitleText;
            _titleText.OnLeftClick += (evt, listener) => ToggleExpand();
            _mainPanel.Append(_titleText);

            // 声望列表
            _factionList = new UIList();
            _factionList.Left.Set(8f, 0f);
            _factionList.Top.Set(36f, 0f);
            _factionList.Width.Set(240f, 0f);
            _factionList.Height.Set(280f, 0f);
            _factionList.OverflowHidden = true;
            _mainPanel.Append(_factionList);
        }

        private void ToggleExpand()
        {
            _isExpanded = !_isExpanded;
            _mainPanel.Height.Set(_isExpanded ? 340f : 36f, 0f);
            _titleText.SetText(_isExpanded ? "势力声望 [点击折叠]" : "势力声望 [点击展开]");
            if (_isExpanded) RefreshFactionList();
            Recalculate();
        }

        public void RefreshFactionList()
        {
            _factionList.Clear();
            var worldPlayer = Main.LocalPlayer.GetModPlayer<GuWorldPlayer>();
            if (worldPlayer == null) return;

            foreach (var (fid, rel) in worldPlayer.FactionRelations)
            {
                string displayName = GuWorldSystem.GetFactionDisplayName(fid);
                string levelName = GetRepLevelName(rel.GetLevel());
                string points = rel.ReputationPoints >= 0
                    ? $"+{rel.ReputationPoints}"
                    : $"{rel.ReputationPoints}";

                Color textColor = rel.GetLevel() switch
                {
                    RepLevel.Hostile => UIStyles.TextDanger,
                    RepLevel.Unfriendly => UIStyles.TextWarning,
                    RepLevel.Neutral => UIStyles.TextDim,
                    RepLevel.Friendly => UIStyles.TextSuccess,
                    RepLevel.Allied => UIStyles.TitleText,
                    _ => UIStyles.TextMain
                };

                string text = $"{displayName}:  {levelName}  ({points})";
                var entry = new UIText(text, 0.75f);
                entry.TextColor = textColor;
                entry.Left.Set(4f, 0f);
                entry.Top.Set(2f, 0f);
                _factionList.Add(entry);
            }

            // 分隔线
            _factionList.Add(new UIText("——————————", 0.65f) { TextColor = UIStyles.TextDim });

            var infamyText = new UIText($"恶名值: {worldPlayer.InfamyPoints}", 0.75f);
            infamyText.TextColor = UIStyles.TextDanger;
            _factionList.Add(infamyText);

            var fameText = new UIText($"声望值: {worldPlayer.FamePoints}", 0.75f);
            fameText.TextColor = UIStyles.TextInfo;
            _factionList.Add(fameText);
        }

        private static string GetRepLevelName(RepLevel level) => level switch
        {
            RepLevel.Hostile => "敌对",
            RepLevel.Unfriendly => "不友好",
            RepLevel.Neutral => "中立",
            RepLevel.Friendly => "友善",
            RepLevel.Allied => "盟友",
            _ => "未知"
        };

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
