using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
	public class HeZeBuff : ModBuff
	{
		private static readonly Dictionary<int, int> _stacks = new();

		public static int GetStacks(int playerWhoAmI)
		{
			return _stacks.TryGetValue(playerWhoAmI, out int s) ? s : 0;
		}

		public static void SetStacks(int playerWhoAmI, int count)
		{
			_stacks[playerWhoAmI] = count;
		}

		public static void AddStack(int playerWhoAmI)
		{
			if (_stacks.ContainsKey(playerWhoAmI))
				_stacks[playerWhoAmI]++;
			else
				_stacks[playerWhoAmI] = 1;
		}

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			Main.lightPet[Type] = false;
			Main.buffNoTimeDisplay[Type] = false;
			BuffID.Sets.LongerExpertDebuff[Type] = false;
			Main.pvpBuff[Type] = false;
			Main.persistentBuff[Type] = false;
			Main.vanityPet[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex) {
			int stacks = GetStacks(player.whoAmI);
			if (stacks <= 0) stacks = 1;

			player.GetDamage(ModContent.GetInstance<InsectDamageClass>()) += 1f * stacks;

			for (int i = 0; i < stacks; i++)
			{
				var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Water);
				d.velocity *= 0.3f;
				d.noGravity = true;
				d.scale = 1.2f;
			}

			if (player.buffTime[buffIndex] <= 2)
			{
				Item heldItem = player.HeldItem;
				if (heldItem != null && !heldItem.IsAir && heldItem.ModItem is GuWeaponItem guWeapon)
				{
					guWeapon.controlRate -= 30 * stacks;
					if (guWeapon.controlRate < 0)
						guWeapon.controlRate = 0;
					if (guWeapon.controlRate < 100)
						guWeapon.hasBeenControlled = false;

					Text.ShowTextRed(player, $"涸泽蛊反噬！炼化度下降{30 * stacks}点");
				}
				_stacks.Remove(player.whoAmI);
			}
		}
	}
}
