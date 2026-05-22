using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.GlobalNPCs
{
	class GlobalWolf:GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			if (WolfSystem.isWolfWave) {
				spawnRate=27;
				maxSpawns = 200;
			}
		}
	}
}
