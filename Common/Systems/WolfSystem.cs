using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Content;
using VerminLordMod.Content.NPCs.Boss;
using VerminLordMod.Content.NPCs.Town;

namespace VerminLordMod.Common.Systems
{
	public class WolfSystem:ModSystem
	{
		public static bool isWolfWave = false;
		public static float WolfWaveRate = 0f;
		public static bool hasSummonesKing = false;
		public override void PreUpdateNPCs() {
			Player player = Main.LocalPlayer;
			QiPlayer qiPlayer=player.GetModPlayer<QiPlayer>();
			if (player.statLifeMax >= 150 && qiPlayer.qiEnabled) {
				if (Main.time == 0d) {
					if (Main.dayTime) {
						//Main.NewText($"尝试生成狼潮{Main.time}");
						if (isWolfWave == false && Randommer.Roll(5)) {
							Main.NewText($"狼潮来了！");
							WolfWaveRate = 0f;
							hasSummonesKing = false;
							isWolfWave = true;
						}
					}
					//else {
					//	if(isWolfWave)
					//	Main.NewText($"生成狼潮结束");
					//	isWolfWave = false;
					//	WolfWaveRate = 0;
					//}
					
				}
			}

			if (WolfWaveRate >= 80f) {
				bool KingWolfIsThere = (NPC.FindFirstNPC(ModContent.NPCType<ElectricWolfKing>()) != -1);
				//var entitySource = NPC.GetSource_FromAI();
				
				if (Randommer.Roll(100)&&!KingWolfIsThere&&!hasSummonesKing) {
					NPC.NewNPCDirect(Terraria.Entity.GetSource_NaturalSpawn(), Main.LocalPlayer.position + new Vector2(0, -200), ModContent.NPCType<ElectricWolfKing>());
					hasSummonesKing = true;
				}
			}

			if (WolfWaveRate >= 100f) {
				isWolfWave = false;
				Main.NewText($"狼潮结束了...");
				WolfWaveRate = 0;
			}
		}
		public override void SaveWorldData(TagCompound tag) {
			tag["isWolfWave"] = isWolfWave;
			tag["WolfWaveRate"] = WolfWaveRate;
		}
		public override void LoadWorldData(TagCompound tag) {
			isWolfWave = tag.GetBool("isWolfWave");
			WolfWaveRate = tag.GetFloat("WolfWaveRate");
		}
	}

}
