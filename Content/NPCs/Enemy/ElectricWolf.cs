using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Accessories.Three;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.NPCs.Enemy
{
	//The ExampleZombieThief is essentially the same as a regular Zombie, but it steals ExampleItems and keep them until it is killed, being saved with the world if it has enough of them.
	//ExampleZombieThief与普通僵尸基本相同，但它会窃取ExampleItem并保存它们，直到被杀死，如果它有足够的物品，它会与世界一起保存。
	public class ElectricWolf : ModNPC
	{

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				// Influences how the NPC looks in the Bestiary
				//影响NPC在Bestiary中的外观
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				//在动物模型中绘制NPC，就像它在x方向上行走+1块瓷砖一样
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
			Main.npcFrameCount[Type] = 13;
		}

		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 14;
			NPC.defense = 20;
			NPC.lifeMax = 100;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Unicorn; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic
			//战斗机AI，选择与我们想要模仿的NPCID匹配的aiStyle很重要

			AIType = NPCID.Wolf;
			AnimationType = NPCID.Wolf; 
			Banner = Item.NPCtoBanner(NPCID.Wolf);
			BannerItem = Item.BannerToItem(Banner);
			
			
			//SpawnModBiomes = new int[] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("电狼"),
			});
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

		}

		//public override void SendExtraAI(BinaryWriter writer) {
		//	writer.Write(StolenItems);
		//}

		//public override void ReceiveExtraAI(BinaryReader reader) {
		//	StolenItems = reader.ReadInt32();
		//}

		public override void OnKill() {
			if (WolfSystem.isWolfWave) {
				//Main.NewText($"寄");
				WolfSystem.WolfWaveRate += 1;
			}
		}
		//	// Drop all the stolen items when the NPC dies
		//	while (StolenItems > 0) {
		//		// Loop until all items are dropped, to avoid dropping more than maxStack items
		//		int droppedAmount = Math.Min(ModContent.GetInstance<ExampleItem>().Item.maxStack, StolenItems);
		//		StolenItems -= droppedAmount;
		//		Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<ExampleItem>(), droppedAmount, true);
		//	}
		//}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// Can only spawn in the ExampleSurfaceBiome and if there are no other ExampleZombieThiefs
			if (WolfSystem.isWolfWave && Math.Abs(spawnInfo.SpawnTileX - spawnInfo.PlayerFloorX) < 4000) {
				//Main.NewText($"生成狼{NPC}");
				return 1f;
			}
			return SpawnCondition.OverworldDaySlime.Chance * 0.02f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// 这是我们添加物品掉落规则的地方，这里是一个简单的例子:
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderBallGu>(), 600));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderWings>(), 2000));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 200));
		}
		//public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
		//	target.AddBuff(BuffID.)
		//}

	}
}