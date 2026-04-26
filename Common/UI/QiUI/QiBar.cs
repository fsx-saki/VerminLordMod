using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.UI.QiUI
{
	// This custom UI will show whenever the player is holding the ExampleCustomResourceWeapon item and will display the player's custom resource amounts that are tracked in ExampleResourcePlayer
	public class QiBar : UIState
	{
		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private UIText text;
		private UIText text2;
		private UIElement area;
		private UIImage barFrame;
		private Color gradientA;
		private Color gradientB;

		public override void OnInitialize() {
			// Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
			// UIElement is invisible and has no padding.
			area = new UIElement();
			area.Left.Set(-area.Width.Pixels - 600, 1f); // Place the resource bar to the left of the hearts.
			area.Top.Set(30, 0f); // Placing it just a bit below the top of the screen.
			area.Width.Set(182, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
			area.Height.Set(60, 0f);

			barFrame = new UIImage(ModContent.Request<Texture2D>("VerminLordMod/Common/UI/QiUI/QiFrame")); // Frame of our resource bar
			barFrame.Left.Set(22, 0f);
			barFrame.Top.Set(0, 0f);
			barFrame.Width.Set(138, 0f);
			barFrame.Height.Set(34, 0f);

			text = new UIText("0/0", 0.8f); // text to show stat
			text.Width.Set(138, 0f);
			text.Height.Set(34, 0f);
			text.Top.Set(40, 0f);
			text.Left.Set(0, 0f);

			text2 = new UIText("0", 0.8f); // text to show stat
			text2.Width.Set(138, 0f);
			text2.Height.Set(34, 0f);
			text2.Top.Set(60, 0f);
			text2.Left.Set(0, 0f);

			gradientA = new Color(123, 25, 138); // A dark purple
			gradientB = new Color(187, 91, 201); // A light purple

			area.Append(text);
			area.Append(text2);
			area.Append(barFrame);
			Append(area);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
		}
		string yuan = "真元";
		string qiao = "空窍";
		// Here we draw our UI
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			var modPlayer = Main.LocalPlayer.GetModPlayer<QiPlayer>();
			// Calculate quotient
			float quotient = modPlayer.kongQiaoMax == 0 ? 0 : (float)modPlayer.qiCurrent / modPlayer.kongQiaoMax; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
			quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.

			// Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
			var hitbox = barFrame.GetInnerDimensions().ToRectangle();
			hitbox.X += 12;
			hitbox.Width -= 24;
			hitbox.Y += 8;
			hitbox.Height -= 16;

			// Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
			switch (modPlayer.qiLevel) {
				case 1://一转青铜
					gradientA = new Color(192, 255, 62); 
					gradientB = new Color(102, 205, 0); 
					break;
				case 2:
					gradientA = new Color(205, 155, 155);
					gradientB = new Color(205, 92, 92); 
					break;
				case 3:
					gradientA = new Color(112, 128, 144); 
					gradientB = new Color(0, 0, 0); 
					break;
				case 4:
					gradientA = new Color(255,255, 0); 
					gradientB = new Color(220, 220, 0); 
					break;
				case 5:
					gradientA = new Color(123, 25, 138); 
					gradientB = new Color(187, 91, 201); 
					break;
				case 6:
					gradientA = new Color(0, 250, 154); 
					gradientB = new Color(0 ,255, 255); 
					break;
				case 7:
					gradientA = new Color(255, 240, 245); 
					gradientB = new Color(255, 106, 106);
					break;
				case 8:
					gradientA = new Color(74, 112, 139); 
					gradientB = new Color(0, 0, 0); 
					break;
				case 9:
					gradientA = new Color(255, 127, 0); 
					gradientB = new Color(238, 201, 0); 
					break;
				case 10:
					gradientA = new Color(255,255,255); 
					gradientB = new Color(0, 0, 0); 
					break;
			}
			int left = hitbox.Left;
			int right = hitbox.Right;
			int steps = (int)((right - left) * quotient);
			for (int i = 0; i < steps; i += 1) {
				// float percent = (float)i / steps; // Alternate Gradient Approach
				float percent = (float)i / (right - left);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
			}
		}

		public override void Update(GameTime gameTime) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<QiPlayer>();

			// Setting the text per tick to update and show our resource values.
			string str;
			//float r = (float)(modPlayer.qiCurrent / modPlayer.qiMax2) * 10;
			//int rr = (int)r;
			int rr = modPlayer.kongQiaoMax == 0 ? -1 : modPlayer.qiCurrent * 10 / modPlayer.kongQiaoMax;
			if (!modPlayer.qiEnabled)
				rr = -1;
			if (modPlayer.qiLevel >= 6) {
				yuan = "仙元";
				qiao = "仙窍";
			}
			else {
				yuan = "真元";
				qiao = "空窍";
			}
			switch (rr) {
				case -1:
					str = "未开元海";
					break;
				case 0:
					str = yuan+"耗尽";
					break;
				case 1:
					str = "一成"+yuan;
					break;
				case 2:
					str = "二成" + yuan;
					break;
				case 3:
					str = "三成" + yuan;
					break;
				case 4:
					str = "四成" + yuan;
					break;
				case 5:
					str = "五成" + yuan;
					break;
				case 6:
					str = "六成"+yuan;
					break;
				case 7:
					str = "七成"+yuan;
					break;
				case 8:
					str = "八成"+yuan;
					break;
				case 9:
					str = "九成"+yuan;
					break;
				case 10:
					str = "十成"+yuan;
					break;
				default:
					str = "未知"+yuan;
					break;
			}
			//text.SetText(modPlayer.qiCurrent.ToString()+"/"+modPlayer.qiMax2.ToString());
			text.SetText(qiao+"：" + str + "[" + modPlayer.qiCurrent.ToString() + "/" + modPlayer.qiMax2.ToString() + "/" + modPlayer.kongQiaoMax.ToString() + "]");



			string str2 = "";
			switch (modPlayer.levelStage) {
				case 0:
					str2 = "初期";
					break;
				case 1:
					str2 = "中期";
					break;
				case 2:
					str2 = "后期";
					break;
				case 3:
					str2 = "巅峰";
					break;
			}
			string str3 = "";

			switch (modPlayer.PlayerZiZhi) {
				case ZiZhi.RJIA:
					str3 = "甲等";
					break;
				case ZiZhi.RYI:
					str3 = "乙等";
					break;
				case ZiZhi.RBING:
					str3 = "丙等";
					break;
				case ZiZhi.RDING:
					str3 = "丁等";
					break;
				case ZiZhi.RO:
					str3 = "未知";
					break;
				case ZiZhi.GUA:
					str3 = "开发者";
					break;
			}

			text2.SetText($"资质：{str3}  境界：{modPlayer.qiLevel}转{str2}");
			base.Update(gameTime);
		}
	}

	// This class will only be autoloaded/registered if we're not loading on a server
	[Autoload(Side = ModSide.Client)]
	public class ExampleResourceUISystem : ModSystem
	{
		private UserInterface QiBarUserInterface;

		public QiBar QiBar;

		public static LocalizedText QiText { get; private set; }

		public override void Load() {
			QiBar = new();
			QiBarUserInterface = new();
			QiBarUserInterface.SetState(QiBar);

			string category = "UI";
			QiText ??= Mod.GetLocalization($"{category}.Qi");
		}

		public override void UpdateUI(GameTime gameTime) {
			QiBarUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"VerminLordMod: Qi Bar",
					delegate {
						QiBarUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
