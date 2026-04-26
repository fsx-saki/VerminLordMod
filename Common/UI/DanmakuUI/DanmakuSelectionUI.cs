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
using VerminLordMod.Content.Items.Debuggers;

namespace VerminLordMod.Common.UI.DanmakuUI
{
	/// <summary>
	/// 弹幕条目数据结构（命名空间级别，可被所有类访问）
	/// </summary>
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
	/// 弹幕选择UI - 列出模组中所有ModProjectile供选择
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

		/// <summary>
		/// 所有已缓存的弹幕条目
		/// </summary>
		private List<ProjectileEntry> _allProjectiles = new List<ProjectileEntry>();

		/// <summary>
		/// 当前显示的弹幕条目（过滤后）
		/// </summary>
		private List<ProjectileEntry> _filteredProjectiles = new List<ProjectileEntry>();

		/// <summary>
		/// 当前选中的分类过滤
		/// </summary>
		private string _currentCategory = "全部";

		/// <summary>
		/// 分类按钮列表
		/// </summary>
		private List<UITextPanel<string>> _categoryButtons = new List<UITextPanel<string>>();

		public override void OnInitialize()
		{
			// 主面板
			_mainPanel = new UIPanel();
			_mainPanel.Width.Set(700f, 0f);
			_mainPanel.Height.Set(540f, 0f);
			_mainPanel.Left.Set(Main.screenWidth / 2f - 350f, 0f);
			_mainPanel.Top.Set(Main.screenHeight / 2f - 270f, 0f);
			_mainPanel.BackgroundColor = new Color(30, 30, 50, 230);
			_mainPanel.BorderColor = new Color(80, 80, 140, 255);
			Append(_mainPanel);

			// 标题
			_titleText = new UIText("弹幕测试武器 - 选择弹幕", 1.2f);
			_titleText.Left.Set(20f, 0f);
			_titleText.Top.Set(10f, 0f);
			_titleText.TextColor = Color.Gold;
			_mainPanel.Append(_titleText);

			// 状态文本
			_statusText = new UIText("正在加载弹幕列表...", 0.8f);
			_statusText.Left.Set(20f, 0f);
			_statusText.Top.Set(40f, 0f);
			_statusText.TextColor = Color.Gray;
			_mainPanel.Append(_statusText);

			// 关闭按钮
			_closeButton = new UITextPanel<string>("关闭 [ESC]", 0.9f);
			_closeButton.Width.Set(120f, 0f);
			_closeButton.Height.Set(30f, 0f);
			_closeButton.Left.Set(560f, 0f);
			_closeButton.Top.Set(10f, 0f);
			_closeButton.BackgroundColor = new Color(80, 30, 30);
			_closeButton.OnLeftClick += (evt, listener) => CloseUI();
			_mainPanel.Append(_closeButton);

			// 分类面板（放置分类按钮，支持换行）
			_categoryPanel = new UIPanel();
			_categoryPanel.Width.Set(660f, 0f);
			_categoryPanel.Height.Set(80f, 0f);
			_categoryPanel.Left.Set(20f, 0f);
			_categoryPanel.Top.Set(70f, 0f);
			_categoryPanel.BackgroundColor = new Color(20, 20, 40, 200);
			_categoryPanel.BorderColor = new Color(50, 50, 100, 200);
			_mainPanel.Append(_categoryPanel);

			// 滚动条
			_scrollbar = new UIScrollbar();
			_scrollbar.Left.Set(670f, 0f);
			_scrollbar.Top.Set(160f, 0f);
			_scrollbar.Height.Set(360f, 0f);
			_scrollbar.Width.Set(16f, 0f);
			_mainPanel.Append(_scrollbar);

			// 弹幕列表
			_projectileList = new UIList();
			_projectileList.Left.Set(20f, 0f);
			_projectileList.Top.Set(160f, 0f);
			_projectileList.Width.Set(650f, 0f);
			_projectileList.Height.Set(360f, 0f);
			_projectileList.SetScrollbar(_scrollbar);
			_mainPanel.Append(_projectileList);
		}

		/// <summary>
		/// 加载所有模组弹幕
		/// </summary>
		public void LoadProjectiles()
		{
			_allProjectiles.Clear();

			try
			{
				// 通过 ModLoader.Mods 遍历所有已加载的模组，获取所有 ModProjectile
				foreach (var mod in ModLoader.Mods)
				{
					if (mod == null) continue;

					// 获取该模组的所有弹幕类型
					var projTypes = mod.GetContent<ModProjectile>();
					foreach (var modProj in projTypes)
					{
						if (modProj == null) continue;

						int type = modProj.Type;
						string name = modProj.Name;
						string fullName = modProj.GetType().FullName ?? "";

						// 推断分类：根据命名空间或类名前缀
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

				// 按分类排序，同分类内按名称排序
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

			// 构建分类按钮
			BuildCategoryButtons();

			// 应用过滤
			ApplyFilter();
		}

		/// <summary>
		/// 根据完整类型名和类名推断弹幕分类
		/// </summary>
		private string InferCategory(string fullName, string name)
		{
			// 根据命名空间判断
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

				// 根据类名关键词
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

		/// <summary>
		/// 构建分类过滤按钮（支持换行）
		/// </summary>
		private void BuildCategoryButtons()
		{
			_categoryButtons.Clear();
			_categoryPanel.RemoveAllChildren();

			// 收集所有分类
			var categories = _allProjectiles
				.Select(p => p.Category)
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			categories.Insert(0, "全部");

			float panelWidth = _categoryPanel.Width.Pixels - 10f;
			float xOffset = 5f;
			float yOffset = 5f;
			float rowHeight = 30f;

			foreach (var category in categories)
			{
				var btn = new UITextPanel<string>(category, 0.7f);
				float btnWidth = Math.Max(60f, category.Length * 14f);
				btn.Width.Set(btnWidth, 0f);
				btn.Height.Set(26f, 0f);

				// 如果当前行放不下，换行
				if (xOffset + btnWidth + 5f > panelWidth)
				{
					xOffset = 5f;
					yOffset += rowHeight;
				}

				btn.Left.Set(xOffset, 0f);
				btn.Top.Set(yOffset, 0f);
				btn.BackgroundColor = category == _currentCategory
					? new Color(60, 80, 160)
					: new Color(40, 40, 80);
				btn.BorderColor = new Color(80, 80, 140);

				string capturedCategory = category;
				btn.OnLeftClick += (evt, listener) =>
				{
					_currentCategory = capturedCategory;
					ApplyFilter();
					BuildCategoryButtons(); // 刷新按钮高亮
				};

				_categoryPanel.Append(btn);
				_categoryButtons.Add(btn);
				xOffset += btnWidth + 5f;
			}
		}

		/// <summary>
		/// 应用分类过滤
		/// </summary>
		private void ApplyFilter()
		{
			_projectileList.Clear();

			if (_currentCategory == "全部")
			{
				_filteredProjectiles = new List<ProjectileEntry>(_allProjectiles);
			}
			else
			{
				_filteredProjectiles = _allProjectiles
					.Where(p => p.Category == _currentCategory)
					.ToList();
			}

			// 构建列表项
			foreach (var entry in _filteredProjectiles)
			{
				var item = new DanmakuListItem(entry, OnProjectileSelected);
				_projectileList.Add(item);
			}

			_statusText.SetText($"共 {_allProjectiles.Count} 个弹幕 | 当前显示: {_filteredProjectiles.Count} 个 ({_currentCategory})");
		}

		/// <summary>
		/// 弹幕被选中时的回调
		/// </summary>
		private void OnProjectileSelected(ProjectileEntry entry)
		{
			DanmakuTestWeapon.SelectedProjectileType = entry.Type;
			DanmakuTestWeapon.SelectedProjectileName = entry.DisplayName;
			Main.NewText($"[弹幕测试武器] 已选择弹幕: {entry.DisplayName} (Type: {entry.Type})", Color.Green);
			CloseUI();
		}

		/// <summary>
		/// 关闭UI
		/// </summary>
		private void CloseUI()
		{
			ModContent.GetInstance<DanmakuSelectionUISystem>().ToggleUI();
		}

		private bool _escapeWasDown = false;

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// 按ESC关闭（边沿触发：检测按键从按下到释放，避免反复触发）
			bool escapeDown = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape);
			if (!escapeDown && _escapeWasDown)
			{
				CloseUI();
			}
			_escapeWasDown = escapeDown;

			// 跟随屏幕大小调整位置
			_mainPanel.Left.Set(Main.screenWidth / 2f - 350f, 0f);
			_mainPanel.Top.Set(Main.screenHeight / 2f - 270f, 0f);
		}
	}

	/// <summary>
	/// 弹幕列表项UI元素
	/// </summary>
	public class DanmakuListItem : UIPanel
	{
		private ProjectileEntry _entry;
		private Action<ProjectileEntry> _onClick;
		private UIText _nameText;
		private UIText _typeText;
		private UIText _categoryText;

		public DanmakuListItem(ProjectileEntry entry, Action<ProjectileEntry> onClick)
		{
			_entry = entry;
			_onClick = onClick;

			Width.Set(630f, 0f);
			Height.Set(32f, 0f);
			BackgroundColor = new Color(40, 40, 70, 200);
			BorderColor = new Color(60, 60, 100, 200);

			// 弹幕名称
			_nameText = new UIText(entry.DisplayName, 0.8f);
			_nameText.Left.Set(10f, 0f);
			_nameText.Top.Set(6f, 0f);
			_nameText.TextColor = Color.White;
			Append(_nameText);

			// 分类标签
			_categoryText = new UIText($"[{entry.Category}]", 0.7f);
			_categoryText.Left.Set(400f, 0f);
			_categoryText.Top.Set(6f, 0f);
			_categoryText.TextColor = Color.CornflowerBlue;
			Append(_categoryText);

			// Type ID
			_typeText = new UIText($"Type: {entry.Type}", 0.7f);
			_typeText.Left.Set(530f, 0f);
			_typeText.Top.Set(6f, 0f);
			_typeText.TextColor = Color.Gray;
			Append(_typeText);

			// 悬停效果
			OnMouseOver += (evt, listener) =>
			{
				BackgroundColor = new Color(60, 80, 120, 200);
			};
			OnMouseOut += (evt, listener) =>
			{
				BackgroundColor = new Color(40, 40, 70, 200);
			};
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			base.LeftClick(evt);
			_onClick?.Invoke(_entry);
		}
	}

	/// <summary>
	/// 弹幕选择UI系统 - 管理UI的显示/隐藏
	/// </summary>
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

		/// <summary>
		/// 切换UI显示/隐藏
		/// </summary>
		public void ToggleUI()
		{
			if (_danmakuUI.CurrentState == null)
			{
				// 打开UI时重新加载弹幕列表
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
						{
							_danmakuUI.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
