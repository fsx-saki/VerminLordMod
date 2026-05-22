using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.Town;

namespace VerminLordMod.Common.Systems
{
	class TravelMerchantSystem:ModSystem
	{
		public override void PreUpdateNPCs() {
			JiasTravelingMerchant.UpdateTravelingMerchant();
		}
	}
}
