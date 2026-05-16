using System.Collections.Generic;
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
    public readonly struct GuDropEntry
    {
        public readonly int ItemType;
        public readonly int ChanceDenominator;
        public readonly int MinStack;
        public readonly int MaxStack;

        public GuDropEntry(int itemType, int chanceDenominator, int minStack = 1, int maxStack = 1)
        {
            ItemType = itemType;
            ChanceDenominator = chanceDenominator;
            MinStack = minStack;
            MaxStack = maxStack;
        }
    }

    public readonly struct BossDropEntry
    {
        public readonly int NpcType;
        public readonly GuDropEntry[] Drops;

        public BossDropEntry(int npcType, GuDropEntry[] drops)
        {
            NpcType = npcType;
            Drops = drops;
        }
    }

    public readonly struct NpcGroupDrop
    {
        public readonly int[] NpcTypes;
        public readonly GuDropEntry[] Drops;

        public NpcGroupDrop(int[] npcTypes, GuDropEntry[] drops)
        {
            NpcTypes = npcTypes;
            Drops = drops;
        }
    }

    public static class GuDropRegistry
    {
        public static readonly GuDropEntry[] UniversalDrops = new GuDropEntry[]
        {
            new(ModContent.ItemType<DirtGu>(), 200),
            new(ModContent.ItemType<ABitGu>(), 2000),
            new(ModContent.ItemType<WineBug>(), 20000),
        };

        public static readonly GuDropEntry[] BossCommonDrops = new GuDropEntry[]
        {
            new(ModContent.ItemType<WhitePig>(), 100),
            new(ModContent.ItemType<BlackPig>(), 100),
            new(ModContent.ItemType<BloodSkullGu>(), 100),
        };

        public static readonly BossDropEntry[] BossDrops = new BossDropEntry[]
        {
            new(NPCID.EaterofWorldsBody, new GuDropEntry[]
            {
                new(ModContent.ItemType<BronzeShari>(), 600),
            }),
            new(NPCID.SkeletronHead, new GuDropEntry[]
            {
                new(ModContent.ItemType<BoneSpearGu>(), 1),
                new(ModContent.ItemType<FirstToSecond>(), 1),
                new(ItemID.FallenStar, 1, 300, 300),
            }),
            new(NPCID.WallofFlesh, new GuDropEntry[]
            {
                new(ModContent.ItemType<SecondToThird>(), 1),
                new(ModContent.ItemType<BloodSkullGu>(), 20),
            }),
            new(NPCID.QueenSlimeBoss, new GuDropEntry[]
            {
                new(ModContent.ItemType<SliverShari>(), 40),
            }),
            new(NPCID.Plantera, new GuDropEntry[]
            {
                new(ModContent.ItemType<ForthToFifth>(), 1),
            }),
            new(NPCID.Golem, new GuDropEntry[]
            {
                new(ModContent.ItemType<PurpleCrystalShari>(), 30),
            }),
            new(NPCID.MoonLordCore, new GuDropEntry[]
            {
                new(ModContent.ItemType<FifthToSixth>(), 4),
            }),
            new(NPCID.HallowBoss, new GuDropEntry[]
            {
                new(ModContent.ItemType<TaiGuangGu>(), 1),
            }),
        };

        public static readonly NpcGroupDrop[] EarlyBossGroupDrops = new NpcGroupDrop[]
        {
            new(new int[] { NPCID.BrainofCthulhu, NPCID.KingSlime, NPCID.QueenBee, NPCID.EyeofCthulhu }, new GuDropEntry[]
            {
                new(ModContent.ItemType<BronzeShari>(), 100),
                new(ItemID.LovePotion, 2),
            }),
            new(new int[] { NPCID.Deerclops, NPCID.SkeletronHand }, new GuDropEntry[]
            {
                new(ModContent.ItemType<RedSteelShari>(), 50),
            }),
            new(new int[] { NPCID.Retinazer, NPCID.Spazmatism, NPCID.TheDestroyer, NPCID.SkeletronPrime }, new GuDropEntry[]
            {
                new(ModContent.ItemType<ThirdToForth>(), 4),
                new(ModContent.ItemType<SliverShari>(), 100),
                new(ModContent.ItemType<GoldShari>(), 100),
            }),
            new(new int[] { NPCID.HallowBoss, NPCID.DukeFishron }, new GuDropEntry[]
            {
                new(ModContent.ItemType<BronzeShari>(), 20),
                new(ModContent.ItemType<RedSteelShari>(), 20),
                new(ModContent.ItemType<SliverShari>(), 20),
                new(ModContent.ItemType<GoldShari>(), 20),
                new(ModContent.ItemType<PurpleCrystalShari>(), 20),
            }),
        };

        public static readonly NpcGroupDrop[] SpecialNpcDrops = new NpcGroupDrop[]
        {
            new(new int[] { NPCID.QueenBee }, new GuDropEntry[]
            {
                new(ModContent.ItemType<CareerGu>(), 15),
                new(ModContent.ItemType<StraightCollisionGu>(), 15),
            }),
            new(new int[] { NPCID.BloodCrawler, NPCID.WallCreeper }, new GuDropEntry[]
            {
                new(ModContent.ItemType<WidowSpider>(), 50),
                new(ModContent.ItemType<WarmStringSpider>(), 50),
            }),
            new(new int[] { NPCID.BlackRecluse }, new GuDropEntry[]
            {
                new(ModContent.ItemType<WidowSpider>(), 25),
                new(ModContent.ItemType<WarmStringSpider>(), 25),
            }),
            new(new int[] { NPCID.Scorpion, NPCID.ScorpionBlack }, new GuDropEntry[]
            {
                new(ModContent.ItemType<RedNeedleScorpion>(), 10),
                new(ModContent.ItemType<PoisonNeedleGu>(), 100),
            }),
            new(new int[] { NPCID.Grasshopper }, new GuDropEntry[]
            {
                new(ModContent.ItemType<DragonBallCricket>(), 35),
            }),
            new(new int[] { NPCID.MotherSlime }, new GuDropEntry[]
            {
                new(ModContent.ItemType<OneMinion>(), 15),
            }),
        };

        public static readonly NpcGroupDrop[] PreHardmodeElementDrops = new NpcGroupDrop[]
        {
            new(new int[] { NPCID.DarkCaster, NPCID.Skeleton, NPCID.AngryBones, NPCID.BoneSerpentHead, NPCID.CursedSkull, NPCID.SporeSkeleton, NPCID.Tim, NPCID.UndeadMiner, NPCID.UndeadViking }, new GuDropEntry[]
            {
                new(ModContent.ItemType<BoneSpearGu>(), 25),
                new(ModContent.ItemType<BoneWings>(), 25),
            }),
            new(new int[] { NPCID.Dandelion, NPCID.JungleSlime, NPCID.ManEater, NPCID.Snatcher, NPCID.SpikedJungleSlime }, new GuDropEntry[]
            {
                new(ModContent.ItemType<GrassPuppet>(), 15),
                new(ModContent.ItemType<PineNeedleGu>(), 5),
                new(ModContent.ItemType<QingTengGu>(), 15),
                new(ModContent.ItemType<JiaoLeiPotatoGu>(), 12),
                new(ModContent.ItemType<PotatoMotherGu>(), 45),
                new(ModContent.ItemType<MuMeiGu>(), 30),
                new(ModContent.ItemType<EternalLifeGu>(), 45),
                new(ModContent.ItemType<ThreeStepGrassGu>(), 10),
            }),
            new(new int[] { NPCID.MeteorHead }, new GuDropEntry[]
            {
                new(ModContent.ItemType<StarDartGu>(), 20),
                new(ModContent.ItemType<StarArrowGu>(), 20),
                new(ModContent.ItemType<MeteorGu>(), 20),
                new(ModContent.ItemType<StarfireGu>(), 20),
                new(ModContent.ItemType<StarRiverGu>(), 20),
                new(ModContent.ItemType<SkyMeteorGu>(), 20),
                new(ModContent.ItemType<ABitGu>(), 20),
                new(ModContent.ItemType<TwoStarRadianceReflectingGu>(), 40),
                new(ModContent.ItemType<ThreeStarsSkyGu>(), 80),
                new(ModContent.ItemType<FourStarCubeGu>(), 160),
                new(ModContent.ItemType<FiveStarLinkedBeadGu>(), 320),
            }),
            new(new int[] { NPCID.IceBat, NPCID.IceSlime, NPCID.SnowFlinx, NPCID.ZombieEskimo, NPCID.UndeadViking }, new GuDropEntry[]
            {
                new(ModContent.ItemType<IceKnifeGu>(), 20),
                new(ModContent.ItemType<ShuangLinMoonGu>(), 100),
                new(ModContent.ItemType<ShuangXiGu>(), 100),
                new(ModContent.ItemType<IceCrystalGu>(), 100),
            }),
            new(new int[] { NPCID.BlueSlime, NPCID.BlueJellyfish, NPCID.GreenJellyfish, NPCID.PinkJellyfish, NPCID.Piranha, NPCID.Squid }, new GuDropEntry[]
            {
                new(ModContent.ItemType<WaterArrowGu>(), 24),
                new(ModContent.ItemType<WaterDrillGu>(), 24),
                new(ModContent.ItemType<WaterShellGu>(), 24),
                new(ModContent.ItemType<WaterJiaGu>(), 24),
            }),
            new(new int[] { NPCID.SpikedIceSlime }, new GuDropEntry[]
            {
                new(ModContent.ItemType<IceAwlGu>(), 12),
                new(ModContent.ItemType<FrostArrowGu>(), 24),
            }),
            new(new int[] { NPCID.Ghost, NPCID.Demon, NPCID.FireImp }, new GuDropEntry[]
            {
                new(ModContent.ItemType<GhostFireGu>(), 25),
                new(ModContent.ItemType<GhostlyCallingGu>(), 25),
                new(ModContent.ItemType<GiantSpiritBodyGu>(), 25),
                new(ModContent.ItemType<GiantSpiritHeartGu>(), 25),
                new(ModContent.ItemType<GiantSpiritIntentGu>(), 25),
                new(ModContent.ItemType<LittleSoulGu>(), 10),
                new(ModContent.ItemType<YingShangGu>(), 10),
                new(ModContent.ItemType<BloodQiGu>(), 10),
            }),
            new(new int[] { NPCID.Piranha, NPCID.Salamander, NPCID.Salamander2, NPCID.Salamander3, NPCID.Salamander4, NPCID.Salamander5, NPCID.Salamander6, NPCID.Salamander7, NPCID.Salamander8, NPCID.Salamander9 }, new GuDropEntry[]
            {
                new(ModContent.ItemType<AcidWaterGu>(), 17),
                new(ModContent.ItemType<MoonPoisonGu>(), 17),
                new(ModContent.ItemType<BigBelliedFrogGu>(), 26),
            }),
            new(new int[] { NPCID.GraniteFlyer, NPCID.GraniteGolem, NPCID.GoblinScout }, new GuDropEntry[]
            {
                new(ModContent.ItemType<PlasmaGu>(), 7),
                new(ModContent.ItemType<ThunderShieldGu>(), 35),
            }),
            new(new int[] { NPCID.LavaSlime, NPCID.FireImp, NPCID.Hellbat, NPCID.Firefly }, new GuDropEntry[]
            {
                new(ModContent.ItemType<FireClothesGu>(), 6),
                new(ModContent.ItemType<StoveGu>(), 6),
                new(ModContent.ItemType<JiaoLeiPotatoGu>(), 3),
                new(ModContent.ItemType<YanZhouGu>(), 27),
                new(ModContent.ItemType<FireHeartGu>(), 27),
            }),
            new(new int[] { NPCID.FlyingFish, NPCID.Harpy, NPCID.Vulture }, new GuDropEntry[]
            {
                new(ModContent.ItemType<SpoutGu>(), 12),
                new(ModContent.ItemType<BreezeWheelGu>(), 10),
                new(ModContent.ItemType<TwistedHeelGu>(), 12),
                new(ModContent.ItemType<Cyclone>(), 8),
            }),
        };

        public static readonly NpcGroupDrop[] HardmodeElementDrops = new NpcGroupDrop[]
        {
            new(new int[] { NPCID.IceQueen, NPCID.IceElemental, NPCID.IceMimic, NPCID.IceTortoise, NPCID.IcyMerman, NPCID.IceGolem, NPCID.MisterStabby, NPCID.SnowBalla, NPCID.SnowmanGangsta, NPCID.Flocko, NPCID.Yeti }, new GuDropEntry[]
            {
                new(ModContent.ItemType<ShuangLinMoonGu>(), 8),
                new(ModContent.ItemType<ShuangXiGu>(), 8),
                new(ModContent.ItemType<IceCrystalGu>(), 8),
                new(ModContent.ItemType<FrostArrowGu>(), 8),
            }),
            new(new int[] { NPCID.Lavabat, NPCID.RuneWizard, NPCID.DesertGhoul, NPCID.RedDevil }, new GuDropEntry[]
            {
                new(ModContent.ItemType<FireClothesGu>(), 4),
                new(ModContent.ItemType<StoveGu>(), 4),
                new(ModContent.ItemType<YanZhouGu>(), 20),
                new(ModContent.ItemType<FireHeartGu>(), 20),
            }),
            new(new int[] { NPCID.BoneLee }, new GuDropEntry[]
            {
                new(ModContent.ItemType<MoonHandKnife>(), 7),
                new(ModContent.ItemType<RageGu>(), 7),
                new(ModContent.ItemType<DingShenGu>(), 7),
            }),
            new(new int[] { NPCID.BoneLee, NPCID.CrimsonAxe, NPCID.CursedHammer, NPCID.EnchantedSword }, new GuDropEntry[]
            {
                new(ModContent.ItemType<KnifeLightGu>(), 7),
                new(ModContent.ItemType<SwordShadowGu>(), 7),
                new(ModContent.ItemType<SwordQiGu>(), 7),
                new(ModContent.ItemType<GoldenThreadCloakGu>(), 7),
                new(ModContent.ItemType<SawtoothGoldenWuGu>(), 7),
            }),
            new(new int[] { NPCID.RedDevil, NPCID.HoppinJack, NPCID.DungeonSpirit }, new GuDropEntry[]
            {
                new(ModContent.ItemType<GhostFireGu>(), 5),
                new(ModContent.ItemType<YingShangGu>(), 5),
                new(ModContent.ItemType<GhostlyCallingGu>(), 5),
                new(ModContent.ItemType<ToilGu>(), 5),
                new(ModContent.ItemType<GiantSpiritBodyGu>(), 5),
                new(ModContent.ItemType<GiantSpiritHeartGu>(), 5),
                new(ModContent.ItemType<GiantSpiritIntentGu>(), 5),
                new(ModContent.ItemType<WarBoneWheel>(), 50),
                new(ModContent.ItemType<BloodHandprintGu>(), 50),
                new(ModContent.ItemType<BloodQiGu>(), 5),
            }),
            new(new int[] { NPCID.BlackRecluse, NPCID.IchorSticker, NPCID.JungleCreeper, NPCID.MossHornet, NPCID.ToxicSludge, NPCID.SwampThing }, new GuDropEntry[]
            {
                new(ModContent.ItemType<AcidWaterGu>(), 5),
                new(ModContent.ItemType<MoonPoisonGu>(), 5),
                new(ModContent.ItemType<BigBelliedFrogGu>(), 13),
            }),
            new(new int[] { NPCID.AngryTrapper, NPCID.Arapaima, NPCID.MossHornet }, new GuDropEntry[]
            {
                new(ModContent.ItemType<GrassPuppet>(), 5),
                new(ModContent.ItemType<QingTengGu>(), 5),
                new(ModContent.ItemType<TengClawGu>(), 30),
                new(ModContent.ItemType<PotatoMotherGu>(), 17),
                new(ModContent.ItemType<EternalLifeGu>(), 17),
                new(ModContent.ItemType<MuMeiGu>(), 17),
                new(ModContent.ItemType<PineNeedleGu>(), 5),
                new(ModContent.ItemType<ThreeStepGrassGu>(), 7),
                new(ModContent.ItemType<JiaoLeiPotatoGu>(), 7),
            }),
            new(new int[] { NPCID.AnglerFish, NPCID.Arapaima, NPCID.BloodFeeder, NPCID.BloodJelly, NPCID.FungoFish, NPCID.PigronCorruption, NPCID.PigronHallow, NPCID.PigronCrimson }, new GuDropEntry[]
            {
                new(ModContent.ItemType<WaterArrowGu>(), 6),
                new(ModContent.ItemType<WaterDrillGu>(), 6),
                new(ModContent.ItemType<WaterShellGu>(), 6),
                new(ModContent.ItemType<ReWaterGu>(), 7),
            }),
            new(new int[] { NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex, NPCID.MartianDrone, NPCID.MartianEngineer, NPCID.MartianOfficer, NPCID.MartianProbe, NPCID.MartianSaucer, NPCID.MartianSaucerCannon, NPCID.MartianSaucerCore, NPCID.MartianSaucerTurret, NPCID.MartianTurret, NPCID.MartianWalker }, new GuDropEntry[]
            {
                new(ModContent.ItemType<StarDartGu>(), 15),
                new(ModContent.ItemType<StarArrowGu>(), 15),
                new(ModContent.ItemType<MeteorGu>(), 15),
                new(ModContent.ItemType<StarfireGu>(), 20),
                new(ModContent.ItemType<StarRiverGu>(), 20),
                new(ModContent.ItemType<SkyMeteorGu>(), 20),
                new(ModContent.ItemType<ABitGu>(), 5),
                new(ModContent.ItemType<TwoStarRadianceReflectingGu>(), 10),
                new(ModContent.ItemType<ThreeStarsSkyGu>(), 20),
                new(ModContent.ItemType<FourStarCubeGu>(), 40),
                new(ModContent.ItemType<FiveStarLinkedBeadGu>(), 80),
            }),
            new(new int[] { NPCID.AngryNimbus, NPCID.BrainScrambler, NPCID.GigaZapper, NPCID.MartianDrone, NPCID.MartianEngineer, NPCID.MartianOfficer, NPCID.MartianWalker, NPCID.MartianTurret }, new GuDropEntry[]
            {
                new(ModContent.ItemType<PlasmaGu>(), 5),
                new(ModContent.ItemType<ThunderShieldGu>(), 25),
            }),
        };

        public static readonly NpcGroupDrop[] TownNpcDrops = new NpcGroupDrop[]
        {
            new(new int[] { ModContent.NPCType<XueTangJiaLao>() }, new GuDropEntry[]
            {
                new(ModContent.ItemType<WanShi>(), 2, 1, 50),
                new(ModContent.ItemType<Minilight>(), 2),
                new(ModContent.ItemType<Moonlight>(), 2),
            }),
            new(new int[] { ModContent.NPCType<YaoTangJiaLao>() }, new GuDropEntry[]
            {
                new(ItemID.HealingPotion, 2, 1, 5),
                new(ModContent.ItemType<LivingGrass>(), 2),
                new(ModContent.ItemType<LivingLeaf>(), 2),
            }),
        };

        private static readonly Dictionary<int, List<GuDropEntry>> _npcDropCache = new();

        public static void BuildCache()
        {
            _npcDropCache.Clear();

            foreach (var group in EarlyBossGroupDrops)
                AddGroupToCache(group);
            foreach (var group in SpecialNpcDrops)
                AddGroupToCache(group);
            foreach (var group in PreHardmodeElementDrops)
                AddGroupToCache(group);
            foreach (var group in HardmodeElementDrops)
                AddGroupToCache(group);
            foreach (var group in TownNpcDrops)
                AddGroupToCache(group);
        }

        private static void AddGroupToCache(NpcGroupDrop group)
        {
            foreach (int npcType in group.NpcTypes)
            {
                if (!_npcDropCache.TryGetValue(npcType, out var list))
                {
                    list = new List<GuDropEntry>();
                    _npcDropCache[npcType] = list;
                }
                list.AddRange(group.Drops);
            }
        }

        public static bool TryGetDrops(int npcType, out List<GuDropEntry> drops)
        {
            return _npcDropCache.TryGetValue(npcType, out drops);
        }

        public static bool TryGetBossDrops(int npcType, out GuDropEntry[] drops)
        {
            foreach (var boss in BossDrops)
            {
                if (boss.NpcType == npcType)
                {
                    drops = boss.Drops;
                    return true;
                }
            }
            drops = null;
            return false;
        }

        public static bool IsInEarlyBossGroup(int npcType)
        {
            foreach (var group in EarlyBossGroupDrops)
            {
                foreach (int t in group.NpcTypes)
                {
                    if (t == npcType) return true;
                }
            }
            return false;
        }
    }
}
