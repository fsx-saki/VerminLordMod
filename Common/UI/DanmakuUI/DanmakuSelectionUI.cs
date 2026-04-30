using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.UI.UIUtils;
using VerminLordMod.Content.Items.Debuggers;

namespace VerminLordMod.Common.UI.DanmakuUI
{
    public struct ProjectileEntry
    {
        public int Type;
        public string Name;
        public string ModName;
        public string Category;
        public ModProjectile ModProjectile;
        public string DisplayName => $"[{ModName}] {Name}";
    }

    /// <summary>
    /// 弹幕选择UI — 现代化扁平轻量风格
    /// </summary>
    public class DanmakuSelectionUI : UIState
    {
        private UIPanel _mainPanel;
        private UIPanel _categoryPanel;
        private UIScrollbar _scrollbar;
        private UIList _projectileList;
        private UIText _titleText;
        private UIText _statusText;
        private UITextPanel<string> _closeButton;

        private List<ProjectileEntry> _allProjectiles = new List<ProjectileEntry>();
        private List<ProjectileEntry> _filteredProjectiles = new List<ProjectileEntry>();
        private string _currentCategory = "全部";
        private List<UITextPanel<string>> _categoryButtons = new List<UITextPanel<string>>();

        public override void OnInitialize()
        {
            _mainPanel = UIHelper.CreatePanel(680f, 520f);
            _mainPanel.BackgroundColor = UIStyles.PanelBg;
            _mainPanel.BorderColor = UIStyles.Border;
            Append(_mainPanel);

            // 标题栏
            var titleBar = UIHelper.CreateTitleBar(670f);
            titleBar.Left.Set(5f, 0f);
            titleBar.Top.Set(5f, 0f);
            _mainPanel.Append(titleBar);

            _titleText = UIHelper.CreateTitle("弹幕测试武器 - 选择弹幕", 12f, 6f);
            titleBar.Append(_titleText);

            _statusText = UIHelper.CreateText("正在加载弹幕列表...", 360f, 8f, UIStyles.TextSecondary, 0.7f);
            titleBar.Append(_statusText);

            _closeButton = UIHelper.CreateCloseButton(670f);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            titleBar.Append(_closeButton);

            // 分类面板
            _categoryPanel = new UIPanel();
            _categoryPanel.Width.Set(660f, 0f);
            _categoryPanel.Height.Set(70f, 0f);
            _categoryPanel.Left.Set(10f, 0f);
            _categoryPanel.Top.Set(48f, 0f);
            _categoryPanel.BackgroundColor = UIStyles.CategoryPanelBg;
            _categoryPanel.BorderColor = UIStyles.BorderLight;
            _mainPanel.Append(_categoryPanel);

            // 滚动条
            _scrollbar = UIHelper.CreateScrollbar(660f, 128f, 370f, 10f);
            _mainPanel.Append(_scrollbar);

            // 弹幕列表
            _projectileList = UIHelper.CreateUIList(10f, 128f, 650f, 370f);
            _projectileList.SetScrollbar(_scrollbar);
            _mainPanel.Append(_projectileList);
        }

        public void LoadProjectiles()
        {
            _allProjectiles.Clear();

            try
            {
                foreach (var mod in ModLoader.Mods)
                {
                    if (mod == null) continue;
                    var projTypes = mod.GetContent<ModProjectile>();
                    foreach (var modProj in projTypes)
                    {
                        if (modProj == null) continue;
                        int type = modProj.Type;
                        string name = modProj.Name;
                        string fullName = modProj.GetType().FullName ?? "";
                        string category = InferCategory(fullName, name);

                        _allProjectiles.Add(new ProjectileEntry
                        {
                            Type = type,
                            Name = name,
                            ModName = mod.Name,
                            Category = category,
                            ModProjectile = modProj
                        });
                    }
                }

                _allProjectiles = _allProjectiles
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .ToList();

                _statusText.SetText($"共加载 {_allProjectiles.Count} 个弹幕");
            }
            catch (Exception ex)
            {
                _statusText.SetText($"加载失败: {ex.Message}");
            }

            BuildCategoryButtons();
            ApplyFilter();
        }

        private string InferCategory(string fullName, string name)
        {
            if (fullName.Contains("VerminLordMod"))
            {
                if (fullName.Contains("Moon")) return "月道";
                if (fullName.Contains("Star")) return "星道";
                if (fullName.Contains("Water") || fullName.Contains("Ice") || fullName.Contains("Spout")) return "水道";
                if (fullName.Contains("Fire") || fullName.Contains("Flame")) return "火道";
                if (fullName.Contains("Poison") || fullName.Contains("Acid")) return "毒道";
                if (fullName.Contains("Lightning") || fullName.Contains("Thunder")) return "雷道";
                if (fullName.Contains("Wind") || fullName.Contains("Cyclone")) return "风道";
                if (fullName.Contains("Blood") || fullName.Contains("Soul") || fullName.Contains("Ghost")) return "血道/魂道";
                if (fullName.Contains("Bone") || fullName.Contains("Skull")) return "骨道";
                if (fullName.Contains("Sword") || fullName.Contains("Knife") || fullName.Contains("Blade")) return "剑道/刀道";
                if (fullName.Contains("Wood") || fullName.Contains("Grass") || fullName.Contains("Pine") || fullName.Contains("QingTeng")) return "木道";
                if (fullName.Contains("Gold") || fullName.Contains("Metal")) return "金道";
                if (fullName.Contains("Earth") || fullName.Contains("Rock") || fullName.Contains("Stone") || fullName.Contains("Mud")) return "地道";
                if (fullName.Contains("Light") || fullName.Contains("Shine") || fullName.Contains("White")) return "光道";
                if (fullName.Contains("Dark") || fullName.Contains("Shadow")) return "暗道";
                if (fullName.Contains("Sound") || fullName.Contains("Music")) return "音道";
                if (fullName.Contains("Love") || fullName.Contains("Charm")) return "情道";
                if (fullName.Contains("Space") || fullName.Contains("Void")) return "空道";
                if (fullName.Contains("Time") || fullName.Contains("Age")) return "宙道";
                if (fullName.Contains("War") || fullName.Contains("Fight")) return "战道";
                if (fullName.Contains("Draw") || fullName.Contains("Tactical")) return "智道";
                if (fullName.Contains("Luck") || fullName.Contains("Fortune")) return "运道";
                if (fullName.Contains("Rule") || fullName.Contains("Law")) return "律道";
                if (fullName.Contains("Transform") || fullName.Contains("Change")) return "变化道";
                if (fullName.Contains("Enslave") || fullName.Contains("Dog")) return "奴道";

                if (name.Contains("Moon")) return "月道";
                if (name.Contains("Star")) return "星道";
                if (name.Contains("Water") || name.Contains("Ice") || name.Contains("Spout")) return "水道";
                if (name.Contains("Fire") || name.Contains("Flame")) return "火道";
                if (name.Contains("Poison") || name.Contains("Acid")) return "毒道";
                if (name.Contains("Lightning") || name.Contains("Thunder")) return "雷道";
                if (name.Contains("Wind") || name.Contains("Cyclone")) return "风道";
                if (name.Contains("Blood") || name.Contains("Soul") || name.Contains("Ghost")) return "血道/魂道";
                if (name.Contains("Bone") || name.Contains("Skull")) return "骨道";
                if (name.Contains("Sword") || name.Contains("Knife") || name.Contains("Blade")) return "剑道/刀道";
                if (name.Contains("Wood") || name.Contains("Grass") || name.Contains("Pine") || name.Contains("QingTeng")) return "木道";
                if (name.Contains("Gold") || name.Contains("Metal")) return "金道";
                if (name.Contains("Rock") || name.Contains("Stone") || name.Contains("Mud")) return "地道";
                if (name.Contains("Light") || name.Contains("Shine") || name.Contains("White")) return "光道";
                if (name.Contains("Dark") || name.Contains("Shadow")) return "暗道";
                if (name.Contains("Sound") || name.Contains("Music")) return "音道";
                if (name.Contains("Love") || name.Contains("Charm")) return "情道";
                if (name.Contains("Void") || name.Contains("Space")) return "空道";
                if (name.Contains("Time") || name.Contains("Age")) return "宙道";
                if (name.Contains("War") || name.Contains("BoneWheel")) return "战道";
                if (name.Contains("Draw") || name.Contains("Tactical")) return "智道";
                if (name.Contains("Luck") || name.Contains("Fortune")) return "运道";
                if (name.Contains("Rule") || name.Contains("Law")) return "律道";
                if (name.Contains("Transform") || name.Contains("Change")) return "变化道";
                if (name.Contains("Enslave") || name.Contains("Dog")) return "奴道";
                if (name.Contains("JiaoLei") || name.Contains("Potato")) return "木道";
                if (name.Contains("TianDiHongYin")) return "律道";
                if (name.Contains("ElectricTeeth") || name.Contains("Killing")) return "金道";
                if (name.Contains("Sawtooth") || name.Contains("Golden")) return "金道";
                if (name.Contains("ShuangXi")) return "情道";
                if (name.Contains("Grating")) return "音道";
                if (name.Contains("SpiralBone")) return "骨道";
                if (name.Contains("BlueBird")) return "鸟道";
                if (name.Contains("WidowSpider")) return "虫道";
                if (name.Contains("RedNeedle")) return "变化道";
                if (name.Contains("StoneCJB")) return "地道";

                return "其他";
            }
            return "其他模组";
        }

        private void BuildCategoryButtons()
        {
            _categoryButtons.Clear();
            _categoryPanel.RemoveAllChildren();

            var categories = _allProjectiles
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            categories.Insert(0, "全部");

            float panelWidth = _categoryPanel.Width.Pixels - 10f;
            float xOffset = 5f;
            float yOffset = 5f;
            float rowHeight = 28f;

            foreach (var category in categories)
            {
                var btn = new UITextPanel<string>(category, 0.65f);
                float btnWidth = System.Math.Max(55f, category.Length * 13f);
                btn.Width.Set(btnWidth, 0f);
                btn.Height.Set(24f, 0f);

                if (xOffset + btnWidth + 5f > panelWidth)
                {
                    xOffset = 5f;
                    yOffset += rowHeight;
                }

                btn.Left.Set(xOffset, 0f);
                btn.Top.Set(yOffset, 0f);
                btn.BackgroundColor = category == _currentCategory ? UIStyles.BtnSelected : UIStyles.BtnSecondary;
                btn.BorderColor = UIStyles.BorderLight;

                string capturedCategory = category;
                btn.OnLeftClick += (evt, listener) =>
                {
                    _currentCategory = capturedCategory;
                    ApplyFilter();
                    BuildCategoryButtons();
                };

                _categoryPanel.Append(btn);
                _categoryButtons.Add(btn);
                xOffset += btnWidth + 5f;
            }
        }

        private void ApplyFilter()
        {
            _projectileList.Clear();

            _filteredProjectiles = _currentCategory == "全部"
                ? new List<ProjectileEntry>(_allProjectiles)
                : _allProjectiles.Where(p => p.Category == _currentCategory).ToList();

            foreach (var entry in _filteredProjectiles)
            {
                var item = new DanmakuListItem(entry, OnProjectileSelected);
                _projectileList.Add(item);
            }

            _statusText.SetText($"共 {_allProjectiles.Count} 个 | 当前: {_filteredProjectiles.Count} ({_currentCategory})");
        }

        private void OnProjectileSelected(ProjectileEntry entry)
        {
            DanmakuTestWeapon.SelectedProjectileType = entry.Type;
            DanmakuTestWeapon.SelectedProjectileName = entry.DisplayName;
            Main.NewText($"[弹幕测试武器] 已选择弹幕: {entry.DisplayName} (Type: {entry.Type})", Color.Green);
            CloseUI();
        }

        private void CloseUI() => ModContent.GetInstance<DanmakuSelectionUISystem>().ToggleUI();

        private bool _escapeWasDown = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (UIHelper.CheckEscapeReleased(ref _escapeWasDown))
                CloseUI();
            UIHelper.UpdatePanelCenter(_mainPanel, 680f, 520f);
        }
    }

    /// <summary>
    /// 弹幕列表项 — 扁平风格
    /// </summary>
    public class DanmakuListItem : UIPanel
    {
        private ProjectileEntry _entry;
        private Action<ProjectileEntry> _onClick;

        public DanmakuListItem(ProjectileEntry entry, Action<ProjectileEntry> onClick)
        {
            _entry = entry;
            _onClick = onClick;

            Width.Set(630f, 0f);
            Height.Set(30f, 0f);
            MarginTop = 2f;
            BackgroundColor = UIStyles.ListItemBg;
            BorderColor = UIStyles.BorderLight;

            var nameText = new UIText(entry.DisplayName, 0.75f);
            nameText.Left.Set(8f, 0f);
            nameText.Top.Set(5f, 0f);
            nameText.TextColor = UIStyles.TextMain;
            Append(nameText);

            var catText = new UIText($"[{entry.Category}]", 0.65f);
            catText.Left.Set(420f, 0f);
            catText.Top.Set(5f, 0f);
            catText.TextColor = UIStyles.TextInfo;
            Append(catText);

            var typeText = new UIText($"Type: {entry.Type}", 0.65f);
            typeText.Left.Set(540f, 0f);
            typeText.Top.Set(5f, 0f);
            typeText.TextColor = UIStyles.TextDim;
            Append(typeText);

            OnMouseOver += (evt, listener) => { BackgroundColor = UIStyles.ListItemHover; };
            OnMouseOut += (evt, listener) => { BackgroundColor = UIStyles.ListItemBg; };
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            _onClick?.Invoke(_entry);
        }
    }

    public class DanmakuSelectionUISystem : ModSystem
    {
        private UserInterface _danmakuUI;
        public static DanmakuSelectionUI DanmakuUIInstance;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _danmakuUI = new UserInterface();
                DanmakuUIInstance = new DanmakuSelectionUI();
                DanmakuUIInstance.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _danmakuUI?.Update(gameTime);
        }

        public void ToggleUI()
        {
            if (_danmakuUI.CurrentState == null)
            {
                DanmakuUIInstance.LoadProjectiles();
                _danmakuUI.SetState(DanmakuUIInstance);
            }
            else
            {
                _danmakuUI.SetState(null);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Danmaku Selection UI",
                    () =>
                    {
                        if (_danmakuUI?.CurrentState != null)
                            _danmakuUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
