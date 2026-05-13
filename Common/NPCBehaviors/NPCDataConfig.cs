using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.GuYue;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Accessories.One;
using VerminLordMod.Content.Items.Accessories.Two;
using VerminLordMod.Content.Items.Accessories.Three;
using VerminLordMod.Content.Items.Accessories.Four;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class NPCDataEntry
    {
        public GuYueNPCType NPCType { get; set; }
        public string DisplayName { get; set; }
        public string FirstMeeting { get; set; }
        public string CasualTalk { get; set; }
        public string Farewell { get; set; }
        public int Damage { get; set; }
        public float Knockback { get; set; }
        public int Cooldown { get; set; }
        public int RandExtraCooldown { get; set; }
        public int ProjType { get; set; }
        public int AttackDelay { get; set; }
        public float ProjSpeed { get; set; }
        public int MinGuLevel { get; set; }
        public int MinLife { get; set; }
        public int CoinType { get; set; }
        public int CoinMin { get; set; }
        public int CoinMax { get; set; }
        public List<ShopItemData> ShopItems { get; set; } = new();
        public List<LootItemData> LootItems { get; set; } = new();
    }

    public class ShopItemData
    {
        public int ItemType { get; set; }
        public int? CustomPrice { get; set; }
        public int? SpecialCurrency { get; set; }
    }

    public class LootItemData
    {
        public int ItemType { get; set; }
        public int MinStack { get; set; } = 1;
        public int MaxStack { get; set; } = 1;
        public float Chance { get; set; } = 1f;
    }

    public static class NPCDataRegistry
    {
        private static Dictionary<GuYueNPCType, NPCDataEntry> _data;
        private static bool _initialized;

        public static NPCDataEntry Get(GuYueNPCType type)
        {
            EnsureInitialized();
            return _data.TryGetValue(type, out var entry) ? entry : _data[GuYueNPCType.Commoner];
        }

        public static IEnumerable<NPCDataEntry> GetAll()
        {
            EnsureInitialized();
            return _data.Values;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _data = new Dictionary<GuYueNPCType, NPCDataEntry>();
            _initialized = true;

            int moonlightProj = ModContent.ProjectileType<MoonlightProj>();
            int yuanS = VerminLordMod.YuanSId;

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.Chief,
                DisplayName = "古月族长",
                FirstMeeting = "\"欢迎来到古月山寨，年轻人。\"",
                CasualTalk = "\"在寨子里好好修行，将来为家族出力。\"",
                Farewell = "\"好好干，我看好你。\"",
                Damage = 30, Knockback = 6f, Cooldown = 20, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 14f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ModContent.ItemType<KongQiaoStone>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<FirstToSecond>(), CustomPrice = 800, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SecondToThird>(), CustomPrice = 3000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ThirdToForth>(), CustomPrice = 10000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ForthToFifth>(), CustomPrice = 30000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<FifthToSixth>(), CustomPrice = 80000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<UnbindGuItem>(), CustomPrice = 200, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<GoldenThreadCloakGu>(), CustomPrice = 15000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<TengYunWings>(), CustomPrice = 20000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<NineWineBug>(), CustomPrice = 5000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<KsitigarbhaFlowerGu>(), CustomPrice = 8000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<Hopeness>(), CustomPrice = 12000, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.GoldCoin, CoinMin = 1, CoinMax = 3,
                LootItems =
                {
                    new LootItemData { ItemType = ModContent.ItemType<GoldShari>(), MinStack = 1, MaxStack = 3, Chance = 0.3f },
                    new LootItemData { ItemType = ModContent.ItemType<NineWineBug>(), MinStack = 1, MaxStack = 1, Chance = 0.15f },
                    new LootItemData { ItemType = ModContent.ItemType<KsitigarbhaFlowerGu>(), MinStack = 1, MaxStack = 1, Chance = 0.1f },
                    new LootItemData { ItemType = ItemID.GoldCoin, MinStack = 3, MaxStack = 8, Chance = 0.5f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.SchoolElder,
                DisplayName = "学堂家老",
                FirstMeeting = "\"年轻人，可愿听老夫讲讲蛊师的奥妙？\"",
                CasualTalk = "\"蛊师一道，贵在坚持。\"",
                Farewell = "\"不错，你有空可以多来学堂坐坐。\"",
                Damage = 20, Knockback = 4f, Cooldown = 30, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 12f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ModContent.ItemType<WanShi>(), CustomPrice = 5, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<KongQiaoStone>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<FirstToSecond>(), CustomPrice = 800, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SecondToThird>(), CustomPrice = 3000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ThirdToForth>(), CustomPrice = 10000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<UnbindGuItem>(), CustomPrice = 200, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ZizhiDing>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ZizhiYi>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ZizhiBing>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ZizhiJia>(), CustomPrice = 300, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 15, CoinMax = 40,
                LootItems =
                {
                    new LootItemData { ItemType = ModContent.ItemType<WanShi>(), MinStack = 1, MaxStack = 5, Chance = 0.4f },
                    new LootItemData { ItemType = ModContent.ItemType<BronzeShari>(), MinStack = 1, MaxStack = 2, Chance = 0.3f },
                    new LootItemData { ItemType = ModContent.ItemType<ZizhiDing>(), MinStack = 1, MaxStack = 1, Chance = 0.15f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.MedicineElder,
                DisplayName = "药堂家老",
                FirstMeeting = "\"受伤了就来药堂，我给你看看。\"",
                CasualTalk = "\"修行路上，保重身体要紧。\"",
                Farewell = "\"拿着，对你有好处。\"",
                Damage = 15, Knockback = 3f, Cooldown = 10, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 10f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.HealingPotion, CustomPrice = 10, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.GreaterHealingPotion, CustomPrice = 30, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.RegenerationPotion, CustomPrice = 15, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.LifeforcePotion, CustomPrice = 25, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<LivingLeaf>(), CustomPrice = 50, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<TenLifeGu>(), CustomPrice = 100, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<HundredLifeGu>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ThousandLifeGu>(), CustomPrice = 2000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WineBug>(), CustomPrice = 80, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SevenWineBug>(), CustomPrice = 400, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<FourFlavorWineBug>(), CustomPrice = 2000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Daybloom, CustomPrice = 3, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Mushroom, CustomPrice = 3, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 10, CoinMax = 30,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.HealingPotion, MinStack = 2, MaxStack = 5, Chance = 0.5f },
                    new LootItemData { ItemType = ModContent.ItemType<LivingLeaf>(), MinStack = 1, MaxStack = 2, Chance = 0.3f },
                    new LootItemData { ItemType = ModContent.ItemType<TenLifeGu>(), MinStack = 1, MaxStack = 1, Chance = 0.15f },
                    new LootItemData { ItemType = ModContent.ItemType<WineBug>(), MinStack = 1, MaxStack = 1, Chance = 0.1f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.DefenseElder,
                DisplayName = "御堂家老",
                FirstMeeting = "\"想挑选些防身之物？我这里倒是有几件好货。\"",
                CasualTalk = "\"防御之道，在于未雨绸缪。\"",
                Farewell = "\"有需要就来御堂看看。\"",
                Damage = 20, Knockback = 4f, Cooldown = 10, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 12f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ModContent.ItemType<CopperSkin>(), CustomPrice = 100, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<IronSkin>(), CustomPrice = 120, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<StoneSkin>(), CustomPrice = 50, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<JadeSkin>(), CustomPrice = 250, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ScaleGu>(), CustomPrice = 150, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<InvisibleStoneGu>(), CustomPrice = 200, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<CopperSkinS>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<InvisibleScaleGu>(), CustomPrice = 600, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<CopperSkinSS>(), CustomPrice = 2000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<BearPower>(), CustomPrice = 180, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 15, CoinMax = 40,
                LootItems =
                {
                    new LootItemData { ItemType = ModContent.ItemType<CopperSkin>(), MinStack = 1, MaxStack = 1, Chance = 0.2f },
                    new LootItemData { ItemType = ModContent.ItemType<StoneSkin>(), MinStack = 1, MaxStack = 1, Chance = 0.25f },
                    new LootItemData { ItemType = ModContent.ItemType<ScaleGu>(), MinStack = 1, MaxStack = 1, Chance = 0.15f },
                    new LootItemData { ItemType = ItemID.IronBar, MinStack = 3, MaxStack = 8, Chance = 0.4f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.ChiElder,
                DisplayName = "赤脉家老",
                FirstMeeting = "\"我赤脉一系，向来是家族的中流砥柱。\"",
                CasualTalk = "\"好好修行，别给家族丢脸。\"",
                Farewell = "\"嗯，还算有点出息。\"",
                Damage = 25, Knockback = 5f, Cooldown = 25, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 12f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.FlamingArrow, CustomPrice = 5, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Fireblossom, CustomPrice = 10, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.HellstoneBar, CustomPrice = 50, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<StrengthLongicorn>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<HuangLuoLongicorn>(), CustomPrice = 800, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<DragonBallCricket>(), CustomPrice = 1500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<RedSteelShari>(), CustomPrice = 400, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ShiJunLiGu>(), CustomPrice = 600, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<EagleYangGu>(), CustomPrice = 5000, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 10, CoinMax = 30,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.Fireblossom, MinStack = 2, MaxStack = 5, Chance = 0.5f },
                    new LootItemData { ItemType = ItemID.FlamingArrow, MinStack = 10, MaxStack = 30, Chance = 0.4f },
                    new LootItemData { ItemType = ModContent.ItemType<RedSteelShari>(), MinStack = 1, MaxStack = 1, Chance = 0.15f },
                    new LootItemData { ItemType = ModContent.ItemType<StrengthLongicorn>(), MinStack = 1, MaxStack = 1, Chance = 0.1f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.MoElder,
                DisplayName = "漠脉家老",
                FirstMeeting = "\"年轻人，稳扎稳打才是修行正道。\"",
                CasualTalk = "\"修行如逆水行舟，不进则退。\"",
                Farewell = "\"不错，继续保持。\"",
                Damage = 22, Knockback = 4f, Cooldown = 25, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 12f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.SandBlock, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.AntlionMandible, CustomPrice = 15, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.FossilOre, CustomPrice = 20, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<PurpleCrystalShari>(), CustomPrice = 400, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ShiJinLiGu>(), CustomPrice = 600, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<JinLiGu>(), CustomPrice = 800, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<JunLiGu>(), CustomPrice = 1000, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ToilGu>(), CustomPrice = 4000, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 10, CoinMax = 30,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.AntlionMandible, MinStack = 1, MaxStack = 3, Chance = 0.5f },
                    new LootItemData { ItemType = ItemID.FossilOre, MinStack = 2, MaxStack = 5, Chance = 0.4f },
                    new LootItemData { ItemType = ModContent.ItemType<PurpleCrystalShari>(), MinStack = 1, MaxStack = 1, Chance = 0.12f },
                    new LootItemData { ItemType = ModContent.ItemType<JinLiGu>(), MinStack = 1, MaxStack = 1, Chance = 0.08f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.MedicinePulseElder,
                DisplayName = "药脉家老",
                FirstMeeting = "\"受伤了就来药堂，我会尽力医治。\"",
                CasualTalk = "\"你的气色不太好，要注意休息。\"",
                Farewell = "\"这是我调制的伤药，拿着备用。\"",
                Damage = 12, Knockback = 3f, Cooldown = 15, RandExtraCooldown = 0,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 10f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.HealingPotion, CustomPrice = 8, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.GreaterHealingPotion, CustomPrice = 25, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.RegenerationPotion, CustomPrice = 15, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.LifeforcePotion, CustomPrice = 25, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Daybloom, CustomPrice = 3, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<LivingLeaf>(), CustomPrice = 40, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<TenLifeGu>(), CustomPrice = 80, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<HundredLifeGu>(), CustomPrice = 400, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ThousandLifeGu>(), CustomPrice = 1800, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WhitePigGu>(), CustomPrice = 150, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<BlackPigGu>(), CustomPrice = 200, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<FlowerPig>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SliverShari>(), CustomPrice = 300, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 5, CoinMax = 20,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.HealingPotion, MinStack = 1, MaxStack = 3, Chance = 0.5f },
                    new LootItemData { ItemType = ItemID.Daybloom, MinStack = 2, MaxStack = 5, Chance = 0.4f },
                    new LootItemData { ItemType = ModContent.ItemType<LivingLeaf>(), MinStack = 1, MaxStack = 1, Chance = 0.2f },
                    new LootItemData { ItemType = ModContent.ItemType<WhitePigGu>(), MinStack = 1, MaxStack = 1, Chance = 0.1f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.FirstTurnGuMaster,
                DisplayName = "古月蛊师",
                FirstMeeting = "\"你是新来的？我也是刚成为蛊师不久。\"",
                CasualTalk = "\"我最近又学会了一个新蛊术！\"",
                Farewell = "\"一起努力修行吧！\"",
                Damage = 10, Knockback = 2f, Cooldown = 30, RandExtraCooldown = 10,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 8f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.Mushroom, CustomPrice = 3, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Gel, CustomPrice = 2, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WanShi>(), CustomPrice = 5, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<BronzeShari>(), CustomPrice = 100, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WineBug>(), CustomPrice = 80, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<TenLifeGu>(), CustomPrice = 100, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.CopperCoin, CoinMin = 20, CoinMax = 50,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.Mushroom, MinStack = 1, MaxStack = 3, Chance = 0.4f },
                    new LootItemData { ItemType = ItemID.Gel, MinStack = 2, MaxStack = 5, Chance = 0.3f },
                    new LootItemData { ItemType = ModContent.ItemType<BronzeShari>(), MinStack = 1, MaxStack = 1, Chance = 0.15f },
                    new LootItemData { ItemType = ModContent.ItemType<WanShi>(), MinStack = 1, MaxStack = 3, Chance = 0.2f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.SecondTurnGuMaster,
                DisplayName = "古月资深蛊师",
                FirstMeeting = "\"二转修为？不错，有前途。\"",
                CasualTalk = "\"修行之路漫长，我花了五年才到二转。\"",
                Farewell = "\"有什么不懂的可以问我。\"",
                Damage = 15, Knockback = 3f, Cooldown = 25, RandExtraCooldown = 5,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 10f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.FallenStar, CustomPrice = 20, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.ManaCrystal, CustomPrice = 100, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SliverShari>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<SevenWineBug>(), CustomPrice = 400, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<HundredLifeGu>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<StrengthLongicorn>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<CopperSkinS>(), CustomPrice = 500, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<InvisibleScaleGu>(), CustomPrice = 600, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<OneMinion>(), CustomPrice = 200, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 5, CoinMax = 20,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.FallenStar, MinStack = 1, MaxStack = 3, Chance = 0.4f },
                    new LootItemData { ItemType = ItemID.ManaCrystal, MinStack = 1, MaxStack = 1, Chance = 0.1f },
                    new LootItemData { ItemType = ModContent.ItemType<SliverShari>(), MinStack = 1, MaxStack = 1, Chance = 0.12f },
                    new LootItemData { ItemType = ModContent.ItemType<SevenWineBug>(), MinStack = 1, MaxStack = 1, Chance = 0.08f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.FistInstructor,
                DisplayName = "拳脚教头",
                FirstMeeting = "\"想学拳脚功夫？找我准没错！\"",
                CasualTalk = "\"拳脚是蛊师的基础，根基不牢，地动山摇！\"",
                Farewell = "\"来，陪我练练！\"",
                Damage = 18, Knockback = 6f, Cooldown = 15, RandExtraCooldown = 5,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 10f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.WoodenSword, CustomPrice = 10, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.IronBroadsword, CustomPrice = 50, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Spear, CustomPrice = 30, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.WarAxeoftheNight, CustomPrice = 80, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<BearPower>(), CustomPrice = 180, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<StrengthLongicorn>(), CustomPrice = 300, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ShiJunLiGu>(), CustomPrice = 600, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WolfWaveCard>(), CustomPrice = 500, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 5, CoinMax = 20,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.WoodenSword, MinStack = 1, MaxStack = 1, Chance = 0.3f },
                    new LootItemData { ItemType = ItemID.Spear, MinStack = 1, MaxStack = 1, Chance = 0.2f },
                    new LootItemData { ItemType = ModContent.ItemType<BearPower>(), MinStack = 1, MaxStack = 1, Chance = 0.1f },
                    new LootItemData { ItemType = ModContent.ItemType<StrengthLongicorn>(), MinStack = 1, MaxStack = 1, Chance = 0.08f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.Servant,
                DisplayName = "杂役",
                FirstMeeting = "\"您有什么吩咐？我正忙着打扫呢。\"",
                CasualTalk = "\"虽然我只是个杂役，但能为家族出力就很高兴了。\"",
                Farewell = "\"您慢走，有什么需要尽管吩咐。\"",
                Damage = 6, Knockback = 4f, Cooldown = 30, RandExtraCooldown = 10,
                ProjType = 0, AttackDelay = 1, ProjSpeed = 0f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.Torch, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Wood, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Rope, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WanShi>(), CustomPrice = 5, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.CopperCoin, CoinMin = 10, CoinMax = 30,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.CopperAxe, Chance = 0.25f },
                    new LootItemData { ItemType = ItemID.CopperPickaxe, Chance = 0.25f },
                    new LootItemData { ItemType = ItemID.CopperBar, MinStack = 1, MaxStack = 3, Chance = 0.25f },
                    new LootItemData { ItemType = ItemID.CopperOre, MinStack = 3, MaxStack = 8, Chance = 0.25f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.Commoner,
                DisplayName = "古月凡人",
                FirstMeeting = "\"您是蛊师大人吧？真厉害！\"",
                CasualTalk = "\"我资质太差，怕是这辈子都开不了空窍了。\"",
                Farewell = "\"能在古月山寨生活，已经很知足了。\"",
                Damage = 4, Knockback = 3f, Cooldown = 40, RandExtraCooldown = 15,
                ProjType = 0, AttackDelay = 1, ProjSpeed = 0f,
                MinGuLevel = 1, MinLife = 100,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.Wood, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Torch, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.HealingPotion, CustomPrice = 10, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.CopperCoin, CoinMin = 5, CoinMax = 15,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.WoodenSword, Chance = 0.33f },
                    new LootItemData { ItemType = ItemID.Wood, MinStack = 5, MaxStack = 15, Chance = 0.33f },
                    new LootItemData { ItemType = ItemID.Gel, MinStack = 1, MaxStack = 3, Chance = 0.33f }
                }
            });

            Register(new NPCDataEntry
            {
                NPCType = GuYueNPCType.PatrolGuMaster,
                DisplayName = "古月巡逻蛊师",
                FirstMeeting = "\"站住！你是什么人？\"",
                CasualTalk = "\"山寨周边不太平，提高警惕。\"",
                Farewell = "\"没事别在野外乱逛。\"",
                Damage = 18, Knockback = 3f, Cooldown = 25, RandExtraCooldown = 5,
                ProjType = moonlightProj, AttackDelay = 1, ProjSpeed = 9f,
                MinGuLevel = 1, MinLife = 120,
                ShopItems =
                {
                    new ShopItemData { ItemType = ItemID.HealingPotion, CustomPrice = 10, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.Torch, CustomPrice = 1, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ItemID.IronBar, CustomPrice = 15, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<WanShi>(), CustomPrice = 5, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<BronzeShari>(), CustomPrice = 100, SpecialCurrency = yuanS },
                    new ShopItemData { ItemType = ModContent.ItemType<ScaleGu>(), CustomPrice = 150, SpecialCurrency = yuanS }
                },
                CoinType = ItemID.SilverCoin, CoinMin = 5, CoinMax = 15,
                LootItems =
                {
                    new LootItemData { ItemType = ItemID.Torch, MinStack = 5, MaxStack = 15, Chance = 0.5f },
                    new LootItemData { ItemType = ItemID.IronOre, MinStack = 2, MaxStack = 5, Chance = 0.3f },
                    new LootItemData { ItemType = ItemID.HealingPotion, MinStack = 1, MaxStack = 2, Chance = 0.2f },
                    new LootItemData { ItemType = ModContent.ItemType<BronzeShari>(), MinStack = 1, MaxStack = 1, Chance = 0.1f }
                }
            });
        }

        private static void Register(NPCDataEntry entry)
        {
            _data[entry.NPCType] = entry;
        }
    }
}
