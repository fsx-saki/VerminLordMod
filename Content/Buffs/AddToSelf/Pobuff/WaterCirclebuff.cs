using VerminLordMod.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static VerminLordMod.Content.Items.Weapons.One.RiverStream;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
	class WaterCirclebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			// 不显示buff时间
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			Bais modPlayer = player.GetModPlayer<Bais>();
			// 如果当前有属于玩家的僚机的弹幕
			if (player.ownedProjectileCounts[ModContent.ProjectileType<WaterCircle>()] > 0) {
				modPlayer.WaterCircle = true;
			}
			// 如果玩家取消了这个召唤物就让buff消失
			if (!modPlayer.WaterCircle) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
			else {
				// 无限buff时间
				player.buffTime[buffIndex] = 9999;
			}
			player.statDefense += 5;
			player.lifeRegen += 5;
		}

	}
}
