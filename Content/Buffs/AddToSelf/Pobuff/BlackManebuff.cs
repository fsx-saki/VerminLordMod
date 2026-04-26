using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
	public class BlackManebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			// 因为buff严格意义上不是一个TR里面自定义的数据类型，所以没有像buff.XXXX这样的设置属性方式了
			// 我们需要用另外一种方式设置属性
			// 这个属性决定buff在游戏退出再进来后会不会仍然持续，true就是不会，false就是会
			Main.buffNoSave[Type] = false;
			// 用来判定这个buff算不算一个debuff，如果设置为true会得到TR里对于debuff的限制，比如无法取消
			Main.debuff[Type] = false;
			// 决定这个buff能不能被被护士治疗给干掉，true是不可以，false则可以取消
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
			// 决定这个buff是不是照明宠物的buff，以后讲宠物和召唤物的时候会用到的，现在先设为false
			Main.lightPet[Type] = false;
			// 决定这个buff会不会显示持续时间，false就是会显示，true就是不会显示，一般宠物buff都不会显示
			Main.buffNoTimeDisplay[Type] = false;
			// 决定这个buff在专家模式会不会持续时间加长，false是不会，true是会
			// 这个持续时间，专家翻倍，大师三倍
			BuffID.Sets.LongerExpertDebuff[Type] = false;
			// 如果这个属性为true，pvp的时候就可以给对手加上这个debuff/buff
			Main.pvpBuff[Type] = false;

			// 死亡后是否不清除buff，true为不清除，false清除，默认清除
			Main.persistentBuff[Type] = false;
			// 决定这个buff是不是一个装饰性宠物，用来判定的，比如消除buff的时候不会消除它
			Main.vanityPet[Type] = false;
		}


		// 这里给的第二个buffIndex是buff在玩家身上的索引
		public override void Update(Player player, ref int buffIndex) {
			// 每帧1/3的概率让玩家散发绿色粒子（其实是诅咒焰那个粒子）
			if (Main.rand.NextBool(3)) {
				var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GreenFairy);
				d.velocity *= 0.5f;
			}
			player.GetDamage<GenericDamageClass>() += 0.15f;
			player.statDefense += 20;
			//// 把玩家的所有生命回复清除
			//if (player.lifeRegen > 0) {
			//	player.lifeRegen = 0;
			//}
			//player.lifeRegenTime = 0;
			//// 让玩家的减血速率随着时间而减少
			//// player.buffTime[buffIndex]就是这个buff的剩余时间
			//player.lifeRegen -= player.buffTime[buffIndex];
			//// cool，如果这是一个一分钟的buff，那么获得buff的第一帧你的生命回复速率就是-3600了
			//// 这意味着下一帧你就寄了
		}
		//public override void Update(NPC npc, ref int buffIndex) {
		//	// npc也有liferegen，你们可以自己试着写
		//}
	}
}
