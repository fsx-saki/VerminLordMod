using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	/// <summary>
	/// 三转火道蛊虫 — 焚身蛊
	/// 使用后全身被火焰包裹，持续灼烧自身生命，
	/// 所有攻击额外发射追踪火弹。
	/// </summary>
	public class FenShenGu : FireWeapon
	{
		protected override int qiCost => 20;
		protected override int _useTime => 18;
		protected override int _guLevel => 3;
		protected override int controlQiCost => 12;
		protected override float unitConntrolRate => 15;

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.damage = 0; // 不直接造成伤害
			Item.knockBack = 0;
			Item.crit = 0;
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
			Item.value = 5000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.UseSound = SoundID.Item4;
			Item.scale = 1f;
			Item.shoot = 0; // 不射出弹幕
			Item.shootSpeed = 0;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = false;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
				return false;

			player.AddBuff(ModContent.BuffType<FenShenBuff>(), 600); // 10秒
			return true;
		}
	}
}
