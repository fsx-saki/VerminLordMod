using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // FactionWarSystem — 家族战争系统大框
    //
    // 系统定位：
    // 家族战争是蛊世界的大型事件，两个或多个家族间爆发冲突。
    // 战争影响所有家族NPC和玩家的交互。
    //
    // 功能规划：
    // 1. 战争触发条件：声望极端对立、资源争夺、族长命令
    // 2. 战争阶段：宣战 → 冲突 → 全面战争 → 停战谈判
    // 3. 战争效果：敌对家族NPC主动攻击、贸易中断、领地封锁
    // 4. 战争NPC生成：各家族派出战斗蛊师
    // 5. 防守任务：玩家可参与家族防守
    // 6. 停战条件：一方投降、双方谈判、第三方调解
    // 7. 战争结果：领土变更、声望大幅变化、经济影响
    //
    // TODO:
    //   - 实现战争触发逻辑
    //   - 实现战争阶段推进
    //   - 实现战争NPC生成
    //   - 实现战争对日常交互的影响
    //   - 实现停战/投降逻辑
    //   - 实现战争UI（战争状态面板）
    // ============================================================

    public enum WarPhase
    {
        Tension,           // 紧张期 - 双方对立但未开战
        Declaration,        // 宣战期 - 正式宣战
        BorderConflict,     // 边境冲突 - 小规模战斗
        FullWar,            // 全面战争 - 大规模战斗
        Negotiation,        // 谈判期 - 双方尝试停战
        Ceasefire,          // 停火期 - 暂时停战
        Peace               // 和平期 - 战争结束
    }

    public enum WarOutcome
    {
        Victory,           // 一方胜利
        Defeat,            // 一方失败
        Stalemate,         // 僵持
        NegotiatedPeace,   // 谈判和平
        ThirdPartyMediation // 第三方调解
    }

    public class WarInstance
    {
        public FactionID AttackerFaction;
        public FactionID DefenderFaction;
        public WarPhase CurrentPhase;
        public int StartDay;
        public int DurationDays;
        public List<FactionID> AlliedFactions = new();      // 参战盟友
        public Dictionary<FactionID, int> Casualties = new(); // 各方伤亡数
        public Dictionary<FactionID, int> BattlesWon = new();  // 各方胜场数
        public WarOutcome Outcome;
        public bool IsActive;
        public int WarProgress;           // 0-100，100=一方完全胜利
    }

    public class FactionWarSystem : ModSystem
    {
        public static FactionWarSystem Instance => ModContent.GetInstance<FactionWarSystem>();

        public List<WarInstance> ActiveWars = new();
        public List<WarInstance> WarHistory = new();

        public override void OnWorldLoad()
        {
            ActiveWars.Clear();
            WarHistory.Clear();
        }

        public bool IsFactionInWar(FactionID faction)
        {
            foreach (var war in ActiveWars)
            {
                if (war.AttackerFaction == faction || war.DefenderFaction == faction ||
                    war.AlliedFactions.Contains(faction))
                    return true;
            }
            return false;
        }

        public WarInstance GetActiveWar(FactionID faction)
        {
            foreach (var war in ActiveWars)
            {
                if (war.AttackerFaction == faction || war.DefenderFaction == faction)
                    return war;
            }
            return null;
        }

        public void DeclareWar(FactionID attacker, FactionID defender, string reason)
        {
            var war = new WarInstance
            {
                AttackerFaction = attacker,
                DefenderFaction = defender,
                CurrentPhase = WarPhase.Declaration,
                StartDay = (int)(Main.time / 36000),
                DurationDays = 0,
                Outcome = WarOutcome.Stalemate,
                IsActive = true,
                WarProgress = 0
            };

            ActiveWars.Add(war);

            string attackerName = WorldStateMachine.GetFactionDisplayName(attacker);
            string defenderName = WorldStateMachine.GetFactionDisplayName(defender);
            Main.NewText($"【战争】{attackerName}向{defenderName}宣战！原因：{reason}",
                Microsoft.Xna.Framework.Color.Red);

            ApplyWarEffects(war);
        }

        private void ApplyWarEffects(WarInstance war)
        {
            var economySystem = FactionEconomySystem.Instance;
            if (economySystem != null)
            {
                if (economySystem.Economies.TryGetValue(war.AttackerFaction, out var attackerEcon))
                    attackerEcon.DailyExpense += 200;
                if (economySystem.Economies.TryGetValue(war.DefenderFaction, out var defenderEcon))
                    defenderEcon.DailyExpense += 200;
            }

            var powerSystem = FactionPowerSystem.Instance;
            if (powerSystem != null)
            {
                var attackerStruct = powerSystem.GetPowerStructure(war.AttackerFaction);
                var defenderStruct = powerSystem.GetPowerStructure(war.DefenderFaction);
                if (attackerStruct != null) attackerStruct.InternalConflictLevel = System.Math.Min(100, attackerStruct.InternalConflictLevel + 10);
                if (defenderStruct != null) defenderStruct.InternalConflictLevel = System.Math.Min(100, defenderStruct.InternalConflictLevel + 10);
            }
        }

        public void AdvanceWarPhase(WarInstance war)
        {
            war.CurrentPhase++;
            if (war.CurrentPhase > WarPhase.FullWar)
            {
                war.CurrentPhase = WarPhase.Negotiation;
                if (Main.netMode != Terraria.ID.NetmodeID.Server)
                {
                    string attackerName = WorldStateMachine.GetFactionDisplayName(war.AttackerFaction);
                    string defenderName = WorldStateMachine.GetFactionDisplayName(war.DefenderFaction);
                    Main.NewText($"【战争】{attackerName}与{defenderName}进入谈判阶段...",
                        Microsoft.Xna.Framework.Color.Yellow);
                }
            }
        }

        public void ResolveWar(WarInstance war, WarOutcome outcome)
        {
            war.Outcome = outcome;
            war.IsActive = false;
            ActiveWars.Remove(war);
            WarHistory.Add(war);

            switch (outcome)
            {
                case WarOutcome.Victory:
                    ApplyVictoryEffects(war.AttackerFaction, war.DefenderFaction);
                    break;
                case WarOutcome.Defeat:
                    ApplyVictoryEffects(war.DefenderFaction, war.AttackerFaction);
                    break;
                case WarOutcome.NegotiatedPeace:
                    ApplyPeaceEffects(war);
                    break;
            }

            RemoveWarEffects(war);
        }

        private void ApplyVictoryEffects(FactionID winner, FactionID loser)
        {
            var economySystem = FactionEconomySystem.Instance;
            if (economySystem != null)
            {
                if (economySystem.Economies.TryGetValue(winner, out var winnerEcon))
                    winnerEcon.YuanStoneReserve += 5000;
                if (economySystem.Economies.TryGetValue(loser, out var loserEcon))
                    loserEcon.YuanStoneReserve = System.Math.Max(0, loserEcon.YuanStoneReserve - 3000);
            }

            var territorySystem = TerritoryControlSystem.Instance;
            if (territorySystem != null)
            {
                var loserTerritories = territorySystem.GetFactionTerritoriesList(loser);
                if (loserTerritories.Count > 0)
                {
                    var target = loserTerritories[Main.rand.Next(loserTerritories.Count)];
                    territorySystem.CaptureTerritory(target.TerritoryID, winner);
                }
            }
        }

        private void ApplyPeaceEffects(WarInstance war)
        {
            if (Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                string attackerName = WorldStateMachine.GetFactionDisplayName(war.AttackerFaction);
                string defenderName = WorldStateMachine.GetFactionDisplayName(war.DefenderFaction);
                Main.NewText($"【战争】{attackerName}与{defenderName}达成和平协议。",
                    Microsoft.Xna.Framework.Color.Green);
            }
        }

        private void RemoveWarEffects(WarInstance war)
        {
            var economySystem = FactionEconomySystem.Instance;
            if (economySystem != null)
            {
                if (economySystem.Economies.TryGetValue(war.AttackerFaction, out var attackerEcon))
                    attackerEcon.DailyExpense = System.Math.Max(0, attackerEcon.DailyExpense - 200);
                if (economySystem.Economies.TryGetValue(war.DefenderFaction, out var defenderEcon))
                    defenderEcon.DailyExpense = System.Math.Max(0, defenderEcon.DailyExpense - 200);
            }
        }

        public void PlayerJoinWar(Player player, FactionID side)
        {
            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            guWorld.AddReputation(side, 50, "参战");

            var war = GetActiveWar(side);
            if (war != null)
            {
                war.WarProgress += 5;
                if (!war.BattlesWon.ContainsKey(side))
                    war.BattlesWon[side] = 0;
                war.BattlesWon[side]++;
            }
        }

        public int GetFactionWarParticipation(FactionID faction)
        {
            int count = 0;
            foreach (var war in ActiveWars)
            {
                if (war.AttackerFaction == faction || war.DefenderFaction == faction)
                    count++;
            }
            foreach (var war in WarHistory)
            {
                if (war.AttackerFaction == faction || war.DefenderFaction == faction)
                    count++;
            }
            return count;
        }

        private int _lastDay = -1;

        public override void PostUpdateWorld()
        {
            if (!WorldTimeHelper.IsNewDay(ref _lastDay)) return;

            for (int i = ActiveWars.Count - 1; i >= 0; i--)
            {
                var war = ActiveWars[i];
                war.DurationDays++;

                war.WarProgress += Main.rand.Next(1, 5);

                if (war.DurationDays >= 3 && war.CurrentPhase < WarPhase.FullWar)
                {
                    AdvanceWarPhase(war);
                }

                if (war.CurrentPhase == WarPhase.Negotiation && war.DurationDays >= 7)
                {
                    if (Main.rand.NextFloat() < 0.5f)
                        ResolveWar(war, WarOutcome.NegotiatedPeace);
                    else
                        ResolveWar(war, Main.rand.NextFloat() < 0.5f ? WarOutcome.Victory : WarOutcome.Defeat);
                }

                if (war.WarProgress >= 100)
                {
                    ResolveWar(war, WarOutcome.Victory);
                }
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var activeList = new List<TagCompound>();
            foreach (var war in ActiveWars)
            {
                activeList.Add(new TagCompound
                {
                    ["attacker"] = (int)war.AttackerFaction,
                    ["defender"] = (int)war.DefenderFaction,
                    ["phase"] = (int)war.CurrentPhase,
                    ["startDay"] = war.StartDay,
                    ["duration"] = war.DurationDays,
                    ["progress"] = war.WarProgress,
                });
            }
            tag["activeWars"] = activeList;
            tag["warDayCounter"] = _lastDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveWars.Clear();
            WarHistory.Clear();

            var activeList = tag.GetList<TagCompound>("activeWars");
            if (activeList != null)
            {
                foreach (var t in activeList)
                {
                    ActiveWars.Add(new WarInstance
                    {
                        AttackerFaction = (FactionID)t.GetInt("attacker"),
                        DefenderFaction = (FactionID)t.GetInt("defender"),
                        CurrentPhase = (WarPhase)t.GetInt("phase"),
                        StartDay = t.GetInt("startDay"),
                        DurationDays = t.GetInt("duration"),
                        WarProgress = t.GetInt("progress"),
                        IsActive = true,
                    });
                }
            }

            _lastDay = tag.GetInt("warDayCounter");
        }
    }
}