using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Debuff;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	class NineLeavesLivingGrass : WoodWeapon
	{
		protected override int qiCost => 45;
		protected override int _useTime => 5;
		protected override int _guLevel => 2;
		private int times = 0;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			Item.value = 50000;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2)
				return false;
			QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
			//Main.NewText("secc");
			if (player.HasBuff<NineLeavesCDbuff>()) {
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), 5, 0);
				return false;
			}
			if (times < 9) {
				qiPlayer.qiCurrent -= qiCost;
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ModContent.ItemType<LivingLeaf>());
				times++;
			}
			else {
				times = 0;
				player.AddBuff(ModContent.BuffType<NineLeavesCDbuff>(),72000);
			}
			return true;
		}


		public override void SaveData(TagCompound tag) {
			tag["times"] = times;
			tag["controlRate"] = controlRate;
			tag["hasBeenControlled"] = hasBeenControlled;
		}

		public override void LoadData(TagCompound tag) {
			times = tag.GetInt("times");
			controlRate = tag.GetFloat("controlRate");
			hasBeenControlled = tag.GetBool("hasBeenControlled");
		}


	}
}
