using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Accessories.One;
using VerminLordMod.Content.Items.Accessories;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 二转月道蛊虫 — 隐鳞蛊（手持使用型）
	/// 使用后进入隐身状态，隐蔽身形。
	/// 由隐形石蛊 + 鳞甲蛊合成。
	/// </summary>
	class YinLinGu : MoonWeapon
	{
		protected override int qiCost => 10;
		protected override int _useTime => 30;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 6;

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.value = 1500;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.UseSound = SoundID.Item4;
			Item.scale = 1f;
			Item.shootSpeed = 0f;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = false;
			Item.damage = 0;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.shoot = ProjectileID.None;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return false;

			// 消耗真元
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			if (!qiResource.ConsumeQi(qiCost))
			{
				Text.ShowTextRed(player, "真元不足，无法催动隐鳞蛊！");
				return false;
			}

			// 境界压制
			var qiRealm = player.GetModPlayer<QiRealmPlayer>();
			if (_guLevel > qiRealm.GuLevel)
			{
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				player.Hurt(PlayerDeathReason.LegacyDefault(),
					(_guLevel - qiRealm.GuLevel) * player.statLifeMax2 / 20, 0);
			}

			// 获得隐身效果
			player.AddBuff(BuffID.Invisibility, 3600); // 60秒
			player.AddBuff(BuffID.ShadowDodge, 600);   // 10秒暗影闪避

			if (Main.myPlayer == player.whoAmI)
				Main.NewText("隐鳞蛊发动，身形隐匿！", new Color(180, 180, 220));

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<InvisibleStoneGu>(), 1)
				.AddIngredient(ModContent.ItemType<Accessories.One.ScaleGu>(), 1)
				.AddIngredient(ModContent.GetInstance<YuanS>(), 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
