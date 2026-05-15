using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum MainGuState
    {
        None = 0,           // 无本命蛊
        Bonded,             // 已结契（灵魂绑定）
        Growing,            // 成长期（随主人修为成长）
        Mature,             // 成熟期（完全觉醒）
        Transcendent,       // 超越期（与主人合一）
        Severed,            // 契约断裂（主人死亡/主动解除）
    }

    public enum MainGuType
    {
        None = 0,
        SpringAutumnCicada,     // 春秋蝉 — 逆转时空（死亡回溯）
        StrengthGu,             // 力道蛊 — 纯粹力量增幅
        WisdomGu,               // 智慧蛊 — 感知/洞察增强
        ShadowGu,               // 影蛊 — 潜行/暗杀
        LifeGu,                 // 生命蛊 — 治愈/再生
        BloodGu,                // 血蛊 — 吸血/血道
        SoulGu,                 // 魂蛊 — 灵魂操控
        FortuneGu,              // 运蛊 — 幸运/概率操控
        TimeGu,                 // 时间蛊 — 时间操控
        SpaceGu,                // 空间蛊 — 空间操控
        DestinyGu,              // 命运蛊 — 命运干涉
    }

    public class MainGuBond
    {
        public MainGuType Type;
        public MainGuState State;
        public int BondLevel;
        public float BondProgress;
        public float SyncRate;
        public int GuItemID;
        public int DaysBonded;
        public bool IsSoulLinked => State >= MainGuState.Bonded;
        public float DeathProtectionChance => State switch
        {
            MainGuState.Mature => 0.5f,
            MainGuState.Transcendent => 1.0f,
            _ => 0f,
        };
    }

    public class GuSoulSystem : ModSystem
    {
        public static GuSoulSystem Instance => ModContent.GetInstance<GuSoulSystem>();

        public const int MAX_BOND_LEVEL = 10;
        public const float BOND_PROGRESS_PER_DAY = 0.5f;
        public const float SYNC_RATE_BASE = 0.3f;
        public const float SYNC_RATE_MAX = 1.0f;

        private int _dayCounter = 0;

        public override void PostUpdateWorld()
        {
            int currentDay = (int)(Main.GameUpdateCount / 36000);
            if (currentDay <= _dayCounter) return;
            _dayCounter = currentDay;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active) continue;

                var soulPlayer = player.GetModPlayer<GuSoulPlayer>();
                if (!soulPlayer.HasMainGu) continue;

                soulPlayer.MainGu.DaysBonded++;
                soulPlayer.MainGu.BondProgress += BOND_PROGRESS_PER_DAY;

                if (soulPlayer.MainGu.BondProgress >= MAX_BOND_LEVEL * BOND_PROGRESS_PER_DAY)
                {
                    soulPlayer.MainGu.BondProgress = 0f;
                    if (soulPlayer.MainGu.BondLevel < MAX_BOND_LEVEL)
                    {
                        soulPlayer.MainGu.BondLevel++;
                        soulPlayer.MainGu.SyncRate = System.Math.Min(SYNC_RATE_MAX,
                            SYNC_RATE_BASE + soulPlayer.MainGu.BondLevel * 0.07f);
                    }
                }

                UpdateMainGuState(soulPlayer.MainGu);
            }
        }

        private void UpdateMainGuState(MainGuBond bond)
        {
            if (bond.BondLevel >= 8 && bond.SyncRate >= 0.9f)
                bond.State = MainGuState.Transcendent;
            else if (bond.BondLevel >= 5 && bond.SyncRate >= 0.7f)
                bond.State = MainGuState.Mature;
            else if (bond.BondLevel >= 3)
                bond.State = MainGuState.Growing;
        }

        public static MainGuType GetMainGuType(int itemType)
        {
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Six.ChunQiuChan>())
                return MainGuType.SpringAutumnCicada;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Two.BigStrengthGu>())
                return MainGuType.StrengthGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Two.BigSoulGu>())
                return MainGuType.SoulGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Three.BloodMoonGu>())
                return MainGuType.BloodGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Three.EternalLifeGu>())
                return MainGuType.LifeGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Four.BloodSkullGu>())
                return MainGuType.BloodGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Five.BloodHandprintGu>())
                return MainGuType.BloodGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Five.TaiGuangGu>())
                return MainGuType.TimeGu;
            if (itemType == ModContent.ItemType<Content.Items.Weapons.Five.TianDiHongYinGu>())
                return MainGuType.SpaceGu;
            return MainGuType.None;
        }

        public static float GetBondLevelBonus(int bondLevel)
        {
            return 1f + bondLevel * 0.1f;
        }

        public static float GetSyncRateBonus(float syncRate)
        {
            return syncRate * 0.5f;
        }

        public static int GetQiCostForBonding(MainGuType type)
        {
            return type switch
            {
                MainGuType.SpringAutumnCicada => 500,
                MainGuType.TimeGu => 800,
                MainGuType.SpaceGu => 800,
                MainGuType.DestinyGu => 1000,
                _ => 200,
            };
        }

        public static int GetLoyaltyThreshold(MainGuState state)
        {
            return state switch
            {
                MainGuState.Bonded => 60,
                MainGuState.Growing => 70,
                MainGuState.Mature => 80,
                MainGuState.Transcendent => 95,
                _ => 50,
            };
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["dayCounter"] = _dayCounter;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _dayCounter = tag.GetInt("dayCounter");
        }
    }

    public class GuSoulPlayer : ModPlayer
    {
        public MainGuBond MainGu = null;

        public bool HasMainGu => MainGu != null && MainGu.State != MainGuState.None;
        public bool IsSoulLinked => MainGu != null && MainGu.IsSoulLinked;

        public override void ResetEffects()
        {
            if (!HasMainGu) return;

            float bondBonus = GuSoulSystem.GetBondLevelBonus(MainGu.BondLevel);
            float syncBonus = GuSoulSystem.GetSyncRateBonus(MainGu.SyncRate);
            float totalMult = bondBonus + syncBonus;

            switch (MainGu.Type)
            {
                case MainGuType.StrengthGu:
                    Player.GetDamage(DamageClass.Generic) += 0.1f * totalMult;
                    Player.GetAttackSpeed(DamageClass.Generic) += 0.05f * totalMult;
                    break;
                case MainGuType.WisdomGu:
                    Player.detectCreature = true;
                    Player.GetCritChance(DamageClass.Generic) += 5f * totalMult;
                    break;
                case MainGuType.ShadowGu:
                    Player.aggro -= (int)(400 * totalMult);
                    Player.moveSpeed += 0.1f * totalMult;
                    break;
                case MainGuType.LifeGu:
                    Player.lifeRegen += (int)(2 * totalMult);
                    Player.statLifeMax2 += (int)(20 * totalMult);
                    break;
                case MainGuType.BloodGu:
                    Player.GetDamage(DamageClass.Generic) += 0.05f * totalMult;
                    break;
                case MainGuType.FortuneGu:
                    Player.luck += 0.05f * totalMult;
                    break;
                case MainGuType.SoulGu:
                    Player.maxMinions += (int)(1 * totalMult);
                    break;
                case MainGuType.SpringAutumnCicada:
                    if (MainGu.State >= MainGuState.Mature && Main.rand.NextFloat() < MainGu.DeathProtectionChance)
                    {
                        Player.SetImmuneTimeForAllTypes(60);
                    }
                    break;
            }
        }

        public void BondWithGu(int itemType, MainGuType type)
        {
            if (HasMainGu) return;

            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            int cost = GuSoulSystem.GetQiCostForBonding(type);

            MainGu = new MainGuBond
            {
                Type = type,
                State = MainGuState.Bonded,
                BondLevel = 1,
                BondProgress = 0f,
                SyncRate = GuSoulSystem.SYNC_RATE_BASE,
                GuItemID = itemType,
                DaysBonded = 0,
            };

            EventBus.Publish(new MainGuBondedEvent
            {
                PlayerID = Player.whoAmI,
                GuType = type,
            });
        }

        public void SeverBond()
        {
            if (!HasMainGu) return;

            var oldType = MainGu.Type;
            MainGu.State = MainGuState.Severed;

            var qiRealm = Player.GetModPlayer<QiRealmPlayer>();
            qiRealm.BreakthroughProgress = System.Math.Max(0, qiRealm.BreakthroughProgress - 30f);

            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiCurrent = System.Math.Max(0, qiResource.QiCurrent - qiResource.QiMaxCurrent / 4);

            Player.AddBuff(Terraria.ID.BuffID.Weak, 3600);
            Player.AddBuff(Terraria.ID.BuffID.BrokenArmor, 3600);

            EventBus.Publish(new MainGuSeveredEvent
            {
                PlayerID = Player.whoAmI,
                GuType = oldType,
            });

            MainGu = null;
        }

        public override void SaveData(TagCompound tag)
        {
            if (MainGu != null)
            {
                tag["hasMainGu"] = true;
                tag["guType"] = (int)MainGu.Type;
                tag["guState"] = (int)MainGu.State;
                tag["bondLevel"] = MainGu.BondLevel;
                tag["bondProgress"] = MainGu.BondProgress;
                tag["syncRate"] = MainGu.SyncRate;
                tag["guItemID"] = MainGu.GuItemID;
                tag["daysBonded"] = MainGu.DaysBonded;
            }
            else
            {
                tag["hasMainGu"] = false;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.GetBool("hasMainGu"))
            {
                MainGu = new MainGuBond
                {
                    Type = (MainGuType)tag.GetInt("guType"),
                    State = (MainGuState)tag.GetInt("guState"),
                    BondLevel = tag.GetInt("bondLevel"),
                    BondProgress = tag.GetFloat("bondProgress"),
                    SyncRate = tag.GetFloat("syncRate"),
                    GuItemID = tag.GetInt("guItemID"),
                    DaysBonded = tag.GetInt("daysBonded"),
                };
            }
            else
            {
                MainGu = null;
            }
        }
    }

    public class MainGuBondedEvent : GuWorldEvent
    {
        public int PlayerID;
        public MainGuType GuType;
    }

    public class MainGuSeveredEvent : GuWorldEvent
    {
        public int PlayerID;
        public MainGuType GuType;
    }
}
