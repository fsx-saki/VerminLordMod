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

namespace VerminLordMod.Common.UI.ReputationUI
{
    /// <summary>
    /// 势力声望 UI - 可展开/折叠显示所有已知势力的声望点数
    /// 
    /// 设计原则：
    /// - 固定在屏幕右上角
    /// - 点击标题栏展开/折叠
    /// - 显示每个势力的声望等级和点数
    /// - 显示玩家恶名值
    /// - 仅用于展示，不提供交互操作
    /// </summary>
    public class ReputationUI : UIState
    {
        private UIPanel _mainPanel;
        private UIText _titleText;
        private UIList _factionList;
        private bool _isExpanded = false;

        public override void OnInitialize()
        {
            // 主面板 - 固定在屏幕右上角
            _mainPanel = new UIPanel();
            _mainPanel.Width.Set(280f, 0f);
            _mainPanel.Height.Set(40f, 0f); // 折叠时只显示标题
            _mainPanel.Left.Set(-300f, 1f); // 使用百分比定位：屏幕右侧 - 300px
            _mainPanel.Top.Set(100f, 0f);
            _mainPanel.BackgroundColor = new Color(30, 30, 50, 200);
            _mainPanel.BorderColor = new Color(80, 80, 140, 255);
            _mainPanel.SetPadding(6f);
            Append(_mainPanel);

            // 标题 - 点击展开/折叠
            _titleText = new UIText("势力声望 [点击展开]", 0.9f);
            _titleText.Left.Set(10f, 0f);
            _titleText.Top.Set(8f, 0f);
            _titleText.TextColor = Color.Gold;
            _titleText.OnLeftClick += (evt, listener) => ToggleExpand();
            _mainPanel.Append(_titleText);

            // 声望列表（初始隐藏）
            _factionList = new UIList();
            _factionList.Left.Set(10f, 0f);
            _factionList.Top.Set(40f, 0f);
            _factionList.Width.Set(260f, 0f);
            _factionList.Height.Set(300f, 0f); // 增加高度以容纳更多条目
            _factionList.OverflowHidden = true;
            _mainPanel.Append(_factionList);
        }

        /// <summary> 切换展开/折叠状态 </summary>
        private void ToggleExpand()
        {
            _isExpanded = !_isExpanded;
            _mainPanel.Height.Set(_isExpanded ? 360f : 40f, 0f);
            _titleText.SetText(_isExpanded ? "势力声望 [点击折叠]" : "势力声望 [点击展开]");

            // 展开时刷新列表
            if (_isExpanded)
            {
                RefreshFactionList();
            }

            Recalculate();
        }

        /// <summary> 刷新声望列表内容 </summary>
        public void RefreshFactionList()
        {
            _factionList.Clear();

            var worldPlayer = Main.LocalPlayer.GetModPlayer<GuWorldPlayer>();
            if (worldPlayer == null) return;

            // 遍历所有已知家族
            foreach (var (fid, rel) in worldPlayer.FactionRelations)
            {
                string displayName = GuWorldSystem.GetFactionDisplayName(fid);
                string levelName = GetRepLevelName(rel.GetLevel());
                string points = rel.ReputationPoints >= 0
                    ? $"+{rel.ReputationPoints}"
                    : $"{rel.ReputationPoints}";

                // 根据声望等级选择颜色
                Color textColor = rel.GetLevel() switch
                {
                    RepLevel.Hostile => Color.Red,
                    RepLevel.Unfriendly => Color.Orange,
                    RepLevel.Neutral => Color.Gray,
                    RepLevel.Friendly => Color.Green,
                    RepLevel.Allied => Color.Gold,
                    _ => Color.White
                };

                string text = $"{displayName}:  {levelName}  ({points})";
                var entry = new UIText(text, 0.8f);
                entry.TextColor = textColor;
                entry.Left.Set(5f, 0f);
                entry.Top.Set(3f, 0f);
                _factionList.Add(entry);
            }

            // 分隔线
            _factionList.Add(new UIText("----------------", 0.7f) { TextColor = Color.Gray });

            // 恶名值
            var infamyText = new UIText($"恶名值: {worldPlayer.InfamyPoints}", 0.8f);
            infamyText.TextColor = Color.DarkRed;
            _factionList.Add(infamyText);

            // 声望值
            var fameText = new UIText($"声望值: {worldPlayer.FamePoints}", 0.8f);
            fameText.TextColor = Color.LightBlue;
            _factionList.Add(fameText);
        }

        /// <summary> 获取声望等级的中文名称 </summary>
        private static string GetRepLevelName(RepLevel level)
        {
            return level switch
            {
                RepLevel.Hostile => "敌对",
                RepLevel.Unfriendly => "不友好",
                RepLevel.Neutral => "中立",
                RepLevel.Friendly => "友善",
                RepLevel.Allied => "盟友",
                _ => "未知"
            };
        }

        public override void Update(GameTime gameTime)
        {
            // 每帧更新位置（适应窗口大小变化）
            // 使用百分比定位，无需手动更新 Left
            base.Update(gameTime);
        }
    }
}
