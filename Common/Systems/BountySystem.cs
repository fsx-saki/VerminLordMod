using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// BountySystem — 悬赏完整版（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 订阅 BountyPostedEvent，维护活跃悬赏列表
    /// 2. 提供悬赏领取、结算接口
    /// 3. NPC 可领取悬赏追杀玩家（MVA 简化：散修自动领取）
    /// 
    /// MVA 阶段：
    /// - 订阅 BountyPostedEvent，维护活跃悬赏列表
    /// - 悬赏面板功能（P1 实现 UI）
    /// - NPC 领取悬赏追杀玩家（P1 实现 NPC 生成）
    /// - 悬赏结算：目标死亡后给予奖励
    /// 
    /// 依赖：
    /// - EventBus（订阅 BountyPostedEvent）
    /// - GuWorldPlayer（声望/通缉状态）
    /// - NpcDeathHandler（NPC 死亡事件）
    /// - QiResourcePlayer（真元奖励）
    /// </summary>
    public class BountySystem : ModSystem
    {
        // ===== 单例访问 =====
        public static BountySystem Instance => ModContent.GetInstance<BountySystem>();

        // ===== 运行时数据 =====
        /// <summary> 活跃悬赏列表 </summary>
        public List<Bounty> ActiveBounties = new();

        /// <summary> 已完成的悬赏历史 </summary>
        public List<Bounty> CompletedBounties = new();

        // ============================================================
        // 生命周期
        // ============================================================

        public override void OnWorldLoad()
        {
            ActiveBounties.Clear();
            CompletedBounties.Clear();
        }

        public override void OnWorldUnload()
        {
            ActiveBounties.Clear();
            CompletedBounties.Clear();
        }

        // ============================================================
        // 事件订阅
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 注册事件订阅（确保只注册一次）
            // 使用静态标记避免重复订阅
            if (!_isSubscribed)
            {
                EventBus.Subscribe<BountyPostedEvent>(OnBountyPosted);
                EventBus.Subscribe<NPCDeathEvent>(OnNPCDeath);
                _isSubscribed = true;
            }

            // 每帧检查过期悬赏
            CheckExpiredBounties();
        }

        private static bool _isSubscribed = false;

        // ============================================================
        // 事件处理
        // ============================================================

        /// <summary>
        /// 处理悬赏发布事件。
        /// </summary>
        public void OnBountyPosted(BountyPostedEvent evt)
        {
            var bounty = new Bounty
            {
                BountyID = evt.BountyID,
                PostingFaction = evt.PostingFaction,
                TargetPlayerID = evt.TargetPlayerID,
                RewardAmount = evt.RewardAmount,
                Reason = evt.Reason,
                IsActive = true,
                PostedTick = (int)Main.GameUpdateCount,
                ExpiryTick = (int)Main.GameUpdateCount + 54000 // 9 天过期（游戏内）
            };

            ActiveBounties.Add(bounty);

            // 通知目标玩家
            if (evt.TargetPlayerID >= 0 && evt.TargetPlayerID < Main.maxPlayers)
            {
                var target = Main.player[evt.TargetPlayerID];
                if (target.active && target.whoAmI == Main.myPlayer)
                {
                    Main.NewText(
                        $"你被{WorldStateMachine.GetFactionDisplayName(evt.PostingFaction)}悬赏了！赏金 {evt.RewardAmount} 元石",
                        Color.OrangeRed);
                }
            }

            // MVA 简化：散修自动领取悬赏（P1 再实现 NPC 生成）
            if (evt.TargetPlayerID >= 0)
            {
                // 预留：生成追杀 NPC
                // SpawnBountyHunter(evt);
            }
        }

        /// <summary>
        /// 处理 NPC 死亡事件（检查是否与悬赏相关）。
        /// </summary>
        public void OnNPCDeath(NPCDeathEvent evt)
        {
            // 检查是否有针对击杀者的悬赏
            if (evt.KillerPlayerID < 0) return;

            var killer = Main.player[evt.KillerPlayerID];
            if (!killer.active) return;

            // 检查击杀者是否有活跃悬赏（被悬赏者死亡）
            var targetBounties = ActiveBounties.Where(b =>
                b.IsActive && b.TargetPlayerID == evt.KillerPlayerID).ToList();

            foreach (var bounty in targetBounties)
            {
                // 被悬赏者死亡 → 悬赏失效（无需结算）
                bounty.IsActive = false;
                CompletedBounties.Add(bounty);

                Main.NewText(
                    $"{killer.name}已死亡，{WorldStateMachine.GetFactionDisplayName(bounty.PostingFaction)}的悬赏暂时失效。",
                    Color.Gray);
            }

            // 检查是否有针对被击杀 NPC 的悬赏（玩家领取悬赏击杀目标）
            // MVA 简化：暂不实现 NPC 作为悬赏目标
        }

        // ============================================================
        // 悬赏操作
        // ============================================================

        /// <summary>
        /// 领取悬赏。
        /// </summary>
        public bool ClaimBounty(Player player, int bountyID)
        {
            var bounty = ActiveBounties.FirstOrDefault(b => b.BountyID == bountyID);
            if (bounty == null || !bounty.IsActive)
            {
                Main.NewText("该悬赏已失效。", Color.Gray);
                return false;
            }

            // 检查目标是否已被击杀
            if (bounty.TargetPlayerID >= 0 && bounty.TargetPlayerID < Main.maxPlayers)
            {
                Player target = Main.player[bounty.TargetPlayerID];
                if (!target.active || target.dead)
                {
                    // 目标已死亡 → 给予奖励
                    var qiResource = player.GetModPlayer<QiResourcePlayer>();
                    qiResource.RefundQi(bounty.RewardAmount);

                    // 声望影响
                    var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
                    worldPlayer.AddReputation(bounty.PostingFaction, 10, "领取悬赏");

                    bounty.IsActive = false;
                    CompletedBounties.Add(bounty);

                    Main.NewText(
                        $"悬赏结算成功！获得 {bounty.RewardAmount} 真元奖励。",
                        Color.Green);
                    return true;
                }
                else
                {
                    Main.NewText(
                        $"目标 {target.name} 仍然存活，无法结算悬赏。",
                        Color.Yellow);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 取消悬赏。
        /// </summary>
        public void CancelBounty(int bountyID)
        {
            var bounty = ActiveBounties.FirstOrDefault(b => b.BountyID == bountyID);
            if (bounty != null)
            {
                bounty.IsActive = false;
                Main.NewText(
                    $"{WorldStateMachine.GetFactionDisplayName(bounty.PostingFaction)}的悬赏已被取消。",
                    Color.Gray);
            }
        }

        /// <summary>
        /// 获取玩家相关的活跃悬赏。
        /// </summary>
        public List<Bounty> GetPlayerBounties(int playerID)
        {
            return ActiveBounties.Where(b =>
                b.IsActive && b.TargetPlayerID == playerID).ToList();
        }

        /// <summary>
        /// 获取某势力的所有活跃悬赏。
        /// </summary>
        public List<Bounty> GetFactionBounties(FactionID faction)
        {
            return ActiveBounties.Where(b =>
                b.IsActive && b.PostingFaction == faction).ToList();
        }

        // ============================================================
        // 内部逻辑
        // ============================================================

        /// <summary>
        /// 检查过期悬赏。
        /// </summary>
        private void CheckExpiredBounties()
        {
            for (int i = ActiveBounties.Count - 1; i >= 0; i--)
            {
                var bounty = ActiveBounties[i];
                if (bounty.IsActive && Main.GameUpdateCount >= bounty.ExpiryTick)
                {
                    bounty.IsActive = false;
                    CompletedBounties.Add(bounty);

                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText(
                            $"{WorldStateMachine.GetFactionDisplayName(bounty.PostingFaction)}对某玩家的悬赏已过期。",
                            Color.Gray);
                    }
                }
            }
        }

        // ============================================================
        // 数据持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var activeData = new List<TagCompound>();
            foreach (var b in ActiveBounties)
            {
                activeData.Add(new TagCompound
                {
                    ["id"] = b.BountyID,
                    ["faction"] = b.PostingFaction.ToString(),
                    ["target"] = b.TargetPlayerID,
                    ["reward"] = b.RewardAmount,
                    ["reason"] = (int)b.Reason,
                    ["active"] = b.IsActive,
                    ["postedTick"] = b.PostedTick,
                    ["expiryTick"] = b.ExpiryTick
                });
            }
            tag["activeBounties"] = activeData;

            var completedData = new List<TagCompound>();
            foreach (var b in CompletedBounties)
            {
                completedData.Add(new TagCompound
                {
                    ["id"] = b.BountyID,
                    ["faction"] = b.PostingFaction.ToString(),
                    ["target"] = b.TargetPlayerID,
                    ["reward"] = b.RewardAmount,
                    ["reason"] = (int)b.Reason
                });
            }
            tag["completedBounties"] = completedData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveBounties.Clear();
            CompletedBounties.Clear();

            if (tag.TryGet("activeBounties", out List<TagCompound> activeData))
            {
                foreach (var entry in activeData)
                {
                    if (System.Enum.TryParse<FactionID>(entry.GetString("faction"), out var fid))
                    {
                        ActiveBounties.Add(new Bounty
                        {
                            BountyID = entry.GetInt("id"),
                            PostingFaction = fid,
                            TargetPlayerID = entry.GetInt("target"),
                            RewardAmount = entry.GetInt("reward"),
                            Reason = (BountyReason)entry.GetInt("reason"),
                            IsActive = entry.GetBool("active"),
                            PostedTick = entry.GetInt("postedTick"),
                            ExpiryTick = entry.GetInt("expiryTick")
                        });
                    }
                }
            }

            if (tag.TryGet("completedBounties", out List<TagCompound> completedData))
            {
                foreach (var entry in completedData)
                {
                    if (System.Enum.TryParse<FactionID>(entry.GetString("faction"), out var fid))
                    {
                        CompletedBounties.Add(new Bounty
                        {
                            BountyID = entry.GetInt("id"),
                            PostingFaction = fid,
                            TargetPlayerID = entry.GetInt("target"),
                            RewardAmount = entry.GetInt("reward"),
                            Reason = (BountyReason)entry.GetInt("reason"),
                            IsActive = false
                        });
                    }
                }
            }
        }
    }

    // ============================================================
    // 悬赏数据结构
    // ============================================================

    /// <summary>
    /// 单个悬赏条目。
    /// </summary>
    public class Bounty
    {
        /// <summary> 悬赏唯一标识 </summary>
        public int BountyID;

        /// <summary> 发布悬赏的势力 </summary>
        public FactionID PostingFaction;

        /// <summary> 目标玩家 ID </summary>
        public int TargetPlayerID;

        /// <summary> 奖励金额（真元） </summary>
        public int RewardAmount;

        /// <summary> 悬赏原因 </summary>
        public BountyReason Reason;

        /// <summary> 是否活跃 </summary>
        public bool IsActive;

        /// <summary> 发布时的游戏帧 </summary>
        public int PostedTick;

        /// <summary> 过期帧数 </summary>
        public int ExpiryTick;

        /// <summary> 获取悬赏摘要 </summary>
        public string GetSummary()
        {
            string targetName = "未知";
            if (TargetPlayerID >= 0 && TargetPlayerID < Main.maxPlayers)
            {
                var player = Main.player[TargetPlayerID];
                if (player.active)
                    targetName = player.name;
            }

            string reasonStr = Reason switch
            {
                BountyReason.Revenge => "复仇",
                BountyReason.PlayerKill => "击杀成员",
                BountyReason.Theft => "盗窃",
                BountyReason.Betrayal => "背叛",
                _ => "其他"
            };

            return $"[{WorldStateMachine.GetFactionDisplayName(PostingFaction)}] 悬赏 {targetName}：{RewardAmount} 真元（{reasonStr}）";
        }
    }
}
