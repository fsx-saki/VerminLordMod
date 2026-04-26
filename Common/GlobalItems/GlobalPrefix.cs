using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using VerminLordMod.Content.Items;
using VerminLordMod.Content.Items.Accessories;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Prefixes;

namespace VerminLordMod.Common.GlobalItems
{
	enum GuClass {
		Weapon,
		Accessory
	}
	class GlobalPrefix:GlobalItem
	{
		public override int ChoosePrefix(Item item, UnifiedRandom rand) {

			
			if (item.ModItem is IWeaponCanReforge) {
				//Main.NewText("这是一只武器蛊");
				return ChooseGuPrefixRand(GuClass.Weapon);
			}
			
			
			if (item.ModItem is IAccCanReforge) {
				//Main.NewText("这是一只饰品蛊");
				return ChooseGuPrefixRand(GuClass.Accessory);
			}
			
			//Main.NewText("这不是一只蛊");
			return base.ChoosePrefix(item, rand);
		}
		
		private int ChooseGuPrefixRand(GuClass guClass) {
			Random guRandom = new Random();
			List<int> prefixListW = new List<int>() {
				ModContent.PrefixType<ExtroversionPrefix>(),
				ModContent.PrefixType<AutismPrefix>(),
				ModContent.PrefixType<MildPrefix>(),
				ModContent.PrefixType<ShyPrefix>(),
				ModContent.PrefixType<ExtremePrefix>(),
				ModContent.PrefixType<ActivePrefix>(),
				ModContent.PrefixType<IntrovertPrefix>(),
				ModContent.PrefixType<DyingPrefix>(),
				
			};
			List<int> prefixListA = new List<int>() {
				ModContent.PrefixType<CrustaceaPrefix>(),
				ModContent.PrefixType<ColeopteraPrefix>(),
				ModContent.PrefixType<CurlingUpPrefix>(),
				ModContent.PrefixType<StretchPrefix>(),
				ModContent.PrefixType<SharpClawPrefix>(),
				ModContent.PrefixType<SharpTeethPrefix>(),
			};
			if (guClass == GuClass.Weapon)
				return prefixListW[guRandom.Next(0, prefixListW.Count)];
			else
				return prefixListA[guRandom.Next(0, prefixListA.Count)];
		}
		//public static List<int> GusCanHavePrefixW = new List<int>() {
		//	//攻击蛊虫
		//	ModContent.ItemType<Moonlight>(),
		//	ModContent.ItemType<MoonlightPro>(),
		//	ModContent.ItemType<RiverStream>(),
		//	ModContent.ItemType<BoneSpearGu>(),
		//	//ModContent.ItemType<Minilight>(),

		//};
		//public static List<int> GusCanHavePrefixA = new List<int>() {
		//	//饰品蛊虫
		//	ModContent.ItemType<BearPower>(),
		//	ModContent.ItemType<StoneSkin>(),
		//	ModContent.ItemType<IronSkin>(),
		//	ModContent.ItemType<CopperSkin>(),
		//	ModContent.ItemType<JadeSkin>(),
		//};

		///<summary>
		///]C#获取一个类在其所在的程序集中的所有子类
		/// </summary>
		//public static List<Type> GetSubclassNames(Type parentType) { 
		//	var subTypeList = new List<Type>();
		//	var assembly = parentType.Assembly;//获取当前父类所在的程序集、
		//	var assemblyAllTypes = assembly.GetTypes();//获取该程序集中的所有类型
		//	foreach(var itemType in assemblyAllTypes)//遍历所有类型进行查找
		//	{
		//		var baseType = itemType.BaseType;//获取元素类型的基类var(baseType != null)//如果有基类
		//		if(baseType.Name == parentType.Name)//如果基类就是给定的父类
		//		{
		//			subTypeList.Add(baseType);
		//		}
		//	}
		//	return subTypeList.Select(item => item).ToList();//获取所有子类类型的名称
		//}
	}

	
}
