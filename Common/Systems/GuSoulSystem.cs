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

        public override void PostUpdateWorld()
        {
            // TODO: 每日推进所有玩家的本命蛊成长
        }

        public static MainGuType GetMainGuType(int itemType)
        {
            // TODO: 根据物品类型映射本命蛊类型
            return itemType switch
            {
                _ => MainGuType.None,
            };
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
            // TODO: 保存本命蛊数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载本命蛊数据
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

            // TODO: 应用本命蛊被动效果
            switch (MainGu.Type)
            {
                case MainGuType.StrengthGu:
                    // 力量加成
                    break;
                case MainGuType.WisdomGu:
                    // 感知加成
                    break;
                case MainGuType.ShadowGu:
                    // 潜行加成
                    break;
                case MainGuType.LifeGu:
                    // 生命恢复加成
                    break;
                case MainGuType.FortuneGu:
                    // 幸运加成
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

            // TODO: 解契惩罚（修为损失/真元消耗）

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
