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
            if (source.KnownRumors.Contains(rumorID)) return;
            source.KnownRumors.Add(rumorID);

            int spreadCount = 0;
            foreach (var kvp in MortalStates)
            {
                var target = kvp.Value;
                if (target == source || !target.IsAlive) continue;
                if (target.KnownRumors.Contains(rumorID)) continue;

                float spreadChance = 0.1f;
                if (target.Occupation == MortalOccupation.Storyteller)
                    spreadChance = 0.5f;
                if (target.Occupation == MortalOccupation.Merchant)
                    spreadChance = 0.3f;
                if (target.Occupation == MortalOccupation.Beggar)
                    spreadChance = 0.25f;

                if (Main.rand.NextFloat() < spreadChance)
                {
                    target.KnownRumors.Add(rumorID);
                    spreadCount++;
                }
            }

            if (spreadCount > 0 && Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                Main.NewText($"谣言「{rumorID}」在凡人中传播开来...", Microsoft.Xna.Framework.Color.DarkGray);
            }
        }

        public bool HasRumorSpread(string rumorID)
        {
            int count = 0;
            foreach (var kvp in MortalStates)
            {
                if (kvp.Value.KnownRumors.Contains(rumorID))
                    count++;
            }
            return count >= 3;
        }

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            var toRemove = new List<int>();

            foreach (var kvp in MortalStates)
            {
                var mortal = kvp.Value;
                if (!mortal.IsAlive) continue;

                mortal.DaysAlive++;
                UpdateMortalNeeds(mortal);

                if (mortal.IsStarving && mortal.Needs[MortalNeed.Food] < 0.1f)
                {
                    mortal.Happiness -= 0.1f;
                    if (Main.rand.NextFloat() < 0.05f)
                    {
                        mortal.IsAlive = false;
                        toRemove.Add(kvp.Key);
                        continue;
                    }
                }

                if (mortal.IsSick && Main.rand.NextFloat() < 0.02f)
                {
                    mortal.IsAlive = false;
                    toRemove.Add(kvp.Key);
                    continue;
                }

                if (Main.rand.NextFloat() < 0.01f)
                {
                    mortal.IsSick = true;
                }

                if (mortal.Happiness < 0.2f && Main.rand.NextFloat() < 0.03f)
                {
                    mortal.IsAlive = false;
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
                MortalStates.Remove(id);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var kvp in MortalStates)
            {
                var m = kvp.Value;
                var needsTag = new TagCompound();
                foreach (var n in m.Needs)
                    needsTag[n.Key.ToString()] = n.Value;

                list.Add(new TagCompound
                {
                    ["npcID"] = m.NPCID,
                    ["occupation"] = (int)m.Occupation,
                    ["wealth"] = m.WealthLevel,
                    ["happiness"] = m.Happiness,
                    ["fear"] = m.FearOfGuMaster,
                    ["loyalty"] = m.LoyaltyToFaction,
                    ["isSick"] = m.IsSick,
                    ["isStarving"] = m.IsStarving,
                    ["daysAlive"] = m.DaysAlive,
                    ["isAlive"] = m.IsAlive,
                    ["needs"] = needsTag,
                });
            }
            tag["mortals"] = list;
            tag["mortalDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            MortalStates.Clear();
            var list = tag.GetList<TagCompound>("mortals");
            if (list == null) return;

            foreach (var t in list)
            {
                var mortal = new MortalState
                {
                    NPCID = t.GetInt("npcID"),
                    Occupation = (MortalOccupation)t.GetInt("occupation"),
                    WealthLevel = t.GetInt("wealth"),
                    Happiness = t.GetFloat("happiness"),
                    FearOfGuMaster = t.GetFloat("fear"),
                    LoyaltyToFaction = t.GetFloat("loyalty"),
                    IsSick = t.GetBool("isSick"),
                    IsStarving = t.GetBool("isStarving"),
                    DaysAlive = t.GetInt("daysAlive"),
                    IsAlive = t.GetBool("isAlive"),
                };

                if (t.TryGet("needs", out TagCompound needsTag))
                {
                    foreach (MortalNeed need in System.Enum.GetValues<MortalNeed>())
                    {
                        mortal.Needs[need] = needsTag.GetFloat(need.ToString());
                    }
                }

                MortalStates[mortal.NPCID] = mortal;
            }

            _lastDay = tag.GetInt("mortalDayCounter");
        }
    }
}
