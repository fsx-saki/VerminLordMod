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
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Placeable.Banners;

namespace VerminLordMod.Content.NPCs.Enemy
{
	// 刀翅血蝠蛊 - 三转群居蛊虫，肉搏型，以血为食
	// 飞行敌怪，成群出现，攻击附带流血效果
	public class BladeBloodBatGu : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.CaveBat];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
			Main.npcFrameCount[Type] = 4;
		}

		public override void SetDefaults() {
			NPC.width = 28;
			NPC.height = 24;
			NPC.damage = 22;
			NPC.defense = 8;
			NPC.lifeMax = 80;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath4;
			NPC.value = 50f;
			NPC.knockBackResist = 0.6f;
			NPC.aiStyle = NPCAIStyleID.Bat;
			AIType = NPCID.CaveBat;
			AnimationType = NPCID.CaveBat;
			NPC.noGravity = true;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<BladeBloodBatGuBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				new FlavorTextBestiaryInfoElement("刀翅血蝠蛊"),
			});
		}

		public override void AI() {
			base.AI();
			// 夜晚或地下时更活跃
			if (!Main.dayTime || Main.player[Main.myPlayer].ZoneRockLayerHeight) {
				NPC.velocity *= 1.02f; // 略微加速
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			// 刀翅攻击造成流血效果
			if (Main.rand.NextBool(3)) {
				target.AddBuff(BuffID.Bleeding, 180); // 3秒流血
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// 地下和夜晚生成概率较高
			if (spawnInfo.Player.ZoneRockLayerHeight) {
				return SpawnCondition.Underground.Chance * 0.1f;
			}
			if (!Main.dayTime) {
				return SpawnCondition.OverworldNight.Chance * 0.05f;
			}
			return SpawnCondition.OverworldDay.Chance * 0.01f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 150));
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 1, 3));
		}
	}
}
