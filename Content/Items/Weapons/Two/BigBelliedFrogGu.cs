using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Two
{
	class BigBelliedFrogGu : GuWeaponItem//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 10;
		protected override int _useTime => 26;
		protected override int _guLevel => 2;

		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 0;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<AcidWaterProj>();
			Item.shootSpeed = 5f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (items.Count == 0) {
					Text.ShowTextRed(player, "呱！啥也没有了");
					return false;
				}
				Text.ShowTextRed(player, "呱！");
				//Text.ShowTextRed(player, items[items.Count - 1].ToString());

				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), items[items.Count - 1], items[items.Count - 1].stack);
				items.RemoveAt(items.Count - 1);
				return false;
			}else {
				if (items.Count >= 10) {
					Text.ShowTextRed(player, "呱！装不下了");

				}
				else {
					foreach (var item in player.inventory) {
						if (!item.IsAir && !(item.ModItem is IGu)) {
							items.Add(item.Clone());
							//Main.NewText(items);
							item.TurnToAir();
							break;
						}
					}
				}
				return false;

			 }

		}

		//public override bool CanRightClick() {
		//	if(hasBeenControlled) return true;
		//	return false;
		//}
		public List<Item> items = new List<Item>();
		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			tag["items"] = items;
		}
		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			items = (List<Item>)tag.GetList<Item>("items");
		}
		//public override void RightClick(Player player) {
		//BigBelliedFrogGu bigBelliedFrogGu=(BigBelliedFrogGu)this.Clone(this.Entity);
		//Main.NewText("副本创建成功");

		//if (items.Count >= 10){
		//	Text.ShowTextRed(player, "呱！装不下了");
		//	//var bbfg = new BigBelliedFrogGu();
		//	//Item.NewItem(null, player.Center, bbfg.Item);
		//	return;
		//}

		//for (int i = 1; i < 58; i++) {
		//	var item = player.inventory[i];
		//	if (!item.IsAir && !(item.ModItem is IGu)) {
		//		items.Add(item);
		//		Main.NewText(items);
		//		player.inventory[i].TurnToAir();
		//		break;
		//	}
		//}
		//}
	}
}
