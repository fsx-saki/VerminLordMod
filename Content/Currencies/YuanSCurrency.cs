using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace VerminLordMod.Content.Currencies
{
	public class YuanSCurrency : CustomCurrencySingleCoin
	{
		public YuanSCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap) {
			this.CurrencyTextKey = CurrencyTextKey;
			CurrencyTextColor = Color.LightBlue;
		}
	}
}