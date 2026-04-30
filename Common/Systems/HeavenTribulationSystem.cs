using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// HeavenTribulationSystem — 天劫系统（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 管理每个玩家的天劫状态（类型、时间、预警）
    /// 2. 天劫预警：提前 3 天（18000 帧）发送紫色预警文字
    /// 3. 天劫触发：MVA 阶段只播放特效和提示，不造成实际伤害
    /// 4. 规避检查：玩家在安全区/出生点附近可规避
    /// 
    /// MVA 阶段：
    /// - 只实现预警和规避框架
    /// - 不触发实际伤害（P2 再填充）
    /// - 二转以上每 15 天一次天劫
    /// - 规避条件：玩家在出生点附近（简化）
    /// 
    /// 依赖：
    /// - QiRealmPlayer（修为等级，二转以上触发）
    /// - EventBus（预留事件发布）
    /// </summary>
    public class HeavenTribulationSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static HeavenTribulationSystem Instance => ModContent.GetInstance<HeavenTribulationSystem>();

        // ===== 配置常量 =====
        /// <summary> 预警期（提前 3 天 = 18000 帧） </summary>
        public const int WARNING_TICKS = 18000;

        /// <summary> 天劫间隔（15 天 = 90000 帧） </summary>
        public const int TRIBULATION_INTERVAL = 90000;

        /// <summary> 最低触发修为（二转） </summary>
        public const int MIN_GU_LEVEL = 2;

        /// <summary> 规避距离（出生点附近 400 像素） </summary>
        public const float EVADE_DISTANCE = 400f;

        // ===== 运行时数据 =====
        /// <summary> 玩家天劫状态字典（Key = Player.whoAmI） </summary>
        public Dictionary<int, TribulationState> PlayerTribulations = new();

        // ============================================================
        // 天劫状态数据结构
        // ============================================================

        /// <summary> 单个玩家的天劫状态 </summary>
        public class TribulationState
        {
            /// <summary> 玩家 ID </summary>
            public int PlayerID;

            /// <summary> 下次天劫帧数 </summary>
            public int NextTribulationTick;

            /// <summary> 天劫类型 </summary>
            public TribulationType Type;

            /// <summary> 预警期帧数 </summary>
            public int WarningTicks = WARNING_TICKS;

            /// <summary> 是否正在进行 </summary>
            public bool IsActive;

            /// <summary> 是否已发送预警 </summary>
            public bool HasWarned;

            /// <summary> 天劫强度（基于修为） </summary>
            public float Intensity;
        }

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            PlayerTribulations.Clear();
        }

        public override void OnWorldUnload()
        {
            PlayerTribulations.Clear();
        }

        // ============================================================
        // 世界更新
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 只在单人模式下运行
            if (Main.netMode != NetmodeID.SinglePlayer) return;

            // 确保本地玩家有状态
            var localPlayer = Main.LocalPlayer;
            if (!localPlayer.active) return;

            var qiRealm = localPlayer.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < MIN_GU_LEVEL) return;

            // 确保玩家有状态记录
            if (!PlayerTribulations.ContainsKey(localPlayer.whoAmI))
            {
                InitializePlayerTribulation(localPlayer);
            }

            var state = PlayerTribulations[localPlayer.whoAmI];

            // 预警期检查
            if (!state.IsActive && !state.HasWarned &&
                Main.GameUpdateCount >= state.NextTribulationTick - state.WarningTicks)
            {
                state.HasWarned = true;
                int daysRemaining = (int)((state.NextTribulationTick - Main.GameUpdateCount) / 6000);
                Main.NewText(
                    $"天道预警：{daysRemaining} 天后将遭遇 {GetTribulationName(state.Type)} 劫！",
                    Color.Purple);
            }

            // 天劫降临检查
            if (!state.IsActive && Main.GameUpdateCount >= state.NextTribulationTick)
            {
                TriggerTribulation(state);
            }

            // 天劫进行中：每帧更新
            if (state.IsActive)
            {
                UpdateActiveTribulation(state);
            }
        }

        // ============================================================
        // 初始化
        // ============================================================

        /// <summary>
        /// 初始化玩家的天劫状态。
        /// </summary>
        private void InitializePlayerTribulation(Player player)
        {
            var state = new TribulationState
            {
                PlayerID = player.whoAmI,
                NextTribulationTick = (int)Main.GameUpdateCount + TRIBULATION_INTERVAL,
                Type = (TribulationType)Main.rand.Next(3),
                WarningTicks = WARNING_TICKS,
                IsActive = false,
                HasWarned = false,
                Intensity = CalculateIntensity(player)
            };
            PlayerTribulations[player.whoAmI] = state;
        }

        /// <summary>
        /// 计算天劫强度。
        /// </summary>
        private float CalculateIntensity(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            return qiRealm.GuLevel + qiRealm.LevelStage * 0.25f;
        }

        // ============================================================
        // 天劫触发
        // ============================================================

        /// <summary>
        /// 触发天劫。
        /// MVA 阶段：只播放特效和提示，不造成伤害。
        /// </summary>
        private void TriggerTribulation(TribulationState state)
        {
            state.IsActive = true;

            Player player = Main.player[state.PlayerID];
            if (!player.active) return;

            Main.NewText(
                $"天劫降临！{GetTribulationName(state.Type)} 劫正在形成...",
                Color.Purple);

            // 特效：屏幕震动 + 紫色闪电
            // MVA 阶段使用简单的 Dust 特效
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.PurpleTorch,
                    Main.rand.NextFloat(-8f, 8f),
                    Main.rand.NextFloat(-8f, 8f),
                    150,
                    Color.Purple,
                    2f);
            }

            // 规避检查
            bool isEvading = CheckEvading(player);
            if (isEvading)
            {
                Main.NewText("你成功规避了天劫。", Color.Green);
                state.IsActive = false;
                ScheduleNextTribulation(state, player);
            }
            else
            {
                // P2：造成伤害
                // player.Hurt(...)
                Main.NewText(
                    $"你未能规避{GetTribulationName(state.Type)}劫！",
                    Color.Red);
                // 预留：实际伤害逻辑
            }
        }

        /// <summary>
        /// 更新进行中的天劫。
        /// MVA 阶段：持续一段时间后自动结束。
        /// </summary>
        private void UpdateActiveTribulation(TribulationState state)
        {
            Player player = Main.player[state.PlayerID];
            if (!player.active) return;

            // MVA 阶段：天劫持续 5 秒后自动结束
            // P2 再实现持续伤害
            if (Main.GameUpdateCount % 300 == 0) // 每 5 秒检查一次
            {
                // 特效：持续闪电效果
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(
                        player.position,
                        player.width,
                        player.height,
                        DustID.PurpleTorch,
                        Main.rand.NextFloat(-4f, 4f),
                        Main.rand.NextFloat(-4f, 4f),
                        100,
                        Color.Purple,
                        1.5f);
                }

                // 5 秒后结束天劫
                state.IsActive = false;
                Main.NewText($"{GetTribulationName(state.Type)}劫已平息。", Color.Gray);
                ScheduleNextTribulation(state, player);
            }
        }

        // ============================================================
        // 规避检查
        // ============================================================

        /// <summary>
        /// 检查玩家是否规避了天劫。
        /// MVA 简化：只要玩家在出生点附近就视为规避。
        /// </summary>
        private bool CheckEvading(Player player)
        {
            // 出生点附近
            Vector2 spawnPoint = new Vector2(player.SpawnX * 16f, player.SpawnY * 16f);
            if (Vector2.Distance(player.Center, spawnPoint) < EVADE_DISTANCE)
                return true;

            // 预留：安全区/福地/避雷蛊检查
            // P1 扩展

            return false;
        }

        // ============================================================
        // 调度
        // ============================================================

        /// <summary>
        /// 安排下次天劫。
        /// </summary>
        private void ScheduleNextTribulation(TribulationState state, Player player)
        {
            state.NextTribulationTick = (int)Main.GameUpdateCount + TRIBULATION_INTERVAL;
            state.Type = (TribulationType)Main.rand.Next(3);
            state.HasWarned = false;
            state.Intensity = CalculateIntensity(player);
        }

        // ============================================================
        // 工具方法
        // ============================================================

        /// <summary>
        /// 获取天劫类型的中文名称。
        /// </summary>
        public static string GetTribulationName(TribulationType type)
        {
            return type switch
            {
                TribulationType.Lightning => "雷",
                TribulationType.Fire => "火",
                TribulationType.HeartDemon => "心魔",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取玩家当前天劫状态的摘要。
        /// </summary>
        public string GetStatusSummary(Player player)
        {
            if (!PlayerTribulations.TryGetValue(player.whoAmI, out var state))
                return "未触发";

            if (state.IsActive)
                return $"正在经历{GetTribulationName(state.Type)}劫";

            int ticksRemaining = state.NextTribulationTick - (int)Main.GameUpdateCount;
            if (ticksRemaining <= state.WarningTicks)
            {
                int daysRemaining = ticksRemaining / 6000;
                return $"预警中：{daysRemaining} 天后 {GetTribulationName(state.Type)}劫";
            }

            int totalDays = ticksRemaining / 6000;
            return $"下次天劫：{totalDays} 天后";
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var tribData = new List<TagCompound>();
            foreach (var (playerID, state) in PlayerTribulations)
            {
                tribData.Add(new TagCompound
                {
                    ["playerID"] = playerID,
                    ["nextTick"] = state.NextTribulationTick,
                    ["type"] = (int)state.Type,
                    ["warningTicks"] = state.WarningTicks,
                    ["isActive"] = state.IsActive,
                    ["hasWarned"] = state.HasWarned,
                    ["intensity"] = state.Intensity
                });
            }
            tag["playerTribulations"] = tribData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            PlayerTribulations.Clear();
            if (tag.TryGet("playerTribulations", out List<TagCompound> tribData))
            {
                foreach (var entry in tribData)
                {
                    var state = new TribulationState
                    {
                        PlayerID = entry.GetInt("playerID"),
                        NextTribulationTick = entry.GetInt("nextTick"),
                        Type = (TribulationType)entry.GetInt("type"),
                        WarningTicks = entry.GetInt("warningTicks"),
                        IsActive = entry.GetBool("isActive"),
                        HasWarned = entry.GetBool("hasWarned"),
                        Intensity = entry.GetFloat("intensity")
                    };
                    PlayerTribulations[state.PlayerID] = state;
                }
            }
        }
    }

    // ============================================================
    // 天劫类型枚举
    // ============================================================

    /// <summary>
    /// 天劫类型。
    /// </summary>
    public enum TribulationType
    {
        Lightning,  // 雷劫
        Fire,       // 火劫
        HeartDemon  // 心魔劫
    }
}
