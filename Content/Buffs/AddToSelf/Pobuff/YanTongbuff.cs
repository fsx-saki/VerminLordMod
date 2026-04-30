using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
	class YanTongbuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
		}
	}
}
