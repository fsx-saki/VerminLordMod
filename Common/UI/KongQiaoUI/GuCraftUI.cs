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
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 合炼面板 UI — 现代化扁平轻量风格
    /// </summary>
    public class GuCraftUI : UIState
    {
        private const int PanelWidth = 740;
        private const int PanelHeight = 540;

        private UIPanel _mainPanel;
        private UIText _titleText;
        private UITextPanel<string> _closeButton;
        private UIText _qiInfoText;

        private UITextBox _searchBox;
        private List<UICategoryButton> _categoryButtons;
        private string _selectedCategory;

        private UIList _recipeList;
        private UIScrollbar _recipeScrollbar;
        private List<GuRecipe> _filteredRecipes;

        private UIPanel _detailPanel;
        private UIText _detailNameText;
        private UIText _detailLevelText;
        private UIText _detailDescText;
        private UIText _detailQiCostText;

        private const int MaxMaterialSlots = 8;
        private UIItemSlot[] _ingredientSlots;
        private UIText[] _materialNameLabels;

        private UIItemSlot _resultSlot;
        private UIText _resultNameLabel;

        private UIText _arrowText;
        private UIText _resultLabel;
        private UIText _tipText;

        private UITextPanel<string> _craftButton;
        private UIText _craftFeedbackText;
        private int _feedbackTimer;

        private GuRecipe _selectedRecipe;
        private int _searchDebounceTimer;
        private string _pendingSearchText;

        public override void OnInitialize()
        {
            _mainPanel = UIHelper.CreatePanel(PanelWidth, PanelHeight);
            _mainPanel.BackgroundColor = UIStyles.PanelBg;
            _mainPanel.BorderColor = UIStyles.Border;
            Append(_mainPanel);

            // 标题栏
            var titleBar = UIHelper.CreateTitleBar(PanelWidth - 10f);
            titleBar.Left.Set(5f, 0f);
            titleBar.Top.Set(5f, 0f);
            _mainPanel.Append(titleBar);

            _titleText = UIHelper.CreateTitle("合炼台", 12f, 6f);
            titleBar.Append(_titleText);

            _qiInfoText = UIHelper.CreateText("真元: --/--", 120f, 8f, UIStyles.TextInfo, 0.7f);
            titleBar.Append(_qiInfoText);

            _closeButton = UIHelper.CreateCloseButton(PanelWidth - 10f);
            _closeButton.OnLeftClick += (evt, listener) => CloseUI();
            titleBar.Append(_closeButton);

            // 搜索框
            var searchPanel = UIHelper.CreateSubPanel(170f, 28f, UIStyles.PanelBgLighter, UIStyles.BorderInput, 0f);
            searchPanel.Left.Set(5f, 0f);
            searchPanel.Top.Set(46f, 0f);
            _mainPanel.Append(searchPanel);

            _searchBox = new UITextBox("", 0.7f, false);
            _searchBox.Width.Set(150f, 0f);
            _searchBox.Height.Set(22f, 0f);
            _searchBox.Left.Set(5f, 0f);
            _searchBox.Top.Set(3f, 0f);
            _searchBox.BackgroundColor = Color.Transparent;
            _searchBox.BorderColor = Color.Transparent;
            searchPanel.Append(_searchBox);

            var searchHint = UIHelper.CreateText("🔍", 148f, 4f, new Color(100, 110, 140), 0.65f);
            searchHint.IgnoresMouseInteraction = true;
            searchPanel.Append(searchHint);

            // 分类按钮
            _categoryButtons = new List<UICategoryButton>();
            string[] categories = { "全部", "力道蛊", "豕蛊", "酒虫", "破境丹" };
            string[] categoryKeys = { "All", "Strength", "PigGu", "WineBug", "RealmBreak" };
            float catX = 185f;
            for (int i = 0; i < categories.Length; i++)
            {
                int idx = i;
                var btn = new UICategoryButton(categories[i], categoryKeys[i]);
                btn.Left.Set(catX, 0f);
                btn.Top.Set(46f, 0f);
                btn.Width.Set(75f, 0f);
                btn.Height.Set(26f, 0f);
                btn.OnLeftClick += (evt, listener) => SelectCategory(categoryKeys[idx]);
                if (i == 0) btn.IsSelected = true;
                _mainPanel.Append(btn);
                _categoryButtons.Add(btn);
                catX += 79f;
            }
            _selectedCategory = "All";

            // 配方列表面板
            var listPanel = UIHelper.CreateSubPanel(290f, 420f, UIStyles.PanelBgLight, UIStyles.BorderLight);
            listPanel.Left.Set(5f, 0f);
            listPanel.Top.Set(80f, 0f);
            _mainPanel.Append(listPanel);

            var listLabel = UIHelper.CreateText("配方列表", 8f, 4f, UIStyles.TextSecondary, 0.75f);
            listPanel.Append(listLabel);

            _recipeScrollbar = UIHelper.CreateScrollbar(272f, 26f, 382f, 10f);
            listPanel.Append(_recipeScrollbar);

            _recipeList = UIHelper.CreateUIList(4f, 26f, 270f, 382f);
            _recipeList.SetScrollbar(_recipeScrollbar);
            listPanel.Append(_recipeList);

            // 详情面板
            _detailPanel = UIHelper.CreateSubPanel(430f, 420f, UIStyles.PanelBgLight, UIStyles.BorderLight);
            _detailPanel.Left.Set(302f, 0f);
            _detailPanel.Top.Set(80f, 0f);
            _mainPanel.Append(_detailPanel);

            _detailNameText = UIHelper.CreateTitle("请选择配方", 10f, 8f);
            _detailPanel.Append(_detailNameText);

            _detailLevelText = UIHelper.CreateText("", 10f, 32f, UIStyles.TextInfo, 0.7f);
            _detailPanel.Append(_detailLevelText);

            _detailDescText = new UIText("", 0.65f);
            _detailDescText.Left.Set(10f, 0f);
            _detailDescText.Top.Set(54f, 0f);
            _detailDescText.Width.Set(410f, 0f);
            _detailDescText.Height.Set(45f, 0f);
            _detailDescText.TextColor = UIStyles.TextSecondary;
            _detailPanel.Append(_detailDescText);

            _detailQiCostText = UIHelper.CreateText("", 10f, 100f, UIStyles.TextInfo, 0.7f);
            _detailPanel.Append(_detailQiCostText);

            var ingredientLabel = UIHelper.CreateText("所需材料:", 10f, 125f);
            _detailPanel.Append(ingredientLabel);

            // 材料槽位
            _ingredientSlots = new UIItemSlot[MaxMaterialSlots];
            _materialNameLabels = new UIText[MaxMaterialSlots];
            for (int i = 0; i < MaxMaterialSlots; i++)
            {
                int slotIdx = i;
                var slot = new UIItemSlot(new Color(38, 35, 45, 180), new Color(42, 48, 65, 200));
                slot.SlotIndex = i;
                slot.OnSlotClick = (idx) => OnIngredientSlotClick(idx);
                slot.Left.Set(-9999f, 0f);
                slot.Top.Set(-9999f, 0f);
                _ingredientSlots[i] = slot;
                _detailPanel.Append(slot);

                var matName = new UIText("", 0.5f);
                matName.Left.Set(-9999f, 0f);
                matName.Top.Set(-9999f, 0f);
                matName.Width.Set(50f, 0f);
                matName.Height.Set(18f, 0f);
                matName.TextColor = UIStyles.TextDim;
                _materialNameLabels[i] = matName;
                _detailPanel.Append(matName);
            }

            _arrowText = new UIText("→", 1.1f);
            _arrowText.Left.Set(-9999f, 0f);
            _arrowText.Top.Set(-9999f, 0f);
            _arrowText.TextColor = UIStyles.TextWarning;
            _arrowText.IgnoresMouseInteraction = true;
            _detailPanel.Append(_arrowText);

            _resultLabel = UIHelper.CreateText("产物:", -9999f, -9999f, UIStyles.TextMain, 0.7f);
            _resultLabel.IgnoresMouseInteraction = true;
            _detailPanel.Append(_resultLabel);

            _resultSlot = new UIItemSlot(new Color(42, 38, 30, 180), new Color(55, 70, 45, 200));
            _resultSlot.Left.Set(-9999f, 0f);
            _resultSlot.Top.Set(-9999f, 0f);
            _resultSlot.SetShowStack(false);
            _detailPanel.Append(_resultSlot);

            _resultNameLabel = new UIText("", 0.5f);
            _resultNameLabel.Left.Set(-9999f, 0f);
            _resultNameLabel.Top.Set(-9999f, 0f);
            _resultNameLabel.Width.Set(80f, 0f);
            _resultNameLabel.Height.Set(18f, 0f);
            _resultNameLabel.TextColor = UIStyles.TitleText;
            _detailPanel.Append(_resultNameLabel);

            // 合成按钮
            _craftButton = UIHelper.CreateButton("合炼", 110f, 34f, -9999f, -9999f, UIStyles.BtnPrimary, 0.85f);
            _craftButton.OnLeftClick += (evt, listener) => TryCraft(false);
            _craftButton.OnRightClick += (evt, listener) => TryCraft(true);
            _detailPanel.Append(_craftButton);

            _craftFeedbackText = new UIText("", 0.65f);
            _craftFeedbackText.Left.Set(10f, 0f);
            _craftFeedbackText.Top.Set(-9999f, 0f);
            _craftFeedbackText.Width.Set(410f, 0f);
            _craftFeedbackText.Height.Set(30f, 0f);
            _craftFeedbackText.TextColor = UIStyles.TextSuccess;
            _detailPanel.Append(_craftFeedbackText);

            _tipText = UIHelper.CreateText("提示: 点击材料槽查看背包 | 右键合炼按钮批量合成(最多10个)", 10f, -9999f, UIStyles.TextDim, 0.55f);
            _tipText.Width.Set(410f, 0f);
            _detailPanel.Append(_tipText);

            RefreshRecipeList();
        }

        private void SelectCategory(string categoryKey)
        {
            _selectedCategory = categoryKey;
            foreach (var btn in _categoryButtons)
                btn.IsSelected = btn.CategoryKey == categoryKey;
            _searchBox.SetText("");
            _pendingSearchText = "";
            RefreshRecipeList();
        }

        private void ApplySearch() => RefreshRecipeList();

        private void RefreshRecipeList()
        {
            _recipeList.Clear();
            _filteredRecipes = GetFilteredRecipes();

            if (_filteredRecipes.Count == 0)
            {
                var emptyText = UIHelper.CreateText("没有找到匹配的配方", 8f, 8f, UIStyles.TextDim, 0.7f);
                _recipeList.Add(emptyText);
                return;
            }

            for (int i = 0; i < _filteredRecipes.Count; i++)
            {
                int recipeIdx = i;
                var recipe = _filteredRecipes[i];

                var recipeItem = new UIPanel();
                recipeItem.Width.Set(260f, 0f);
                recipeItem.Height.Set(30f, 0f);
                recipeItem.MarginTop = 2f;

                bool isSelected = _selectedRecipe != null && recipe.ResultType == _selectedRecipe.ResultType;
                recipeItem.BackgroundColor = isSelected ? UIStyles.ListItemSelected : UIStyles.ListItemBg;
                recipeItem.BorderColor = isSelected ? UIStyles.BorderAccent : UIStyles.BorderLight;

                string levelTag = $"Lv{recipe.GuLevel}";
                var levelLabel = new UIText(levelTag, 0.6f);
                levelLabel.Left.Set(4f, 0f);
                levelLabel.Top.Set(6f, 0f);
                levelLabel.TextColor = UIStyles.GetGuLevelColor(recipe.GuLevel);
                recipeItem.Append(levelLabel);

                var nameLabel = new UIText(recipe.ResultName, 0.7f);
                nameLabel.Left.Set(36f, 0f);
                nameLabel.Top.Set(5f, 0f);
                nameLabel.TextColor = isSelected ? UIStyles.TextMain : UIStyles.TextSecondary;
                recipeItem.Append(nameLabel);

                var catLabel = new UIText(GuCraftSystem.GetCategoryDisplayName(recipe.Category), 0.5f);
                catLabel.Left.Set(180f, 0f);
                catLabel.Top.Set(7f, 0f);
                catLabel.TextColor = UIStyles.TextDim;
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
                _craftButton.BackgroundColor = UIStyles.BtnDisabled;
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

            _detailNameText.SetText(recipe.ResultName);
            _detailLevelText.SetText($"蛊虫等级: {recipe.GuLevel}转");
            _detailDescText.SetText(recipe.Description);

            bool hasQi = qiResource.QiCurrent >= recipe.QiCost;
            _detailQiCostText.SetText($"消耗真元: {recipe.QiCost}");
            _detailQiCostText.TextColor = hasQi ? UIStyles.TextInfo : UIStyles.TextDanger;

            const int SlotsPerRow = 4;
            const float SlotSpacingX = 52f;
            const float SlotSpacingY = 52f;
            const float MaterialStartY = 150f;

            var materials = recipe.GetMaterials();
            int materialCount = System.Math.Min(materials.Count, MaxMaterialSlots);
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
                _materialNameLabels[idx].Top.Set(slotY + 48f, 0f);
                _materialNameLabels[idx].SetText($"{matItem.Name}\n{playerStack}/{kvp.Value}");
                _materialNameLabels[idx].TextColor = hasMaterial ? UIStyles.TextSuccess : UIStyles.TextDanger;

                idx++;
            }
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

            int lastRow = (materialCount - 1) / SlotsPerRow;
            int lastRowCol = (materialCount - 1) % SlotsPerRow;
            float arrowX = 10f + System.Math.Min(lastRowCol + 1, SlotsPerRow) * SlotSpacingX + 5f;
            float arrowY = MaterialStartY + lastRow * SlotSpacingY;

            _arrowText.Left.Set(arrowX, 0f);
            _arrowText.Top.Set(arrowY, 0f);

            _resultLabel.Left.Set(arrowX + 32f, 0f);
            _resultLabel.Top.Set(MaterialStartY - 22f + lastRow * SlotSpacingY, 0f);

            _resultSlot.Left.Set(arrowX + 32f, 0f);
            _resultSlot.Top.Set(arrowY, 0f);

            _resultNameLabel.Left.Set(arrowX + 32f, 0f);
            _resultNameLabel.Top.Set(arrowY + 48f, 0f);

            var resultItem = new Item();
            resultItem.SetDefaults(recipe.ResultType);
            _resultSlot.SetItem(resultItem);
            _resultNameLabel.SetText(resultItem.Name);

            float buttonY = MaterialStartY + (lastRow + 1) * SlotSpacingY + 10f;
            _craftButton.Left.Set(150f, 0f);
            _craftButton.Top.Set(buttonY, 0f);

            bool canCraft = hasQi && allMaterialsMet;
            _craftButton.BackgroundColor = canCraft ? UIStyles.BtnPrimary : UIStyles.BtnDisabled;
            _craftButton.SetText(canCraft ? "合炼" : "条件不足");

            _craftFeedbackText.Top.Set(buttonY + 38f, 0f);
            _tipText.Top.Set(buttonY + 80f, 0f);
        }

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
                Main.NewText($"[材料充足] {itemName} 背包 x{playerStack}（需要 x{needed}）", Color.LightGreen);
            else
                Main.NewText($"[材料不足] {itemName} 背包 x{playerStack}（需要 x{needed}）", Color.Red);

            RefreshDetail();
        }

        private void TryCraft(bool isBulk)
        {
            if (_selectedRecipe == null)
            {
                ShowFeedback("请先选择配方", UIStyles.TextDanger);
                return;
            }

            var player = Main.LocalPlayer;
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var recipe = _selectedRecipe;

            if (qiResource.QiCurrent < recipe.QiCost)
            {
                ShowFeedback($"真元不足！需要 {recipe.QiCost}，当前 {qiResource.QiCurrent:F0}", UIStyles.TextDanger);
                return;
            }

            var materials = recipe.GetMaterials();
            foreach (var kvp in materials)
            {
                if (CountItemInInventory(kvp.Key) < kvp.Value)
                {
                    ShowFeedback($"材料不足: {Lang.GetItemName(kvp.Key)}", UIStyles.TextDanger);
                    return;
                }
            }

            int craftCount = 1;
            if (isBulk)
            {
                int maxByQi = (int)(qiResource.QiCurrent / recipe.QiCost);
                int maxByMaterials = int.MaxValue;
                foreach (var kvp in materials)
                {
                    int available = CountItemInInventory(kvp.Key);
                    maxByMaterials = System.Math.Min(maxByMaterials, available / kvp.Value);
                }
                craftCount = System.Math.Min(maxByQi, maxByMaterials);
                craftCount = System.Math.Min(craftCount, 10);
                if (craftCount <= 0) craftCount = 1;
            }

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

                foreach (var kvp in materials)
                    ConsumeItemFromInventory(kvp.Key, kvp.Value);

                qiResource.QiCurrent -= recipe.QiCost;

                var resultItem = new Item();
                resultItem.SetDefaults(recipe.ResultType);
                resultItem.stack = 1;
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), resultItem);
                successCount++;
            }

            if (successCount > 0)
            {
                string msg = successCount > 1
                    ? $"合炼成功！共合成 {successCount} 个 {_selectedRecipe.ResultName}"
                    : $"合炼成功！获得 {_selectedRecipe.ResultName}";
                ShowFeedback(msg, UIStyles.TextSuccess);
                Main.NewText(msg, Color.LightGreen);
            }
            else
            {
                ShowFeedback("合成失败，请检查条件", UIStyles.TextDanger);
            }

            RefreshDetail();
        }

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
                    int take = System.Math.Min(item.stack, remaining);
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

        private static Color GetLevelColor(int level) => UIStyles.GetGuLevelColor(level);

        private void CloseUI() => ModContent.GetInstance<KongQiaoUISystem>().CloseGuCraftUI();

        private bool _escapeWasDown;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (UIHelper.CheckEscapeReleased(ref _escapeWasDown))
                CloseUI();

            UIHelper.UpdatePanelCenter(_mainPanel, PanelWidth, PanelHeight);

            string currentSearchText = _searchBox.Text ?? "";
            if (currentSearchText != _pendingSearchText)
            {
                _pendingSearchText = currentSearchText;
                _searchDebounceTimer = 10;
            }

            if (_searchDebounceTimer > 0)
            {
                _searchDebounceTimer--;
                if (_searchDebounceTimer == 0)
                    ApplySearch();
            }

            if (_feedbackTimer > 0)
            {
                _feedbackTimer--;
                if (_feedbackTimer == 0)
                    _craftFeedbackText.SetText("");
            }

            var player = Main.LocalPlayer;
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            _qiInfoText.SetText($"真元: {qiResource.QiCurrent:F0}/{qiResource.QiMaxCurrent:F0}  |  可用: {qiResource.QiAvailable}");

            if (_selectedRecipe != null && _feedbackTimer == 0 && Main.GameUpdateCount % 30 == 0)
                RefreshDetail();
        }
    }
}
