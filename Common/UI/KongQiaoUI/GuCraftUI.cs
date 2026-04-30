using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 合炼面板 UI — 随身工作台，用于合成蛊虫
    /// 
    /// 优化内容：
    /// 1. 配方分类筛选（力道蛊/豕蛊/酒虫/破境丹等）
    /// 2. 配方搜索功能
    /// 3. 可滚动配方列表
    /// 4. 交互式原料槽位（点击自动从背包放入/取出）
    /// 5. 配方详情展示（描述、等级、材料清单）
    /// 6. 批量合成支持（右键合炼按钮批量合成）
    /// 7. 真元不足/材料不足时按钮禁用+提示
    /// 8. 合成成功/失败反馈
    /// </summary>
    public class GuCraftUI : UIState
    {
        // ===== 面板尺寸 =====
        private const int PanelWidth = 760;
        private const int PanelHeight = 560;

        // ===== UI 元素 =====
        private UIPanel _mainPanel;
        private UIText _titleText;
        private UITextPanel<string> _closeButton;
        private UIText _qiInfoText;

        // 搜索
        private UITextBox _searchBox;

        // 分类按钮
        private List<UICategoryButton> _categoryButtons;
        private string _selectedCategory;

        // 配方列表
        private UIList _recipeList;
        private UIScrollbar _recipeScrollbar;
        private List<GuRecipe> _filteredRecipes;

        // 配方详情区域
        private UIPanel _detailPanel;
        private UIText _detailNameText;
        private UIText _detailLevelText;
        private UIText _detailDescText;
        private UIText _detailQiCostText;

        // 原料槽位（弹性数量，最多支持8个材料槽）
        private const int MaxMaterialSlots = 8;
        private UIItemSlot[] _ingredientSlots;
        private UIText[] _materialNameLabels;

        // 结果槽位
        private UIItemSlot _resultSlot;
        private UIText _resultNameLabel;

        // 动态定位的元素（箭头、结果标签、合成按钮、反馈文本、底部提示）
        private UIText _arrowText;
        private UIText _resultLabel;
        private UIText _tipText;

        // 合成按钮
        private UITextPanel<string> _craftButton;
        private UIText _craftFeedbackText;
        private int _feedbackTimer;

        // 当前选中的配方
        private GuRecipe _selectedRecipe;

        // 搜索防抖
        private int _searchDebounceTimer;
        private string _pendingSearchText;

        public override void OnInitialize()
        {
            // ===== 主面板 =====
            _mainPanel = new UIPanel();
            _mainPanel.Width.Set(PanelWidth, 0f);
            _mainPanel.Height.Set(PanelHeight, 0f);
            _mainPanel.Left.Set(Main.screenWidth / 2f - PanelWidth / 2f, 0f);
            _mainPanel.Top.Set(Main.screenHeight / 2f - PanelHeight / 2f, 0f);
            _mainPanel.BackgroundColor = new Color(15, 22, 40, 240);
            _mainPanel.BorderColor = new Color(60, 100, 180, 255);
            Append(_mainPanel);

            // ===== 标题栏 =====
            var titleBar = new UIPanel();
            titleBar.Width.Set(PanelWidth - 10, 0f);
            titleBar.Height.Set(36f, 0f);
            titleBar.Left.Set(5f, 0f);
            titleBar.Top.Set(5f, 0f);
            titleBar.BackgroundColor = new Color(25, 40, 70, 200);
            titleBar.BorderColor = new Color(60, 100, 180, 100);
            _mainPanel.Append(titleBar);

            _titleText = new UIText("合炼台", 1.1f);
            _titleText.Left.Set(12f, 0f);
            _titleText.Top.Set(6f, 0f);
            _titleText.TextColor = Color.Gold;
            titleBar.Append(_titleText);

            // 真元信息
            _qiInfoText = new UIText("真元: --/--", 0.75f);
            _qiInfoText.Left.Set(120f, 0f);
            _qiInfoText.Top.Set(8f, 0f);
            _qiInfoText.TextColor = Color.LightBlue;
            titleBar.Append(_qiInfoText);

            // 关闭按钮
            _closeButton = new UITextPanel<string>("✕", 0.8f);
            _closeButton.Width.Set(30f, 0f);
            _closeButton.Height.Set(26f, 0f);
            _closeButton.Left.Set(PanelWidth - 50, 0f);
            _closeButton.Top.Set(5f, 0f);
            _closeButton.BackgroundColor = new Color(80, 30, 30);
            _closeButton.BorderColor = new Color(120, 50, 50);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            titleBar.Append(_closeButton);

            // ===== 搜索框 =====
            var searchPanel = new UIPanel();
            searchPanel.Width.Set(180f, 0f);
            searchPanel.Height.Set(30f, 0f);
            searchPanel.Left.Set(5f, 0f);
            searchPanel.Top.Set(46f, 0f);
            searchPanel.BackgroundColor = new Color(30, 35, 55, 200);
            searchPanel.BorderColor = new Color(60, 80, 120, 150);
            _mainPanel.Append(searchPanel);

            _searchBox = new UITextBox("", 0.75f, false);
            _searchBox.Width.Set(160f, 0f);
            _searchBox.Height.Set(24f, 0f);
            _searchBox.Left.Set(6f, 0f);
            _searchBox.Top.Set(3f, 0f);
            _searchBox.BackgroundColor = Color.Transparent;
            _searchBox.BorderColor = Color.Transparent;
            // 注意：UITextBox 在 tModLoader 中没有 OnTextChanged 事件，
            // 搜索文本的更新在 Update() 中通过轮询 _searchBox.Text 实现
            searchPanel.Append(_searchBox);

            // 搜索图标
            var searchHint = new UIText("🔍", 0.7f);
            searchHint.Left.Set(155f, 0f);
            searchHint.Top.Set(5f, 0f);
            searchHint.TextColor = new Color(100, 120, 160);
            searchHint.IgnoresMouseInteraction = true;
            searchPanel.Append(searchHint);

            // ===== 分类按钮行 =====
            _categoryButtons = new List<UICategoryButton>();
            string[] categories = { "全部", "力道蛊", "豕蛊", "酒虫", "破境丹" };
            string[] categoryKeys = { "All", "Strength", "PigGu", "WineBug", "RealmBreak" };
            float catX = 195f;
            for (int i = 0; i < categories.Length; i++)
            {
                int idx = i;
                var btn = new UICategoryButton(categories[i], categoryKeys[i]);
                btn.Left.Set(catX, 0f);
                btn.Top.Set(46f, 0f);
                btn.Width.Set(80f, 0f);
                btn.Height.Set(28f, 0f);
                btn.OnLeftClick += (evt, listener) => SelectCategory(categoryKeys[idx]);
                if (i == 0) btn.IsSelected = true;
                _mainPanel.Append(btn);
                _categoryButtons.Add(btn);
                catX += 84f;
            }
            _selectedCategory = "All";

            // ===== 配方列表（左侧） =====
            var listPanel = new UIPanel();
            listPanel.Width.Set(300f, 0f);
            listPanel.Height.Set(420f, 0f);
            listPanel.Left.Set(5f, 0f);
            listPanel.Top.Set(80f, 0f);
            listPanel.BackgroundColor = new Color(20, 28, 48, 200);
            listPanel.BorderColor = new Color(40, 60, 100, 150);
            _mainPanel.Append(listPanel);

            var listLabel = new UIText("配方列表", 0.8f);
            listLabel.Left.Set(8f, 0f);
            listLabel.Top.Set(4f, 0f);
            listLabel.TextColor = Color.LightGray;
            listPanel.Append(listLabel);

            _recipeScrollbar = new UIScrollbar();
            _recipeScrollbar.Left.Set(280f, 0f);
            _recipeScrollbar.Top.Set(28f, 0f);
            _recipeScrollbar.Height.Set(380f, 0f);
            _recipeScrollbar.Width.Set(12f, 0f);
            listPanel.Append(_recipeScrollbar);

            _recipeList = new UIList();
            _recipeList.Left.Set(4f, 0f);
            _recipeList.Top.Set(28f, 0f);
            _recipeList.Width.Set(280f, 0f);
            _recipeList.Height.Set(380f, 0f);
            _recipeList.SetScrollbar(_recipeScrollbar);
            listPanel.Append(_recipeList);

            // ===== 配方详情区域（右侧） =====
            _detailPanel = new UIPanel();
            _detailPanel.Width.Set(440f, 0f);
            _detailPanel.Height.Set(420f, 0f);
            _detailPanel.Left.Set(312f, 0f);
            _detailPanel.Top.Set(80f, 0f);
            _detailPanel.BackgroundColor = new Color(20, 28, 48, 200);
            _detailPanel.BorderColor = new Color(40, 60, 100, 150);
            _mainPanel.Append(_detailPanel);

            // 配方名称
            _detailNameText = new UIText("请选择配方", 1.0f);
            _detailNameText.Left.Set(10f, 0f);
            _detailNameText.Top.Set(8f, 0f);
            _detailNameText.TextColor = Color.Gold;
            _detailPanel.Append(_detailNameText);

            // 蛊虫等级
            _detailLevelText = new UIText("", 0.75f);
            _detailLevelText.Left.Set(10f, 0f);
            _detailLevelText.Top.Set(34f, 0f);
            _detailLevelText.TextColor = Color.LightCyan;
            _detailPanel.Append(_detailLevelText);

            // 描述
            _detailDescText = new UIText("", 0.7f);
            _detailDescText.Left.Set(10f, 0f);
            _detailDescText.Top.Set(56f, 0f);
            _detailDescText.Width.Set(420f, 0f);
            _detailDescText.Height.Set(50f, 0f);
            _detailDescText.TextColor = new Color(180, 180, 200);
            _detailPanel.Append(_detailDescText);

            // 真元消耗
            _detailQiCostText = new UIText("", 0.75f);
            _detailQiCostText.Left.Set(10f, 0f);
            _detailQiCostText.Top.Set(100f, 0f);
            _detailQiCostText.TextColor = Color.LightBlue;
            _detailPanel.Append(_detailQiCostText);

            // 材料标签
            var ingredientLabel = new UIText("所需材料:", 0.8f);
            ingredientLabel.Left.Set(10f, 0f);
            ingredientLabel.Top.Set(130f, 0f);
            ingredientLabel.TextColor = Color.White;
            _detailPanel.Append(ingredientLabel);

            // 材料槽位（弹性数量，固定创建 MaxMaterialSlots 个，根据配方动态显示/隐藏）
            _ingredientSlots = new UIItemSlot[MaxMaterialSlots];
            _materialNameLabels = new UIText[MaxMaterialSlots];
            for (int i = 0; i < MaxMaterialSlots; i++)
            {
                int slotIdx = i;
                var slot = new UIItemSlot(new Color(35, 30, 40, 180), new Color(40, 50, 70, 200));
                slot.SlotIndex = i;
                slot.OnSlotClick = (idx) => OnIngredientSlotClick(idx);
                // 位置在 RefreshDetail 中动态计算，先默认隐藏
                slot.Left.Set(-9999f, 0f);
                slot.Top.Set(-9999f, 0f);
                _ingredientSlots[i] = slot;
                _detailPanel.Append(slot);

                var matName = new UIText("", 0.55f);
                matName.Left.Set(-9999f, 0f);
                matName.Top.Set(-9999f, 0f);
                matName.Width.Set(50f, 0f);
                matName.Height.Set(20f, 0f);
                matName.TextColor = Color.Gray;
                _materialNameLabels[i] = matName;
                _detailPanel.Append(matName);
            }

            // 箭头（位置由 RefreshDetail 动态计算）
            _arrowText = new UIText("→", 1.2f);
            _arrowText.Left.Set(-9999f, 0f);
            _arrowText.Top.Set(-9999f, 0f);
            _arrowText.TextColor = Color.Yellow;
            _arrowText.IgnoresMouseInteraction = true;
            _detailPanel.Append(_arrowText);

            // 结果标签
            _resultLabel = new UIText("产物:", 0.75f);
            _resultLabel.Left.Set(-9999f, 0f);
            _resultLabel.Top.Set(-9999f, 0f);
            _resultLabel.TextColor = Color.White;
            _resultLabel.IgnoresMouseInteraction = true;
            _detailPanel.Append(_resultLabel);

            // 结果槽位
            _resultSlot = new UIItemSlot(new Color(40, 35, 25, 180), new Color(60, 80, 50, 200));
            _resultSlot.Left.Set(-9999f, 0f);
            _resultSlot.Top.Set(-9999f, 0f);
            _resultSlot.SetShowStack(false);
            _detailPanel.Append(_resultSlot);

            // 结果名称
            _resultNameLabel = new UIText("", 0.55f);
            _resultNameLabel.Left.Set(-9999f, 0f);
            _resultNameLabel.Top.Set(-9999f, 0f);
            _resultNameLabel.Width.Set(80f, 0f);
            _resultNameLabel.Height.Set(20f, 0f);
            _resultNameLabel.TextColor = Color.Gold;
            _detailPanel.Append(_resultNameLabel);

            // ===== 合成按钮 =====
            _craftButton = new UITextPanel<string>("合炼", 0.9f);
            _craftButton.Width.Set(120f, 0f);
            _craftButton.Height.Set(36f, 0f);
            _craftButton.Left.Set(-9999f, 0f);
            _craftButton.Top.Set(-9999f, 0f);
            _craftButton.BackgroundColor = new Color(40, 80, 40);
            _craftButton.BorderColor = new Color(60, 120, 60);
            _craftButton.OnLeftClick += (evt, listener) => TryCraft(false);
            _craftButton.OnRightClick += (evt, listener) => TryCraft(true);
            _detailPanel.Append(_craftButton);

            // 合成反馈文本
            _craftFeedbackText = new UIText("", 0.7f);
            _craftFeedbackText.Left.Set(10f, 0f);
            _craftFeedbackText.Top.Set(-9999f, 0f);
            _craftFeedbackText.Width.Set(420f, 0f);
            _craftFeedbackText.Height.Set(40f, 0f);
            _craftFeedbackText.TextColor = Color.LightGreen;
            _detailPanel.Append(_craftFeedbackText);

            // 底部提示
            _tipText = new UIText("提示: 点击材料槽查看背包材料 | 右键合炼按钮批量合成(最多10个)", 0.6f);
            _tipText.Left.Set(10f, 0f);
            _tipText.Top.Set(-9999f, 0f);
            _tipText.Width.Set(420f, 0f);
            _tipText.TextColor = new Color(100, 100, 120);
            _detailPanel.Append(_tipText);

            // 初始化配方列表
            RefreshRecipeList();
        }

        // ==================== 分类/搜索 ====================

        private void SelectCategory(string categoryKey)
        {
            _selectedCategory = categoryKey;
            foreach (var btn in _categoryButtons)
                btn.IsSelected = btn.CategoryKey == categoryKey;

            _searchBox.SetText("");
            _pendingSearchText = "";
            RefreshRecipeList();
        }

        private void ApplySearch()
        {
            RefreshRecipeList();
        }

        private void RefreshRecipeList()
        {
            _recipeList.Clear();
            _filteredRecipes = GetFilteredRecipes();

            if (_filteredRecipes.Count == 0)
            {
                var emptyText = new UIText("没有找到匹配的配方", 0.75f);
                emptyText.Left.Set(8f, 0f);
                emptyText.Top.Set(8f, 0f);
                emptyText.TextColor = Color.Gray;
                _recipeList.Add(emptyText);
                return;
            }

            for (int i = 0; i < _filteredRecipes.Count; i++)
            {
                int recipeIdx = i;
                var recipe = _filteredRecipes[i];

                var recipeItem = new UIPanel();
                recipeItem.Width.Set(270f, 0f);
                recipeItem.Height.Set(32f, 0f);
                recipeItem.MarginTop = 2f;

                bool isSelected = _selectedRecipe != null && recipe.ResultType == _selectedRecipe.ResultType;
                recipeItem.BackgroundColor = isSelected
                    ? new Color(50, 80, 120, 200)
                    : new Color(30, 35, 55, 180);
                recipeItem.BorderColor = isSelected
                    ? new Color(80, 140, 200, 200)
                    : new Color(40, 50, 70, 100);

                // 等级标签
                string levelTag = $"Lv{recipe.GuLevel}";
                var levelLabel = new UIText(levelTag, 0.65f);
                levelLabel.Left.Set(4f, 0f);
                levelLabel.Top.Set(6f, 0f);
                levelLabel.TextColor = GetLevelColor(recipe.GuLevel);
                recipeItem.Append(levelLabel);

                // 配方名称
                var nameLabel = new UIText(recipe.ResultName, 0.75f);
                nameLabel.Left.Set(40f, 0f);
                nameLabel.Top.Set(6f, 0f);
                nameLabel.TextColor = isSelected ? Color.White : Color.LightGray;
                recipeItem.Append(nameLabel);

                // 类别标签
                var catLabel = new UIText(GuCraftSystem.GetCategoryDisplayName(recipe.Category), 0.55f);
                catLabel.Left.Set(180f, 0f);
                catLabel.Top.Set(8f, 0f);
                catLabel.TextColor = new Color(120, 140, 180);
                recipeItem.Append(catLabel);

                int capturedIdx = recipeIdx;
                recipeItem.OnLeftClick += (evt, listener) => SelectRecipe(capturedIdx);
                _recipeList.Add(recipeItem);
            }
        }

        private List<GuRecipe> GetFilteredRecipes()
        {
            var allRecipes = GuCraftSystem.GetAllRecipes();

            IEnumerable<GuRecipe> filtered = allRecipes;
            if (_selectedCategory != "All")
            {
                var category = (RecipeCategory)Enum.Parse(typeof(RecipeCategory), _selectedCategory);
                filtered = filtered.Where(r => r.Category == category);
            }

            string searchText = _pendingSearchText?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(r =>
                    r.ResultName.ToLower().Contains(searchText) ||
                    r.Description.ToLower().Contains(searchText));
            }

            return filtered.ToList();
        }

        // ==================== 配方选择 ====================

        private void SelectRecipe(int index)
        {
            if (index < 0 || index >= _filteredRecipes.Count) return;

            _selectedRecipe = _filteredRecipes[index];
            RefreshDetail();
            RefreshRecipeList();
        }

        private void RefreshDetail()
        {
            if (_selectedRecipe == null)
            {
                _detailNameText.SetText("请选择配方");
                _detailLevelText.SetText("");
                _detailDescText.SetText("");
                _detailQiCostText.SetText("");
                _craftButton.BackgroundColor = new Color(40, 40, 40);
                _craftButton.SetText("合炼");

                foreach (var slot in _ingredientSlots) { slot.Clear(); slot.Left.Set(-9999f, 0f); slot.Top.Set(-9999f, 0f); }
                _resultSlot.Clear(); _resultSlot.Left.Set(-9999f, 0f); _resultSlot.Top.Set(-9999f, 0f);
                _resultNameLabel.SetText("");
                foreach (var label in _materialNameLabels) { label.SetText(""); label.Left.Set(-9999f, 0f); label.Top.Set(-9999f, 0f); }
                _arrowText.Left.Set(-9999f, 0f); _arrowText.Top.Set(-9999f, 0f);
                _resultLabel.Left.Set(-9999f, 0f); _resultLabel.Top.Set(-9999f, 0f);
                _craftButton.Left.Set(-9999f, 0f); _craftButton.Top.Set(-9999f, 0f);
                _craftFeedbackText.Top.Set(-9999f, 0f);
                _tipText.Top.Set(-9999f, 0f);
                return;
            }

            var recipe = _selectedRecipe;
            var player = Main.LocalPlayer;
            var qiResource = player.GetModPlayer<QiResourcePlayer>();

            // 名称
            _detailNameText.SetText(recipe.ResultName);

            // 等级
            _detailLevelText.SetText($"蛊虫等级: {recipe.GuLevel}转");

            // 描述
            _detailDescText.SetText(recipe.Description);

            // 真元消耗
            bool hasQi = qiResource.QiCurrent >= recipe.QiCost;
            _detailQiCostText.SetText($"消耗真元: {recipe.QiCost}");
            _detailQiCostText.TextColor = hasQi ? Color.LightBlue : Color.Red;

            // ===== 弹性材料槽布局 =====
            // 每行最多4个槽位，槽位间距55px，行间距55px
            const int SlotsPerRow = 4;
            const float SlotSpacingX = 55f;
            const float SlotSpacingY = 55f;
            const float MaterialStartY = 155f;

            var materials = recipe.GetMaterials();
            int materialCount = Math.Min(materials.Count, MaxMaterialSlots);
            bool allMaterialsMet = true;

            int idx = 0;
            foreach (var kvp in materials)
            {
                if (idx >= MaxMaterialSlots) break;

                int row = idx / SlotsPerRow;
                int col = idx % SlotsPerRow;
                float slotX = 10f + col * SlotSpacingX;
                float slotY = MaterialStartY + row * SlotSpacingY;

                var matItem = new Item();
                matItem.SetDefaults(kvp.Key);
                matItem.stack = kvp.Value;

                int playerStack = CountItemInInventory(kvp.Key);
                bool hasMaterial = playerStack >= kvp.Value;
                if (!hasMaterial) allMaterialsMet = false;

                _ingredientSlots[idx].Left.Set(slotX, 0f);
                _ingredientSlots[idx].Top.Set(slotY, 0f);
                _ingredientSlots[idx].SetItem(matItem);
                _ingredientSlots[idx].SetHighlight(!hasMaterial);

                _materialNameLabels[idx].Left.Set(slotX, 0f);
                _materialNameLabels[idx].Top.Set(slotY + 50f, 0f);
                _materialNameLabels[idx].SetText($"{matItem.Name}\n{playerStack}/{kvp.Value}");
                _materialNameLabels[idx].TextColor = hasMaterial ? Color.LightGreen : Color.Red;

                idx++;
            }
            // 隐藏剩余槽位
            for (; idx < MaxMaterialSlots; idx++)
            {
                _ingredientSlots[idx].Clear();
                _ingredientSlots[idx].Left.Set(-9999f, 0f);
                _ingredientSlots[idx].Top.Set(-9999f, 0f);
                _ingredientSlots[idx].SetHighlight(false);
                _materialNameLabels[idx].SetText("");
                _materialNameLabels[idx].Left.Set(-9999f, 0f);
                _materialNameLabels[idx].Top.Set(-9999f, 0f);
            }

            // ===== 箭头和结果槽位置 =====
            // 箭头放在材料槽最后一行的第4个位置右侧
            int lastRow = (materialCount - 1) / SlotsPerRow;
            int lastRowCol = (materialCount - 1) % SlotsPerRow;
            float arrowX = 10f + Math.Min(lastRowCol + 1, SlotsPerRow) * SlotSpacingX + 5f;
            float arrowY = MaterialStartY + lastRow * SlotSpacingY;

            _arrowText.Left.Set(arrowX, 0f);
            _arrowText.Top.Set(arrowY, 0f);

            // 结果标签
            _resultLabel.Left.Set(arrowX + 35f, 0f);
            _resultLabel.Top.Set(MaterialStartY - 25f + lastRow * SlotSpacingY, 0f);

            // 结果槽位
            _resultSlot.Left.Set(arrowX + 35f, 0f);
            _resultSlot.Top.Set(arrowY, 0f);

            // 结果名称
            _resultNameLabel.Left.Set(arrowX + 35f, 0f);
            _resultNameLabel.Top.Set(arrowY + 50f, 0f);

            var resultItem = new Item();
            resultItem.SetDefaults(recipe.ResultType);
            _resultSlot.SetItem(resultItem);
            _resultNameLabel.SetText(resultItem.Name);

            // ===== 合成按钮位置 =====
            float buttonY = MaterialStartY + (lastRow + 1) * SlotSpacingY + 10f;
            _craftButton.Left.Set(160f, 0f);
            _craftButton.Top.Set(buttonY, 0f);

            bool canCraft = hasQi && allMaterialsMet;
            _craftButton.BackgroundColor = canCraft
                ? new Color(40, 80, 40)
                : new Color(60, 40, 40);
            _craftButton.SetText(canCraft ? "合炼" : "条件不足");

            // 反馈文本
            _craftFeedbackText.Top.Set(buttonY + 40f, 0f);

            // 底部提示
            _tipText.Top.Set(buttonY + 85f, 0f);
        }

        // ==================== 材料槽交互 ====================

        private void OnIngredientSlotClick(int slotIndex)
        {
            if (_selectedRecipe == null) return;

            var materials = _selectedRecipe.GetMaterials().ToList();
            if (slotIndex >= materials.Count) return;

            int itemType = materials[slotIndex].Key;
            int needed = materials[slotIndex].Value;

            int playerStack = CountItemInInventory(itemType);
            var itemName = Lang.GetItemName(itemType);

            if (playerStack >= needed)
            {
                Main.NewText($"[材料充足] {itemName} 背包 x{playerStack}（需要 x{needed}）", Color.LightGreen);
            }
            else
            {
                Main.NewText($"[材料不足] {itemName} 背包 x{playerStack}（需要 x{needed}）", Color.Red);
            }

            RefreshDetail();
        }

        // ==================== 合成逻辑 ====================

        private void TryCraft(bool isBulk)
        {
            if (_selectedRecipe == null)
            {
                ShowFeedback("请先选择配方", Color.Red);
                return;
            }

            var player = Main.LocalPlayer;
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var recipe = _selectedRecipe;

            // 检查真元
            if (qiResource.QiCurrent < recipe.QiCost)
            {
                ShowFeedback($"真元不足！需要 {recipe.QiCost}，当前 {qiResource.QiCurrent:F0}", Color.Red);
                return;
            }

            // 检查材料
            var materials = recipe.GetMaterials();
            foreach (var kvp in materials)
            {
                if (CountItemInInventory(kvp.Key) < kvp.Value)
                {
                    ShowFeedback($"材料不足: {Lang.GetItemName(kvp.Key)}", Color.Red);
                    return;
                }
            }

            // 批量合成次数
            int craftCount = 1;
            if (isBulk)
            {
                int maxByQi = (int)(qiResource.QiCurrent / recipe.QiCost);
                int maxByMaterials = int.MaxValue;
                foreach (var kvp in materials)
                {
                    int available = CountItemInInventory(kvp.Key);
                    maxByMaterials = Math.Min(maxByMaterials, available / kvp.Value);
                }
                craftCount = Math.Min(maxByQi, maxByMaterials);
                craftCount = Math.Min(craftCount, 10);
                if (craftCount <= 0) craftCount = 1;
            }

            // 执行合成
            int successCount = 0;
            for (int i = 0; i < craftCount; i++)
            {
                if (qiResource.QiCurrent < recipe.QiCost) break;

                bool hasAllMaterials = true;
                foreach (var kvp in materials)
                {
                    if (CountItemInInventory(kvp.Key) < kvp.Value)
                    {
                        hasAllMaterials = false;
                        break;
                    }
                }
                if (!hasAllMaterials) break;

                // 消耗材料
                foreach (var kvp in materials)
                {
                    ConsumeItemFromInventory(kvp.Key, kvp.Value);
                }

                // 消耗真元
                qiResource.QiCurrent -= recipe.QiCost;

                // 生成产物
                var resultItem = new Item();
                resultItem.SetDefaults(recipe.ResultType);
                resultItem.stack = 1;
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), resultItem);

                successCount++;
            }

            // 反馈
            if (successCount > 0)
            {
                string msg = successCount > 1
                    ? $"合炼成功！共合成 {successCount} 个 {_selectedRecipe.ResultName}"
                    : $"合炼成功！获得 {_selectedRecipe.ResultName}";
                ShowFeedback(msg, Color.LightGreen);
                Main.NewText(msg, Color.LightGreen);
            }
            else
            {
                ShowFeedback("合成失败，请检查条件", Color.Red);
            }

            RefreshDetail();
        }

        // ==================== 辅助方法 ====================

        private int CountItemInInventory(int itemType)
        {
            int count = 0;
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (item.type == itemType)
                    count += item.stack;
            }
            return count;
        }

        private void ConsumeItemFromInventory(int itemType, int count)
        {
            int remaining = count;
            var player = Main.LocalPlayer;
            for (int i = 0; i < player.inventory.Length && remaining > 0; i++)
            {
                var item = player.inventory[i];
                if (item.type == itemType)
                {
                    int take = Math.Min(item.stack, remaining);
                    item.stack -= take;
                    remaining -= take;
                    if (item.stack <= 0)
                        item.TurnToAir();
                }
            }
        }

        private void ShowFeedback(string text, Color color)
        {
            _craftFeedbackText.SetText(text);
            _craftFeedbackText.TextColor = color;
            _feedbackTimer = 180;
        }

        private static Color GetLevelColor(int level)
        {
            return level switch
            {
                1 => Color.LightGreen,
                2 => Color.LightBlue,
                3 => Color.LightCyan,
                4 => Color.Yellow,
                5 => Color.Orange,
                >= 6 => Color.Red,
                _ => Color.Gray,
            };
        }

        private void CloseUI()
        {
            ModContent.GetInstance<KongQiaoUISystem>().CloseGuCraftUI();
        }

        // ==================== 更新循环 ====================

        private bool _escapeWasDown;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ESC 关闭
            bool escapeDown = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
            if (!escapeDown && _escapeWasDown)
            {
                CloseUI();
            }
            _escapeWasDown = escapeDown;

            // 面板居中
            _mainPanel.Left.Set(Main.screenWidth / 2f - PanelWidth / 2f, 0f);
            _mainPanel.Top.Set(Main.screenHeight / 2f - PanelHeight / 2f, 0f);

            // 搜索文本轮询 — UITextBox 没有 OnTextChanged 事件，需要手动检测文本变化
            string currentSearchText = _searchBox.Text ?? "";
            if (currentSearchText != _pendingSearchText)
            {
                _pendingSearchText = currentSearchText;
                _searchDebounceTimer = 10;
            }

            // 搜索防抖
            if (_searchDebounceTimer > 0)
            {
                _searchDebounceTimer--;
                if (_searchDebounceTimer == 0)
                {
                    ApplySearch();
                }
            }

            // 反馈文本计时
            if (_feedbackTimer > 0)
            {
                _feedbackTimer--;
                if (_feedbackTimer == 0)
                {
                    _craftFeedbackText.SetText("");
                }
            }

            // 更新真元信息
            var player = Main.LocalPlayer;
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            _qiInfoText.SetText($"真元: {qiResource.QiCurrent:F0}/{qiResource.QiMaxCurrent:F0}  |  可用: {qiResource.QiAvailable}");

            // 定期刷新详情（更新材料数量显示）
            if (_selectedRecipe != null && _feedbackTimer == 0 && Main.GameUpdateCount % 30 == 0)
            {
                RefreshDetail();
            }
        }
    }
}
