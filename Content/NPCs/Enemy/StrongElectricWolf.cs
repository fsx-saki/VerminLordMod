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
using VerminLordMod.Content.Items.Placeable.Banners;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.NPCs.Enemy
{
	// 豪电狼 - 电狼中的百狼王，牛犊大小，比普通电狼更强
	public class StrongElectricWolf : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
			Main.npcFrameCount[Type] = 13;
		}

		public override void SetDefaults() {
			NPC.width = 24;
			NPC.height = 48;
			NPC.damage = 28;
			NPC.defense = 30;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 120f;
			NPC.knockBackResist = 0.3f;
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			AIType = NPCID.Wolf;
			AnimationType = NPCID.Wolf;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<StrongElectricWolfBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("豪电狼"),
			});
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}
		}

		public override void OnKill() {
			if (WolfSystem.isWolfWave) {
				WolfSystem.WolfWaveRate += 2;
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (WolfSystem.isWolfWave && Math.Abs(spawnInfo.SpawnTileX - spawnInfo.PlayerFloorX) < 4000) {
				return 0.3f;
			}
			return SpawnCondition.OverworldDaySlime.Chance * 0.005f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderBallGu>(), 300));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderWings>(), 1000));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 100));
		}
	}
}
