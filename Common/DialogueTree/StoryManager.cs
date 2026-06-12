using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.DialogueTree.Actions;
using VerminLordMod.Content.Dialogues.Stories;
using VerminLordMod.Content.NPCs.GuMasters;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// StoryManager — 剧情推进核心管理器。
    ///
    /// 职责：
    /// 1. 追踪每个玩家的剧情阶段和完成状态
    /// 2. 根据阶段决定哪些 NPC 携带哪些剧情对话
    /// 3. 处理阶段推进条件（完成前置剧情 → 解锁下一阶段）
    /// 4. 持久化剧情进度
    ///
    /// 工作流程：
    ///   Player 进入小世界 → StoryManager 检查当前阶段 →
    ///   根据阶段为 NPC 绑定对应的 StoryDialogueProvider →
    ///   Player 与 NPC 对话 → 剧情推进 → 阶段更新 →
    ///   下一阶段 NPC 刷新/对话更新
    ///
    /// 使用方式：
    ///   <code>
    ///   // 获取当前阶段
    ///   var phase = StoryManager.Instance.GetPhase(player);
    ///
    ///   // 推进到下一阶段
    ///   StoryManager.Instance.AdvancePhase(player);
    ///
    ///   // 完成某个剧情
    ///   StoryManager.Instance.CompleteStory(player, "Story_GuYueVillageEntry");
    ///   </code>
    /// </summary>
    public class StoryManager : ModSystem
    {
        // ===== 单例 =====
        public static StoryManager Instance => ModContent.GetInstance<StoryManager>();

        // ===== 玩家剧情进度 =====
        private readonly Dictionary<string, PlayerStoryProgress> _playerProgress = new();

        // ===== 阶段 → 剧情 ID 映射 =====
        // 每个阶段对应一个主要剧情，完成后自动推进到下一阶段
        private static readonly Dictionary<StoryPhase, string> PhaseStoryMap = new()
        {
            { StoryPhase.Arrival, "Story_GuYueVillageEntry" },
            { StoryPhase.AwakeningCeremony, "Story_AwakeningCeremony" },
            { StoryPhase.SchoolTraining, "Story_GuYueSchoolTrial" },
            { StoryPhase.MedicineRequest, "Story_GuYueMedicineRequest" },
            { StoryPhase.JiaJinShengDeath, "Story_JiaJinShengDeath" },
            { StoryPhase.HuaJiuInheritance, "Story_HuaJiuInheritance" },
            { StoryPhase.FamilyRecognition, "Story_GuYueFamilyRecognition" },
            { StoryPhase.PreTournament, "Story_PreTournament" },
            { StoryPhase.TournamentBegin, "Story_TournamentBegin" },
            { StoryPhase.TournamentFinal, "Story_TournamentFinal" },
            { StoryPhase.TianHeAttack, "Story_TianHeAttack" },
            { StoryPhase.BaiNingBingIceSeal, "Story_BaiNingBingIceSeal" },
            { StoryPhase.BloodSacrifice, "Story_BloodSacrifice" },
            { StoryPhase.LeftQingMao, "Story_LeftQingMao" },
            { StoryPhase.SouthBorderArrival, "Story_SouthBorderArrival" },
            { StoryPhase.ShangXinCiMeet, "Story_ShangXinCiMeet" },
            { StoryPhase.ThreeKingsInheritance, "Story_ThreeKingsInheritance" },
            { StoryPhase.ChunQiuChanFragment, "Story_ChunQiuChanFragment" },
            { StoryPhase.SanXiuCampComplete, "Story_SanXiuCampComplete" },
            { StoryPhase.YiTianShanAppears, "Story_YiTianShanAppears" },
            { StoryPhase.YiTianShanDungeon, "Story_YiTianShanDungeon" },
            { StoryPhase.DaTongFeng, "Story_DaTongFeng" },
            { StoryPhase.FangYuanReveal, "Story_FangYuanReveal" },
            { StoryPhase.YiTianShanComplete, "Story_YiTianShanComplete" },
            { StoryPhase.NorthDesertArrival, "Story_NorthDesertArrival" },
            { StoryPhase.WangTingAlly, "Story_WangTingAlly" },
            { StoryPhase.ChangShengTianContact, "Story_ChangShengTianContact" },
            { StoryPhase.TaiBaiYunShengDeath, "Story_TaiBaiYunShengDeath" },
            { StoryPhase.ImmortalZombieChoice, "Story_ImmortalZombieChoice" },
            { StoryPhase.HeavenPrelude, "Story_HeavenPrelude" },
            { StoryPhase.DestinyWarBegin, "Story_DestinyWarBegin" },
            { StoryPhase.FourPillarsDown, "Story_FourPillarsDown" },
            { StoryPhase.FactionChoice, "Story_FactionChoice" },
            { StoryPhase.LongGongPhase1, "Story_LongGongPhase1" },
            { StoryPhase.ChunQiuRebirth, "Story_ChunQiuRebirth" },
            { StoryPhase.LongGongPhase2, "Story_LongGongPhase2" },
            { StoryPhase.DestinyShattered, "Story_DestinyShattered" },
            { StoryPhase.Ascension, "Story_Ascension" },
            { StoryPhase.SevenTurnBegin, "Story_SevenTurnBegin" },
            { StoryPhase.ApertureBuilt, "Story_ApertureBuilt" },
            { StoryPhase.EightTurnBegin, "Story_EightTurnBegin" },
            { StoryPhase.DaoLordChallenge, "Story_DaoLordChallenge" },
            { StoryPhase.NineTurnBegin, "Story_NineTurnBegin" },
            { StoryPhase.VenerableBattle, "Story_VenerableBattle" },
            { StoryPhase.TenTurnFinale, "Story_TenTurnFinale" },
            { StoryPhase.EndingChosen, "Story_EndingChosen" },
        };

        // ===== 阶段顺序 =====
        private static readonly List<StoryPhase> PhaseOrder = new()
        {
            StoryPhase.NotEntered,
            StoryPhase.Arrival,
            StoryPhase.AwakeningCeremony,
            StoryPhase.SchoolTraining,
            StoryPhase.MedicineRequest,
            StoryPhase.JiaJinShengDeath,
            StoryPhase.HuaJiuInheritance,
            StoryPhase.FamilyRecognition,
            StoryPhase.PreTournament,
            StoryPhase.TournamentBegin,
            StoryPhase.TournamentFinal,
            StoryPhase.TianHeAttack,
            StoryPhase.BaiNingBingIceSeal,
            StoryPhase.BloodSacrifice,
            StoryPhase.LeftQingMao,
            StoryPhase.SouthBorderArrival,
            StoryPhase.ShangXinCiMeet,
            StoryPhase.ThreeKingsInheritance,
            StoryPhase.ChunQiuChanFragment,
            StoryPhase.SanXiuCampComplete,
            StoryPhase.YiTianShanAppears,
            StoryPhase.YiTianShanDungeon,
            StoryPhase.DaTongFeng,
            StoryPhase.FangYuanReveal,
            StoryPhase.YiTianShanComplete,
            StoryPhase.NorthDesertArrival,
            StoryPhase.WangTingAlly,
            StoryPhase.ChangShengTianContact,
            StoryPhase.TaiBaiYunShengDeath,
            StoryPhase.ImmortalZombieChoice,
            StoryPhase.HeavenPrelude,
            StoryPhase.DestinyWarBegin,
            StoryPhase.FourPillarsDown,
            StoryPhase.FactionChoice,
            StoryPhase.LongGongPhase1,
            StoryPhase.ChunQiuRebirth,
            StoryPhase.LongGongPhase2,
            StoryPhase.DestinyShattered,
            StoryPhase.Ascension,
            StoryPhase.SevenTurnBegin,
            StoryPhase.ApertureBuilt,
            StoryPhase.EightTurnBegin,
            StoryPhase.DaoLordChallenge,
            StoryPhase.NineTurnBegin,
            StoryPhase.VenerableBattle,
            StoryPhase.TenTurnFinale,
            StoryPhase.EndingChosen,
        };

        // ============================================================
        // 阶段查询
        // ============================================================

        /// <summary> 获取玩家的当前阶段 </summary>
        public StoryPhase GetPhase(Player player)
        {
            return GetProgress(player).CurrentPhase;
        }

        /// <summary> 获取玩家的剧情进度对象 </summary>
        public PlayerStoryProgress GetProgress(Player player)
        {
            string name = player.name;
            if (!_playerProgress.TryGetValue(name, out var progress))
            {
                progress = new PlayerStoryProgress();
                _playerProgress[name] = progress;
            }
            return progress;
        }

        /// <summary> 检查玩家是否已完成指定剧情 </summary>
        public bool HasCompleted(Player player, string storyID)
        {
            return GetProgress(player).HasCompleted(storyID);
        }

        /// <summary> 检查玩家是否已达到或超过指定阶段 </summary>
        public bool HasReachedPhase(Player player, StoryPhase phase)
        {
            var current = GetPhase(player);
            return PhaseOrder.IndexOf(current) >= PhaseOrder.IndexOf(phase);
        }

        // ============================================================
        // 阶段推进
        // ============================================================

        /// <summary> 推进到下一阶段（自动完成当前阶段的剧情） </summary>
        public void AdvancePhase(Player player)
        {
            var progress = GetProgress(player);
            int currentIdx = PhaseOrder.IndexOf(progress.CurrentPhase);

            if (currentIdx < 0 || currentIdx >= PhaseOrder.Count - 1)
                return;

            // 完成当前阶段的剧情
            if (PhaseStoryMap.TryGetValue(progress.CurrentPhase, out var storyID))
            {
                progress.CompleteStory(storyID);
            }

            // 记录旧阶段
            StoryPhase oldPhase = progress.CurrentPhase;

            // 推进到下一阶段
            progress.CurrentPhase = PhaseOrder[currentIdx + 1];

            // 触发阶段进入事件
            OnPhaseEnter(player, progress.CurrentPhase, oldPhase);
        }

        /// <summary> 直接设置阶段（用于调试或特殊跳转） </summary>
        public void SetPhase(Player player, StoryPhase phase)
        {
            var progress = GetProgress(player);
            StoryPhase oldPhase = progress.CurrentPhase;
            progress.CurrentPhase = phase;
            OnPhaseEnter(player, phase, oldPhase);
        }

        /// <summary> 标记某个剧情为已完成 </summary>
        public void CompleteStory(Player player, string storyID)
        {
            var progress = GetProgress(player);
            progress.CompleteStory(storyID);

            // 检查是否当前阶段的剧情已完成，自动推进
            if (PhaseStoryMap.TryGetValue(progress.CurrentPhase, out var phaseStory)
                && phaseStory == storyID)
            {
                AdvancePhase(player);
            }
        }

        /// <summary> 设置剧情标记 </summary>
        public void SetStoryFlag(Player player, string key, string value)
        {
            GetProgress(player).SetFlag(key, value);
        }

        /// <summary> 获取剧情标记 </summary>
        public string GetStoryFlag(Player player, string key)
        {
            return GetProgress(player).GetFlag(key);
        }

        // ============================================================
        // 阶段进入事件
        // ============================================================

        /// <summary>
        /// 当玩家进入新阶段时调用。
        /// 子类可重写此方法以添加阶段进入时的特殊逻辑
        /// （如刷出新的 NPC、解锁区域、全局通知等）。
        /// </summary>
        protected virtual void OnPhaseEnter(Player player, StoryPhase phase, StoryPhase oldPhase)
        {
            // 发布阶段推进事件
            EventBus.Publish(new StoryPhaseAdvancedEvent
            {
                OldPhase = (int)oldPhase,
                NewPhase = (int)phase,
                PlayerID = player.whoAmI
            });

            // 显示Toast通知
            var uiSystem = ModContent.GetInstance<global::VerminLordMod.Common.UI.StoryUI.StoryUISystem>();
            string phaseName = phase switch
            {
                StoryPhase.Arrival => "初入山寨",
                StoryPhase.AwakeningCeremony => "开窍仪式",
                StoryPhase.SchoolTraining => "学堂历练",
                StoryPhase.MedicineRequest => "药堂求助",
                StoryPhase.JiaJinShengDeath => "贾金生之死",
                StoryPhase.HuaJiuInheritance => "花酒行者传承",
                StoryPhase.FamilyRecognition => "家族认可",
                StoryPhase.PreTournament => "三寨大比前夕",
                StoryPhase.TournamentFinal => "三寨大比决赛",
                StoryPhase.TianHeAttack => "天鹤来袭！",
                StoryPhase.BaiNingBingIceSeal => "白凝冰的抉择",
                StoryPhase.BloodSacrifice => "血祭之夜",
                StoryPhase.LeftQingMao => "离开青茅山",
                StoryPhase.SouthBorderArrival => "到达南疆",
                StoryPhase.ThreeKingsInheritance => "三王传承",
                StoryPhase.ChunQiuChanFragment => "春秋蝉残影",
                StoryPhase.YiTianShanAppears => "义天山异变！",
                StoryPhase.FangYuanReveal => "方源真面目！",
                StoryPhase.NorthDesertArrival => "到达北原",
                StoryPhase.TaiBaiYunShengDeath => "太白云生",
                StoryPhase.DestinyWarBegin => "宿命大战！",
                StoryPhase.FactionChoice => "阵营选择",
                StoryPhase.LongGongPhase1 => "龙公之战",
                StoryPhase.ChunQiuRebirth => "春秋蝉回溯！",
                StoryPhase.DestinyShattered => "宿命碎裂！",
                StoryPhase.Ascension => "升仙！",
                StoryPhase.SevenTurnBegin => "七转·蛊仙之路",
                StoryPhase.DaoLordChallenge => "道主争夺",
                StoryPhase.VenerableBattle => "尊者之战",
                StoryPhase.TenTurnFinale => "十转终局",
                StoryPhase.EndingChosen => "结局",
                _ => phase.ToString()
            };

            Color msgColor = phase switch
            {
                StoryPhase.TianHeAttack or StoryPhase.BloodSacrifice or StoryPhase.DestinyWarBegin
                    => Color.Red,
                StoryPhase.Ascension or StoryPhase.DestinyShattered or StoryPhase.ChunQiuRebirth
                    => Color.Gold,
                _ => Color.Gold
            };

            Main.NewText($"[蛊世界] 剧情推进——{phaseName}", msgColor);

            if (uiSystem != null)
            {
                uiSystem.ShowToast(phaseName, msgColor, 6f);
            }

            // ==================== 阶段推进世界效果 ====================
            ApplyPhaseWorldEffects(player, phase);
        }

        /// <summary>
        /// 根据阶段应用世界变化效果（刷NPC、给物品、解锁系统等）
        /// </summary>
        private void ApplyPhaseWorldEffects(Player player, StoryPhase phase)
        {
            switch (phase)
            {
                // === Stage1 ===
                case StoryPhase.AwakeningCeremony:
                    // 开窍仪式完成，解锁空窍系统
                    GiveItemSafe(player, "开窍丹", 1);
                    break;

                case StoryPhase.HuaJiuInheritance:
                    // 花酒传承，获得洗髓蛊
                    GiveItemSafe(player, "洗髓蛊", 1);
                    break;

                case StoryPhase.FamilyRecognition:
                    // 家族认可，获得家族令
                    GiveItemSafe(player, "古月家族令", 1);
                    break;

                // === Stage2 ===
                case StoryPhase.PreTournament:
                    // 三寨大比前夕，获得参赛资格
                    GiveItemSafe(player, "三寨大比令", 1);
                    break;

                case StoryPhase.BloodSacrifice:
                    // 血祭之夜，世界变暗
                    // 天气联动由StoryWeatherIntegration处理
                    break;

                case StoryPhase.LeftQingMao:
                    // 离开青茅山，获得推荐信
                    GiveItemSafe(player, "推荐信", 1);
                    break;

                // === Stage3 ===
                case StoryPhase.SouthBorderArrival:
                    // 到达南疆，解锁散修声望
                    break;

                case StoryPhase.ChunQiuChanFragment:
                    // 获得春秋蝉残影
                    GiveItemSafe(player, "春秋蝉·残影", 1);
                    break;

                // === Stage4 ===
                case StoryPhase.YiTianShanAppears:
                    // 义天山异变，获得义天山令
                    GiveItemSafe(player, "义天山令", 1);
                    break;

                case StoryPhase.FangYuanReveal:
                    // 方源暴露，获得影宗情报
                    break;

                // === Stage5 ===
                case StoryPhase.NorthDesertArrival:
                    // 到达北原
                    break;

                case StoryPhase.HeavenPrelude:
                    // 天庭前奏，获得天庭通告
                    GiveItemSafe(player, "天庭通告", 1);
                    break;

                // === Stage6 ===
                case StoryPhase.DestinyWarBegin:
                    // 宿命大战开始
                    break;

                case StoryPhase.ChunQiuRebirth:
                    // 春秋蝉回溯，获得宿命回溯Buff
                    player.AddBuff(Terraria.ID.BuffID.IceBarrier, 18000); // 5分钟防护
                    break;

                case StoryPhase.Ascension:
                    // 升仙完成
                    global::VerminLordMod.Common.Systems.DownBossSystem.hasAscended = true;
                    break;

                // === Stage7 ===
                case StoryPhase.SevenTurnBegin:
                    // 七转开始，获得仙窍入口
                    GiveItemSafe(player, "仙窍入口", 1);
                    break;
            }
        }

        /// <summary> 安全地给予玩家物品（通过名称查找） </summary>
        private void GiveItemSafe(Player player, string itemName, int count)
        {
            // 尝试通过物品名称找到对应的ModItem
            foreach (var modItem in ModContent.GetContent<ModItem>())
            {
                if (modItem.Name == itemName || modItem.Item.Name == itemName)
                {
                    player.QuickSpawnItem(null, modItem.Item.type, count);
                    return;
                }
            }
            // 如果找不到对应物品，只显示消息
            Main.NewText($"[蛊世界] 获得：{itemName}×{count}", Microsoft.Xna.Framework.Color.Yellow);
        }

        // ============================================================
        // NPC 绑定
        // ============================================================

        /// <summary>
        /// 根据当前阶段，为 NPC 绑定对应的剧情对话提供者。
        /// 返回应该绑定到该 NPC 的 StoryDialogueProvider，如果没有则返回 null。
        /// </summary>
        public StoryDialogueProvider GetStoryForNPC(Player player, NPC npc)
        {
            var phase = GetPhase(player);

            // 检查 NPC 类型，返回对应阶段的剧情
            if (npc.ModNPC is GuMasterBase guMaster)
            {
                string npcTypeName = guMaster.GetType().Name;

                switch (phase)
                {
                    // === Stage1: 一转·蛊师入门 ===
                    case StoryPhase.Arrival:
                        if (npcTypeName.Contains("PatrolGuMaster") || npcTypeName.Contains("Commoner"))
                            return CreateProvider<GuYueVillageEntry>();
                        break;

                    case StoryPhase.AwakeningCeremony:
                    case StoryPhase.SchoolTraining:
                        if (npcTypeName.Contains("SchoolElder") || npcTypeName.Contains("FistInstructor"))
                            return CreateProvider<GuYueSchoolTrial>();
                        break;

                    case StoryPhase.MedicineRequest:
                        if (npcTypeName.Contains("MedicineElder") || npcTypeName.Contains("MedicinePulseElder"))
                            return CreateProvider<GuYueMedicineRequest>();
                        break;

                    case StoryPhase.FamilyRecognition:
                        if (npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueFamilyRecognition>();
                        break;

                    // === Stage2: 二转·家族争锋 ===
                    case StoryPhase.PreTournament:
                    case StoryPhase.TournamentBegin:
                    case StoryPhase.TournamentFinal:
                        if (npcTypeName.Contains("Chief") || npcTypeName.Contains("GuYueBo"))
                            return CreateProvider<GuYueFamilyRecognition>();
                        break;

                    case StoryPhase.TianHeAttack:
                        if (npcTypeName.Contains("DefenseElder") || npcTypeName.Contains("Chief"))
                            return CreateProvider<Stage2TianHeAttack>();
                        break;

                    case StoryPhase.BaiNingBingIceSeal:
                        if (npcTypeName.Contains("BaiNingBing") || npcTypeName.Contains("Bai"))
                            return CreateProvider<Stage2BaiNingBing>();
                        break;

                    case StoryPhase.BloodSacrifice:
                        if (npcTypeName.Contains("FangYuan") || npcTypeName.Contains("GuYueFangYuan"))
                            return CreateProvider<Stage2BloodSacrifice>();
                        if (npcTypeName.Contains("DefenseElder") || npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueCrisis>();
                        break;

                    case StoryPhase.LeftQingMao:
                        if (npcTypeName.Contains("Chief"))
                            return CreateProvider<GuYueFinale>();
                        break;

                    // === Stage3: 三转·南疆流浪 ===
                    case StoryPhase.SouthBorderArrival:
                        if (npcTypeName.Contains("TaiBaiYunSheng"))
                            return CreateProvider<Stage3SouthBorderArrival>();
                        break;

                    case StoryPhase.ShangXinCiMeet:
                        if (npcTypeName.Contains("ShangXinCi"))
                            return CreateProvider<Stage3ShangXinCiMeet>();
                        break;

                    case StoryPhase.ThreeKingsInheritance:
                    case StoryPhase.ChunQiuChanFragment:
                    case StoryPhase.SanXiuCampComplete:
                        if (npcTypeName.Contains("TaiBaiYunSheng"))
                            return CreateProvider<Stage3SouthBorderArrival>();
                        if (npcTypeName.Contains("ShangXinCi"))
                            return CreateProvider<Stage3ShangXinCiMeet>();
                        break;

                    // === Stage4: 四转·义天山大战 ===
                    case StoryPhase.YiTianShanAppears:
                    case StoryPhase.YiTianShanDungeon:
                        if (npcTypeName.Contains("TaiBaiYunSheng"))
                            return CreateProvider<Stage4YiTianShanAppears>();
                        break;

                    case StoryPhase.DaTongFeng:
                    case StoryPhase.FangYuanReveal:
                    case StoryPhase.YiTianShanComplete:
                        if (npcTypeName.Contains("FangYuan") || npcTypeName.Contains("GuYueFangYuan"))
                            return CreateProvider<Stage2BloodSacrifice>();
                        if (npcTypeName.Contains("TaiBaiYunSheng"))
                            return CreateProvider<Stage4YiTianShanAppears>();
                        break;

                    // === Stage5: 五转·北原争霸 ===
                    case StoryPhase.NorthDesertArrival:
                    case StoryPhase.WangTingAlly:
                        if (npcTypeName.Contains("HeiLouLan"))
                            return CreateProvider<Stage5NorthDesertArrival>();
                        break;

                    case StoryPhase.ChangShengTianContact:
                    case StoryPhase.TaiBaiYunShengDeath:
                    case StoryPhase.ImmortalZombieChoice:
                    case StoryPhase.HeavenPrelude:
                        if (npcTypeName.Contains("HeiLouLan"))
                            return CreateProvider<Stage5NorthDesertArrival>();
                        break;

                    // === Stage6: 六转·宿命大战与升仙 ===
                    case StoryPhase.DestinyWarBegin:
                    case StoryPhase.FourPillarsDown:
                    case StoryPhase.FactionChoice:
                    case StoryPhase.LongGongPhase1:
                    case StoryPhase.ChunQiuRebirth:
                    case StoryPhase.LongGongPhase2:
                    case StoryPhase.DestinyShattered:
                    case StoryPhase.Ascension:
                        // Stage6 使用通用对话，由具体Boss/NPC触发
                        break;

                    // === Stage7: 七转以上·蛊仙之路 ===
                    case StoryPhase.SevenTurnBegin:
                    case StoryPhase.ApertureBuilt:
                    case StoryPhase.EightTurnBegin:
                    case StoryPhase.DaoLordChallenge:
                    case StoryPhase.NineTurnBegin:
                    case StoryPhase.VenerableBattle:
                    case StoryPhase.TenTurnFinale:
                    case StoryPhase.EndingChosen:
                        // Stage7 全部原创，后续扩展
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建剧情提供者实例并绑定到 NPC
        /// </summary>
        private StoryDialogueProvider CreateProvider<T>() where T : StoryDialogueProvider, new()
        {
            return new T();
        }

        // ============================================================
        // 持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            var progressList = new List<TagCompound>();
            foreach (var kvp in _playerProgress)
            {
                var data = kvp.Value.Save();
                data["playerName"] = kvp.Key;
                progressList.Add(data);
            }
            tag["storyProgress"] = progressList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _playerProgress.Clear();

            if (tag.TryGet("storyProgress", out List<TagCompound> progressList))
            {
                foreach (var data in progressList)
                {
                    string playerName = data.GetString("playerName");
                    var progress = PlayerStoryProgress.Load(data);
                    _playerProgress[playerName] = progress;
                }
            }
        }
    }
}

// ===== 临时占位：尚未实现的剧情 =====
// 这些将在后续步骤中实现
namespace VerminLordMod.Common.DialogueTree
{
    /// <summary> 家族认可剧情（占位） </summary>
    public class GuYueFamilyRecognition : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFamilyRecognition";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月族长端坐在议事厅主位上，示意你上前。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}微笑着看向你：\"听说你在学堂和药堂的表现都不错。" +
                "作为族长，我很高兴看到有才华的年轻人加入我们古月家族。\"\n\n" +
                "他站起身，走到你面前：\"从今天起，你就是我古月家族的正式客卿了。" +
                "希望你能为家族的发展出一份力。\"")
                .AddOption("多谢族长！", "accept", DialogueOptionType.Exit,
                    tooltip: "接受客卿身份")
                .AddOption("我需要考虑一下", "decline", DialogueOptionType.Exit,
                    tooltip: "婉拒客卿身份");

            b.StartNode("accept",
                "{npcName}拍了拍你的肩膀：\"好！明日我让人给你安排住处。" +
                "家族藏经阁对你开放，你可以随意查阅。\"")
                .EndsDialogue();

            b.StartNode("decline",
                "{npcName}有些意外，但并未强求：\"也罢，人各有志。" +
                "古月山寨的大门永远为你敞开。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary> 危机降临剧情（占位） </summary>
    public class GuYueCrisis : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueCrisis";
        protected override string DisplayName => "古月御堂家老";
        protected override string GreetingText =>
            "御堂家老神色凝重地站在寨墙上，眺望远方。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}看到你来，急促地说：\"你来得正好！" +
                "探子来报，有一伙来历不明的蛊师正在向山寨逼近。" +
                "人数不少，来者不善！\"\n\n" +
                "他握紧腰间的蛊虫袋：\"我需要你帮忙防守山寨。" +
                "愿意助我一臂之力吗？\"")
                .AddOption("义不容辞！", "defend", DialogueOptionType.Combat,
                    tooltip: "参与山寨防御")
                .AddOption("我去通知其他人", "alert", DialogueOptionType.Quest,
                    tooltip: "帮忙疏散民众");

            b.StartNode("defend",
                "{npcName}重重地点头：\"好！随我来寨墙！\"")
                .EndsDialogue();

            b.StartNode("alert",
                "{npcName}松了口气：\"也好，你去通知寨民躲进地窖。" +
                "我在这里守着。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary> 终章剧情（占位） </summary>
    public class GuYueFinale : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFinale";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月族长在议事厅设宴，庆祝危机解除。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");
            b.StartNode("greeting",
                "{npcName}举起酒杯：\"这次多亏了你，古月山寨才能化险为夷。" +
                "我代表整个古月家族，敬你一杯！\"\n\n" +
                "他郑重地取出一块令牌：\"这是古月家族的客卿令牌。" +
                "从今往后，你就是我古月家族最尊贵的客人。" +
                "无论你走到哪里，只要出示此令，古月弟子必当鼎力相助！\"")
                .AddOption("谢族长！", "end", DialogueOptionType.Exit);

            b.StartNode("end",
                "宴会上欢声笑语不断。你看着手中的令牌，知道自己在古月山寨的旅程告一段落。" +
                "但在这片广袤的天地间，还有更多的故事等待着你……")
                .EndsDialogue();

            return b.Build();
        }
    }
}