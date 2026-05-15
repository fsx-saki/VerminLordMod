using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
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

    public class GuDiseaseInstance
    {
        public GuDiseaseType Type;
        public DiseaseSeverity Severity;
        public int RemainingTicks;
        public int TotalTicks;
        public float Intensity;
        public int SourcePlayerID;
        public bool IsContagious;
        public float ContagionRange;
        public float Progress => TotalTicks > 0 ? 1f - (float)RemainingTicks / TotalTicks : 0f;
        public bool IsExpired => RemainingTicks <= 0;
    }

    public class PlagueZone
    {
        public Vector2 Center;
        public float Radius;
        public GuDiseaseType DiseaseType;
        public int RemainingTicks;
        public int SourceFactionID;
        public float Intensity;
    }

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
            // TODO: 保存瘟疫区域数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载瘟疫区域数据
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

                // TODO: 应用疾病效果
            }

            // 检查瘟疫区域
            if (GuDiseaseSystem.Instance.IsInPlagueZone(Player.Center, out var zone))
            {
                if (!HasDisease(zone.DiseaseType))
                    Infect(zone.DiseaseType, 600, zone.Intensity);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            // TODO: 保存疾病数据
        }

        public override void LoadData(TagCompound tag)
        {
            ActiveDiseases.Clear();
        }
    }
}
