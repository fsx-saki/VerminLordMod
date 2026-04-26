using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Accessories.One;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	class MoonLuminousTextile : MoonWeapon//必要继承moditem
	{
		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 30;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<MoonlightProj>();
			Item.shootSpeed = 10f;
		}
		public override bool? UseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();
			player.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
			qiPlayer.qiCurrent -= qiCost;
			//player.GetDamage<DamageClass.Melee>()+=0.01f
			float maxDis = 500;
			foreach (var aplayer in Main.ActivePlayers) {
				float dis = Vector2.Distance(player.Center, aplayer.Center);
				if (dis < +maxDis) {
					aplayer.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
				}
			}
			foreach (var npc in Main.npc.Where(n => n.active && n.friendly)) {
				float dis = Vector2.Distance(player.Center, npc.Center);
				if (dis < maxDis) {
					npc.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
				}
			}
			return true;
		}
		//public new bool? UseItem(Player player) {
		//	var qiPlayer = player.GetModPlayer<QiPlayer>();

		//	qiPlayer.qiCurrent -= qiCost;
		//	player.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
		//	Main.NewText("addbuff");

		//	float maxDis = 500;
		//	foreach (var aplayer in Main.ActivePlayers) {
		//		float dis = Vector2.Distance(player.Center, aplayer.Center);
		//		if (dis < +maxDis) {
		//			aplayer.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
		//		}
		//	}
		//	foreach (var npc in Main.npc.Where(n => n.active && n.friendly)) {
		//		float dis = Vector2.Distance(player.Center, npc.Center);
		//		if (dis < maxDis) {
		//			npc.AddBuff(ModContent.BuffType<MoonLuminousTextilebuff>(), 360);
		//		}
		//	}
		//	return true;
		//}
		public new bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;

			var p = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<MoonlightProj>(), damage, knockback, player.whoAmI);


			return false;

		}


		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<JadeSkin>(), 1)
			.AddIngredient(ModContent.GetInstance<Moonlight>(), 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 10)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}
	}
}
