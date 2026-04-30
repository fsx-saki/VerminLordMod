using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
	/// <summary>
	/// 卸蛊符 - 用于卸下空窍中的本命蛊，消耗此道具
	/// </summary>
	public class UnbindGuItem : ModItem
	{
		public override void SetStaticDefaults() {
			// Tooltip显示在游戏内
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 9999;
			Item.value = 1000;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 30;
			Item.useAnimation = 30;
		}

		public override bool CanUseItem(Player player) {
			var kongQiao = player.GetModPlayer<KongQiaoPlayer>();
			// 检查是否有本命蛊可以卸下
			return kongQiao.KongQiao.Any(s => s.IsMainGu);
		}

		public override bool? UseItem(Player player) {
			if (Main.myPlayer != player.whoAmI) return null;

			var kongQiao = player.GetModPlayer<KongQiaoPlayer>();

			// 找到本命蛊
			var mainGuSlot = kongQiao.KongQiao.FirstOrDefault(s => s.IsMainGu);
			if (mainGuSlot == null) return null;

			// 卸下本命蛊
			player.QuickSpawnItem(player.GetSource_GiftOrReward(), mainGuSlot.GuItem.Clone());
			kongQiao.KongQiao.Remove(mainGuSlot);
			kongQiao.RefreshQiOccupied();

			Main.NewText($"已卸下本命蛊：{mainGuSlot.GuItem.Name}", Color.LightGreen);
			return true;
		}
	}
}
