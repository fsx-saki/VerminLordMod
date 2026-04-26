using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class TianyuanBaolianGu : PractiseWeapon
	{
		protected override int _guLevel => 3;
		protected override int _useTime => 50;
		protected override int qiCost => 40;
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public static LocalizedText yuanRateText { get; private set; }
		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
			yuanRateText = this.GetLocalization("yuanRateText");
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
			tooltips.Add(new TooltipLine(Mod, "yuanRateText", yuanRateText.Format(yuanRate)));

		}
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
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 0;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<Spout>();
			Item.shootSpeed = 0f;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (controlRate >= 100) {
					Text.ShowTextGreen(player, $"获得{(int)this.yuanRate}颗元石");
					player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ModContent.ItemType<YuanS>(),(int)this.yuanRate);
					this.yuanRate = 0f;
				}
				return false;

			}
			else {
				if (controlRate >= 100) {
					QiPlayer qiPlayer=player.GetModPlayer<QiPlayer>();
					int currQi=qiPlayer.qiCurrent;
					qiPlayer.qiCurrent+=(int)this.yuanRate*16;
					this.yuanRate -= (qiPlayer.qiCurrent - currQi)/16;
					Text.ShowTextGreen(player, $"补充{qiPlayer.qiCurrent - currQi}真元");
				}
				return false;
			}



		}
		public float yuanRate = 0f;
		public override void UpdateInventory(Player player) {
			if (controlRate == 100) {
				hasBeenControlled = true;
				this.yuanRate += 0.0002f;
			}
			if (hasBeenControlled)
				return;
			controlRate -= uncontrolRate;
			controlRate = Utils.Clamp(controlRate, 0, 100);
		}
		public override void SaveData(TagCompound tag) {
			tag["controlRate"] = controlRate;
			tag["hasBeenControlled"] = hasBeenControlled;
			tag["yuanRate"] = yuanRate;
		}
		public override void LoadData(TagCompound tag) {
			yuanRate = tag.GetFloat("yuanRate");
			controlRate = tag.GetFloat("controlRate");
			hasBeenControlled = tag.GetBool("hasBeenControlled");
		}
	}
}
