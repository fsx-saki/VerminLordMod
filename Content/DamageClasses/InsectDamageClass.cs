using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.DamageClasses
{
	class InsectDamageClass : DamageClass
	{
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == DamageClass.Generic)
				return StatInheritanceData.Full;//吃全部的通用伤害加成

			if (damageClass == DamageClass.Magic)
				return new StatInheritanceData(
					damageInheritance: 1f,
					critChanceInheritance: -1f,
					attackSpeedInheritance: 0.4f,
					armorPenInheritance: 2.5f,
					knockbackInheritance: 0f
				);//一半的魔法伤害加成

			return StatInheritanceData.None;//不吃其他任何加成



		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			// 此方法允许你使你的伤害类型触发本该由其它伤害类型触发的效果 (如岩浆石只对近战伤害生效)
			// 不像上面的属性继承, 你不需要在此方法里写通用加成
			if (damageClass == DamageClass.Magic)
				return true;//触发魔法伤害效果

			return false;//不触发其他效果
		}


		public override void SetDefaultStats(Player player) {
			// 此方法让你设置此伤害类型的默认属性加成 (像原版的伤害默认有+4%暴击率)
			//player.GetCritChance<AlchemicalDamageClass>() += 0;
			//player.GetArmorPenetration<AlchemicalDamageClass>() += 0;
			// 你也可以在这里写伤害 (GetDamage), 击退 (GetKnockback), 和攻速 (GetAttackSpeed)
		}


		// 此属性决定此伤害类型是否使用标准的暴击计算公式
		// 请注意将其设为 false 会阻止描述中 "暴击率" 一行的显示
		// 并且即使你在 ShowStatTooltipLine 返回 true 也不行, 所以要小心!
		public override bool UseStandardCritCalcs => true;


		public override bool ShowStatTooltipLine(Player player, string lineName) {
			// 此方法允许你隐藏物品描述中特定伤害类型的数据显示
			// 四个可用的名称是 "Damage", "CritChance", "Speed", 和 "Knockback"
			if(lineName== "Speed"||lineName== "Knockback")return false;
			return true;
			// 注 意: 这个钩子将来会被移除, 并以一个更好的替代
		}
	}
}
