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
	// 军团蚁 - 蛊仙创造的奴道蚁群，地面群居，集成蚁河
	// 地面爬行敌怪，成群出现，攻击附带中毒效果
	public class LegionAnt : ModNPC
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
			NPC.width = 26;
			NPC.height = 20;
			NPC.damage = 18;
			NPC.defense = 12;
			NPC.lifeMax = 60;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 30f;
			NPC.knockBackResist = 0.4f;
			NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.Zombie;
			AnimationType = NPCID.Zombie;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<LegionAntBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("军团蚁"),
			});
		}

		public override void AI() {
			base.AI();
			// 军团蚁成群时攻击更强
			int nearbyAnts = 0;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC other = Main.npc[i];
				if (other.active && other.type == Type && other.whoAmI != NPC.whoAmI) {
					float dist = Vector2.Distance(NPC.Center, other.Center);
					if (dist < 200f) {
						nearbyAnts++;
					}
				}
			}
			// 每多一只附近的军团蚁增加5%伤害
			if (nearbyAnts > 0) {
				NPC.damage = (int)(NPC.defDamage * (1f + nearbyAnts * 0.05f));
			} else {
				NPC.damage = NPC.defDamage;
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			// 蚁酸造成中毒效果
			if (Main.rand.NextBool(4)) {
				target.AddBuff(BuffID.Poisoned, 120); // 2秒中毒
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// 地表和地下都有生成
			if (spawnInfo.Player.ZoneOverworldHeight) {
				return SpawnCondition.OverworldDay.Chance * 0.08f;
			}
			if (spawnInfo.Player.ZoneRockLayerHeight) {
				return SpawnCondition.Underground.Chance * 0.05f;
			}
			return 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 200));
			npcLoot.Add(ItemDropRule.Common(ItemID.AntlionMandible, 50));
			npcLoot.Add(ItemDropRule.Common(ItemID.Stinger, 30));
		}
	}
}
