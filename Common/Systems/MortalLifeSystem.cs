using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    public enum MortalOccupation
    {
        Farmer,         // 农夫 — 种植/收获
        Hunter,         // 猎人 — 狩猎/采集
        Fisherman,      // 渔夫 — 捕鱼
        Miner,          // 矿工 — 采矿
        Merchant,       // 商人 — 交易
        Blacksmith,     // 铁匠 — 锻造
        Herbalist,      // 药师 — 采药/制药
        Weaver,         // 织工 — 纺织
        Cook,           // 厨师 — 烹饪
        Scholar,        // 学者 — 读书/记录
        Guard,          // 守卫 — 巡逻/防御
        Servant,        // 仆从 — 杂务
        Beggar,         // 乞丐 — 乞讨/情报
        Storyteller,    // 说书人 — 传播消息
    }

    public enum MortalNeed
    {
        Food,           // 食物
        Shelter,        // 住所
        Safety,         // 安全
        Medicine,       // 医药
        Wealth,         // 财富
        Social,         // 社交
        Knowledge,      // 知识
    }

    public class MortalSchedule
    {
        public int WakeHour;            // 起床时间
        public int WorkStartHour;       // 工作开始
        public int WorkEndHour;         // 工作结束
        public int SleepHour;           // 睡觉时间
        public string WorkLocationID;   // 工作地点
        public string HomeLocationID;   // 家的位置
    }

    public class MortalState
    {
        public int NPCID;
        public MortalOccupation Occupation;
        public Dictionary<MortalNeed, float> Needs = new();
        public MortalSchedule Schedule;
        public int WealthLevel;         // 0-100 财富等级
        public float Happiness;         // 0-1 幸福度
        public float FearOfGuMaster;    // 0-1 对蛊师的恐惧
        public float LoyaltyToFaction;  // 0-1 对所属势力的忠诚
        public bool IsSick;             // 是否生病
        public bool IsStarving;         // 是否饥饿
        public List<string> KnownRumors = new();
        public int DaysAlive;
        public bool IsAlive = true;
    }

    public class MortalLifeSystem : ModSystem
    {
        public static MortalLifeSystem Instance => ModContent.GetInstance<MortalLifeSystem>();

        public Dictionary<int, MortalState> MortalStates = new();

        public override void OnWorldLoad()
        {
            MortalStates.Clear();
        }

        public MortalState GetOrCreateMortal(int npcID, MortalOccupation occupation)
        {
            if (!MortalStates.TryGetValue(npcID, out var state))
            {
                state = new MortalState
                {
                    NPCID = npcID,
                    Occupation = occupation,
                    WealthLevel = GetDefaultWealth(occupation),
                    Happiness = 0.5f,
                    FearOfGuMaster = 0.3f,
                    LoyaltyToFaction = 0.5f,
                    IsSick = false,
                    IsStarving = false,
                    Schedule = GetDefaultSchedule(occupation),
                };

                foreach (MortalNeed need in System.Enum.GetValues<MortalNeed>())
                    state.Needs[need] = 0.5f;

                MortalStates[npcID] = state;
            }
            return state;
        }

        private int GetDefaultWealth(MortalOccupation occupation)
        {
            return occupation switch
            {
                MortalOccupation.Merchant => 60,
                MortalOccupation.Blacksmith => 50,
                MortalOccupation.Herbalist => 40,
                MortalOccupation.Scholar => 35,
                MortalOccupation.Beggar => 5,
                _ => 20,
            };
        }

        private MortalSchedule GetDefaultSchedule(MortalOccupation occupation)
        {
            return new MortalSchedule
            {
                WakeHour = occupation == MortalOccupation.Guard ? 4 : 6,
                WorkStartHour = occupation == MortalOccupation.Guard ? 5 : 7,
                WorkEndHour = occupation == MortalOccupation.Guard ? 17 : 18,
                SleepHour = occupation == MortalOccupation.Guard ? 22 : 21,
            };
        }

        public void UpdateMortalNeeds(MortalState mortal)
        {
            if (!mortal.IsAlive) return;

            mortal.Needs[MortalNeed.Food] -= 0.02f;
            mortal.Needs[MortalNeed.Safety] -= 0.01f;
            mortal.Needs[MortalNeed.Social] -= 0.005f;

            if (mortal.Needs[MortalNeed.Food] < 0.2f)
                mortal.IsStarving = true;

            if (mortal.IsStarving)
                mortal.Happiness -= 0.05f;

            mortal.Happiness = MathHelper.Clamp(mortal.Happiness, 0f, 1f);
            mortal.FearOfGuMaster = MathHelper.Clamp(mortal.FearOfGuMaster, 0f, 1f);
        }

        public void OnGuMasterActionNearby(MortalState mortal, bool isViolent)
        {
            if (isViolent)
            {
                mortal.FearOfGuMaster = MathHelper.Clamp(mortal.FearOfGuMaster + 0.3f, 0f, 1f);
                mortal.Needs[MortalNeed.Safety] -= 0.3f;
            }
            else
            {
                mortal.FearOfGuMaster = MathHelper.Clamp(mortal.FearOfGuMaster - 0.05f, 0f, 1f);
            }
        }

        public void SpreadRumor(MortalState source, string rumorID)
        {
            source.KnownRumors.Add(rumorID);
            // TODO: 通过社交网络传播谣言
        }

        public override void PostUpdateWorld()
        {
            // TODO: 每日更新凡人需求
            // TODO: 凡人生病/治愈
            // TODO: 凡人死亡/新生
            // TODO: 凡人迁移
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存凡人数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载凡人数据
        }
    }
}
