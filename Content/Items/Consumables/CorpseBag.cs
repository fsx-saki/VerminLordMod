using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Content.Items.Consumables
{
	/// <summary>
	/// 尸囊袋 — 击杀敌人后掉落的战利品袋。
	/// 类似宝藏袋，右键打开获得模组物品（元石、蛊虫等）。
	/// 原版物品仍然正常掉落。
	/// </summary>
	public class CorpseBag : ModItem
	{
		/// <summary>袋内存储的物品</summary>
		public List<Item> StoredItems = new();

		/// <summary>来源 NPC 名称（显示用）</summary>
		public string SourceNPCName = "";

		public override void SetStaticDefaults()
		{
			// 允许储存物品数据
			ItemID.Sets.ItemNoGravity[Type] = false;
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
			Item.value = 0;
			Item.rare = ItemRarityID.Quest;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.UseSound = SoundID.Item7;
		}

		public override bool CanRightClick() => StoredItems.Count > 0;

		public override void RightClick(Player player)
		{
			if (StoredItems.Count == 0) return;

			foreach (var item in StoredItems)
			{
				if (item != null && !item.IsAir)
				{
					player.QuickSpawnItem(player.GetSource_OpenItem(Type), item);
				}
			}

			StoredItems.Clear();

			if (player.whoAmI == Main.myPlayer)
			{
				Main.NewText(
					string.IsNullOrEmpty(SourceNPCName)
						? "搜刮了尸体战利品。"
						: $"从 {SourceNPCName} 的尸体中搜刮到了战利品。",
					Color.LightGreen);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			int count = StoredItems.Count;
			if (count > 0)
			{
				string label = string.IsNullOrEmpty(SourceNPCName)
					? $"右键打开 ({count} 件)"
					: $"{SourceNPCName} 的遗物 ({count} 件)";
				tooltips.Add(new TooltipLine(Mod, "CorpseBagInfo", label));
			}
			else
			{
				tooltips.Add(new TooltipLine(Mod, "CorpseBagEmpty", "空的"));
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["source"] = SourceNPCName ?? "";

			var items = new List<TagCompound>();
			foreach (var item in StoredItems)
			{
				if (item != null && !item.IsAir)
				{
					items.Add(ItemIO.Save(item));
				}
			}
			tag["items"] = items;
		}

		public override void LoadData(TagCompound tag)
		{
			SourceNPCName = tag.GetString("source");

			StoredItems.Clear();
			if (tag.TryGet("items", out List<TagCompound> items))
			{
				foreach (var entry in items)
				{
					var item = ItemIO.Load(entry);
					if (item != null && !item.IsAir)
						StoredItems.Add(item);
				}
			}
		}
	}
}
