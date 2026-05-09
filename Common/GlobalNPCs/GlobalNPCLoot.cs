using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Accessories.Four;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Five;
using VerminLordMod.Content.Items.Weapons.Four;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.NPCs.Town;

namespace VerminLordMod.Common.GlobalNPCs
{
	class GlobalNPCLoot:GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			//boss
			if (npc.boss) {//boss均有概率掉落黑白豕蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WhitePig>(), 100));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlackPig>(), 100));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodSkullGu>(), 100));
			}
			if (npc.type == NPCID.EaterofWorldsBody) {//世界吞噬者体节1/600概率掉落青铜舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BronzeShari>(), 600));
			}
			if (npc.type == NPCID.BrainofCthulhu || npc.type == NPCID.KingSlime || npc.type == NPCID.QueenBee || npc.type == NPCID.EyeofCthulhu) {
				//克脑、克眼、史王、蜂后1/100概率掉落青铜舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BronzeShari>(), 100));
				npcLoot.Add(ItemDropRule.Common(ItemID.LovePotion, 2));
			}
			if (npc.type == NPCID.SkeletronHead) {
				// 打败骷髅王掉落骨枪蛊和二转凭证以及桑百颗坠落之星
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSpearGu>(), 1));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FirstToSecond>(), 1));
				npcLoot.Add(ItemDropRule.Common(ItemID.FallenStar, 1, 300, 300));
			}
			if (npc.type == NPCID.Deerclops || npc.type == NPCID.SkeletronHand) {//巨鹿和骷髅王的手1/50概率掉落赤铁舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RedSteelShari>(), 50));
			}
			if (npc.type == NPCID.WallofFlesh) {
				// 打败肉山掉落三转凭证
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SecondToThird>(), 1));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodSkullGu>(), 20));
			}
			if (npc.type == NPCID.QueenSlimeBoss) {//史莱姆皇后1/40概率掉落白银舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SliverShari>(), 40));
			}
			if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism || npc.type == NPCID.TheDestroyer || npc.type == NPCID.SkeletronPrime) {
				// 打败新三王概率掉落四转凭证
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThirdToForth>(), 4));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SliverShari>(), 100));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GoldShari>(), 100));
			}
			if (npc.type == NPCID.Plantera) {
				// 打败世纪之花掉落五转凭证
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ForthToFifth>(), 1));
			}
			if (npc.type == NPCID.Golem) {
				//石巨人1/30概率掉落紫晶舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PurpleCrystalShari>(), 30));
			}
			if (npc.type == NPCID.MoonLordCore) {
				// 打败月总掉落六转凭证
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FifthToSixth>(), 4));
			}

			if (npc.type == NPCID.HallowBoss || npc.type == NPCID.DukeFishron) {
				//猪鲨和光女1/20概率掉落任意舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BronzeShari>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RedSteelShari>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SliverShari>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GoldShari>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PurpleCrystalShari>(), 20));
			}
			if (npc.type == NPCID.HallowBoss) {
				//猪鲨和光女1/20概率掉落任意舍利蛊
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TaiGuangGu>(), 1));
			}
			if (npc.boss) {
				//boss掉落元石
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 3, 1, 100));
			}
			//蜂后
			if (npc.type==NPCID.QueenBee) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CareerGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StraightCollisionGu>(), 15));
			}


			//血地蜘蛛和普通蜘蛛
			if (npc.type == NPCID.BloodCrawler||npc.type==NPCID.WallCreeper) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WidowSpider>(), 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WarmStringSpider>(), 50));
			}
			//黑隐士
			if (npc.type == NPCID.BlackRecluse) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WidowSpider>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WarmStringSpider>(), 25));

			}
			//蝎子和黑蝎子
			if (npc.type == NPCID.Scorpion || npc.type == NPCID.ScorpionBlack) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RedNeedleScorpion>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PoisonNeedleGu>(), 100));
			}
			//蚂蚱
			if (npc.type == NPCID.Grasshopper) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DragonBallCricket>(), 35));
			}
			//史莱姆之母
			if (npc.type == NPCID.MotherSlime) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OneMinion>(), 15));
			}
			//肉前与骨头有关的
			if (npc.type == NPCID.DarkCaster || npc.type == NPCID.Skeleton|| npc.type == NPCID.AngryBones|| npc.type == NPCID.BoneSerpentHead|| npc.type == NPCID.CursedSkull|| npc.type == NPCID.Skeleton|| npc.type == NPCID.SporeSkeleton|| npc.type == NPCID.Tim|| npc.type == NPCID.UndeadMiner|| npc.type == NPCID.UndeadViking) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSpearGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneWings>(), 25));
			}
			//肉前与草有关的
			if (npc.type == NPCID.Dandelion || npc.type == NPCID.JungleSlime || npc.type == NPCID.ManEater || npc.type == NPCID.Snatcher || npc.type == NPCID.SpikedJungleSlime) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GrassPuppet>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PineNeedleGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<QingTengGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JiaoLeiPotatoGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PotatoMotherGu>(), 45));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MuMeiGu>(), 30));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalLifeGu>(), 45));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThreeStepGrassGu>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JiaoLeiPotatoGu>(), 4));
			}
			//肉前与流星有关的
			if (npc.type == NPCID.MeteorHead) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarDartGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarArrowGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MeteorGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarfireGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarRiverGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SkyMeteorGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ABitGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TwoStarRadianceReflectingGu>(), 40));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThreeStarsSkyGu>(), 80));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FourStarCubeGu>(), 160));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FiveStarLinkedBeadGu>(), 320));
			}
			//肉前与冰有关的
			if (npc.type==NPCID.IceBat || npc.type == NPCID.IceSlime || npc.type == NPCID.SnowFlinx || npc.type == NPCID.ZombieEskimo || npc.type == NPCID.UndeadViking) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IceKnifeGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShuangLinMoonGu>(), 100));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShuangXiGu>(), 100));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IceCrystalGu>(), 100));
			}
			//肉前与水有关的
			if (npc.type == NPCID.BlueSlime|| npc.type == NPCID.BlueJellyfish|| npc.type == NPCID.GreenJellyfish|| npc.type == NPCID.PinkJellyfish|| npc.type == NPCID.Piranha|| npc.type == NPCID.Squid) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterArrowGu>(), 24));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterDrillGu>(), 24));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterShellGu>(), 24));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterJiaGu>(), 24));
			}
			//冰雪尖刺史莱姆
			if (npc.type == NPCID.SpikedIceSlime) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IceAwlGu>(), 12));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostArrowGu>(), 24));
			}
			//肉前与鬼有关的
			if (npc.type == NPCID.Ghost|| npc.type == NPCID.Demon || npc.type == NPCID.FireImp) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GhostFireGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GhostlyCallingGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritBodyGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritHeartGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritIntentGu>(), 25));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LittleSoulGu>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YingShangGu>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodQiGu>(), 10));
			}
			//肉前与毒有关的
			if (npc.type == NPCID.Piranha || npc.type == NPCID.Salamander || npc.type == NPCID.Salamander2 || npc.type == NPCID.Salamander3 || npc.type == NPCID.Salamander4 || npc.type == NPCID.Salamander5 || npc.type == NPCID.Salamander6 || npc.type == NPCID.Salamander7 || npc.type == NPCID.Salamander8 || npc.type == NPCID.Salamander9) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AcidWaterGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonPoisonGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BigBelliedFrogGu>(), 26));
			}
			//肉前与电有关的
			if (npc.type == NPCID.GraniteFlyer|| npc.type == NPCID.GraniteGolem || npc.type == NPCID.GoblinScout) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PlasmaGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderShieldGu>(), 35));
			}
			//肉前与火有关的
			if (npc.type == NPCID.LavaSlime || npc.type == NPCID.FireImp || npc.type == NPCID.Hellbat || npc.type == NPCID.Firefly) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FireClothesGu>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StoveGu>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JiaoLeiPotatoGu>(), 3));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YanZhouGu>(), 27));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FireHeartGu>(), 27));
			}
			//肉前与风有关的
			if (npc.type == NPCID.FlyingFish || npc.type == NPCID.Harpy || npc.type == NPCID.Vulture) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpoutGu>(), 12));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BreezeWheelGu>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TwistedHeelGu>(), 12));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Cyclone>(), 8));
			}

			//肉后与冰有关的
			if (npc.type == NPCID.IceQueen || npc.type == NPCID.IceElemental || npc.type == NPCID.IceMimic || npc.type == NPCID.IceTortoise || npc.type == NPCID.IcyMerman || npc.type == NPCID.IceGolem || npc.type == NPCID.MisterStabby || npc.type == NPCID.SnowBalla || npc.type == NPCID.SnowmanGangsta || npc.type == NPCID.Flocko || npc.type == NPCID.Yeti) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShuangLinMoonGu>(), 8));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShuangXiGu>(), 8));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IceCrystalGu>(), 8));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostArrowGu>(), 8));
			}
			//肉后与火有关的
			if (npc.type == NPCID.Lavabat || npc.type == NPCID.RuneWizard || npc.type == NPCID.DesertGhoul|| npc.type == NPCID.RedDevil) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FireClothesGu>(), 4));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StoveGu>(), 4));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YanZhouGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FireHeartGu>(), 20));
			}
			//骷髅李小龙
			if (npc.type == NPCID.BoneLee) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonHandKnife>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RageGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DingShenGu>(), 7));
			}
			//肉后与兵器相关的
			if (npc.type == NPCID.BoneLee|| npc.type == NPCID.CrimsonAxe|| npc.type == NPCID.CursedHammer|| npc.type == NPCID.EnchantedSword) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KnifeLightGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SwordShadowGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SwordQiGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GoldenThreadCloakGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SawtoothGoldenWuGu>(), 7));
			}
			//肉后与鬼相关的
			if (npc.type == NPCID.RedDevil || npc.type == NPCID.HoppinJack || npc.type == NPCID.DungeonSpirit) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GhostFireGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YingShangGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GhostlyCallingGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ToilGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritBodyGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritHeartGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GiantSpiritIntentGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WarBoneWheel>(), 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodHandprintGu>(), 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodQiGu>(), 5));
			}
			//肉后与毒相关的
			if (npc.type == NPCID.BlackRecluse || npc.type == NPCID.IchorSticker || npc.type == NPCID.JungleCreeper || npc.type == NPCID.MossHornet || npc.type == NPCID.ToxicSludge || npc.type == NPCID.SwampThing) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AcidWaterGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonPoisonGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BigBelliedFrogGu>(), 13));
			}
			//肉后与草相关的
			if (npc.type == NPCID.AngryTrapper || npc.type == NPCID.Arapaima || npc.type == NPCID.MossHornet) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GrassPuppet>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<QingTengGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TengClawGu>(), 30));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PotatoMotherGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PotatoMotherGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EternalLifeGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MuMeiGu>(), 17));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PineNeedleGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThreeStepGrassGu>(), 7));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JiaoLeiPotatoGu>(), 7));
			}
			//肉后与水有关的
			if (npc.type == NPCID.AnglerFish || npc.type == NPCID.Arapaima || npc.type == NPCID.BloodFeeder || npc.type == NPCID.BloodJelly || npc.type == NPCID.FungoFish || npc.type == NPCID.PigronCorruption || npc.type == NPCID.PigronHallow || npc.type == NPCID.PigronCrimson) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterArrowGu>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterDrillGu>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WaterShellGu>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ReWaterGu>(), 7));
			}
			//肉后与流星有关的
			if (npc.type == NPCID.LunarTowerNebula|| npc.type == NPCID.LunarTowerSolar|| npc.type == NPCID.LunarTowerStardust|| npc.type == NPCID.LunarTowerVortex|| npc.type == NPCID.MartianDrone||npc.type == NPCID.MartianEngineer || npc.type == NPCID.MartianOfficer || npc.type == NPCID.MartianProbe || npc.type == NPCID.MartianSaucer || npc.type == NPCID.MartianSaucerCannon || npc.type == NPCID.MartianSaucerCore || npc.type == NPCID.MartianSaucerTurret || npc.type == NPCID.MartianTurret || npc.type == NPCID.MartianWalker) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarDartGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarArrowGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MeteorGu>(), 15));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarfireGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarRiverGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SkyMeteorGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ABitGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TwoStarRadianceReflectingGu>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThreeStarsSkyGu>(), 20));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FourStarCubeGu>(), 40));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FiveStarLinkedBeadGu>(), 80));
				
			}
			//肉后与电有关的
			if(npc.type==NPCID.AngryNimbus|| npc.type == NPCID.BrainScrambler|| npc.type == NPCID.GigaZapper|| npc.type == NPCID.MartianDrone|| npc.type == NPCID.MartianEngineer|| npc.type == NPCID.MartianOfficer|| npc.type == NPCID.MartianWalker|| npc.type == NPCID.MartianTurret) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PlasmaGu>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThunderShieldGu>(), 25));
			}


			// D-32: 移除元石直接掉落 — 元石仅通过交易和资源节点获取
			// 替代方案：后续可通过 D-34 资源节点扩展添加蛊虫材料掉落
			if (npc.townNPC)
			npcLoot.Add(ItemDropRule.Common(ItemID.FleshBlock, 1,1,7));
			if (npc.type == ModContent.NPCType<XueTangJiaLao>()) {
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WanShi>(), 2, 1, 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Minilight>(), 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Moonlight>(), 2));
			}
			if (npc.type == ModContent.NPCType<YaoTangJiaLao>()) {
				npcLoot.Add(ItemDropRule.Common(ItemID.HealingPotion, 2, 1, 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LivingGrass>(), 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LivingLeaf>(), 2));
			}

			//任何npc都有概率掉落的蛊
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DirtGu>(), 200));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ABitGu>(), 2000));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::VerminLordMod.Content.Items.Consumables.WineBug>(), 20000));
		}
	}
}
