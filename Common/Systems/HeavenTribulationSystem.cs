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
                    OnTribulationFailed(trib);
                    CompletedTribulations.Add(trib);
                    ActiveTribulations.RemoveAt(i);
                    continue;
                }

                trib.PhaseTimer++;

                switch (trib.Phase)
                {
                    case TribulationPhase.Approaching:
                        UpdateApproachingPhase(trib, player);
                        break;

                    case TribulationPhase.Gathering:
                        UpdateGatheringPhase(trib, player);
                        break;

                    case TribulationPhase.Striking:
                        UpdateStrikingPhase(trib, player);
                        break;

                    case TribulationPhase.FinalBlow:
                        UpdateFinalBlowPhase(trib, player);
                        break;
                }

                if (trib.IsComplete)
                {
                    CompletedTribulations.Add(trib);
                    ActiveTribulations.RemoveAt(i);
                }
            }
        }

        private void UpdateApproachingPhase(TribulationInstance trib, Player player)
        {
            if (trib.PhaseTimer % 60 == 0 && player.whoAmI == Main.myPlayer)
            {
                int remainingSeconds = (APPROACHING_DURATION - trib.PhaseTimer) / 60;
                if (remainingSeconds <= 5 && remainingSeconds > 0)
                    Main.NewText($"天劫将至... {remainingSeconds}秒后劫云聚集！", Color.OrangeRed);
            }

            if (trib.PhaseTimer >= APPROACHING_DURATION)
            {
                trib.Phase = TribulationPhase.Gathering;
                trib.PhaseTimer = 0;
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("劫云开始聚集，天地变色！", Color.Red);
            }
        }

        private void UpdateGatheringPhase(TribulationInstance trib, Player player)
        {
            if (trib.PhaseTimer % 30 == 0)
            {
                SpawnGatheringDust(trib, player);
            }

            if (trib.PhaseTimer >= GATHERING_DURATION)
            {
                trib.Phase = TribulationPhase.Striking;
                trib.PhaseTimer = 0;
                trib.CurrentWave = 0;
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText($"天劫降临！共{trib.TotalWaves}波！", Color.Red);
            }
        }

        private void UpdateStrikingPhase(TribulationInstance trib, Player player)
        {
            if (trib.Type == TribulationType.HeartDemonTribulation)
            {
                UpdateHeartDemonTribulation(trib, player);
                return;
            }

            if (trib.CurrentWave >= trib.TotalWaves)
            {
                trib.Phase = TribulationPhase.FinalBlow;
                trib.PhaseTimer = 0;
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("最后一击！撑过去就能突破！", Color.Gold);
                return;
            }

            var wave = trib.Waves[trib.CurrentWave];
            wave.ElapsedTicks++;

            int spawnInterval = (int)System.Math.Max(10, wave.DurationTicks / wave.ProjectileCount);
            if (wave.ElapsedTicks % spawnInterval == 0 && wave.ElapsedTicks < wave.DurationTicks)
            {
                SpawnTribulationProjectile(trib, player, wave);
            }

            if (wave.IsComplete)
            {
                trib.CurrentWave++;
                trib.PhaseTimer = 0;
                if (player.whoAmI == Main.myPlayer && trib.CurrentWave < trib.TotalWaves)
                    Main.NewText($"第{trib.CurrentWave}波已过！准备迎接下一波...", Color.Yellow);
            }
        }

        private void UpdateHeartDemonTribulation(TribulationInstance trib, Player player)
        {
            if (trib.CurrentWave >= trib.TotalWaves)
            {
                trib.Phase = TribulationPhase.FinalBlow;
                trib.PhaseTimer = 0;
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("心魔即将现形！直面你的内心！", Color.Purple);
                return;
            }

            var wave = trib.Waves[trib.CurrentWave];
            wave.ElapsedTicks++;

            if (wave.ElapsedTicks % 120 == 0)
            {
                ApplyHeartDemonDebuff(player);
            }

            if (wave.ElapsedTicks % 180 == 0 && wave.ElapsedTicks < wave.DurationTicks)
            {
                SpawnHeartDemonIllusion(trib, player);
            }

            if (wave.IsComplete)
            {
                trib.CurrentWave++;
                trib.PhaseTimer = 0;
            }
        }

        private void UpdateFinalBlowPhase(TribulationInstance trib, Player player)
        {
            if (trib.PhaseTimer == 1)
            {
                SpawnFinalBlowProjectiles(trib, player);
            }

            if (trib.PhaseTimer >= 300)
            {
                trib.Phase = TribulationPhase.Completed;
                OnTribulationSuccess(trib);
            }
        }

        private void SpawnGatheringDust(TribulationInstance trib, Player player)
        {
            for (int j = 0; j < 3; j++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(200f, trib.Radius);
                Vector2 pos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * dist;
                Dust d = Dust.NewDustPerfect(pos, Terraria.ID.DustID.Cloud, Vector2.Zero, 100, Color.DarkGray, 2f);
                d.noGravity = true;
                d.velocity = (player.Center - pos) * 0.02f;
            }
        }

        private void SpawnTribulationProjectile(TribulationInstance trib, Player player, TribulationWave wave)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = trib.Radius * 0.8f;
            Vector2 spawnPos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * dist;

            Vector2 velocity = (player.Center - spawnPos);
            velocity.Normalize();
            velocity *= 4f + trib.CurrentWave * 0.5f;

            int projType = wave.ProjectileType;
            if (projType <= 0) projType = Terraria.ID.ProjectileID.CultistBossLightningOrbArc;

            int proj = Projectile.NewProjectile(
                Terraria.Entity.GetSource_NaturalSpawn(),
                spawnPos, velocity, projType,
                (int)(wave.Damage * 0.5f), 2f, Main.myPlayer);

            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].hostile = true;
                Main.projectile[proj].friendly = false;
                Main.projectile[proj].tileCollide = false;
            }
        }

        private void ApplyHeartDemonDebuff(Player player)
        {
            int debuffType = Main.rand.Next(3) switch
            {
                0 => Terraria.ID.BuffID.Confused,
                1 => Terraria.ID.BuffID.Darkness,
                _ => Terraria.ID.BuffID.Slow,
            };
            player.AddBuff(debuffType, 300);
        }

        private void SpawnHeartDemonIllusion(TribulationInstance trib, Player player)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = Main.rand.NextFloat(100f, 300f);
            Vector2 spawnPos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * dist;

            int npcType = Terraria.ID.NPCID.DarkCaster;
            int npc = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, npcType);
            if (npc >= 0 && npc < Main.maxNPCs)
            {
                Main.npc[npc].lifeMax = 200 + trib.CurrentWave * 100;
                Main.npc[npc].life = Main.npc[npc].lifeMax;
                Main.npc[npc].damage = 30 + trib.CurrentWave * 10;
                Main.npc[npc].color = Color.Purple * 0.5f;
            }
        }

        private void SpawnFinalBlowProjectiles(TribulationInstance trib, Player player)
        {
            int count = trib.Type switch
            {
                TribulationType.GreatDaoTribulation => 30,
                TribulationType.LifeDeathTribulation => 25,
                _ => 15,
            };

            for (int j = 0; j < count; j++)
            {
                float angle = MathHelper.TwoPi * j / count;
                float dist = trib.Radius;
                Vector2 spawnPos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * dist;
                Vector2 velocity = (player.Center - spawnPos);
                velocity.Normalize();
                velocity *= 6f;

                int projType = GetTribulationProjectileType(trib.Type);
                int proj = Projectile.NewProjectile(
                    Terraria.Entity.GetSource_NaturalSpawn(),
                    spawnPos, velocity, projType,
                    (int)(GetTribulationDamage(trib.Type) * 1.5f), 3f, Main.myPlayer);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].hostile = true;
                    Main.projectile[proj].friendly = false;
                    Main.projectile[proj].tileCollide = false;
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

            CultivationLoopSystem.OnTribulationSurvived(player);

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

            CultivationLoopSystem.OnTribulationFailed(player);

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

        public bool HasPlayerSurvivedTribulation(Player player)
        {
            foreach (var trib in CompletedTribulations)
            {
                if (trib.TargetPlayerID == player.whoAmI)
                    return true;
            }
            return false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var activeList = new List<TagCompound>();
            foreach (var trib in ActiveTribulations)
            {
                activeList.Add(new TagCompound
                {
                    ["type"] = (int)trib.Type,
                    ["phase"] = (int)trib.Phase,
                    ["playerID"] = trib.TargetPlayerID,
                    ["currentWave"] = trib.CurrentWave,
                    ["totalWaves"] = trib.TotalWaves,
                    ["phaseTimer"] = trib.PhaseTimer,
                    ["centerX"] = trib.Center.X,
                    ["centerY"] = trib.Center.Y,
                    ["radius"] = trib.Radius,
                });
            }
            tag["activeTribulations"] = activeList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveTribulations.Clear();
            if (tag.TryGet("activeTribulations", out List<TagCompound> activeList))
            {
                foreach (var data in activeList)
                {
                    var trib = new TribulationInstance
                    {
                        Type = (TribulationType)data.GetInt("type"),
                        Phase = (TribulationPhase)data.GetInt("phase"),
                        TargetPlayerID = data.GetInt("playerID"),
                        CurrentWave = data.GetInt("currentWave"),
                        TotalWaves = data.GetInt("totalWaves"),
                        PhaseTimer = data.GetInt("phaseTimer"),
                        Center = new Vector2(data.GetFloat("centerX"), data.GetFloat("centerY")),
                        Radius = data.GetFloat("radius"),
                    };

                    for (int i = 0; i < trib.TotalWaves; i++)
                    {
                        trib.Waves.Add(new TribulationWave
                        {
                            WaveIndex = i,
                            Damage = GetTribulationDamage(trib.Type) * (1f + i * 0.2f),
                            DurationTicks = 180 + i * 60,
                            ProjectileCount = 5 + i * 3,
                            ProjectileType = GetTribulationProjectileType(trib.Type),
                        });
                    }

                    ActiveTribulations.Add(trib);
                }
            }
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
