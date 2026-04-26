using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common;

namespace VerminLordMod.Content.Prefixes
{
	class ExtremePrefix:ModPrefix
	{
		// 声明一个自定义的 *virtual* 属性，所以另一种前缀，ExampleDerivedPrefix，可以重写它自己的Power（效力）
		public virtual int Power => 8;

		// 修改该前缀的类别，默认为 PrefixCategory.Custom。影响哪些物品可以获得此前缀
		public override PrefixCategory Category => PrefixCategory.Custom;
		

		// 原版前缀的权重和更多信息参见tML文档
		// 当多个前缀有相似的作用时，可以与 switch 或 case 使用以为不同的前缀提供不同的概率
		// 注意：即使权重是0f，也有可能被抽到。排除前缀请参见 CanRoll（就在下面）
		// 注意：如果前缀的类别是 PrefixCategory.Custom，改用 ModItem.ChoosePrefix
		public override float RollChance(Item item) {
			return 10f;
		}

		// 决定该前缀是否能被抽到
		// 设为 true 就是能，false 就是不能（废话）
		public override bool CanRoll(Item item) {
			return true;
		}

		// 用这个方法来修改拥有此前缀的物品的属性：
		// damageMult 伤害乘数，knockbackMult 击退乘数，useTimeMult 使用时间乘数，scaleMult 大小乘数，shootSpeedMult 弹速（射速，射出的速度）乘数，manaMult 魔力消耗乘数，critBonus 暴击增量
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult *= 1f + 0.05f * Power;
			knockbackMult *= 1f + 0.02f * Power;
			useTimeMult *= 1f - 0.02f * Power;
			scaleMult *= 1f;
			shootSpeedMult *= 1f + 0.02f * Power;
			critBonus *= 1;
		}

		// 修改获得此前缀的物品的价格，valueMult 为价格乘数
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1f + 0.2f * Power;
		}

		// 这个方法用来修改获得此前缀的物品的其它属性
		public override void Apply(Item item) {

			// 开始你的表演
			//IGu gu = item as IGu;
			//gu.ChangeQiCost(-0.5f);
			
		}
	}
}
