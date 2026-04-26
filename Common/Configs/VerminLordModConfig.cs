using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace VerminLordMod.Common.Configs
{
	public class VerminLordModConfig : ModConfig
	{
		// ConfigScope.ClientSide should be used for client side, usually visual or audio tweaks.
		// ConfigScope.ServerSide should be used for basically everything else, including disabling items or changing NPC behaviors
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// The things in brackets are known as "Attributes".

		[Header($"\u8bbe\u7f6e")] // Headers are like titles in a config. You only need to declare a header on the item it should appear over, not every item in the category. 
		//[Label("$Some.Key")] // A label is the text displayed next to the option. This should usually be a short description of what it does. By default all ModConfig fields and properties have an automatic label translation key, but modders can specify a specific translation key.
		[Label("\u7b49\u7ea7\u9650\u5236")]
		[Tooltip("\u9650\u5236\u6bcf\u8f6c\u86ca\u5e08\u53ef\u4ee5\u83b7\u53d6\u7684\u989d\u5916\u8840\u91cf\u3001\u529b\u91cf\u3001\u9632\u5fa1\u52a0\u6210\uff0c\u63d0\u5347\u5e73\u8861\u6027")]
		//[Tooltip("$Some.Key")] // A tooltip is a description showed when you hover your mouse over the option. It can be used as a more in-depth explanation of the option. Like with Label, a specific key can be provided.
		[DefaultValue(true)] // This sets the configs default value.
		//[ReloadRequired] // Marking it with [ReloadRequired] makes tModLoader force a mod reload if the option is changed. It should be used for things like item toggles, which only take effect during mod loading
		public bool LimitSth; // To see the implementation of this option, see ExampleWings.cs
	}
}

