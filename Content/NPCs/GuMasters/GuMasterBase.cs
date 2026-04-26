using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    // ============================================================
    // GuMasterBase - 蛊师NPC抽象基类
    // 
    // 整合了：
    // - IGuMasterAI 智能接口（感知/信念/决策/行为）
    // - BeliefState 信念黑箱（替代确定性态度计算）
    // - GuWorldPlayer 声望系统（交互处理）
    // - 修为系统（QiPlayer 真元关联）
    // - 对话/战斗双模式
    //
    // 子类只需重写：
    // - SetupGuMaster() - 设置蛊师特有属性
    // - GetPersonality() - 返回性格
    // - GetFaction() - 返回所属势力
    // - GetRank() - 返回修为等级
    // ============================================================

    public abstract class GuMasterBase : ModNPC, IGuMasterAI
    {
        // ===== 蛊师属性 =====
        public abstract FactionID GetFaction();
        public abstract GuRank GetRank();
        public abstract GuPersonality GetPersonality();

        public virtual string GuMasterDisplayName => GuWorldSystem.GetFactionDisplayName(GetFaction()) + "蛊师";
        public virtual int GuMasterDamage => 15;
        public virtual int GuMasterLife => 150;
        public virtual int GuMasterDefense => 10;

        // ===== 运行时状态 =====
        public GuMasterAIState CurrentAIState = GuMasterAIState.Idle;
        public GuAttitude CurrentAttitude = GuAttitude.Ignore;
        public bool HasBeenHitByPlayer = false;
        public int AggroTimer = 0;
        public int TalkCount = 0;

        // ===== 首次被击中的话语标记（每个NPC一生只触发一次） =====
        public bool HasSpokenFirstHitLine = false;

        // ===== 首次目击族人被攻击的话语标记（每个NPC一生只触发一次） =====
        public bool HasSpokenWitnessLine = false;

        // ===== 弹幕保护系统 =====
        /// <summary> 玩家对此NPC的弹幕保护是否开启（默认开启） </summary>
        public bool ProjectileProtectionEnabled = true;

        // ===== 信念系统（黑暗森林核心） =====
        /// <summary> 对每个玩家的信念状态（以玩家名为key） </summary>
        public Dictionary<string, BeliefState> PlayerBeliefs = new Dictionary<string, BeliefState>();

        // ===== 对话相关 =====
        public const string ShopName = "GuMasterShop";
        public int NumberOfTimesTalkedTo = 0;

        // ===== 静态默认配置 =====
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers
                {
                    Velocity = 1f,
                    Direction = 1
                };
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1; // 使用自定义AI
            NPC.value = Item.buyPrice(0, 0, 10, 0);
            AnimationType = NPCID.Guide;

            // 根据修为调整属性
            ApplyRankBonuses();
        }

        protected void ApplyRankBonuses()
        {
            int rankBase = (int)GetRank();
            NPC.lifeMax += rankBase * 5;
            NPC.damage += rankBase * 2;
            NPC.defense += rankBase;
        }

        // ============================================================
        // AI 主循环（信念驱动）
        // ============================================================

        public override void AI()
        {
            // 0. 确保 npc.target 指向最近的活跃玩家（非 townNPC 需要手动管理）
            NPC.TargetClosest(true);

            // 1. 感知
            var context = Perceive(NPC);

            // 2. 更新信念（核心新增步骤）
            UpdateBelief(NPC, context);

            // 3. 获取当前玩家的信念状态
            string playerName = Main.LocalPlayer.name;
            var belief = GetBelief(playerName);

            // 4. 计算态度（基于信念分布）
            var attCtx = new AttitudeContext
            {
                WorldPlayer = Main.LocalPlayer.GetModPlayer<GuWorldPlayer>(),
                NpcFaction = GetFaction(),
                Personality = GetPersonality(),
                Rank = GetRank(),
                HasBeenHitByPlayer = HasBeenHitByPlayer,
                AggroTimer = AggroTimer,
                Belief = belief
            };
            CurrentAttitude = CalculateAttitude(NPC, attCtx);

            // 5. 决策
            var decision = Decide(NPC, context);

            // 6. 执行
            ExecuteAI(NPC, decision);

            // 7. 更新计时器
            // 注意：HasBeenHitByPlayer 不再由 AggroTimer 自动重置
            // 改为由 Decide() 中的强制战斗逻辑保证行为正确
            // AggroTimer 仅用于"仇恨持续时长"的参考，到期后 NPC 回到信念驱动
            if (AggroTimer > 0) AggroTimer--;
        }

        // ============================================================
        // IGuMasterAI 实现 - 感知
        // ============================================================

        public virtual PerceptionContext Perceive(NPC npc)
        {
            var player = Main.LocalPlayer;
            float dist = Vector2.Distance(npc.Center, player.Center);
            var qiPlayer = player.GetModPlayer<QiPlayer>();

            return new PerceptionContext
            {
                TargetPlayer = player,
                DistanceToPlayer = dist,
                PlayerLifePercent = (int)((float)player.statLife / player.statLifeMax2 * 100),
                PlayerHasQiEnabled = qiPlayer.qiEnabled,
                NearbyAlliesCount = CountNearbyAllies(npc, 400f),
                NearbyEnemiesCount = CountNearbyEnemies(npc, 400f),
                IsInOwnTerritory = false, // 后续由小世界系统提供
                TimeOfDay = (float)Main.time,
                IsRaining = Main.raining,
                PlayerInfamy = player.GetModPlayer<GuWorldPlayer>().InfamyPoints,
                PlayerQiLevel = qiPlayer.qiLevel,
                PlayerDamage = player.HeldItem.damage
            };
        }

        private int CountNearbyAllies(NPC npc, float range)
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.type == npc.type && Vector2.Distance(npc.Center, other.Center) < range)
                    count++;
            }
            return count;
        }

        private int CountNearbyEnemies(NPC npc, float range)
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.friendly != npc.friendly && Vector2.Distance(npc.Center, other.Center) < range)
                    count++;
            }
            return count;
        }

        // ============================================================
        // IGuMasterAI 实现 - 信念系统
        // ============================================================

        /// <summary> 获取对指定玩家的信念状态（不存在则创建默认） </summary>
        public BeliefState GetBelief(string playerName)
        {
            if (!PlayerBeliefs.ContainsKey(playerName))
                PlayerBeliefs[playerName] = BeliefState.Default(playerName);
            return PlayerBeliefs[playerName];
        }

        /// <summary> 更新对当前目标玩家的信念 </summary>
        public virtual void UpdateBelief(NPC npc, PerceptionContext context)
        {
            string playerName = context.TargetPlayer.name;
            var belief = GetBelief(playerName);

            // 调用工具方法更新信念
            GuAttitudeHelper.UpdateBeliefState(belief, context, false, false);
        }

        // ============================================================
        // IGuMasterAI 实现 - 态度计算（基于信念）
        // ============================================================

        public virtual GuAttitude CalculateAttitude(NPC npc, AttitudeContext context)
        {
            return GuAttitudeHelper.CalculateFromBelief(
                context.Belief,
                context.Personality,
                context.HasBeenHitByPlayer
            );
        }

        // ============================================================
        // IGuMasterAI 实现 - 决策
        // ============================================================

        public virtual Decision Decide(NPC npc, PerceptionContext context)
        {
            var decision = new Decision { NewState = CurrentAIState };

            // ===== 强制战斗检查 =====
            // 如果被玩家攻击过且仇恨未消，强制进入战斗模式
            // 防止 AggroTimer 到期后 NPC 因信念漂移而突然逃跑
            if (HasBeenHitByPlayer && AggroTimer > 0)
            {
                decision.NewState = GuMasterAIState.Combat;
                decision.ShouldAttack = true;
                return decision;
            }

            // 态度驱动决策
            switch (CurrentAttitude)
            {
                case GuAttitude.Hostile:
                    if (context.DistanceToPlayer < 500f)
                    {
                        decision.NewState = GuMasterAIState.Combat;
                        decision.ShouldAttack = true;
                        decision.DialogueLine = "找死！";
                    }
                    else
                    {
                        decision.NewState = GuMasterAIState.Idle;
                    }
                    break;

                case GuAttitude.Wary:
                case GuAttitude.Contemptuous:
                    if (context.DistanceToPlayer < 300f)
                    {
                        decision.NewState = GuMasterAIState.Approach;
                        decision.Interaction = InteractionType.Provoke;
                    }
                    else
                    {
                        decision.NewState = GuMasterAIState.Idle;
                    }
                    break;

                case GuAttitude.Fearful:
                    // 恐惧态度不再逃跑，改为保持距离观察
                    if (context.DistanceToPlayer < 300f)
                    {
                        decision.NewState = GuMasterAIState.Approach;
                        decision.Interaction = InteractionType.Provoke;
                    }
                    else
                    {
                        decision.NewState = GuMasterAIState.Idle;
                    }
                    break;

                case GuAttitude.Friendly:
                case GuAttitude.Respectful:
                    if (context.DistanceToPlayer < 200f)
                    {
                        decision.NewState = GuMasterAIState.Talk;
                        decision.Interaction = InteractionType.Talk;
                    }
                    else if (context.DistanceToPlayer < 400f)
                    {
                        decision.NewState = GuMasterAIState.Approach;
                    }
                    else
                    {
                        decision.NewState = GuMasterAIState.Idle;
                    }
                    break;

                default: // Ignore
                    decision.NewState = GuMasterAIState.Idle;
                    break;
            }

            return decision;
        }

        // ============================================================
        // IGuMasterAI 实现 - 行为执行
        // ============================================================

        public virtual void ExecuteAI(NPC npc, Decision decision)
        {
            CurrentAIState = decision.NewState;

            switch (decision.NewState)
            {
                case GuMasterAIState.Idle:
                    // 简单闲逛
                    npc.velocity.X *= 0.95f;
                    if (Main.rand.NextBool(200))
                        npc.velocity.X = Main.rand.NextBool() ? 1 : -1;
                    break;

                case GuMasterAIState.Approach:
                    // 走向玩家
                    var player = Main.player[npc.target];
                    float dir = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.velocity.X = dir * 1.5f;
                    npc.spriteDirection = (int)dir;
                    break;

                case GuMasterAIState.Combat:
                    if (decision.ShouldAttack)
                        ExecuteCombatAI(npc);
                    break;

                // Flee 状态已废弃：NPC不再逃跑，恐惧态度改为保持距离观察

                case GuMasterAIState.Talk:
                    npc.velocity.X *= 0.9f;
                    break;
            }
        }

        /// <summary> 战斗AI - 子类可重写 </summary>
        public virtual void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            // 追逐玩家
            float dir = target.Center.X > npc.Center.X ? 1 : -1;
            npc.velocity.X = dir * 2f;
            npc.spriteDirection = (int)dir;

            // 跳跃
            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -6f;
        }

        // ============================================================
        // IGuMasterAI 实现 - 交互处理
        // ============================================================

        public virtual InteractionResult HandleInteraction(NPC npc, Player player, InteractionType interaction)
        {
            var result = new InteractionResult { Success = true };
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();

            switch (interaction)
            {
                case InteractionType.Talk:
                    result.Message = GetDialogue(npc, CurrentAttitude);
                    break;

                case InteractionType.Trade:
                    if (CurrentAttitude == GuAttitude.Hostile || CurrentAttitude == GuAttitude.Wary)
                    {
                        result.Success = false;
                        result.Message = "对方不愿意与你交易。";
                    }
                    else
                    {
                        result.Message = "打开交易界面...";
                        // 后续由NPCShop实现
                    }
                    break;

                case InteractionType.Attack:
                    HasBeenHitByPlayer = true;
                    AggroTimer = 1800; // 30秒仇恨
                    // 注意：恶名不在攻击时增加，只在击杀且有目击者时增加（见 OnKill）

                    // 进入战斗状态后自动去除弹幕保护，让玩家也能打他
                    ProjectileProtectionEnabled = false;

                    // 弹幕保护关闭时被攻击 → 震惊反应
                    if (!ProjectileProtectionEnabled)
                    {
                        // 弹出震惊感叹号特效
                        SpawnShockEffect(NPC.Center);

                        // 第一次被击中时头顶飘出随机话语（仅一次）
                        if (!HasSpokenFirstHitLine)
                        {
                            HasSpokenFirstHitLine = true;
                            string[] hitLines = new string[]
                            {
                                "你竟敢偷袭我！",
                                "好胆！",
                                "找死！",
                                "哼，早就防着你了！",
                                "大胆！",
                                "你找死！"
                            };
                            string hitLine = hitLines[Main.rand.Next(hitLines.Length)];
                            CombatText.NewText(NPC.getRect(), Color.OrangeRed, hitLine, true);
                        }

                        // 无论自信与否，都直接反击（站定发射弹幕）
                        result.Message = "受到攻击！";
                        result.TriggerCombat = true;
                    }
                    else
                    {
                        // 有保护时不应该走到这里（ModifyHitByProjectile 已拦截），但近战攻击不受保护
                        result.Message = "你攻击了" + GuMasterDisplayName + "！";
                        result.TriggerCombat = true;
                    }

                    // 通知附近同阵营NPC：有人被攻击了，一起参战
                    AlertNearbyAllies(NPC, 500f);
                    break;

                case InteractionType.Provoke:
                    if (GetPersonality() == GuPersonality.Proud)
                    {
                        HasBeenHitByPlayer = true;
                        AggroTimer = 300;
                        result.TriggerCombat = true;
                        result.Message = GuMasterDisplayName + "被激怒了！";
                    }
                    break;

                case InteractionType.Ally:
                    result.Success = worldPlayer.FormAlliance(GetFaction());
                    break;

                case InteractionType.Betray:
                    result.Success = worldPlayer.BetrayAlly(GetFaction());
                    if (result.Success)
                    {
                        HasBeenHitByPlayer = true;
                        AggroTimer = 1200;
                        result.TriggerCombat = true;
                    }
                    break;

                case InteractionType.Bribe:
                    if (GetPersonality() == GuPersonality.Greedy)
                    {
                        worldPlayer.AddReputation(GetFaction(), 30, "贿赂");
                        result.Message = GuMasterDisplayName + "收下了你的贿赂。";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = GuMasterDisplayName + "拒绝了你的贿赂。";
                    }
                    break;
            }

            return result;
        }

        // ============================================================
        // IGuMasterAI 实现 - 对话
        // ============================================================

        public virtual string GetDialogue(NPC npc, GuAttitude attitude)
        {
            return GuAttitudeHelper.GetDefaultDialogue(attitude, GuMasterDisplayName);
        }

        public virtual void GetChatButtons(NPC npc, ref string button, ref string button2)
        {
            button = "对话";
            if (CurrentAttitude != GuAttitude.Hostile && CurrentAttitude != GuAttitude.Wary)
            {
                // 根据弹幕保护状态显示不同的按钮文本
                if (ProjectileProtectionEnabled)
                    button2 = "去除保护";
                else
                    button2 = "开启保护";
            }
        }

        public virtual void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                // 对话
                Main.npcChatText = GetDialogue(npc, CurrentAttitude);
            }
            else
            {
                // 切换弹幕保护状态
                ProjectileProtectionEnabled = !ProjectileProtectionEnabled;
                if (ProjectileProtectionEnabled)
                {
                    Main.npcChatText = "你重新开启了弹幕保护。";
                }
                else
                {
                    Main.npcChatText = "你悄悄去除了弹幕保护。";
                }
            }
        }

        // ============================================================
        // tModLoader 钩子重写
        // ============================================================

        public override bool CanChat()
        {
            // 只有非敌对状态才能对话
            return CurrentAttitude != GuAttitude.Hostile;
        }

        public override string GetChat()
        {
            return GetDialogue(NPC, CurrentAttitude);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            GetChatButtons(NPC, ref button, ref button2);
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            OnChatButtonClicked(NPC, firstButton, ref shop);
        }

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 近战攻击不受弹幕保护影响，直接处理
            HandleInteraction(NPC, player, InteractionType.Attack);
        }

        /// <summary>
        /// 弹幕保护开启时，完全阻止弹幕击中NPC（连1点强制伤害都没有）
        /// 保护关闭时，允许弹幕击中并触发反击
        /// </summary>
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            // 只拦截玩家发射的弹幕
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                var player = Main.player[projectile.owner];
                if (player.active && ProjectileProtectionEnabled)
                {
                    // 弹幕保护开启：完全免疫玩家弹幕
                    return false;
                }
            }
            return base.CanBeHitByProjectile(projectile);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                var player = Main.player[projectile.owner];
                if (player.active)
                {
                    // 能走到这里说明保护已关闭（CanBeHitByProjectile 已拦截保护开启的情况）
                    // 正常处理攻击：触发反击
                    HandleInteraction(NPC, player, InteractionType.Attack);
                }
            }
        }

        public override void OnKill()
        {
            // ===== 目击者检查 =====
            // 检查击杀者是否是玩家
            int? killerPlayerId = WhoKilledMe();
            if (killerPlayerId.HasValue)
            {
                var killer = Main.player[killerPlayerId.Value];
                if (killer.active)
                {
                    var worldPlayer = killer.GetModPlayer<GuWorldPlayer>();

                    // 检查是否有目击者（同家族NPC在附近）
                    if (HasWitnesses(400f))
                    {
                        worldPlayer.AddInfamy(10);
                        worldPlayer.RemoveReputation(GetFaction(), 20, "击杀成员");
                        Main.NewText($"你击杀了{GuMasterDisplayName}！{GuWorldSystem.GetFactionDisplayName(GetFaction())}声望下降。", Color.OrangeRed);
                    }
                    else
                    {
                        // 无目击者：秘密击杀，不扣声望也不增加恶名
                        Main.NewText($"你秘密击杀了{GuMasterDisplayName}，无人知晓。", Color.Gray);
                    }
                }
            }
            // 非玩家击杀（怪物等）→ 不扣玩家声望
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 默认：地表，玩家已开启空窍
            var qiPlayer = spawnInfo.Player.GetModPlayer<QiPlayer>();
            if (!qiPlayer.qiEnabled) return 0f;
            return 0.03f;
        }

        // ============================================================
        // 商店系统（子类可重写）
        // ============================================================

        public override void AddShops()
        {
            // 子类实现具体商店内容
        }

        // ============================================================
        // 持久化（信念数据）
        // ============================================================

        public override void SaveData(TagCompound tag)
        {
            var beliefsData = new List<TagCompound>();
            foreach (var (playerName, belief) in PlayerBeliefs)
            {
                beliefsData.Add(new TagCompound
                {
                    ["playerName"] = playerName,
                    ["riskThreshold"] = belief.RiskThreshold,
                    ["confidenceLevel"] = belief.ConfidenceLevel,
                    ["observationCount"] = belief.ObservationCount,
                    ["estimatedPower"] = belief.EstimatedPower,
                    ["hasTraded"] = belief.HasTraded,
                    ["hasFought"] = belief.HasFought,
                    ["wasDefeated"] = belief.WasDefeated,
                    ["hasDefeatedPlayer"] = belief.HasDefeatedPlayer,
                    ["lastInteractionDay"] = belief.LastInteractionDay
                });
            }
            tag["playerBeliefs"] = beliefsData;
        }

        public override void LoadData(TagCompound tag)
        {
            PlayerBeliefs.Clear();
            if (tag.TryGet("playerBeliefs", out List<TagCompound> beliefsData))
            {
                foreach (var entry in beliefsData)
                {
                    var belief = new BeliefState
                    {
                        PlayerName = entry.GetString("playerName"),
                        RiskThreshold = entry.GetFloat("riskThreshold"),
                        ConfidenceLevel = entry.GetFloat("confidenceLevel"),
                        ObservationCount = entry.GetInt("observationCount"),
                        EstimatedPower = entry.GetFloat("estimatedPower"),
                        HasTraded = entry.GetBool("hasTraded"),
                        HasFought = entry.GetBool("hasFought"),
                        WasDefeated = entry.GetBool("wasDefeated"),
                        HasDefeatedPlayer = entry.GetBool("hasDefeatedPlayer"),
                        LastInteractionDay = entry.GetInt("lastInteractionDay")
                    };
                    PlayerBeliefs[belief.PlayerName] = belief;
                }
            }
        }

        // ============================================================
        // 工具方法
        // ============================================================

        /// <summary> 获取附近同家族NPC数量 </summary>
        protected int GetNearbyFamilyCount(float range = 400f)
        {
            return CountNearbyAllies(NPC, range);
        }

        /// <summary> 发送聊天消息 </summary>
        protected void Say(string message, Color? color = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(message, color ?? Color.White);
        }

        /// <summary> 通知附近同阵营NPC：有人被攻击了，一起参战 </summary>
        /// <summary>
        /// 通知附近同阵营NPC：有人被玩家攻击了
        /// 目击到攻击事件的同阵营NPC直接进入战斗状态（HasBeenHitByPlayer = true）
        /// 因为亲眼看到玩家攻击同家族成员，这是明确的敌对行为
        /// </summary>
        protected void AlertNearbyAllies(NPC npc, float range)
        {
            // 目击者头顶飘出的话语
            string[] witnessLines = new string[]
            {
                "好大的胆子！",
                "竟敢伤我族人！",
                "拿下他！",
                "找死！",
                "兄弟们上！",
                "休要猖狂！"
            };

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.whoAmI != npc.whoAmI && other.ModNPC is GuMasterBase ally)
                {
                    if (ally.GetFaction() == GetFaction() &&
                        Vector2.Distance(npc.Center, other.Center) < range)
                    {
                        // 目击到同家族成员被玩家攻击 → 直接参战
                        // 这是"目击者参战"逻辑，不是"以多欺少"逻辑
                        // 只有在玩家实际攻击时才会触发（AlertNearbyAllies 只在 HandleInteraction(Attack) 中被调用）
                        ally.HasBeenHitByPlayer = true;
                        ally.AggroTimer = 1800;
                        ally.ProjectileProtectionEnabled = false;

                        // 每个目击者只有第一次看到族人被打时触发话语
                        if (!ally.HasSpokenWitnessLine)
                        {
                            ally.HasSpokenWitnessLine = true;
                            string witnessLine = witnessLines[Main.rand.Next(witnessLines.Length)];
                            CombatText.NewText(other.getRect(), Color.OrangeRed, witnessLine, true);
                        }
                    }
                }
            }
        }

        /// <summary> 检查是否有同家族NPC在附近（目击者） </summary>
        protected bool HasWitnesses(float range)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.ModNPC is GuMasterBase guMaster)
                {
                    if (guMaster.GetFaction() == GetFaction() &&
                        Vector2.Distance(NPC.Center, other.Center) < range)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary> 获取击杀此NPC的玩家ID（如果有） </summary>
        protected int? WhoKilledMe()
        {
            // 使用 tML 提供的 lastInteraction 追踪最后交互的玩家
            if (NPC.lastInteraction >= 0 && NPC.lastInteraction < Main.maxPlayers)
            {
                var player = Main.player[NPC.lastInteraction];
                if (player.active)
                    return NPC.lastInteraction;
            }
            return null;
        }

        // ============================================================
        // 特效方法
        // ============================================================

        /// <summary> 在NPC位置生成震惊感叹号特效 </summary>
        protected void SpawnShockEffect(Vector2 position)
        {
            // 使用 Dust 模拟感叹号爆炸效果
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(position, 10, 10, DustID.Torch,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    100, Color.Yellow, 1.8f);
            }
            // 显示感叹号战斗文本
            CombatText.NewText(NPC.getRect(), Color.Yellow, "！", true);
        }
    }
}
