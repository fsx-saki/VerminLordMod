using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.GlobalNPCs
{
	/// <summary>
	/// 全局 NPC 掉落处理器（简化版）
	/// 
	/// 职责：仅处理非模组的原版附加掉落。
	/// 模组掉落（蛊虫、元石等）现在通过 CorpseBag 系统掉落，
	/// 由 GlobalNPCCorpseHandler 在 NPC 死亡时生成。
	/// </summary>
	public class GlobalNPCLoot : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			// 城镇 NPC 掉落血肉块（不影响 CorpseBag）
			if (npc.townNPC)
				npcLoot.Add(ItemDropRule.Common(ItemID.FleshBlock, 1, 1, 7));
		}
	}
}
