using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace VerminLordMod.Content
{
	public class Text
	{
		public static void ShowTextRed(Player player, string text) {
			CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, 16, 16), Color.Red, text);
		}
		public static void ShowTextGreen(Player player, string text) {
			CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, 16, 16), Color.Green, text);
		}

		/// <summary>
		/// 在消息栏（聊天区域）显示金色文本
		/// </summary>
		public static void ShowMessageGold(string text) {
			Main.NewText(text, new Color(255, 215, 0));
		}
	}
}
