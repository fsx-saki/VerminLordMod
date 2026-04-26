using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Biomes;

namespace VerminLordMod.Content.NPCs.Town
{
	[AutoloadHead]
	class YaoTangJiaLao : ModNPC
	{
		public const string ShopName = "Shop";
		public int NumberOfTimesTalkedTo = 0;
		private static Profiles.StackedNPCProfile NPCProfile;
		public override void SetStaticDefaults() {
			//base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 25;//帧图数量
			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs. This is the remaining frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 4; // The amount of frames in the attacking animation.
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the NPC that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 2; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			NPCID.Sets.AttackTime[Type] = 40; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.

			var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
							  // Rotation = MathHelper.ToRadians(180) // You can also change the rotation of an NPC. Rotation is measured in radians
							  // If you want to see an example of manually modifying these when the NPC is drawn, see PreDraw
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);


			NPC.Happiness
			.SetBiomeAffection<JungleBiome>(AffectionLevel.Like) // Example Person prefers the forest.
			.SetBiomeAffection<GuYueCompoundBiome>(AffectionLevel.Love)
			.SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike) // Example Person prefers the forest.
			.SetBiomeAffection<DesertBiome>(AffectionLevel.Hate) // Example Person dislikes the snow.
																  //.SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love) // Example Person likes the Example Surface Biome
			.SetNPCAffection(633, AffectionLevel.Love) // Loves living near the dryad.
			.SetNPCAffection(NPCID.Truffle, AffectionLevel.Like) // Likes living near the guide.
			.SetNPCAffection(ModContent.NPCType<BaiA>(), AffectionLevel.Dislike) // Dislikes living near the merchant.
			.SetNPCAffection(NPCID.Nurse, AffectionLevel.Hate) // Hates living near the demolitionist.
			; // < Mind the semicolon!


			NPCProfile = new Profiles.StackedNPCProfile(new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"));





		}
		public override void SetDefaults() {
			NPC.townNPC = true; // Sets NPC to be a Town NPC
			NPC.friendly = true; // NPC Will not attack player
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 100;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;

			AnimationType = NPCID.Guide;
		}


		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

				// Sets your NPC's flavor text in the bestiary.
				new FlavorTextBestiaryInfoElement("古月家族药堂家老"),

				// You can add multiple elements if you really wanted to
				// You can also use localization keys (see Localization/en-US.lang)
				new FlavorTextBestiaryInfoElement("Mods.VerminLordMod.Bestiary.YaoTangJiaLao")
			});
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// This code slowly rotates the NPC in the bestiary
			// (simply checking NPC.IsABestiaryIconDummy and incrementing NPC.Rotation won't work here as it gets overridden by drawModifiers.Rotation each tick)
			if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers)) {
				drawModifiers.Rotation += 0.005f;

				// Replace the existing NPCBestiaryDrawModifiers with our new one with an adjusted rotation
				NPCID.Sets.NPCBestiaryDrawOffset.Remove(Type);
				NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
			}

			return true;
		}
		//public override void HitEffect(NPC.HitInfo hit) {
		//	int num = NPC.life > 0 ? 1 : 5;

		//	for (int k = 0; k < num; k++) {
		//		//Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
		//	}
		//}
		public override bool CanTownNPCSpawn(int numTownNPCs) { // Requirements for the town NPC to spawn.
			foreach (var player in Main.ActivePlayers) {
				// Player has to have either an ExampleItem or an ExampleBlock in order for the NPC to spawn
				//if (player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>() || item.type == ModContent.ItemType<Items.Placeable.ExampleBlock>())) {
				//	return true;
				//}
				QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
				if (qiPlayer.qiEnabled && player.statLifeMax2 > 120)
					return true;
			}

			return false;
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return NameList;
		}
		public override string GetChat() {
			var chat = new WeightedRandom<string>();

			//int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			//if (partyGirl >= 0 && Main.rand.NextBool(4)) {
			//	chat.Add(Language.GetTextValue("Mods.VerminLordMod.Dialogue.ExamplePerson.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			//}
			// These are things that the NPC has a chance of telling you when you talk to it.
			chat.Add(Language.GetTextValue("战斗中受伤在所难免。"));
			//chat.Add(Language.GetTextValue("在人类繁衍生息的过程中，先贤们逐步发现了蛊虫的奥妙。已经开辟空窍，运用本身真元来喂养、炼化、操控这些蛊，达到各种目的的人，我们统称为蛊师。"));
			//chat.Add(Language.GetTextValue("蛊师一共有九大境界，从下到上，分别是一转、二转、三转直至九转。每一转大境界中又分初阶、中阶、高阶、巅峰四个小境界。"));
			//chat.Add(Language.GetTextValue("四代族长。天资卓越，一直修行到了五转蛊师的境界。要不是那个卑鄙无耻的魔头花酒行者偷袭的话，兴许能晋升成六转蛊师也说不定。唉……"), 0.2);
			//chat.Add(Language.GetTextValue("Mods.VerminLordMod.Dialogue.XueTangJiaLao.5"), 5.0);
			//chat.Add(Language.GetTextValue("Mods.VerminLordMod.Dialogue.XueTangJiaLao.6"), 0.1);

			//NumberOfTimesTalkedTo++;
			//if (NumberOfTimesTalkedTo >= 10) {
			//	//This counter is linked to a single instance of the NPC, so if ExamplePerson is killed, the counter will reset.
			//	chat.Add(Language.GetTextValue("Mods.VerminLordMod.Dialogue.ExamplePerson.TalkALot"));
			//}

			string chosenChat = chat; // chat is implicitly cast to a string. This is where the random choice is made.

			//// Here is some additional logic based on the chosen chat line. In this case, we want to display an item in the corner for StandardDialogue4.
			//if (chosenChat == Language.GetTextValue("Mods.VerminLordMod.Dialogue.ExamplePerson.StandardDialogue4")) {
			//	// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
			//	Main.npcChatCornerItem = ItemID.HiveBackpack;
			//}

			return chosenChat;
		}
		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
			button = Language.GetTextValue("LegacyInterface.28");
			//button2 = "Awesomeify";
			//if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack)) {
			//	button = "Upgrade " + Lang.GetItemNameValue(ItemID.HiveBackpack);
			//}
		}


		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = ShopName;
			}
		}




		public override void AddShops() {
			var npcShop = new NPCShop(Type, ShopName)
				.Add(new Item(ItemID.HealingPotion) {
					shopCustomPrice = 10,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<LivingLeaf>()) {
					shopCustomPrice = 50,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<LivingGrass>()) {
					shopCustomPrice = 100,
					shopSpecialCurrency = VerminLordMod.YuanSId
				});
			npcShop.Register();
		}


		public override void ModifyActiveShop(string shopName, Item[] items) {
			foreach (Item item in items) {
				// Skip 'air' items and null items.
				if (item == null || item.type == ItemID.None) {
					continue;
				}
			}
		}


		// Make this Town NPC teleport to the King and/or Queen statue when triggered. Return toKingStatue for only King Statues. Return !toKingStatue for only Queen Statues. Return true for both.
		public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;



		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 10;
			randExtraCooldown = 0;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<RedNeedle>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 0f;
			// SparklingBall is not affected by gravity, so gravityCorrection is left alone.
		}

		public static List<string> NameList = new List<string>() {
			"古月翰墨","古月庆雪","古月伶俐","古月新","古月昆皓","古月映雪","古月安娴","古月文乐","古月乐章","古月娅童","古月冷松","古月麦冬","古月碧春","古月觅露","古月嘉玉","古月妙晴","古月从筠","古月焱","古月锐锋","古月书萱","古月香春","古月采白","古月舒","古月馨兰","古月梦桐","古月宏壮","古月承","古月香彤","古月碧菡","古月寄南","古月绣文","古月大","古月问夏","古月吉玟","古月含桃","古月清韵","古月亦绿","古月阳阳","古月初阳","古月博厚","古月婉然","古月安荷","古月衍","古月秋蝶","古月思天","古月初兰","古月建树","古月景铄","古月慕蕊","古月晴画","古月绮山","古月绿海","古月浩言","古月晴波","古月思源","古月嘉云","古月秀竹","古月蔼","古月格","古月浩阔","古月懿轩","古月姣","古月访彤","古月轩","古月涵育","古月舞","古月斯琪","古月彦珺","古月晴曦","古月之玉","古月映寒","古月白容","古月乐蓉","古月悦恺","古月傲之","古月央","古月俊逸","古月婉淑","古月德辉","古月如","古月颜","古月芷雪","古月孤容","古月半雪","古月古韵","古月阳煦","古月嘉美","古月善静","古月元绿","古月筠溪","古月谷蕊","古月痴梅","古月荷","古月蔓菁","古月雪羽","古月宁","古月雅静","古月清怡","古月访曼","古月安吉","古月嘉熙","古月良奥","古月峻","古月景龙","古月涵易","古月夜梅","古月千儿","古月晴丽","古月建同","古月恨风"
		};
	}
}
