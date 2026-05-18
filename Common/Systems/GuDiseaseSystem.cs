using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum GuDiseaseType
    {
        None = 0,
        PoisonGu,           // 毒蛊侵蚀 — 持续扣血
        ParasiteGu,         // 寄生蛊 — 吸取真元
        MindControlGu,      // 控心蛊 — 混乱/叛变
        BloodDevourGu,      // 噬血蛊 — 吸血/虚弱
        BoneCorrodeGu,      // 蚀骨蛊 — 防御降低
        SoulDrainGu,        // 夺魂蛊 — 精神伤害
        Plague,             // 瘟疫 — 传染性，影响区域
        GuFever,            // 蛊热 — 蛊虫失控引起
    }

    public enum DiseaseSeverity
    {
        Mild = 0,           // 轻微 — 小幅减益
        Moderate = 1,       // 中度 — 明显减益
        Severe = 2,         // 严重 — 大幅减益
        Critical = 3,       // 危急 — 致命
    }

    /// <summary>
    /// 蛊病实例 — 记录一次蛊病发作的完整状态
    /// </summary>
    public class GuDiseaseInstance
    {
        /// <summary>蛊病类型</summary>
        public GuDiseaseType Type;

        /// <summary>严重程度</summary>
        public DiseaseSeverity Severity;

        /// <summary>剩余持续帧数</summary>
        public int RemainingTicks;

        /// <summary>总持续帧数</summary>
        public int TotalTicks;

        /// <summary>强度 [0, 1]，影响严重程度和伤害</summary>
        public float Intensity;

        /// <summary>施加者的玩家ID（-1表示环境/瘟疫区）</summary>
        public int SourcePlayerID;

        /// <summary>是否具有传染性</summary>
        public bool IsContagious;

        /// <summary>传染范围（像素）</summary>
        public float ContagionRange;

        /// <summary>病情进度 [0, 1]</summary>
        public float Progress => TotalTicks > 0 ? 1f - (float)RemainingTicks / TotalTicks : 0f;

        /// <summary>是否已过期（自然痊愈）</summary>
        public bool IsExpired => RemainingTicks <= 0;
    }

    /// <summary>
    /// 瘟疫区域 — 由瘟疫类蛊病产生的持续影响区域
    /// </summary>
    public class PlagueZone
    {
        /// <summary>瘟疫中心坐标</summary>
        public Vector2 Center;

        /// <summary>影响半径（像素）</summary>
        public float Radius;

        /// <summary>瘟疫类型</summary>
        public GuDiseaseType DiseaseType;

        /// <summary>剩余持续帧数</summary>
        public int RemainingTicks;

        /// <summary>来源家族ID（0=无来源）</summary>
        public int SourceFactionID;

        /// <summary>瘟疫强度</summary>
        public float Intensity;
    }

    /// <summary>
    /// 蛊病系统 — 管理蛊病施加、持续伤害、传染和瘟疫区域
    /// 
    /// 蛊病是蛊师对敌施加的持续性负面状态，包括：
    /// - 毒蛊侵蚀（持续扣血）、寄生蛊（吸取真元）、控心蛊（混乱）
    /// - 噬血蛊（吸血/虚弱）、蚀骨蛊（防御降低）、夺魂蛊（精神伤害）
    /// - 瘟疫（传染性区域效果）、蛊热（蛊虫失控引起）
    /// </summary>
    public class GuDiseaseSystem : ModSystem
    {
        public static GuDiseaseSystem Instance => ModContent.GetInstance<GuDiseaseSystem>();

        public List<PlagueZone> ActivePlagueZones = new();

        public static DiseaseSeverity CalculateSeverity(float intensity)
        {
            if (intensity >= 0.8f) return DiseaseSeverity.Critical;
            if (intensity >= 0.6f) return DiseaseSeverity.Severe;
            if (intensity >= 0.3f) return DiseaseSeverity.Moderate;
            return DiseaseSeverity.Mild;
        }

        public static float GetDiseaseDamage(GuDiseaseType type, DiseaseSeverity severity)
        {
            float baseDmg = type switch
            {
                GuDiseaseType.PoisonGu => 5f,
                GuDiseaseType.BloodDevourGu => 3f,
                GuDiseaseType.BoneCorrodeGu => 2f,
                GuDiseaseType.SoulDrainGu => 4f,
                GuDiseaseType.Plague => 3f,
                GuDiseaseType.GuFever => 6f,
                _ => 0f,
            };
            return baseDmg * (1f + (int)severity * 0.5f);
        }

        public static float GetQiDrain(GuDiseaseType type, DiseaseSeverity severity)
        {
            float baseDrain = type switch
            {
                GuDiseaseType.ParasiteGu => 2f,
                GuDiseaseType.BloodDevourGu => 1f,
                GuDiseaseType.SoulDrainGu => 3f,
                _ => 0f,
            };
            return baseDrain * (1f + (int)severity * 0.3f);
        }

        public static int GetCureCost(GuDiseaseType type, DiseaseSeverity severity)
        {
            int baseCost = type switch
            {
                GuDiseaseType.PoisonGu => 10,
                GuDiseaseType.ParasiteGu => 20,
                GuDiseaseType.MindControlGu => 50,
                GuDiseaseType.BloodDevourGu => 15,
                GuDiseaseType.BoneCorrodeGu => 15,
                GuDiseaseType.SoulDrainGu => 40,
                GuDiseaseType.Plague => 30,
                GuDiseaseType.GuFever => 25,
                _ => 5,
            };
            return baseCost * (1 + (int)severity);
        }

        public void CreatePlagueZone(Vector2 center, float radius, GuDiseaseType type, int duration, int sourceFaction = 0)
        {
            ActivePlagueZones.Add(new PlagueZone
            {
                Center = center,
                Radius = radius,
                DiseaseType = type,
                RemainingTicks = duration,
                SourceFactionID = sourceFaction,
                Intensity = 0.5f,
            });
        }

        public bool IsInPlagueZone(Vector2 position, out PlagueZone zone)
        {
            foreach (var pz in ActivePlagueZones)
            {
                if (Vector2.Distance(position, pz.Center) <= pz.Radius)
                {
                    zone = pz;
                    return true;
                }
            }
            zone = null;
            return false;
        }

        public override void PostUpdateWorld()
        {
            for (int i = ActivePlagueZones.Count - 1; i >= 0; i--)
            {
                ActivePlagueZones[i].RemainingTicks--;
                if (ActivePlagueZones[i].RemainingTicks <= 0)
                    ActivePlagueZones.RemoveAt(i);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var zone in ActivePlagueZones)
            {
                list.Add(new TagCompound
                {
                    ["centerX"] = zone.Center.X,
                    ["centerY"] = zone.Center.Y,
                    ["radius"] = zone.Radius,
                    ["type"] = (int)zone.DiseaseType,
                    ["remaining"] = zone.RemainingTicks,
                    ["faction"] = zone.SourceFactionID,
                    ["intensity"] = zone.Intensity,
                });
            }
            tag["plagueZones"] = list;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActivePlagueZones.Clear();
            var list = tag.GetList<TagCompound>("plagueZones");
            if (list == null) return;

            foreach (var t in list)
            {
                ActivePlagueZones.Add(new PlagueZone
                {
                    Center = new Vector2(t.GetFloat("centerX"), t.GetFloat("centerY")),
                    Radius = t.GetFloat("radius"),
                    DiseaseType = (GuDiseaseType)t.GetInt("type"),
                    RemainingTicks = t.GetInt("remaining"),
                    SourceFactionID = t.GetInt("faction"),
                    Intensity = t.GetFloat("intensity"),
                });
            }
        }
    }

    public class GuDiseasePlayer : ModPlayer
    {
        public List<GuDiseaseInstance> ActiveDiseases = new();

        public bool HasDisease(GuDiseaseType type)
        {
            return ActiveDiseases.Exists(d => d.Type == type);
        }

        public GuDiseaseInstance GetDisease(GuDiseaseType type)
        {
            return ActiveDiseases.Find(d => d.Type == type);
        }

        public void Infect(GuDiseaseType type, int duration, float intensity, int sourcePlayerID = -1)
        {
            var existing = GetDisease(type);
            if (existing != null)
            {
                existing.Intensity = System.Math.Max(existing.Intensity, intensity);
                existing.RemainingTicks = System.Math.Max(existing.RemainingTicks, duration);
                return;
            }

            ActiveDiseases.Add(new GuDiseaseInstance
            {
                Type = type,
                Severity = GuDiseaseSystem.CalculateSeverity(intensity),
                RemainingTicks = duration,
                TotalTicks = duration,
                Intensity = intensity,
                SourcePlayerID = sourcePlayerID,
                IsContagious = type == GuDiseaseType.Plague,
                ContagionRange = type == GuDiseaseType.Plague ? 200f : 0f,
            });
        }

        public void Cure(GuDiseaseType type)
        {
            ActiveDiseases.RemoveAll(d => d.Type == type);
        }

        public void CureAll()
        {
            ActiveDiseases.Clear();
        }

        public override void ResetEffects()
        {
            for (int i = ActiveDiseases.Count - 1; i >= 0; i--)
            {
                var disease = ActiveDiseases[i];
                disease.RemainingTicks--;

                if (disease.IsExpired)
                {
                    ActiveDiseases.RemoveAt(i);
                    continue;
                }

                ApplyDiseaseEffect(disease);
            }

            if (GuDiseaseSystem.Instance.IsInPlagueZone(Player.Center, out var zone))
            {
                if (!HasDisease(zone.DiseaseType))
                    Infect(zone.DiseaseType, 600, zone.Intensity);
            }
        }

        private void ApplyDiseaseEffect(GuDiseaseInstance disease)
        {
            float dmg = GuDiseaseSystem.GetDiseaseDamage(disease.Type, disease.Severity);
            float qiDrain = GuDiseaseSystem.GetQiDrain(disease.Type, disease.Severity);

            if (dmg > 0 && Main.rand.NextFloat() < 0.1f)
            {
                Player.statLife -= (int)dmg;
                if (Player.statLife <= 0)
                    Player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(
                        NetworkText.FromLiteral($"{Player.name}被蛊病夺去了生命")), dmg, 0);
            }

            if (qiDrain > 0)
            {
                var qiResource = Player.GetModPlayer<QiResourcePlayer>();
                qiResource.QiCurrent = System.Math.Max(0, qiResource.QiCurrent - qiDrain * 0.1f);
            }

            switch (disease.Type)
            {
                case GuDiseaseType.PoisonGu:
                    Player.AddBuff(Terraria.ID.BuffID.Poisoned, 60);
                    break;
                case GuDiseaseType.BoneCorrodeGu:
                    Player.AddBuff(Terraria.ID.BuffID.BrokenArmor, 60);
                    break;
                case GuDiseaseType.BloodDevourGu:
                    Player.AddBuff(Terraria.ID.BuffID.Bleeding, 60);
                    break;
                case GuDiseaseType.MindControlGu:
                    if (Main.rand.NextFloat() < 0.02f)
                        Player.AddBuff(Terraria.ID.BuffID.Confused, 120);
                    break;
                case GuDiseaseType.SoulDrainGu:
                    Player.AddBuff(Terraria.ID.BuffID.Slow, 60);
                    break;
                case GuDiseaseType.GuFever:
                    Player.AddBuff(Terraria.ID.BuffID.OnFire, 60);
                    break;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var disease in ActiveDiseases)
            {
                list.Add(new TagCompound
                {
                    ["type"] = (int)disease.Type,
                    ["severity"] = (int)disease.Severity,
                    ["remaining"] = disease.RemainingTicks,
                    ["total"] = disease.TotalTicks,
                    ["intensity"] = disease.Intensity,
                    ["source"] = disease.SourcePlayerID,
                });
            }
            tag["diseases"] = list;
        }

        public override void LoadData(TagCompound tag)
        {
            ActiveDiseases.Clear();
            var list = tag.GetList<TagCompound>("diseases");
            if (list == null) return;

            foreach (var t in list)
            {
                ActiveDiseases.Add(new GuDiseaseInstance
                {
                    Type = (GuDiseaseType)t.GetInt("type"),
                    Severity = (DiseaseSeverity)t.GetInt("severity"),
                    RemainingTicks = t.GetInt("remaining"),
                    TotalTicks = t.GetInt("total"),
                    Intensity = t.GetFloat("intensity"),
                    SourcePlayerID = t.GetInt("source"),
                });
            }
        }
    }
}
