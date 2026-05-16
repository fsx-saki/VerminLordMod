using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum SpiritBeastType
    {
        None,
        SpiritFox,
        FlameTiger,
        IceSerpent,
        ThunderEagle,
        EarthBear,
        ShadowWolf,
        LightDeer,
        PoisonToad,
        WindHawk,
        BloodBat,
        JadeRabbit,
        IronTurtle,
        CrystalButterfly,
        VoidCat,
        DreamTapir
    }

    public enum SpiritBeastStage
    {
        Egg,
        Hatchling,
        Juvenile,
        Adult,
        Elder,
        Ancient
    }

    public enum SpiritBeastMood
    {
        Hostile,
        Wary,
        Neutral,
        Friendly,
        Loyal,
        Devoted
    }

    public class SpiritBeastData
    {
        public SpiritBeastType Type;
        public string Name;
        public string Description;
        public int BaseHP;
        public int BaseDamage;
        public int BaseDefense;
        public float GrowthRate;
        public int PreferredBiome;
        public int FavoriteFoodItemType;
        public List<SpiritBeastAbility> Abilities = new();
    }

    public class SpiritBeastAbility
    {
        public string Name;
        public string Description;
        public int RequiredStage;
        public float CooldownSeconds;
        public float QiCost;
        public AbilityType Type;
    }

    public enum AbilityType
    {
        Passive,
        ActiveAttack,
        ActiveDefense,
        ActiveHeal,
        ActiveBuff,
        ActiveUtility
    }

    public class SpiritBeastInstance
    {
        public SpiritBeastType Type;
        public string CustomName;
        public SpiritBeastStage Stage;
        public SpiritBeastMood Mood;
        public int HP;
        public int MaxHP;
        public int Damage;
        public int Defense;
        public int GrowthProgress;
        public int RequiredGrowth;
        public int BondLevel;
        public int BondExp;
        public int HungerLevel;
        public int MaxHunger;
        public bool IsSummoned;
        public int SlotIndex;
        public List<string> UnlockedAbilities = new();
    }

    public class SpiritBeastSystem : ModSystem
    {
        public static SpiritBeastSystem Instance => ModContent.GetInstance<SpiritBeastSystem>();

        public Dictionary<SpiritBeastType, SpiritBeastData> BeastRegistry = new();

        public override void OnWorldLoad()
        {
            BeastRegistry.Clear();
            RegisterSpiritBeasts();
        }

        private void RegisterSpiritBeasts()
        {
            BeastRegistry[SpiritBeastType.SpiritFox] = new SpiritBeastData
            {
                Type = SpiritBeastType.SpiritFox,
                Name = "灵狐",
                Description = "聪慧的灵狐，擅长辅助和侦察。",
                BaseHP = 100, BaseDamage = 10, BaseDefense = 5,
                GrowthRate = 1.2f,
                PreferredBiome = Terraria.ID.TileID.Grass,
                FavoriteFoodItemType = Terraria.ID.ItemID.Daybloom,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "灵识感知", Description = "被动提升主人搜索范围。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "狐火", Description = "释放狐火攻击敌人。", RequiredStage = 2, CooldownSeconds = 10f, QiCost = 20f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "魅惑", Description = "魅惑敌人使其暂时停止攻击。", RequiredStage = 3, CooldownSeconds = 30f, QiCost = 40f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.FlameTiger] = new SpiritBeastData
            {
                Type = SpiritBeastType.FlameTiger,
                Name = "炎虎",
                Description = "威猛的炎虎，擅长近战攻击。",
                BaseHP = 200, BaseDamage = 25, BaseDefense = 15,
                GrowthRate = 1.0f,
                PreferredBiome = Terraria.ID.TileID.Ash,
                FavoriteFoodItemType = Terraria.ID.ItemID.Gel,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "虎威", Description = "被动提升主人攻击力。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "烈焰扑击", Description = "扑向敌人造成火焰伤害。", RequiredStage = 2, CooldownSeconds = 8f, QiCost = 25f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "火焰护体", Description = "为主人附加火焰护盾。", RequiredStage = 3, CooldownSeconds = 25f, QiCost = 35f, Type = AbilityType.ActiveDefense },
                }
            };

            BeastRegistry[SpiritBeastType.IceSerpent] = new SpiritBeastData
            {
                Type = SpiritBeastType.IceSerpent,
                Name = "冰蟒",
                Description = "冰冷的冰蟒，擅长控制和减速。",
                BaseHP = 150, BaseDamage = 15, BaseDefense = 20,
                GrowthRate = 1.1f,
                PreferredBiome = Terraria.ID.TileID.SnowBlock,
                FavoriteFoodItemType = Terraria.ID.ItemID.IceBlock,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "寒冰之躯", Description = "被动提升主人防御力。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "冰锥喷射", Description = "喷射冰锥攻击敌人。", RequiredStage = 2, CooldownSeconds = 10f, QiCost = 20f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "冰冻领域", Description = "冻结周围敌人。", RequiredStage = 4, CooldownSeconds = 40f, QiCost = 50f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.ThunderEagle] = new SpiritBeastData
            {
                Type = SpiritBeastType.ThunderEagle,
                Name = "雷鹰",
                Description = "迅捷的雷鹰，擅长远程攻击和机动。",
                BaseHP = 120, BaseDamage = 20, BaseDefense = 8,
                GrowthRate = 1.3f,
                PreferredBiome = Terraria.ID.TileID.Cloud,
                FavoriteFoodItemType = Terraria.ID.ItemID.FallenStar,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "鹰眼", Description = "被动提升主人移动速度。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "雷霆一击", Description = "召唤雷电攻击敌人。", RequiredStage = 2, CooldownSeconds = 12f, QiCost = 30f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "风暴之翼", Description = "为主人附加飞行能力。", RequiredStage = 4, CooldownSeconds = 60f, QiCost = 60f, Type = AbilityType.ActiveBuff },
                }
            };

            BeastRegistry[SpiritBeastType.EarthBear] = new SpiritBeastData
            {
                Type = SpiritBeastType.EarthBear,
                Name = "地熊",
                Description = "沉稳的地熊，擅长防御和回复。",
                BaseHP = 300, BaseDamage = 18, BaseDefense = 30,
                GrowthRate = 0.8f,
                PreferredBiome = Terraria.ID.TileID.Dirt,
                FavoriteFoodItemType = Terraria.ID.ItemID.Mushroom,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "大地之力", Description = "被动提升主人最大生命。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "地裂冲击", Description = "震击地面造成范围伤害。", RequiredStage = 2, CooldownSeconds = 15f, QiCost = 30f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "大地治愈", Description = "为主人回复生命。", RequiredStage = 3, CooldownSeconds = 20f, QiCost = 40f, Type = AbilityType.ActiveHeal },
                }
            };

            BeastRegistry[SpiritBeastType.ShadowWolf] = new SpiritBeastData
            {
                Type = SpiritBeastType.ShadowWolf,
                Name = "影狼",
                Description = "神秘的影狼，擅长暗杀和潜行。",
                BaseHP = 130, BaseDamage = 28, BaseDefense = 10,
                GrowthRate = 1.15f,
                PreferredBiome = Terraria.ID.TileID.Ebonstone,
                FavoriteFoodItemType = Terraria.ID.ItemID.RottenChunk,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "暗影步", Description = "被动提升主人暴击率。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "暗影突袭", Description = "从暗影中突袭敌人。", RequiredStage = 2, CooldownSeconds = 8f, QiCost = 25f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "暗影潜行", Description = "使主人短暂隐身。", RequiredStage = 3, CooldownSeconds = 45f, QiCost = 50f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.LightDeer] = new SpiritBeastData
            {
                Type = SpiritBeastType.LightDeer,
                Name = "光鹿",
                Description = "圣洁的光鹿，擅长治疗和净化。",
                BaseHP = 180, BaseDamage = 8, BaseDefense = 12,
                GrowthRate = 1.1f,
                PreferredBiome = Terraria.ID.TileID.HallowedGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.PixieDust,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "圣光庇护", Description = "被动提升主人生命回复。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "净化之光", Description = "清除主人的负面状态。", RequiredStage = 2, CooldownSeconds = 15f, QiCost = 30f, Type = AbilityType.ActiveHeal },
                    new SpiritBeastAbility { Name = "圣光治愈", Description = "大范围治疗友方。", RequiredStage = 4, CooldownSeconds = 30f, QiCost = 60f, Type = AbilityType.ActiveHeal },
                }
            };

            BeastRegistry[SpiritBeastType.PoisonToad] = new SpiritBeastData
            {
                Type = SpiritBeastType.PoisonToad,
                Name = "毒蟾",
                Description = "剧毒的毒蟾，擅长毒素和削弱。",
                BaseHP = 160, BaseDamage = 12, BaseDefense = 18,
                GrowthRate = 1.0f,
                PreferredBiome = Terraria.ID.TileID.JungleGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.Stinger,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "毒抗", Description = "被动提升主人毒素抗性。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "毒液喷射", Description = "喷射毒液攻击敌人。", RequiredStage = 2, CooldownSeconds = 10f, QiCost = 20f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "剧毒领域", Description = "在周围制造毒雾。", RequiredStage = 3, CooldownSeconds = 35f, QiCost = 45f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.WindHawk] = new SpiritBeastData
            {
                Type = SpiritBeastType.WindHawk,
                Name = "风隼",
                Description = "轻盈的风隼，擅长速度和侦察。",
                BaseHP = 90, BaseDamage = 14, BaseDefense = 6,
                GrowthRate = 1.4f,
                PreferredBiome = Terraria.ID.TileID.Grass,
                FavoriteFoodItemType = Terraria.ID.ItemID.Feather,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "疾风", Description = "被动大幅提升主人移动速度。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "风刃", Description = "释放风刃攻击敌人。", RequiredStage = 2, CooldownSeconds = 6f, QiCost = 15f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "顺风", Description = "为主人附加顺风buff。", RequiredStage = 3, CooldownSeconds = 20f, QiCost = 25f, Type = AbilityType.ActiveBuff },
                }
            };

            BeastRegistry[SpiritBeastType.BloodBat] = new SpiritBeastData
            {
                Type = SpiritBeastType.BloodBat,
                Name = "血蝠",
                Description = "嗜血的血蝠，擅长吸血和回复。",
                BaseHP = 110, BaseDamage = 18, BaseDefense = 8,
                GrowthRate = 1.25f,
                PreferredBiome = Terraria.ID.TileID.CrimsonGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.Vertebrae,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "嗜血", Description = "被动获得吸血效果。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "血之爪", Description = "用血爪攻击敌人并吸血。", RequiredStage = 2, CooldownSeconds = 10f, QiCost = 25f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "血宴", Description = "牺牲部分生命大幅提升攻击。", RequiredStage = 4, CooldownSeconds = 60f, QiCost = 50f, Type = AbilityType.ActiveBuff },
                }
            };

            BeastRegistry[SpiritBeastType.JadeRabbit] = new SpiritBeastData
            {
                Type = SpiritBeastType.JadeRabbit,
                Name = "玉兔",
                Description = "温顺的玉兔，擅长炼丹辅助。",
                BaseHP = 80, BaseDamage = 5, BaseDefense = 4,
                GrowthRate = 1.5f,
                PreferredBiome = Terraria.ID.TileID.HallowedGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.Daybloom,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "药感", Description = "被动提升炼丹成功率。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "灵药采集", Description = "自动采集附近药材。", RequiredStage = 2, CooldownSeconds = 30f, QiCost = 20f, Type = AbilityType.ActiveUtility },
                    new SpiritBeastAbility { Name = "丹药精炼", Description = "提升丹药品质一级。", RequiredStage = 4, CooldownSeconds = 120f, QiCost = 80f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.IronTurtle] = new SpiritBeastData
            {
                Type = SpiritBeastType.IronTurtle,
                Name = "铁甲龟",
                Description = "坚固的铁甲龟，擅长防御和守护。",
                BaseHP = 400, BaseDamage = 10, BaseDefense = 40,
                GrowthRate = 0.7f,
                PreferredBiome = Terraria.ID.TileID.Sand,
                FavoriteFoodItemType = Terraria.ID.ItemID.IronOre,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "铁壁", Description = "被动大幅提升主人防御。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "龟甲守护", Description = "为主人附加无敌护盾。", RequiredStage = 3, CooldownSeconds = 60f, QiCost = 80f, Type = AbilityType.ActiveDefense },
                    new SpiritBeastAbility { Name = "嘲讽", Description = "吸引周围敌人仇恨。", RequiredStage = 2, CooldownSeconds = 20f, QiCost = 30f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.CrystalButterfly] = new SpiritBeastData
            {
                Type = SpiritBeastType.CrystalButterfly,
                Name = "晶蝶",
                Description = "美丽的晶蝶，擅长阵法辅助。",
                BaseHP = 70, BaseDamage = 6, BaseDefense = 3,
                GrowthRate = 1.6f,
                PreferredBiome = Terraria.ID.TileID.HallowedGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.CrystalShard,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "阵感", Description = "被动提升阵法效果。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "晶尘", Description = "撒播晶尘削弱敌人。", RequiredStage = 2, CooldownSeconds = 15f, QiCost = 25f, Type = AbilityType.ActiveUtility },
                    new SpiritBeastAbility { Name = "晶化领域", Description = "在周围制造晶化领域。", RequiredStage = 4, CooldownSeconds = 45f, QiCost = 60f, Type = AbilityType.ActiveDefense },
                }
            };

            BeastRegistry[SpiritBeastType.VoidCat] = new SpiritBeastData
            {
                Type = SpiritBeastType.VoidCat,
                Name = "虚空猫",
                Description = "神秘的虚空猫，擅长空间能力。",
                BaseHP = 100, BaseDamage = 22, BaseDefense = 10,
                GrowthRate = 1.35f,
                PreferredBiome = Terraria.ID.TileID.Obsidian,
                FavoriteFoodItemType = Terraria.ID.ItemID.SoulofNight,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "虚空感知", Description = "被动提升真元恢复速度。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "虚空撕裂", Description = "撕裂空间攻击敌人。", RequiredStage = 2, CooldownSeconds = 12f, QiCost = 35f, Type = AbilityType.ActiveAttack },
                    new SpiritBeastAbility { Name = "虚空传送", Description = "将主人传送到鼠标位置。", RequiredStage = 4, CooldownSeconds = 90f, QiCost = 100f, Type = AbilityType.ActiveUtility },
                }
            };

            BeastRegistry[SpiritBeastType.DreamTapir] = new SpiritBeastData
            {
                Type = SpiritBeastType.DreamTapir,
                Name = "梦貘",
                Description = "梦幻的梦貘，擅长精神攻击和辅助。",
                BaseHP = 140, BaseDamage = 16, BaseDefense = 12,
                GrowthRate = 1.15f,
                PreferredBiome = Terraria.ID.TileID.HallowedGrass,
                FavoriteFoodItemType = Terraria.ID.ItemID.SoulofLight,
                Abilities = new List<SpiritBeastAbility>
                {
                    new SpiritBeastAbility { Name = "梦境庇护", Description = "被动提升精神力抗性。", RequiredStage = 0, Type = AbilityType.Passive },
                    new SpiritBeastAbility { Name = "梦境侵蚀", Description = "使敌人陷入混乱。", RequiredStage = 2, CooldownSeconds = 20f, QiCost = 30f, Type = AbilityType.ActiveUtility },
                    new SpiritBeastAbility { Name = "梦境治愈", Description = "在梦境中为主人回复。", RequiredStage = 3, CooldownSeconds = 25f, QiCost = 45f, Type = AbilityType.ActiveHeal },
                }
            };
        }

        public SpiritBeastData GetBeastData(SpiritBeastType type)
        {
            BeastRegistry.TryGetValue(type, out var data);
            return data;
        }

        public SpiritBeastInstance CreateBeast(SpiritBeastType type)
        {
            var data = GetBeastData(type);
            if (data == null) return null;

            return new SpiritBeastInstance
            {
                Type = type,
                CustomName = data.Name,
                Stage = SpiritBeastStage.Egg,
                Mood = SpiritBeastMood.Neutral,
                HP = data.BaseHP,
                MaxHP = data.BaseHP,
                Damage = data.BaseDamage,
                Defense = data.BaseDefense,
                GrowthProgress = 0,
                RequiredGrowth = GetGrowthRequirement(SpiritBeastStage.Egg),
                BondLevel = 0,
                BondExp = 0,
                HungerLevel = 100,
                MaxHunger = 100,
                IsSummoned = false,
            };
        }

        private int GetGrowthRequirement(SpiritBeastStage stage)
        {
            return stage switch
            {
                SpiritBeastStage.Egg => 3600,
                SpiritBeastStage.Hatchling => 7200,
                SpiritBeastStage.Juvenile => 14400,
                SpiritBeastStage.Adult => 28800,
                SpiritBeastStage.Elder => 57600,
                SpiritBeastStage.Ancient => 115200,
                _ => 3600,
            };
        }

        public void FeedBeast(SpiritBeastInstance beast, int foodItemType)
        {
            var data = GetBeastData(beast.Type);
            if (data == null) return;

            beast.HungerLevel = System.Math.Min(beast.MaxHunger, beast.HungerLevel + 25);

            if (foodItemType == data.FavoriteFoodItemType)
            {
                beast.BondExp += 10;
                beast.GrowthProgress += 60;
            }
            else
            {
                beast.BondExp += 3;
                beast.GrowthProgress += 30;
            }

            CheckBondLevelUp(beast);
            CheckGrowth(beast);
        }

        public void UpdateBeast(SpiritBeastInstance beast, SpiritBeastData data)
        {
            beast.HungerLevel = System.Math.Max(0, beast.HungerLevel - 1);

            if (beast.HungerLevel <= 0)
            {
                beast.Mood = SpiritBeastMood.Hostile;
            }
            else if (beast.HungerLevel < 30)
            {
                beast.Mood = SpiritBeastMood.Wary;
            }
            else if (beast.BondLevel >= 5)
            {
                beast.Mood = beast.BondLevel >= 10 ? SpiritBeastMood.Devoted : SpiritBeastMood.Loyal;
            }
            else if (beast.BondLevel >= 3)
            {
                beast.Mood = SpiritBeastMood.Friendly;
            }

            if (beast.Mood >= SpiritBeastMood.Friendly)
            {
                beast.GrowthProgress += (int)(data.GrowthRate * 0.5f);
            }
        }

        private void CheckBondLevelUp(SpiritBeastInstance beast)
        {
            int[] bondThresholds = { 50, 150, 300, 500, 800, 1200, 1800, 2500, 3500, 5000 };

            while (beast.BondLevel < 10 && beast.BondExp >= bondThresholds[beast.BondLevel])
            {
                beast.BondExp -= bondThresholds[beast.BondLevel];
                beast.BondLevel++;
            }
        }

        private void CheckGrowth(SpiritBeastInstance beast)
        {
            if (beast.Stage >= SpiritBeastStage.Ancient) return;

            if (beast.GrowthProgress >= beast.RequiredGrowth)
            {
                beast.GrowthProgress -= beast.RequiredGrowth;
                beast.Stage = (SpiritBeastStage)((int)beast.Stage + 1);
                beast.RequiredGrowth = GetGrowthRequirement(beast.Stage);

                var data = GetBeastData(beast.Type);
                if (data != null)
                {
                    beast.MaxHP = data.BaseHP * (1 + (int)beast.Stage);
                    beast.HP = beast.MaxHP;
                    beast.Damage = data.BaseDamage + (int)beast.Stage * 5;
                    beast.Defense = data.BaseDefense + (int)beast.Stage * 3;

                    foreach (var ability in data.Abilities)
                    {
                        if ((int)beast.Stage >= ability.RequiredStage &&
                            !beast.UnlockedAbilities.Contains(ability.Name))
                        {
                            beast.UnlockedAbilities.Add(ability.Name);
                        }
                    }
                }
            }
        }

        public float GetBeastStatBonus(SpiritBeastInstance beast, AbilityType abilityType)
        {
            if (beast == null) return 0f;

            float baseBonus = (int)beast.Stage * 0.05f;
            float bondBonus = beast.BondLevel * 0.02f;

            return abilityType switch
            {
                AbilityType.Passive => baseBonus + bondBonus,
                _ => baseBonus,
            };
        }
    }

    public class SpiritBeastPlayer : ModPlayer
    {
        public List<SpiritBeastInstance> OwnedBeasts = new();
        public int MaxBeasts = 1;
        public int ActiveBeastIndex = -1;

        public SpiritBeastInstance ActiveBeast
        {
            get
            {
                if (ActiveBeastIndex >= 0 && ActiveBeastIndex < OwnedBeasts.Count)
                    return OwnedBeasts[ActiveBeastIndex];
                return null;
            }
        }

        public bool CanTameBeast()
        {
            return OwnedBeasts.Count < MaxBeasts;
        }

        public void TameBeast(SpiritBeastType type)
        {
            if (!CanTameBeast()) return;

            var beast = SpiritBeastSystem.Instance.CreateBeast(type);
            if (beast == null) return;

            OwnedBeasts.Add(beast);
            if (ActiveBeastIndex < 0)
                ActiveBeastIndex = 0;
        }

        public void ReleaseBeast(int index)
        {
            if (index < 0 || index >= OwnedBeasts.Count) return;
            OwnedBeasts.RemoveAt(index);
            if (ActiveBeastIndex >= OwnedBeasts.Count)
                ActiveBeastIndex = OwnedBeasts.Count - 1;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["maxBeasts"] = MaxBeasts;
            tag["activeBeastIndex"] = ActiveBeastIndex;

            var beastList = new List<TagCompound>();
            foreach (var beast in OwnedBeasts)
            {
                beastList.Add(new TagCompound
                {
                    ["type"] = (int)beast.Type,
                    ["customName"] = beast.CustomName,
                    ["stage"] = (int)beast.Stage,
                    ["mood"] = (int)beast.Mood,
                    ["hp"] = beast.HP,
                    ["maxHP"] = beast.MaxHP,
                    ["damage"] = beast.Damage,
                    ["defense"] = beast.Defense,
                    ["growthProgress"] = beast.GrowthProgress,
                    ["requiredGrowth"] = beast.RequiredGrowth,
                    ["bondLevel"] = beast.BondLevel,
                    ["bondExp"] = beast.BondExp,
                    ["hungerLevel"] = beast.HungerLevel,
                    ["abilities"] = new List<string>(beast.UnlockedAbilities),
                });
            }
            tag["ownedBeasts"] = beastList;
        }

        public override void LoadData(TagCompound tag)
        {
            OwnedBeasts.Clear();
            MaxBeasts = tag.GetInt("maxBeasts");
            ActiveBeastIndex = tag.GetInt("activeBeastIndex");

            var beastList = tag.GetList<TagCompound>("ownedBeasts");
            if (beastList != null)
            {
                foreach (var t in beastList)
                {
                    var beast = new SpiritBeastInstance
                    {
                        Type = (SpiritBeastType)t.GetInt("type"),
                        CustomName = t.GetString("customName"),
                        Stage = (SpiritBeastStage)t.GetInt("stage"),
                        Mood = (SpiritBeastMood)t.GetInt("mood"),
                        HP = t.GetInt("hp"),
                        MaxHP = t.GetInt("maxHP"),
                        Damage = t.GetInt("damage"),
                        Defense = t.GetInt("defense"),
                        GrowthProgress = t.GetInt("growthProgress"),
                        RequiredGrowth = t.GetInt("requiredGrowth"),
                        BondLevel = t.GetInt("bondLevel"),
                        BondExp = t.GetInt("bondExp"),
                        HungerLevel = t.GetInt("hungerLevel"),
                        MaxHunger = 100,
                    };

                    var abilities = t.GetList<string>("abilities");
                    if (abilities != null)
                        beast.UnlockedAbilities = new List<string>(abilities);

                    OwnedBeasts.Add(beast);
                }
            }
        }
    }
}