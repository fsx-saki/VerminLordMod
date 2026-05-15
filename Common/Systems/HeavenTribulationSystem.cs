using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum TribulationType
    {
        None = 0,
        WindTribulation,        // 风劫 — 一转突破二转
        FireTribulation,        // 火劫 — 二转突破三转
        ThunderTribulation,     // 雷劫 — 三转突破四转
        HeartDemonTribulation,  // 心魔劫 — 四转突破五转
        BloodTribulation,       // 血劫 — 五转突破六转
        SoulTribulation,        // 魂劫 — 六转突破七转
        HeavenEarthTribulation, // 天地劫 — 七转突破八转
        LifeDeathTribulation,   // 生死劫 — 八转突破九转
        GreatDaoTribulation,    // 大道劫 — 九转突破仙人
    }

    public enum TribulationPhase
    {
        None = 0,
        Approaching,    // 天劫将至（预警期，天空变色）
        Gathering,      // 劫云聚集（倒计时，可准备）
        Striking,       // 天劫降临（弹幕/伤害波次）
        FinalBlow,      // 最后一击（最危险，成功即突破）
        Completed,      // 天劫结束
        Failed,         // 天劫失败
    }

    public class TribulationWave
    {
        public int WaveIndex;
        public int ProjectileType;
        public int ProjectileCount;
        public float Damage;
        public int DurationTicks;
        public int ElapsedTicks;
        public bool IsComplete => ElapsedTicks >= DurationTicks;
    }

    public class TribulationInstance
    {
        public TribulationType Type;
        public TribulationPhase Phase;
        public int TargetPlayerID;
        public int CurrentWave;
        public int TotalWaves;
        public List<TribulationWave> Waves = new();
        public float OverallProgress;
        public int PhaseTimer;
        public bool IsComplete => Phase == TribulationPhase.Completed || Phase == TribulationPhase.Failed;
        public Vector2 Center;
        public float Radius;
    }

    public class HeavenTribulationSystem : ModSystem
    {
        public static HeavenTribulationSystem Instance => ModContent.GetInstance<HeavenTribulationSystem>();

        public List<TribulationInstance> ActiveTribulations = new();
        public List<TribulationInstance> CompletedTribulations = new();

        public const int APPROACHING_DURATION = 600;
        public const int GATHERING_DURATION = 300;
        public const int WAVE_INTERVAL = 120;

        public static TribulationType GetTribulationForLevel(int fromLevel)
        {
            return fromLevel switch
            {
                1 => TribulationType.WindTribulation,
                2 => TribulationType.FireTribulation,
                3 => TribulationType.ThunderTribulation,
                4 => TribulationType.HeartDemonTribulation,
                5 => TribulationType.BloodTribulation,
                6 => TribulationType.SoulTribulation,
                7 => TribulationType.HeavenEarthTribulation,
                8 => TribulationType.LifeDeathTribulation,
                9 => TribulationType.GreatDaoTribulation,
                _ => TribulationType.None,
            };
        }

        public static int GetTotalWaves(TribulationType type)
        {
            return type switch
            {
                TribulationType.WindTribulation => 3,
                TribulationType.FireTribulation => 4,
                TribulationType.ThunderTribulation => 5,
                TribulationType.HeartDemonTribulation => 3,
                TribulationType.BloodTribulation => 6,
                TribulationType.SoulTribulation => 5,
                TribulationType.HeavenEarthTribulation => 7,
                TribulationType.LifeDeathTribulation => 8,
                TribulationType.GreatDaoTribulation => 9,
                _ => 0,
            };
        }

        public static float GetTribulationDamage(TribulationType type)
        {
            return type switch
            {
                TribulationType.WindTribulation => 20f,
                TribulationType.FireTribulation => 35f,
                TribulationType.ThunderTribulation => 50f,
                TribulationType.HeartDemonTribulation => 0f,
                TribulationType.BloodTribulation => 80f,
                TribulationType.SoulTribulation => 60f,
                TribulationType.HeavenEarthTribulation => 120f,
                TribulationType.LifeDeathTribulation => 150f,
                TribulationType.GreatDaoTribulation => 200f,
                _ => 0f,
            };
        }

        public bool CanTriggerTribulation(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.BreakthroughProgress < 100f) return false;
            if (qiRealm.LevelStage != 3) return false;
            foreach (var t in ActiveTribulations)
            {
                if (t.TargetPlayerID == player.whoAmI) return false;
            }
            return true;
        }

        public void TriggerTribulation(Player player)
        {
            if (!CanTriggerTribulation(player)) return;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var type = GetTribulationForLevel(qiRealm.GuLevel);

            var instance = new TribulationInstance
            {
                Type = type,
                Phase = TribulationPhase.Approaching,
                TargetPlayerID = player.whoAmI,
                CurrentWave = 0,
                TotalWaves = GetTotalWaves(type),
                PhaseTimer = 0,
                Center = player.Center,
                Radius = 400f,
            };

            for (int i = 0; i < instance.TotalWaves; i++)
            {
                instance.Waves.Add(new TribulationWave
                {
                    WaveIndex = i,
                    Damage = GetTribulationDamage(type) * (1f + i * 0.2f),
                    DurationTicks = 180 + i * 60,
                    ProjectileCount = 5 + i * 3,
                    ProjectileType = GetTribulationProjectileType(type),
                });
            }

            ActiveTribulations.Add(instance);

            EventBus.Publish(new TribulationStartedEvent
            {
                PlayerID = player.whoAmI,
                TribulationType = type,
            });

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText("天劫将至！天地灵气开始剧烈波动……", Color.Gold);
            }
        }

        private int GetTribulationProjectileType(TribulationType type)
        {
            // TODO: 为每种天劫类型创建专属弹幕
            return type switch
            {
                TribulationType.WindTribulation => Terraria.ID.ProjectileID.HarpyFeather,
                TribulationType.FireTribulation => Terraria.ID.ProjectileID.Fireball,
                TribulationType.ThunderTribulation => Terraria.ID.ProjectileID.CultistBossLightningOrbArc,
                _ => Terraria.ID.ProjectileID.CultistBossLightningOrbArc,
            };
        }

        public override void PostUpdateWorld()
        {
            for (int i = ActiveTribulations.Count - 1; i >= 0; i--)
            {
                var trib = ActiveTribulations[i];
                var player = Main.player[trib.TargetPlayerID];

                if (!player.active || player.dead)
                {
                    trib.Phase = TribulationPhase.Failed;
                    CompletedTribulations.Add(trib);
                    ActiveTribulations.RemoveAt(i);
                    continue;
                }

                trib.PhaseTimer++;

                switch (trib.Phase)
                {
                    case TribulationPhase.Approaching:
                        if (trib.PhaseTimer >= APPROACHING_DURATION)
                        {
                            trib.Phase = TribulationPhase.Gathering;
                            trib.PhaseTimer = 0;
                        }
                        break;

                    case TribulationPhase.Gathering:
                        if (trib.PhaseTimer >= GATHERING_DURATION)
                        {
                            trib.Phase = TribulationPhase.Striking;
                            trib.PhaseTimer = 0;
                            trib.CurrentWave = 0;
                        }
                        break;

                    case TribulationPhase.Striking:
                        // TODO: 生成天劫弹幕
                        // TODO: 检查波次完成
                        // TODO: 心魔劫特殊逻辑（幻觉/Debuff而非伤害）
                        break;

                    case TribulationPhase.FinalBlow:
                        // TODO: 最后一击
                        break;
                }

                if (trib.IsComplete)
                {
                    CompletedTribulations.Add(trib);
                    ActiveTribulations.RemoveAt(i);
                }
            }
        }

        public void OnTribulationSuccess(TribulationInstance trib)
        {
            var player = Main.player[trib.TargetPlayerID];
            if (!player.active) return;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            qiRealm.GuLevel++;
            qiRealm.LevelStage = 0;
            qiRealm.BreakthroughProgress = 0f;

            EventBus.Publish(new TribulationCompletedEvent
            {
                PlayerID = trib.TargetPlayerID,
                TribulationType = trib.Type,
                Success = true,
                NewLevel = qiRealm.GuLevel,
            });

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText($"天劫渡过！你已突破至{qiRealm.GuLevel}转！", Color.Gold);
            }
        }

        public void OnTribulationFailed(TribulationInstance trib)
        {
            var player = Main.player[trib.TargetPlayerID];
            if (!player.active) return;

            EventBus.Publish(new TribulationCompletedEvent
            {
                PlayerID = trib.TargetPlayerID,
                TribulationType = trib.Type,
                Success = false,
            });

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText("天劫失败！修为受损……", Color.Red);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存天劫数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载天劫数据
        }
    }

    public class TribulationStartedEvent : GuWorldEvent
    {
        public int PlayerID;
        public TribulationType TribulationType;
    }

    public class TribulationCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
        public TribulationType TribulationType;
        public bool Success;
        public int NewLevel;
    }
}
