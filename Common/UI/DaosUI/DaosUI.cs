using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.QiUI;
using VerminLordMod.Content;

namespace VerminLordMod.Common.UI.DaosUI
{

	public class DaosUI:UIState
	{
		public override void OnInitialize() {
			var panel = new UIPanel();
			panel.Width.Set(400f, 0f);  // 宽度400像素
			panel.Height.Set(300f, 0f); // 高度300像素

			// 计算居中位置
			panel.Left.Set(Main.screenWidth / 2f - 200f, 0f); // (屏幕宽度/2 - 元素宽度/2)
			panel.Top.Set(Main.screenHeight / 2f - 150f, 0f); // (屏幕高度/2 - 元素高度/2)

			Append(panel);

		}
		public void RecalculatePosition() {
			foreach (var element in Elements) {
				if (element is UIPanel panel) {
					panel.Left.Set(Main.screenWidth / 2f - panel.Width.Pixels / 2f, 0f);
					panel.Top.Set(Main.screenHeight / 2f - panel.Height.Pixels / 2f, 0f);
				}
			}
			Recalculate();
		}
	}
	public class CustomButton : UIButton<Texture2D>  // 明确指定泛型参数为 Texture2D
	{
		public UIText ButtonText { get; private set; }
		public CustomButton(string texture, string text) : base(Main.Assets.Request<Texture2D>("Assets/Textures/UI/Button").Value) {
			SetText( Main.Assets.Request<Texture2D>(texture).Value);
			Width.Set(100f, 0f);
			Height.Set(30f, 0f);
			// 创建文本元素
			ButtonText = new UIText(text);
			ButtonText.HAlign = 0.5f; // 水平居中
			ButtonText.VAlign = 0.5f; // 垂直居中
			ButtonText.Top.Set(-2f, 0f); // 微调垂直位置

			Append(ButtonText); // 将文本添加为按钮的子元素

			// 可选：添加悬停效果
			OnMouseOver += (evt, listener) => BackgroundColor = Color.LightGray;
		}
		public void ChangeText(string newText) {
			ButtonText.SetText(newText);
		}
		public override void LeftClick(UIMouseEvent evt) {
			Main.NewText("按钮被点击了！");
			// 在这里添加点击逻辑
		}
	}
	public class DaosUISystem : ModSystem
	{
		private UserInterface _daosUI;
		public static DaosUI DaosUIInstance;

		public override void Load() {
			if (!Main.dedServ) // 确保不在服务器端运行
			{
				_daosUI = new UserInterface();
				DaosUIInstance = new DaosUI();
				DaosUIInstance.Activate();
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			_daosUI?.Update(gameTime);
		}
		// 显示/隐藏的公共方法
		public void ToggleUI() {
			if (_daosUI.CurrentState == null) {
				_daosUI.SetState(DaosUIInstance); // 显示UI
			}
			else {
				_daosUI.SetState(null); // 隐藏UI
			}
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"VerminLordMod: Daos UI",
					() => {
						_daosUI.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			// 当屏幕尺寸变化时重新计算位置
			if (_daosUI?.CurrentState is DaosUI ui) {
				ui.RecalculatePosition();
			}
		}
	}
}
