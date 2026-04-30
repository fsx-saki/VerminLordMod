using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	// ExampleDrill closely mimics Titanium Drill, except where noted.
	// Of note, this example showcases Item.tileBoost and teaches the basic concepts of a held projectile.
	class WaterDrillGu : WaterWeapon
	{
		public override void SetStaticDefaults() {
			// As mentioned in the documentation, IsDrill and IsChainsaw automatically reduce useTime and useAnimation to 60% of what is set in SetDefaults and decrease tileBoost by 1, but only for vanilla items.
			// We set it here despite it doing nothing because it is likely to be used by other mods to provide special effects to drill or chainsaw items globally.
			ItemID.Sets.IsDrill[Type] = true;
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
		}
		public new static LocalizedText UsesXQiText { get; private set; }
		public new static LocalizedText ControlRate { get; private set; }
		public new static LocalizedText GuLevel { get; private set; }
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
		}
		protected override int _guLevel => 2;
		public override void SetDefaults() {
			Item.damage = 47;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>(); // ignores melee speed bonuses. There's no need for drill animations to play faster, nor drills to dig faster with melee speed.
			Item.width = 20;
			Item.height = 12;
			// IsDrill/IsChainsaw effects must be applied manually, so 60% or 0.6 times the time of the corresponding pickaxe. In this case, 60% of 7 is 4 and 60% of 25 is 15.
			// If you decide to copy values from vanilla drills or chainsaws, you should multiply each one by 0.6 to get the expected behavior.
			Item.useTime = 4;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0.5f;
			Item.value = Item.buyPrice(gold: 12, silver: 60);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item23;
			Item.shoot = ModContent.ProjectileType<WaterDrillProj>(); // Create the drill projectile
			Item.shootSpeed = 32f; // Adjusts how far away from the player to hold the projectile
			Item.noMelee = true; // Turns off damage from the item itself, as we have a projectile
			Item.noUseGraphic = false; // Stops the item from drawing in your hands, for the aforementioned reason
			Item.channel = true; // Important as the projectile checks if the player channels

			// tileBoost changes the range of tiles that the item can reach.
			// To match Titanium Drill, we should set this to -1, but we'll set it to 10 blocks of extra range for the sake of an example.
			Item.tileBoost = 10;

			Item.pick = 190; // How strong the drill is, see https://terraria.wiki.gg/wiki/Pickaxe_power for a list of common values
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			return true;
		}
	}
}
