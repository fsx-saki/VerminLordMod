using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using VerminLordMod.Content.Items.Accessories.Four;
using VerminLordMod.Content.Items.Accessories.One;
using VerminLordMod.Content.Items.Accessories.Three;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Placeable;
using VerminLordMod.Content.Items.Weapons.Five;
using VerminLordMod.Content.Items.Weapons.Four;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Content.Projectiles.Minions;

namespace VerminLordMod.Content.NPCs.Town
{
	[AutoloadHead]
	public class JiasTravelingMerchant : ModNPC
	{
		// Time of day for traveler to leave (6PM)
		public const double despawnTime = 48600.0;

		// the time of day the traveler will spawn (double.MaxValue for no spawn). Saved and loaded with the world in TravelingMerchantSystem
		public static double spawnTime = double.MaxValue;

		// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
		public readonly static List<Item> shopItems = new();

		// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
		public static JiasTravelingMerchantShop Shop;

		//private static int ShimmerHeadIndex;
		private static Profiles.StackedNPCProfile NPCProfile;

		//public int DaysToArrive = 3;
		public override bool PreAI() {
			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)&&Randommer.Roll(33)) // If it's past the despawn time and the NPC isn't onscreen
			{
				// Here we despawn the NPC and send a message stating that the NPC has despawned
				// LegacyMisc.35 is {0) has departed!
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText($"\u8d3e\u5bb6\u5546\u961f\u4ee3\u8868 {NPC.FullName} \u5df2\u79bb\u5f00\uff01", 50, 125, 255);
				else ChatHelper.BroadcastChatMessage(NetworkText.FromKey($"\u8d3e\u5bb6\u5546\u961f\u4ee3\u8868 {NPC.FullName} \u5df2\u79bb\u5f00\uff01", NPC.GetFullNetName()), new Color(50, 125, 255));
				NPC.active = false;
				NPC.netSkip = -1;
				NPC.life = 0;
				
				return false;
			}

			return true;
		}

		public override void AddShops() {
			Shop = new JiasTravelingMerchantShop(NPC.type);

			// Always bring an ExampleItem
			Shop.Add(new Item(ModContent.ItemType<WanShi>()) {
				shopCustomPrice = 10,
				shopSpecialCurrency = VerminLordMod.YuanSId
			});
			Shop.Add(new Item(ModContent.ItemType<Hopeness>()) {
				shopCustomPrice = 10000,
				//shopSpecialCurrency = VerminLordMod.YuanSId
			});
			Shop.Add(new Item(ModContent.ItemType<TianyuanBaolianGu>()) {
				shopCustomPrice = 1000,
				shopSpecialCurrency = VerminLordMod.YuanSId
			});
			// Bring 2 Tools
			Shop.AddPool("Tools", slots: 2)
				.Add(new Item(ModContent.ItemType<CopperSkin>()) {
					shopCustomPrice = 100,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<InvisibleStoneGu>()) {
					shopCustomPrice = 75,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<Minilight>()) {
					shopCustomPrice = 75,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<IronSkin>()) {
					shopCustomPrice = 125,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<StoneSkin>()) {
					shopCustomPrice = 75,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<JadeSkin>()) {
					shopCustomPrice = 275,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<ScaleGu>()) {
					shopCustomPrice = 95,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<TengYunWings>()) {
					shopCustomPrice = 300,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<EagleYangGu>()) {
					shopCustomPrice = 1000,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<EagleWingGu>()) {
					shopCustomPrice = 300,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<WhiteJadeGu>()) {
					shopCustomPrice = 230,
					shopSpecialCurrency = VerminLordMod.YuanSId
				});
			//	.Add<CopperSkin>()
			//	.Add<ExampleHamaxe>()
			//	.Add<ExampleFishingRod>()
			//	.Add<ExampleHookItem>()
			//	.Add<ExampleBugNet>()
			//	.Add<ExamplePickaxe>();

			//// Bring 4 Weapons
			Shop.AddPool("Weapons", slots: 7)
				.Add(new Item(ModContent.ItemType<TraceStoneGu>()) {
					shopCustomPrice = 100,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<Cyclone>()) {
					shopCustomPrice = 100,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<WineBagFlowerGu>()) {
					shopCustomPrice = 20,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<global::VerminLordMod.Content.Items.Consumables.WineBug>()) {
					shopCustomPrice = 520,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<RiceBagGrassGu>()) {
					shopCustomPrice = 20,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<GrassPuppet>()) {
					shopCustomPrice = 30,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<BlackHair>()) {
					shopCustomPrice = 250,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})//.Add(new Item(ModContent.ItemType<DarkArrowGu>()) {
					//shopCustomPrice = 1000000,
					//shopSpecialCurrency = VerminLordMod.YuanSId
				//})
				.Add(new Item(ModContent.ItemType<NineLeavesLivingGrass>()) {
					shopCustomPrice = 1500,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<BreezeWheelGu>()) {
					shopCustomPrice = 170,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<LivingLeaf>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<WaterArrowGu>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<SpiritSalivaGu>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<ABitGu>()) {
					shopCustomPrice = 150,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<StarDartGu>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<StarArrowGu>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<MeteorGu>()) {
					shopCustomPrice = 200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<DogControlGuItem>()) {
					shopCustomPrice = 170,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				//2

				.Add(new Item(ModContent.ItemType<IceAwlGu>()) {
					shopCustomPrice = 450,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<IceKnifeGu>()) {
					shopCustomPrice = 450,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<TwistedHeelGu>()) {
					shopCustomPrice = 450,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<AcidWaterGu>()) {
					shopCustomPrice = 450,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<BigStrengthGu>()) {
					shopCustomPrice = 240,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<GoldNeedleGu>()) {
					shopCustomPrice = 400,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				//3

				.Add(new Item(ModContent.ItemType<MoonHandKnife>()) {
					shopCustomPrice = 750,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<SoundbyteGu>()) {
					shopCustomPrice = 750,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<ShitGu>()) {
					shopCustomPrice = 450,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				//4
				.Add(new Item(ModContent.ItemType<BloodSkullGu>()) {
					 shopCustomPrice = 1850,
					 shopSpecialCurrency = VerminLordMod.YuanSId
				 }).Add(new Item(ModContent.ItemType<JinXiaGu>()) {
					 shopCustomPrice = 1850,
					 shopSpecialCurrency = VerminLordMod.YuanSId
				 }).Add(new Item(ModContent.ItemType<WolfRunGu>()) {
					 shopCustomPrice = 1850,
					 shopSpecialCurrency = VerminLordMod.YuanSId
				 }).Add(new Item(ModContent.ItemType<TianDiHongYinGu>()) {
					 shopCustomPrice = 1850,
					 shopSpecialCurrency = VerminLordMod.YuanSId
				 });
			Shop.AddPool("Consumable", slots: 5)
				.Add(new Item(ModContent.ItemType<StrengthLongicorn>()) {
					shopCustomPrice = 95,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<HuangLuoLongicorn>()) {
					shopCustomPrice = 95,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<WolfWaveCard>()) {
					shopCustomPrice = 95,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<TenLifeGu>()) {
					shopCustomPrice = 2025,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<HundredLifeGu>()) {
					shopCustomPrice = 50000,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})//.Add(new Item(ModContent.ItemType<ThousandLifeGu>()) {
				  //	shopCustomPrice = 1000000,
				  //	shopSpecialCurrency = VerminLordMod.YuanSId
				  //});
				.Add(new Item(ModContent.ItemType<OneMinion>()) {
					shopCustomPrice = 350,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<KsitigarbhaFlowerGu>()) {
					shopCustomPrice = 325,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<JinLiGu>()) {
					shopCustomPrice = 150,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<BronzeShari>()) {
					shopCustomPrice = 500,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<RedSteelShari>()) {
					shopCustomPrice = 1500,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<SliverShari>()) {
					shopCustomPrice = 5000,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<GoldShari>()) {
					shopCustomPrice = 10000,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<ShiJinLiGu>()) {
					shopCustomPrice = 1400,
					shopSpecialCurrency = VerminLordMod.YuanSId
				}).Add(new Item(ModContent.ItemType<JunLiGu>()) {
					shopCustomPrice = 3200,
					shopSpecialCurrency = VerminLordMod.YuanSId
				});


			Shop.AddPool("Furniture", slots: 1)
				.Add(new Item(ModContent.ItemType<NLMusicBox>()) {
					shopCustomPrice = 10000,
				}).Add(new Item(ModContent.ItemType<PoemAMusicBox>()) {
					shopCustomPrice = 10000,
				}).Add(new Item(ModContent.ItemType<MoonlightCreeper>()) {
					shopCustomPrice = 1000,
				});

			Shop.Register();
		}

		

		public static void UpdateTravelingMerchant() {
			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<JiasTravelingMerchant>()) != -1); // Find a Merchant if there's one spawned in the world
			//Main.NewText($"Jia's:{travelerIsThere}");
			//// Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
			//if (Main.dayTime && JiasTravelingMerchant.) {
			//	// insert code here to change the spawn chance based on other conditions (say, NPCs which have arrived, or milestones the player has passed)
			//	// You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

			//	// NPC won't spawn today if it stayed all night
			//	if (!travelerIsThere && Randommer.Roll(100)) { // 4 = 25% Chance
			//		// Here we can make it so the NPC doesn't spawn at the EXACT same time every time it does spawn
			//		spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
			//	}
			//	else {
			//		spawnTime = double.MaxValue; // no spawn today
			//	}
			//}

			// Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
			if (!travelerIsThere&&Main.dayTime&&Main.time==0&&Randommer.Roll(80)) {
				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<JiasTravelingMerchant>(), 1); // Spawning at the world spawn
				NPC traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;

				// Prevents the traveler from spawning again the same day
				//spawnTime = double.MaxValue;

				// Announce that the traveler has spawned in!
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText($"\u8d3e\u5bb6\u5546\u961f\u4ee3\u8868 {traveler.FullName} \u5df2\u5230\u8fbe\uff01", 50, 125, 255);
				else ChatHelper.BroadcastChatMessage(NetworkText.FromKey($"\u8d3e\u5bb6\u5546\u961f\u4ee3\u8868 {traveler.FullName} \u5df2\u5230\u8fbe\uff01", traveler.GetFullNetName()), new Color(50, 125, 255));
			}
		}

		//private static bool CanSpawnNow() {
		//	// can't spawn if any events are running
		//	if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
		//		return false;

		//	// can't spawn if the sundial is active
		//	//if (Main.IsFastForwardingTime())
		//	//	return false;

		//	// can spawn if daytime, and between the spawn and despawn times
		//	return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
		//}

		private static bool IsNpcOnscreen(Vector2 center) {
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.ActivePlayers) {
				// If any player is close enough to the traveling merchant, it will prevent the npc from despawning
				if (player.getRect().Intersects(npcScreenRect)) {
					return true;
				}
			}
			return false;
		}

		//public static double GetRandomSpawnTime(double minTime, double maxTime) {
		//	// A simple formula to get a random time between two chosen times
		//	return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
		//}

		public override void Load() {
			// Adds our Shimmer Head to the NPCHeadLoader.
			//ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25;
			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.AttackType[Type] = 2; // Swings a weapon. This NPC attacks in roughly the same manner as Stylist
			NPCID.Sets.AttackTime[Type] = 12;
			NPCID.Sets.AttackAverageChance[Type] = 1;
			NPCID.Sets.DangerDetectRange[Type] = 700;
			NPCID.Sets.HatOffsetY[Type] = 4;
			//NPCID.Sets.ShimmerTownTransform[Type] = true;
			NPCID.Sets.NoTownNPCHappiness[Type] = true; // Prevents the happiness button
			//NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleTravellingMerchantEmote>();

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 2f, // Draws the NPC in the bestiary as if its walking +2 tiles in the x direction
				Direction = -1 // -1 is left and 1 is right.
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party")
				//new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
			);
		}

		public override void SetDefaults() {
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.Stylist;
			TownNPCStayingHomeless = true;
		}

		public override void OnSpawn(IEntitySource source) {
			shopItems.Clear();
   			shopItems.AddRange(Shop.GenerateNewInventoryList());

			// In multi player, ensure the shop items are synced with clients (see TravelingMerchantSystem.cs)
			if (Main.netMode == NetmodeID.Server) {
				// We recommend modders avoid sending WorldData too often, or filling it with too much data, lest too much bandwidth be consumed sending redundant data repeatedly
				// Consider sending a custom packet instead of WorldData if you have a significant amount of data to synchronise
				NetMessage.SendData(MessageID.WorldData);
   			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface
			});
		}

		public override void HitEffect(NPC.HitInfo hit) {
			int num = NPC.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++) {
				//Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
			}

			// Create gore when the NPC is killed.
			//if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
			//	// Retrieve the gore types. This NPC has shimmer variants for head, arm, and leg gore. It also has a custom hat gore. (7 gores)
			//	// This NPC will spawn either the assigned party hat or a custom hat gore when not shimmered. When shimmered the top hat is part of the head and no hat gore is spawned.
			//	int hatGore = NPC.GetPartyHatGore();
			//	// If not wearing a party hat, and not shimmered, retrieve the custom hat gore 
			//	//if (hatGore == 0 && !NPC.IsShimmerVariant) {
			//	//	hatGore = Mod.Find<ModGore>($"{Name}_Gore_Hat").Type;
			//	//}
			//	string variant = "";
			//	//if (NPC.IsShimmerVariant) variant += "_Shimmer";
			//	int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
			//	int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
			//	int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

			//	// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
			//	if (hatGore > 0) {
			//		Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
			//	}
			//	Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore);
			//	Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
			//	Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
			//	Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			//	Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			//}
		}

		public override bool UsesPartyHat() {
			// ExampleTravelingMerchant likes to keep his hat on while shimmered.
			//if (NPC.IsShimmerVariant) {
			//	return false;
			//}
			return true;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			return false; // This should always be false, because we spawn in the Traveling Merchant manually
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return JiaNameList;
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			//int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			//if (partyGirl >= 0) {
			//	//chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			//}

			//chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue1"));
			//chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue2"));
			//chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue3"));

			//string hivePackDialogue = Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.HiveBackpackDialogue");
			//chat.Add(hivePackDialogue);
			chat.Add("\u6211\u4eec\u8d3e\u5bb6\u7ecf\u5546\uff0c\u7d20\u6765\u4ee5\u8bda\u4fe1\u4e3a\u672c\uff0c\u7ae5\u53df\u65e0\u6b3a\u3002");
			chat.Add("\u8d3e\u5bb6\u884c\u5546\uff0c\u5411\u6765\u4ee5\u8bda\u610f\u4e3a\u672c\u3002");
			string dialogueLine = chat; // chat is implicitly cast to a string.
			//if (hivePackDialogue.Equals(dialogueLine)) {
				// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
			//	Main.npcChatCornerItem = ItemID.HiveBackpack;
			//}

			return dialogueLine;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("LegacyInterface.28");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = Shop.Name; // Opens the shop
			}
		}

		public override void AI() { 
			NPC.homeless = true; // Make sure it stays homeless
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			//npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 15;
			randExtraCooldown = 8;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<GoldNeedleProj>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 0f;
			// SparklingBall is not affected by gravity, so gravityCorrection is left alone.
		}


		public static List<string> JiaNameList = new List<string>() {
			"\u8d3e\u6e05\u5999","\u8d3e\u4f9d\u767d","\u8d3e\u9189\u51ac","\u8d3e\u590f\u7487","\u8d3e\u666f\u5e73","\u8d3e\u4e00\u96ef","\u8d3e\u65b9\u65b9","\u8d3e\u5b64\u5170","\u8d3e\u7eee\u7389","\u8d3e\u5c0f\u6668","\u8d3e\u5982\u610f","\u8d3e\u9a9e\u5c27","\u8d3e\u4e4b\u5349","\u8d3e\u4e91","\u8d3e\u534e\u695a","\u8d3e\u7f8e\u5072","\u8d3e\u5f71","\u8d3e\u5a1f\u598d","\u8d3e\u6167\u4fca","\u8d3e\u6148","\u8d3e\u7b11\u7fe0","\u8d3e\u53ef\u4f73","\u8d3e\u5b50\u82b8","\u8d3e\u548c\u74a7","\u8d3e\u65b0\u7b60","\u8d3e\u68a6\u4e4b","\u8d3e\u7389\u8f69","\u8d3e\u541b\u6d69","\u8d3e\u601d\u6167","\u8d3e\u4ece\u73ca","\u8d3e\u4e66\u6843","\u8d3e\u78a7\u8431","\u8d3e\u5b50\u8431","\u8d3e\u8d6b","\u8d3e\u7ea2\u4e91","\u8d3e\u95ee\u8587","\u8d3e\u7eee\u5357","\u8d3e\u5ff5\u4e4b","\u8d3e\u521a\u6377","\u8d3e\u4f1f\u8bda","\u8d3e\u5143\u57fa","\u8d3e\u5411\u660e","\u8d3e\u5170\u829d","\u8d3e\u51ef\u98ce","\u8d3e\u51b0\u4e4b","\u8d3e\u6e05\u60a6","\u8d3e\u73fa","\u8d3e\u4fca\u4eba","\u8d3e\u7ecf\u7565","\u8d3e\u4e98","\u8d3e\u9633\u66e6","\u8d3e\u656c\u66e6","\u8d3e\u4f41\u7136","\u8d3e\u60a6\u7545","\u8d3e\u4e00\u7487","\u8d3e\u82b3\u82d3","\u8d3e\u971e\u59dd","\u8d3e\u771f\u5982","\u8d3e\u8bed\u5f64","\u8d3e\u4f1f\u8302","\u8d3e\u6210\u548c","\u8d3e\u9e3f\u7574","\u8d3e\u5bc4\u67d4","\u8d3e\u99a5","\u8d3e\u6674\u96ea","\u8d3e\u59ae\u5a1c","\u8d3e\u7426","\u8d3e\u7428","\u8d3e\u9a9e\u9a9e","\u8d3e\u591c\u96ea","\u8d3e\u6155\u854a","\u8d3e\u8bd7\u73ca","\u8d3e\u4ee5","\u8d3e\u9aa5","\u8d3e\u5982\u99a8","\u8d3e\u7b11\u96ef","\u8d3e\u4eae","\u8d3e\u5bfb\u6843","\u8d3e\u7ff0","\u8d3e\u96ea\u5bb9","\u8d3e\u826f\u5965","\u8d3e\u7ea2\u65ed","\u8d3e\u4f1f\u5f66","\u8d3e\u5b9b\u79cb","\u8d3e\u53f6\u5609","\u8d3e\u91ce\u4e91","\u8d3e\u8fb0\u9633","\u8d3e\u84c9\u84c9","\u8d3e\u5609\u77f3","\u8d3e\u6b23\u5408","\u8d3e\u96bd\u96c5","\u8d3e\u5f18\u65b0","\u8d3e\u5143\u69d0","\u8d3e\u5174\u6587","\u8d3e\u8018\u6d9b","\u8d3e\u5f66\u9732","\u8d3e\u51ac\u6885","\u8d3e\u6625\u6843","\u8d3e\u53c8\u84dd","\u8d3e\u7470\u73ae","\u8d3e\u661f\u6ce2","\u8d3e\u51b7\u73cd","\u8d3e\u6e05\u5b81","\u8d3e\u9999\u6843","\u8d3e\u5fd7\u4e13","\u8d3e\u60dc\u7075","\u8d3e\u4fcf\u4e3d","\u8d3e\u632f\u7ff1","\u8d3e\u6668\u5e0c","\u8d3e\u6021\u6728","\u8d3e\u8c37\u5170","\u8d3e\u5c71\u67f3","\u8d3e\u5b9b\u79cb","\u8d3e\u6db5\u857e","\u8d3e\u6e05\u96c5","\u8d3e\u5c0f\u840d","\u8d3e\u96c5\u8273","\u8d3e\u6068\u8377","\u8d3e\u8398\u8398","\u8d3e\u9890\u771f"       };
	}

	// You have the freedom to implement custom shops however you want
	// This example uses a 'pool' concept where items will be randomly selected from a pool with equal weight
	// We copy a bunch of code from NPCShop and NPCShop.Entry, allowing this shop to be easily adjusted by other mods.
	// 
	// This uses some fairly advanced C# to avoid being excessively long, so make sure you learn the language before trying to adapt it significantly
	public class JiasTravelingMerchantShop : AbstractNPCShop
	{
		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

			public bool Disabled { get; private set; }

			public Entry Disable() {
				Disabled = true;
				return this;
			}

			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		public record Pool(string Name, int Slots, List<Entry> Entries)
		{
			public Pool Add(Item item, params Condition[] conditions) {
				Entries.Add(new Entry(item, conditions.ToList()));
				return this;
			}

			public Pool Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
			public Pool Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

			// Picks a number of items (up to Slots) from the entries list, provided conditions are met.
			public IEnumerable<Item> PickItems() {
				// This is not a fast way to pick items without replacement, but it's certainly easy. Be careful not to do this many many times per frame, or on huge lists of items.
				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
				for (int i = 0; i < Slots; i++) {
					if (list.Count == 0)
						break;

					int k = Main.rand.Next(list.Count);
					yield return list[k].Item;

					// remove the entry from the list so it can't be selected again this pick
					list.RemoveAt(k);
				}
			}
		}

		public List<Pool> Pools { get; } = new();

		public JiasTravelingMerchantShop(int npcType) : base(npcType) { }

		public override IEnumerable<Entry> ActiveEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

		public Pool AddPool(string name, int slots) {
			var pool = new Pool(name, slots, new List<Entry>());
			Pools.Add(pool);
			return pool;
		}

		// Some methods to add a pool with a single item
		public void Add(Item item, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, conditions);
		public void Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
		public void Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

		// Here is where we actually 'roll' the contents of the shop
		public List<Item> GenerateNewInventoryList() {
			var items = new List<Item>();
			foreach (var pool in Pools) {
				items.AddRange(pool.PickItems());
			}
			return items;
		}

		public override void FillShop(ICollection<Item> items, NPC npc) {
			// use the items which were selected when the NPC spawned.
			foreach (var item in JiasTravelingMerchant.shopItems) {
				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow) {
			overflow = false;
			int i = 0;
			// use the items which were selected when the NPC spawned.
			foreach (var item in JiasTravelingMerchant.shopItems) {

				if (i == items.Length - 1) {
					// leave the last slot empty for selling
					overflow = true;
					return;
				}

				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items[i++] = item.Clone();
			}
		}




	}
}
