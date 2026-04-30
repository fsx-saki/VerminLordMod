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
using VerminLordMod.Content.Items.Placeable;
using VerminLordMod.Content.Items.Placeable.Furniture;
using VerminLordMod.Content.Biomes;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Items.Weapons.Three;

namespace VerminLordMod.Content.NPCs.Town
{
	[AutoloadHead]
	class BaiA : ModNPC
	{
		public const string ShopName = "Shop";
		public int NumberOfTimesTalkedTo = 0;
		private static Profiles.StackedNPCProfile NPCProfile;

		// 使用古月家族统一贴图
		public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
		public override string HeadTexture => "VerminLordMod/Content/NPCs/Town/BaiA_Head";
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
			.SetBiomeAffection<DesertBiome>(AffectionLevel.Hate) // Example Person prefers the forest.
			.SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike) // Example Person prefers the forest.
			.SetBiomeAffection<JungleBiome>(AffectionLevel.Like) // Example Person prefers the forest.
			.SetBiomeAffection<OceanBiome>(AffectionLevel.Love) // Example Person dislikes the snow.
																  //.SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love) // Example Person likes the Example Surface Biome
			.SetNPCAffection(NPCID.Dryad, AffectionLevel.Love) // Loves living near the dryad.
			.SetNPCAffection(NPCID.Guide, AffectionLevel.Like) // Likes living near the guide.
			.SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike) // Dislikes living near the merchant.
			.SetNPCAffection(NPCID.Angler, AffectionLevel.Hate) // Hates living near the demolitionist.
			; // < Mind the semicolon!


			NPCProfile = new Profiles.StackedNPCProfile(new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"));





		}
		public override void SetDefaults() {
			NPC.townNPC = true; // Sets NPC to be a Town NPC
			NPC.friendly = true; // NPC Will not attack player
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
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
				new FlavorTextBestiaryInfoElement("白家长老"),

				// You can add multiple elements if you really wanted to
				// You can also use localization keys (see Localization/en-US.lang)
				new FlavorTextBestiaryInfoElement("Mods.VerminLordMod.Bestiary.BaiA")
			});
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// This code slowly rotates the NPC in the bestiary
			// (simply checking NPC.IsABestiaryIconDummy and incrementing NPC.Rotation won't work here as it gets overridden by drawModifiers.Rotation each tick)
			if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers)) {
				drawModifiers.Rotation += 0.001f;

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
				QiRealmPlayer qiRealm = player.GetModPlayer<QiRealmPlayer>();
				if (qiRealm.GuLevel > 0)
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
			chat.Add(Language.GetTextValue("人是万物之灵，蛊是天地之精。在这个世界上存在着成千上万种，数不胜数的蛊。它们就生活在我们的周围，在矿土里，在草丛里，甚至在野兽的体内。"));
			chat.Add(Language.GetTextValue("在人类繁衍生息的过程中，先贤们逐步发现了蛊虫的奥妙。已经开辟空窍，运用本身真元来喂养、炼化、操控这些蛊，达到各种目的的人，我们统称为蛊师。"));
			chat.Add(Language.GetTextValue("蛊师一共有九大境界，从下到上，分别是一转、二转、三转直至九转。每一转大境界中又分初阶、中阶、高阶、巅峰四个小境界。"));
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
				.Add(new Item(ModContent.ItemType<RiverStream>()) {
					shopCustomPrice = 70,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<WarmStringSpider>()) {
					shopCustomPrice = 50,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<IceAwlGu>()) {
					shopCustomPrice = 250,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<IceKnifeGu>()) {
					shopCustomPrice = 250,
					shopSpecialCurrency = VerminLordMod.YuanSId
				})
				.Add(new Item(ModContent.ItemType<WanShi>()) {
					shopCustomPrice = 5,
					shopSpecialCurrency = VerminLordMod.YuanSId
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneBlock>()) {
				//	shopCustomPrice = 5
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneChair>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneTable>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneBed>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneClock>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneDoor>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneSink>()) {
				//}).Add(new Item(ModContent.ItemType<QingMaoStoneToilet>()) {
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
		public override bool CanGoToStatue(bool toKingStatue) => true;



		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 10;
			randExtraCooldown = 0;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<IceKnife>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 0f;
			// SparklingBall is not affected by gravity, so gravityCorrection is left alone.
		}

		public static List<string> NameList = new List<string>() {
			"白曼岚","白白竹","白碧蓉","白安兰","白妙梦","白山白","白山灵","白飞荷","白忆之","白醉南","白雅风","白又之","白谷梦","白冷松","白妙菡","白千儿","白曼雁","白夏蝶","白凰","白香菱","白怜南","白亦竹","白绮风","白从之","白醉双","白含雁","白听寒","白寒风","白春翠","白春翠","白幻瑶","白戊","白绿露","白凌寒","白觅雪","白思柔","白依丝","白山云","白南莲","白冷天","白翠阳","白凝风","白恨玉","白半荷","白雅柏","白水蕊","白冬山","白安夏","白雪晴","白雨","白涵雁","白妙蕊","白秋玉","白元春","白凝绿","白雨安","白又珊","白山天","白曼珍","白之彤","白听安","白天卉","白己","白醉南","白若山","白访梦","白秋珊","白元瑶","白半松","白涵柔","白雪蕊","白曼丝","白盼秋","白友槐","白丹蓝","白雁岚","白若松","白香天","白梦筠","白幻雪","白夜柳","白乐岚","白绮南","白访柏","白采露","白念香","白元冬","白妙梦","白青亦","白晓卉","白映安","白秋波","白寄蓉","白谷蓝","白涵柔","白语海","白听梦","白丹菡","白衣","白凡桃","白凌蝶","白若翠","白安双","白谷霜","白阉茂","白雨菱","白醉安","白幼芙","白谷蕊","白宛阳","白以烟","白忆安","白语风","白南珍","白寻巧","白思波","白念雁","白涵阳","白香绿","白曼青","白香阳","白凌凡","白飞之","白沛寒","白芷芹","白怜云","白灵萱","白雁梅","白夏之","白夜蓝","白幻珊","白雁凡","白诗翠","白凌云","白冰云","白靖晴","白安阳","白怜莲","白梦蕊","白访文","白又竹","白幻丝","白依霜","白谷之","白香冬","白尔真","白雪梅","白雅珍","白映梅","白春","白初柔","白寒云","白觅露","白涵易","白飞柏","白笑巧","白平绿","白初夏","白天彤","白以晴","白雨琴","白冷菱","白半双","白新筠","白冷琴","白寻蓉","白南霜","白寒珍","白采冬","白盼露","白白夏","白乐青","白春冬","白媚","白问萱","白凌蝶","白寄灵","白冰之","白向雁","白谷槐","白飞兰","白平卉","白酉","白忆青","白香柏","白摄提格","白雨安","白水蓝","白雅云","白含冬","白书柔","白笑槐","白雁卉","白盼芙","白沛凝","白幻雪","白凡槐","白谷香","白婵","白曼萍","白香之","白凡绿","白秋亦","白巧夏","白诗丹","白冬荷","白天烟","白采香","白凡海","白冬灵","白倩","白醉霜","白妙蕊","白紫萍","白语蓉","白秋之","白幼儿","白绮南","白雨凝","白静卉","白寒梅","白沛芹","白诗双","白尔蝶","白绿松","白天烟","白孤晴","白静筠","白含阳","白巧荷","白如寒","白听容","白凝青","白子","白初阳","白幼霜","白绿荷","白冰桃","白诗双","白安珊","白思真","白醉容","白绫","白灵烟","白妙双","白语云","白寻云","白翠巧","白初真","白碧竹","白依丝","白小春","白亦儿","白亦凝","白紫玉","白媪","白含灵","白从易","白孤蝶","白采珍","白寒晴","白幻蓉","白笑丝","白又天","白雪柔","白书蝶","白巧之","白痴双","白采春","白恨珍","白靖荷","白痴柏","白小柳","白映风","白含巧","白冰蕊","白香桃","白千柔","白傲霜","白代枫","白紫柔","白雨灵","白妙柏","白含真","白思烟","白敦牂","白依山","白尔薇","白之容","白新丹","白宛丝","白凡兰","白静筠","白依波","白绿竹","白半寒","白从文","白飞双","白巧蕊","白晓丝","白春香","白培","白梦筠","白惜芹","白映真","白南霜","白友露","白白梅","白幼柔","白翠旋","白孤波","白傲桃","白碧海","白寻阳","白幻菱","白怜青","白映寒","白迎蕊","白以旋","白秋夏","白如筠","白又蕊","白尔烟","白寒易","白从之","白千冬","白傲文","白谷彤","白静芹","白念之","白雁凡","白平卉","白半兰","白海真","白笑桃","白笑真","白雨露","白水翠","白飞荷","白初兰","白晓卉","白小之","白寒烟","白之山","白元荷","白孤风","白水彤","白若丝","白南莲","白盼山","白诗桃","白依玉","白冰真","白凌山","白尔卉","白怀薇","白初柳","白沛槐","白代阳","白易梦","白如珍","白紫夏","白凡菱","白安荷","白白真","白尔真","白乐松","白映冬","白尔容","白以珊","白雪曼","白春白","白从筠","白妙蝶","白水凡","白怜真","白冬雁","白觅枫","白夏蓉","白亦丝","白恨蓉","白安莲","白映寒","白以旋","白向菱","白含珊","白飞珍","白怀青","白新真","白画","白向槐","白绿萍","白沛白","白痴海","白尔烟","白山冬","白之卉","白采枫","白念蕾","白水冬","白半容","白映柏","白恨桃","白白凝","白谷文","白冷菱","白香阳","白以松","白尔蝶","白孤云","白笑萱","白靖薇","白思烟","白水彤","白雁芙","白胭","白迎旋","白寻翠","白巧兰","白幼绿","白芷芹","白静柏","白沛凝","白山天","白冬易","白安真","白雪梦","白雨灵","白尔青","白妙之","白曼玉","白香菱","白紫丹","白寄蓝","白香莲","白友易","白盼晴","白香琴","白雪柏","白宛竹","白念柳","白以秋","白白易","白凡白","白觅蓉","白山枫","白芷芹","白香巧","白小柳","白思烟","白乐岚","白雪","白若巧","白笑春","白夜蓝","白思萱","白雁旋","白绿波","白迎春","白谷兰","白绿薇","白盼晴","白新雪","白尔柳","白雪曼","白梦安","白友易","白雪山","白从寒","白山菡"     };
	}
}
