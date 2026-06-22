using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.DialogueTreeUI;
using Terraria.GameContent;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    // ============================================================
    // GuMasterBase - 蛊师NPC抽象基类（简化版）
    // 
    // 核心设计：信任值驱动态度系统
    // - 每个NPC对每位玩家有独立的信任值 [0, 100]
    // - 交互事件直接影响信任值（攻击-50，交易+20，对话+10）
    // - 信任值映射为5档态度（Hostile/Wary/Ignore/Friendly/Respectful）
    // 
	    // 注意：保留 IGuMasterAI 接口供对话树/效果系统使用，
	    // 但核心AI逻辑使用信任值驱动（信念状态作为对话系统的持久化兼容层）
	    //
	    // 子类只需重写：
	    // - GetFaction() - 所属势力
	    // - GetRank() - 修为等级
	    // - GetPersonality() - 性格
	    // ============================================================

	    public abstract class GuMasterBase : ModNPC, IGuMasterAI
    {
        // ===== 蛊师属性（子类必须/可重写）=====
        public abstract FactionID GetFaction();
        public abstract GuRank GetRank();
        public abstract GuPersonality GetPersonality();

        public virtual string GuMasterDisplayName => WorldStateMachine.GetFactionDisplayName(GetFaction()) + "蛊师";
        public virtual int GuMasterDamage => 15;
        public virtual int GuMasterLife => 150;
        public virtual int GuMasterDefense => 10;

        public override string HeadTexture => "VerminLordMod/" + GetType().FullName.Substring("VerminLordMod.".Length).Replace('.', '/') + "_Head";

        // ===== 运行时状态 =====
        public GuAttitude CurrentAttitude = GuAttitude.Ignore;
        public GuMasterAIState CurrentAIState = GuMasterAIState.Idle; // 兼容字段（子类/调试器引用）
        public bool HasBeenHitByPlayer = false;
        public int AggroTimer = 0;
        public int TalkCount = 0;

		// ===== 信任值系统 =====
		/// <summary>每个玩家对此NPC的信任值 [0, 100]，默认50</summary>
		public Dictionary<string, float> TrustPerPlayer = new Dictionary<string, float>();
		public const float DefaultTrust = 50f;
		public const float TrustDecayPerFrame = 0.001f;
		public const float TrustMax = 100f;
		public const float TrustMin = 0f;

		// ===== 信念系统（对话系统持久化兼容层）=====
		/// <summary>每个玩家对此NPC的信念状态（对话系统使用，由信任值初始化）</summary>
		public Dictionary<string, BeliefState> PlayerBeliefs = new Dictionary<string, BeliefState>();

        // ===== 首次被击中的话语标记 =====
        public bool HasSpokenFirstHitLine = false;
        public bool HasSpokenWitnessLine = false;

        // ===== 弹幕保护系统 =====
        public bool ProjectileProtectionEnabled = true;

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
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 0, 10, 0);
            AnimationType = NPCID.Guide;

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
        // 信任值系统
        // ============================================================

        /// <summary>获取对指定玩家的信任值（不存在则创建默认值）</summary>
        public float GetTrust(string playerName)
        {
            if (!TrustPerPlayer.ContainsKey(playerName))
                TrustPerPlayer[playerName] = DefaultTrust;
            return TrustPerPlayer[playerName];
        }

        /// <summary>修改信任值（自动限制在[0,100]范围内）</summary>
        public void ModifyTrust(string playerName, float delta)
        {
            float current = GetTrust(playerName);
            float next = current + delta;
            TrustPerPlayer[playerName] = MathHelper.Clamp(next, TrustMin, TrustMax);
        }

		/// <summary>
		/// 获取对指定玩家的信念状态（持久化对象，对话系统使用）。
		/// 首次访问时从信任值初始化，后续修改将持久保存。
		/// RiskThreshold = 1.0 - trust/100（信任越高→风险阈值越低→越友好）
		/// </summary>
		public BeliefState GetBelief(string playerName)
		{
			if (PlayerBeliefs.TryGetValue(playerName, out var belief))
				return belief;

			float trust = GetTrust(playerName);
			belief = new BeliefState
			{
				PlayerName = playerName,
				RiskThreshold = 1.0f - trust / 100f,
				ConfidenceLevel = 0.1f + trust / 100f * 0.8f, // 信任越高，置信度越高
				ObservationCount = 1,
				EstimatedPower = 0.5f,
				HasTraded = trust > 60,
				HasFought = HasBeenHitByPlayer,
				WasDefeated = false,
				HasDefeatedPlayer = false,
				LastInteractionDay = (int)(Main.time / 54000)
			};
			PlayerBeliefs[playerName] = belief;
			return belief;
		}

        /// <summary>信任值→态度映射（透明规则）</summary>
        public static GuAttitude TrustToAttitude(float trust, bool isHostile)
        {
            if (isHostile) return GuAttitude.Hostile;
            if (trust >= 80) return GuAttitude.Friendly;
            if (trust >= 50) return GuAttitude.Ignore;
            if (trust >= 20) return GuAttitude.Wary;
            return GuAttitude.Hostile;
        }

        // ============================================================
        // AI 主循环（简化版：信任值驱动）
        // ============================================================

        public override void AI()
        {
            NPC.TargetClosest(true);

            string playerName = Main.LocalPlayer.name;
            float trust = GetTrust(playerName);

            // 信任值自然衰减
            ModifyTrust(playerName, -TrustDecayPerFrame);

            // 更新态度（信任值映射）
            CurrentAttitude = TrustToAttitude(trust, HasBeenHitByPlayer && AggroTimer > 0);

            // 距离驱动行为
            float dist = Vector2.Distance(NPC.Center, Main.LocalPlayer.Center);
            ExecuteBehaviorByAttitude(dist);

            if (AggroTimer > 0) AggroTimer--;
        }

        /// <summary>根据态度和距离执行行为（替代原Decide+ExecuteAI）</summary>
        private void ExecuteBehaviorByAttitude(float dist)
        {
            switch (CurrentAttitude)
            {
                case GuAttitude.Hostile:
                    if (dist < 500f)
                    {
                        ExecuteCombatAI(NPC);
                    }
                    else
                    {
                        // 闲逛
                        NPC.velocity.X *= 0.95f;
                        if (Main.rand.NextBool(200))
                            NPC.velocity.X = Main.rand.NextBool() ? 1 : -1;
                    }
                    break;

                case GuAttitude.Wary:
                    if (dist < 300f)
                    {
                        // 保持距离观察
                        float dir = Main.player[NPC.target].Center.X > NPC.Center.X ? -1 : 1;
                        NPC.velocity.X = dir * 1f;
                        NPC.spriteDirection = (int)-dir;
                    }
                    else
                    {
                        NPC.velocity.X *= 0.95f;
                    }
                    break;

                case GuAttitude.Friendly:
                case GuAttitude.Respectful:
                    if (dist < 200f)
                    {
                        // 靠近对话
                        NPC.velocity.X *= 0.9f;
                    }
                    else if (dist < 400f)
                    {
                        // 走向玩家
                        float dir = Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1;
                        NPC.velocity.X = dir * 1.5f;
                        NPC.spriteDirection = (int)dir;
                    }
                    else
                    {
                        NPC.velocity.X *= 0.95f;
                    }
                    break;

                default: // Ignore
                    NPC.velocity.X *= 0.95f;
                    if (Main.rand.NextBool(200))
                        NPC.velocity.X = Main.rand.NextBool() ? 1 : -1;
                    break;
            }
        }

        /// <summary>战斗AI - 子类可重写</summary>
        public virtual void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            float dir = target.Center.X > npc.Center.X ? 1 : -1;
            npc.velocity.X = dir * 2f;
            npc.spriteDirection = (int)dir;

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -6f;
        }

        // ============================================================
        // 交互处理（简化版：信任值直接影响）
        // ============================================================

        public virtual InteractionResult HandleInteraction(NPC npc, Player player, InteractionType interaction)
        {
            var result = new InteractionResult { Success = true };
            string playerName = player.name;

            switch (interaction)
            {
                case InteractionType.Talk:
                    result.Message = GetDialogue(npc, CurrentAttitude);
                    ModifyTrust(playerName, +10f);
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
                        ModifyTrust(playerName, +20f);
                    }
                    break;

                case InteractionType.Attack:
                    HasBeenHitByPlayer = true;
                    AggroTimer = 1800;
                    ProjectileProtectionEnabled = false;
                    ModifyTrust(playerName, -50f);

                    SpawnShockEffect(NPC.Center);

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
                        CombatText.NewText(NPC.getRect(), Color.OrangeRed, hitLines[Main.rand.Next(hitLines.Length)], true);
                    }

                    result.Message = "受到攻击！";
                    result.TriggerCombat = true;

                    AlertNearbyAllies(NPC, 500f);
                    break;

                case InteractionType.Provoke:
                    if (GetPersonality() == GuPersonality.Proud)
                    {
                        HasBeenHitByPlayer = true;
                        AggroTimer = 300;
                        ModifyTrust(playerName, -20f);
                        result.TriggerCombat = true;
                        result.Message = GuMasterDisplayName + "被激怒了！";
                    }
                    break;

                case InteractionType.Bribe:
                    if (GetPersonality() == GuPersonality.Greedy)
                    {
                        ModifyTrust(playerName, +30f);
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
        // IGuMasterAI 接口实现（信念兼容层，核心使用信任值）
        // ============================================================

        public virtual PerceptionContext Perceive(NPC npc)
        {
            var player = Main.LocalPlayer;
            float dist = Vector2.Distance(npc.Center, player.Center);
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();

            return new PerceptionContext
            {
                TargetPlayer = player,
                DistanceToPlayer = dist,
                PlayerLifePercent = (int)((float)player.statLife / player.statLifeMax2 * 100),
                PlayerHasQiEnabled = qiRealm.GuLevel > 0,
                NearbyAlliesCount = GetNearbyFamilyCount(400f),
                NearbyEnemiesCount = 0,
                IsInOwnTerritory = false,
                TimeOfDay = (float)Main.time,
                IsRaining = Main.raining,
                PlayerInfamy = player.GetModPlayer<GuWorldPlayer>().InfamyPoints,
                PlayerQiLevel = qiRealm.GuLevel,
                PlayerDamage = player.HeldItem.damage
            };
        }

        public virtual void UpdateBelief(NPC npc, PerceptionContext context)
        {
            // 信念系统由对话交互更新，不由感知循环更新
            // 信任值的自然衰减已在 AI() 中处理
        }

        public virtual Decision Decide(NPC npc, PerceptionContext context)
        {
            return new Decision
            {
                NewState = CurrentAttitude == GuAttitude.Hostile ? GuMasterAIState.Combat : GuMasterAIState.Idle,
                Interaction = InteractionType.None,
                DialogueLine = "",
                ShouldAttack = CurrentAttitude == GuAttitude.Hostile,
                ShouldFlee = CurrentAttitude == GuAttitude.Fearful,
                ShouldCallForHelp = false,
                MoveTarget = Vector2.Zero
            };
        }

        public virtual GuAttitude CalculateAttitude(NPC npc, AttitudeContext context)
        {
            return CurrentAttitude;
        }

        public virtual void ExecuteAI(NPC npc, Decision decision)
        {
            // AI 已在 ExecuteBehaviorByAttitude 中实现
        }

        // ============================================================
        // 对话系统
        // ============================================================

        public virtual string GetDialogue(NPC npc, GuAttitude attitude)
        {
            return GuAttitudeHelper.GetDefaultDialogue(attitude, GuMasterDisplayName);
        }

        public virtual void GetChatButtons(NPC npc, ref string button, ref string button2)
        {
            if (DialogueTreeManager.Instance.HasTree(npc))
            {
                button = "对话";
                button2 = "";
                return;
            }

            var dialogueSystem = ModContent.GetInstance<DialogueSystem>();
            dialogueSystem.GenerateDialogueOptions(Main.LocalPlayer, npc, ref button, ref button2);

            if (string.IsNullOrEmpty(button))
                button = "对话";
            if (string.IsNullOrEmpty(button2))
            {
                if (CurrentAttitude != GuAttitude.Hostile && CurrentAttitude != GuAttitude.Wary)
                {
                    if (ProjectileProtectionEnabled)
                        button2 = "去除保护";
                    else
                        button2 = "开启保护";
                }
            }
        }

        public virtual void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                if (DialogueTreeManager.Instance.HasTree(npc))
                {
                    var mgr = DialogueTreeManager.Instance;
                    if (!mgr.HasActiveSession(Main.LocalPlayer))
                    {
                        mgr.StartDialogue(npc, Main.LocalPlayer);
                    }

                    var currentText = mgr.GetCurrentNPCText(Main.LocalPlayer);
                    var options = mgr.GetCurrentOptions(Main.LocalPlayer);

                    if (options != null && options.Count > 0)
                    {
                        DialogueTreeUI.Instance.Open(
                            npc.GivenName,
                            NPCHeadLoader.GetHeadSlot(HeadTexture),
                            currentText ?? "",
                            options);
                    }
                    else
                    {
                        mgr.EndDialogue(Main.LocalPlayer);
                        Main.npcChatText = currentText ?? GetDialogue(npc, CurrentAttitude);
                    }
                    return;
                }

                // "询问" → 诚实展示修为
                var dialogueSystem = ModContent.GetInstance<DialogueSystem>();
                dialogueSystem.OnDialogueChoice(Main.LocalPlayer, npc, 0);
                // 处理函数已设置 Main.npcChatText，无需覆盖
            }
            else
            {
                if (DialogueTreeManager.Instance.HasTree(npc))
                {
                    return;
                }

                // 友善时打开商店，否则执行第二对话选项
                if (CurrentAttitude != GuAttitude.Hostile && CurrentAttitude != GuAttitude.Wary)
                {
                    shop = ShopName;
                    ModifyTrust(Main.LocalPlayer.name, +20f);
                    return;
                }

                var dialogueSystem = ModContent.GetInstance<DialogueSystem>();
                dialogueSystem.OnDialogueChoice(Main.LocalPlayer, npc, 1);
            }
        }

        // ============================================================
        // tModLoader 钩子重写
        // ============================================================

        public override bool CanChat()
        {
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
            HandleInteraction(NPC, player, InteractionType.Attack);
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                var player = Main.player[projectile.owner];
                if (player.active && ProjectileProtectionEnabled)
                {
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
                    HandleInteraction(NPC, player, InteractionType.Attack);
                }
            }
        }

        public override void OnKill()
        {
            int? killerPlayerId = WhoKilledMe();
            if (killerPlayerId.HasValue)
            {
                var killer = Main.player[killerPlayerId.Value];
                if (killer.active)
                {
                    var worldPlayer = killer.GetModPlayer<GuWorldPlayer>();

                    if (HasWitnesses(400f))
                    {
                        worldPlayer.AddInfamy(10);
                        worldPlayer.RemoveReputation(GetFaction(), 20, "击杀成员");
                        Main.NewText($"你击杀了{GuMasterDisplayName}！{WorldStateMachine.GetFactionDisplayName(GetFaction())}声望下降。", Color.OrangeRed);
                    }
                    else
                    {
                        Main.NewText($"你秘密击杀了{GuMasterDisplayName}，无人知晓。", Color.Gray);
                    }
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetModPlayer<global::VerminLordMod.Common.Players.QiRealmPlayer>().GuLevel <= 0)
                return 0f;

            var phase = global::VerminLordMod.Common.DialogueTree.StoryManager.Instance.GetPhase(spawnInfo.Player);
            var faction = GetFaction();
            int minPhaseValue = faction switch
            {
                global::VerminLordMod.Common.Systems.FactionID.GuYue => (int)StoryPhase.Arrival,
                global::VerminLordMod.Common.Systems.FactionID.Scattered => (int)StoryPhase.SouthBorderArrival,
                global::VerminLordMod.Common.Systems.FactionID.HeiLouLan or global::VerminLordMod.Common.Systems.FactionID.ChangShengTian => (int)StoryPhase.NorthDesertArrival,
                global::VerminLordMod.Common.Systems.FactionID.Heaven => (int)StoryPhase.DestinyWarBegin,
                global::VerminLordMod.Common.Systems.FactionID.ShadowSect => (int)StoryPhase.YiTianShanAppears,
                _ => 0
            };

            if ((int)phase < minPhaseValue)
                return 0f;

            return 0.03f;
        }

        // ============================================================
        // 商店系统
        // ============================================================

        public override void AddShops()
        {
        }

        // ============================================================
        // 持久化（信任值 + 信念）
        // ============================================================

        public override void SaveData(TagCompound tag)
        {
            var trustData = new List<TagCompound>();
            foreach (var (playerName, trust) in TrustPerPlayer)
            {
                trustData.Add(new TagCompound
                {
                    ["playerName"] = playerName,
                    ["trust"] = trust
                });
            }
            tag["trustPerPlayer"] = trustData;

            // 保存信念状态
            var beliefData = new List<TagCompound>();
            foreach (var (playerName, belief) in PlayerBeliefs)
            {
                beliefData.Add(new TagCompound
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
                    ["lastInteractionDay"] = belief.LastInteractionDay,
                });
            }
            tag["playerBeliefs"] = beliefData;
        }

        public override void LoadData(TagCompound tag)
        {
            TrustPerPlayer.Clear();
            if (tag.TryGet("trustPerPlayer", out List<TagCompound> trustData))
            {
                foreach (var entry in trustData)
                {
                    string playerName = entry.GetString("playerName");
                    float trust = entry.GetFloat("trust");
                    TrustPerPlayer[playerName] = trust;
                }
            }

            // 加载信念状态
            PlayerBeliefs.Clear();
            if (tag.TryGet("playerBeliefs", out List<TagCompound> beliefData))
            {
                foreach (var entry in beliefData)
                {
                    string playerName = entry.GetString("playerName");
                    PlayerBeliefs[playerName] = new BeliefState
                    {
                        PlayerName = playerName,
                        RiskThreshold = entry.GetFloat("riskThreshold"),
                        ConfidenceLevel = entry.GetFloat("confidenceLevel"),
                        ObservationCount = entry.GetInt("observationCount"),
                        EstimatedPower = entry.GetFloat("estimatedPower"),
                        HasTraded = entry.GetBool("hasTraded"),
                        HasFought = entry.GetBool("hasFought"),
                        WasDefeated = entry.GetBool("wasDefeated"),
                        HasDefeatedPlayer = entry.GetBool("hasDefeatedPlayer"),
                        LastInteractionDay = entry.GetInt("lastInteractionDay"),
                    };
                }
            }
        }

        // ============================================================
        // 工具方法
        // ============================================================

        protected int GetNearbyFamilyCount(float range = 400f)
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.type == NPC.type && Vector2.Distance(NPC.Center, other.Center) < range)
                    count++;
            }
            return count;
        }

        protected void Say(string message, Color? color = null)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(message, color ?? Color.White);
        }

        protected void AlertNearbyAllies(NPC npc, float range)
        {
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
                        ally.HasBeenHitByPlayer = true;
                        ally.AggroTimer = 1800;
                        ally.ProjectileProtectionEnabled = false;
                        // 目击攻击，信任-30（用攻击者的玩家名）
                        if (npc.target >= 0 && npc.target < Main.maxPlayers)
                            ally.ModifyTrust(Main.player[npc.target].name, -30f);

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

        protected int? WhoKilledMe()
        {
            if (NPC.lastInteraction >= 0 && NPC.lastInteraction < Main.maxPlayers)
            {
                var player = Main.player[NPC.lastInteraction];
                if (player.active)
                    return NPC.lastInteraction;
            }
            return null;
        }

        protected void SpawnShockEffect(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(position, 10, 10, DustID.Torch,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    100, Color.Yellow, 1.8f);
            }
            CombatText.NewText(NPC.getRect(), Color.Yellow, "！", true);
        }
    }
}
