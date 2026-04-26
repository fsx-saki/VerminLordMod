using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Biomes;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    // ============================================================
    // GuYuePatrolGuMaster - 古月巡逻蛊师（掠夺型原型）
    //
    // MVA-Mini 重构版本：
    // - 不再是 townNPC，而是野外敌对NPC
    // - 信念黑箱驱动行为（观望/试探/动手/回避）
    // - 击败玩家后搜刮背包（元石+未炼化蛊虫）
    // - 被击败后记住玩家，下次遇到更谨慎
    // - 保留对话能力（态度友好时可对话）
    // ============================================================

    [AutoloadHead]
    public class GuYuePatrolGuMaster : GuMasterBase
    {
        // ===== 蛊师属性 =====
        public override FactionID GetFaction() => FactionID.GuYue;
        public override GuRank GetRank() => GuRank.Zhuan1_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Cautious;

        public override string GuMasterDisplayName => "古月巡逻蛊师";
        public override int GuMasterDamage => 18;
        public override int GuMasterLife => 180;
        public override int GuMasterDefense => 12;

        // ===== 巡逻状态 =====
        private Vector2 _spawnPoint;
        private const float PatrolRadius = 200f;
        private int _patrolTimer = 0;
        private int _patrolDir = 1;
        private bool _spawnPointInitialized = false;

        // ===== 掠夺状态 =====
        private bool _hasLootedPlayerThisEncounter = false;

        // ============================================================
        // 静态配置
        // ============================================================

        // 使用学堂家老（XueTangJiaLao）的身体贴图和动画
        // 这样古月巡逻蛊师在野外看起来和学堂家老一样，但行为完全不同（敌对、掠夺）
        // 头像必须显式指向自己的文件，因为 Texture 重写后 HeadTexture 默认会追加 "_Head"
        public override string Texture => "VerminLordMod/Content/NPCs/Town/XueTangJiaLao";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster_Head";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 500;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers
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
            NPC.value = Item.buyPrice(0, 0, 15, 0);
            // MVA-Mini：不再是townNPC，改为野外敌对NPC
            NPC.townNPC = false;
            NPC.friendly = false;
            // 不设置 don'tTakeDamageFromHostiles，因为我们要手动控制
            // 通过 CanBeHitByNPC / CanHitNPC 来避免同家族误伤
            AnimationType = NPCID.Guide;

            ApplyRankBonuses();
        }

        /// <summary>
        /// 防止巡逻蛊师被同家族NPC攻击（如学堂家老）
        /// 只对同阵营（GuYue）的NPC免疫
        /// </summary>
        public override bool CanBeHitByNPC(NPC attacker)
        {
            // 如果攻击者是同阵营的GuMasterBase，免疫伤害
            if (attacker.ModNPC is GuMasterBase otherMaster && otherMaster.GetFaction() == GetFaction())
                return false;
            return base.CanBeHitByNPC(attacker);
        }

        /// <summary>
        /// 防止巡逻蛊师攻击同家族NPC
        /// </summary>
        public override bool CanHitNPC(NPC target)
        {
            if (target.ModNPC is GuMasterBase targetMaster && targetMaster.GetFaction() == GetFaction())
                return false;
            return base.CanHitNPC(target);
        }

        public override void OnSpawn(IEntitySource source)
        {
            _spawnPoint = NPC.Center;
            _spawnPointInitialized = true;

            // 初始化所有已有玩家的信念（包括当前玩家）
            // 注意：Main.LocalPlayer 在 NPC 生成时可能为 null（如世界加载时生成）
            // 所以这里遍历所有活跃玩家来初始化信念
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                var player = Main.player[p];
                if (player != null && player.active)
                {
                    string playerName = player.name;
                    var belief = GetBelief(playerName);
                    // 掠夺型NPC：RiskThreshold 从 0.5 开始（默认是 0.9）
                    // 这样第一次见到玩家时更可能进入 Wary 而非 Fearful
                    belief.RiskThreshold = 0.5f;
                    belief.ConfidenceLevel = 0.5f;
                }
            }
        }

        // ============================================================
        // AI 主循环
        // ============================================================

        public override void AI()
        {
            // 记录出生点（OnSpawn 可能不被所有生成方式调用，所以这里也做保护）
            if (!_spawnPointInitialized)
            {
                _spawnPoint = NPC.Center;
                _spawnPointInitialized = true;
            }

            // 调用基类AI（感知→信念更新→态度→决策→行为）
            base.AI();
        }

        // ============================================================
        // 重写信念更新（添加掠夺型特有的观察逻辑）
        // ============================================================

        public override void UpdateBelief(NPC npc, PerceptionContext context)
        {
            string playerName = context.TargetPlayer.name;
            var belief = GetBelief(playerName);

            // 掠夺型特有的观察逻辑：
            // - 玩家持有高价值物品 → 降低 RiskThreshold（更想动手）
            // - 玩家生命值低 → 大幅降低 RiskThreshold
            // - 附近有友方NPC → 降低 RiskThreshold（以多欺少）
            bool wasDefeated = belief.WasDefeated;
            bool defeatedPlayer = belief.HasDefeatedPlayer;

            // 调用基础信念更新
            GuAttitudeHelper.UpdateBeliefState(belief, context, wasDefeated, defeatedPlayer);

            // 掠夺型额外修正：玩家持有贵重物品时更激进
            if (context.PlayerIsHoldingValuable)
                belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.1f);

            // 巡逻蛊师是家族蛊师，不会因为人多就主动攻击玩家
            // 只有被玩家攻击后（HasBeenHitByPlayer）才会进入战斗
            // 所以这里不添加"以多欺少"的 RiskThreshold 降低逻辑

            // 掠夺型核心修正：RiskThreshold 上限锁定在 0.5（Wary 阈值）
            // 防止 UpdateBeliefState 的平滑更新把阈值推回 0.9
            // 这样掠夺蛊师永远处于"试探性攻击"而非"恐惧逃跑"状态
            if (belief.RiskThreshold > 0.5f)
                belief.RiskThreshold = 0.5f;
        }

        // ============================================================
        // 重写决策逻辑（添加巡逻）
        // ============================================================

        public override Decision Decide(NPC npc, PerceptionContext context)
        {
            var decision = base.Decide(npc, context);

            // 如果基类决策为Idle且玩家不在附近，执行巡逻
            if (decision.NewState == GuMasterAIState.Idle && context.DistanceToPlayer > 400f)
            {
                decision.NewState = GuMasterAIState.Patrol;
            }

            return decision;
        }

        // ============================================================
        // 重写行为执行（添加巡逻）
        // ============================================================

        public override void ExecuteAI(NPC npc, Decision decision)
        {
            switch (decision.NewState)
            {
                case GuMasterAIState.Patrol:
                    ExecutePatrolAI(npc);
                    break;

                case GuMasterAIState.CallForHelp:
                    ExecuteCallForHelp(npc);
                    break;

                default:
                    base.ExecuteAI(npc, decision);
                    break;
            }
        }

        /// <summary> 呼叫支援：生成2-3个友方巡逻蛊师 </summary>
        private void ExecuteCallForHelp(NPC npc)
        {
            CurrentAIState = GuMasterAIState.CallForHelp;

            // 生成信号弹效果
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.Firework_Red,
                    Main.rand.NextFloat(-6f, 6f),
                    Main.rand.NextFloat(-8f, -2f),
                    100, Color.Red, 2f);
            }
            Main.NewText($"{GuMasterDisplayName}发出了求救信号！", Color.Red);

            // 生成2-3个友方NPC
            int count = Main.rand.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                int npcType = ModContent.NPCType<GuYuePatrolGuMaster>();
                Vector2 spawnPos = npc.Center + new Vector2(Main.rand.Next(-250, 250), -50);
                int newIdx = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, npcType);
                if (newIdx >= 0 && newIdx < Main.maxNPCs)
                {
                    var patrolNPC = Main.npc[newIdx].ModNPC as GuYuePatrolGuMaster;
                    if (patrolNPC != null)
                    {
                        // 新生成的NPC直接进入战斗状态
                        patrolNPC.HasBeenHitByPlayer = true;
                        patrolNPC.AggroTimer = 1800;
                    }
                }
            }

            // 自身进入战斗状态
            CurrentAIState = GuMasterAIState.Combat;
        }

        /// <summary> 巡逻AI：在出生点附近来回走动 </summary>
        private void ExecutePatrolAI(NPC npc)
        {
            CurrentAIState = GuMasterAIState.Patrol;

            float distFromSpawn = Vector2.Distance(npc.Center, _spawnPoint);

            // 超出巡逻半径则返回
            if (distFromSpawn > PatrolRadius)
            {
                float returnDir = _spawnPoint.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = returnDir * 1.5f;
                npc.spriteDirection = (int)returnDir;
                return;
            }

            // 定时改变方向
            _patrolTimer++;
            if (_patrolTimer > 120 + Main.rand.Next(60))
            {
                _patrolDir = Main.rand.NextBool() ? 1 : -1;
                _patrolTimer = 0;
            }

            npc.velocity.X = _patrolDir * 0.8f;
            npc.spriteDirection = _patrolDir;
        }

        // ============================================================
        // 战斗AI（掠夺型：远程攻击 + 近战追击混合）
        // ============================================================

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            // 巡逻蛊师是远程蛊师，站定发射月光蛊弹幕，不近战追击
            // 面朝玩家
            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            // 战斗状态下每帧按概率头顶飘出喊话
            if (Main.rand.NextBool(120))
            {
                string[] combatLines = new string[]
                {
                    "受死！",
                    "看招！",
                    "哪里跑！",
                    "月光蛊，去！",
                    "哼！",
                    "别想逃！"
                };
                string combatLine = combatLines[Main.rand.Next(combatLines.Length)];
                CombatText.NewText(npc.getRect(), Color.OrangeRed, combatLine, true);
            }

            // ===== 被攻击后的震惊反击阶段 =====
            // AggroTimer > 1500 表示刚被攻击不久（30秒仇恨中的前5秒）
            // 这个阶段站定不动，快速发射弹幕
            if (AggroTimer > 1500)
            {
                // 站定不动
                npc.velocity.X *= 0.9f;

                // 快速发射月光蛊弹幕（每0.5秒一次），带瞄准提前量
                if (Main.rand.NextBool(30))
                {
                    Vector2 aimDir = AimAhead(npc.Center, target.Center, target.velocity, 8f);
                    aimDir *= 8f;

                    int projType = ModContent.ProjectileType<MoonlightProjEnemy>();
                    int damage = npc.damage;
                    Projectile.NewProjectile(
                        npc.GetSource_FromAI(),
                        npc.Center + aimDir * 10f,
                        aimDir,
                        projType,
                        damage,
                        3f,
                        Main.myPlayer
                    );
                }
                return;
            }

            // ===== 正常战斗阶段：纯远程射击 =====
            // 保持与玩家的距离（100-250f 为最佳射击距离）
            if (dist < 100f)
            {
                // 太近则后退
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 2f;
            }
            else if (dist > 250f)
            {
                // 太远则稍微靠近
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 1.5f;
            }
            else
            {
                // 最佳距离：站定射击
                npc.velocity.X *= 0.9f;
            }

            // 跳跃（被地形卡住时）
            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -6f;

            // 发射月光蛊弹幕，带瞄准提前量
            if (dist < 400f && Main.rand.NextBool(45))
            {
                Vector2 aimDir = AimAhead(npc.Center, target.Center, target.velocity, 7f);
                aimDir *= 7f;

                int projType = ModContent.ProjectileType<MoonlightProjEnemy>();
                int damage = npc.damage;
                Projectile.NewProjectile(
                    npc.GetSource_FromAI(),
                    npc.Center,
                    aimDir,
                    projType,
                    damage,
                    3f,
                    Main.myPlayer
                );
            }
        }

        // ============================================================
        // 击败玩家后的搜尸逻辑
        // ============================================================

        /// <summary>
        /// 当NPC击杀玩家时调用：搜刮背包中的元石和未炼化蛊虫
        /// </summary>
        public void OnPlayerDeath(Player player)
        {
            if (_hasLootedPlayerThisEncounter) return;
            _hasLootedPlayerThisEncounter = true;

            // 更新信念：击败了玩家
            string playerName = player.name;
            var belief = GetBelief(playerName);
            belief.HasDefeatedPlayer = true;
            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.1f);

            // 搜刮逻辑：从玩家背包取走元石和未炼化蛊虫
            int lootedValue = 0;
            for (int i = 0; i < 50; i++)
            {
                var item = player.inventory[i];
                if (item == null || item.IsAir) continue;

                // 搜刮元石
                if (item.type == ModContent.ItemType<Content.Items.Consumables.YuanS>())
                {
                    int takeAmount = item.stack / 2; // 拿走一半
                    if (takeAmount > 0)
                    {
                        item.stack -= takeAmount;
                        lootedValue += takeAmount;
                    }
                }
                // 搜刮未炼化的蛊虫（非消耗品、非武器、非饰品 = 未炼化蛊虫材料）
                else if (item.ModItem is Content.Items.Consumables.GuConsumableItem)
                {
                    // 消耗品蛊虫（如酒虫、豕蛊等）视为未炼化，可被搜刮
                    int takeAmount = item.stack / 2;
                    if (takeAmount > 0)
                    {
                        item.stack -= takeAmount;
                        lootedValue += 5; // 每个价值5元石
                    }
                }
            }

            if (lootedValue > 0)
            {
                Main.NewText($"{GuMasterDisplayName}从你身上搜走了价值 {lootedValue} 元石的物品！", Color.OrangeRed);
            }
        }

        // ============================================================
        // 被击杀时更新信念
        // ============================================================

        public override void OnKill()
        {
            // 更新信念：被玩家击败
            string playerName = Main.LocalPlayer.name;
            var belief = GetBelief(playerName);
            belief.WasDefeated = true;
            belief.RiskThreshold = MathHelper.Min(1f, belief.RiskThreshold + 0.25f);

            // ===== 目击者检查 =====
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
                        worldPlayer.AddInfamy(15);
                        worldPlayer.RemoveReputation(FactionID.GuYue, 30, "击杀巡逻蛊师");
                        Main.NewText("你击杀了一名古月巡逻蛊师！古月家族声望下降。", Color.OrangeRed);
                    }
                    else
                    {
                        // 无目击者：秘密击杀，不扣声望
                        Main.NewText("你秘密击杀了一名古月巡逻蛊师，无人知晓。", Color.Gray);
                    }
                }
            }
            // 非玩家击杀（怪物等）→ 不扣玩家声望
        }

        // ============================================================
        // 对话系统（保留，态度友好时可对话）
        // ============================================================

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;

            switch (attitude)
            {
                case GuAttitude.Hostile:
                    return "古月巡逻蛊师拔出武器：" + "\"再靠近我就不客气了！\"";

                case GuAttitude.Wary:
                    return "古月巡逻蛊师警惕地看着你：" + "\"你最好离远点。\"";

                case GuAttitude.Friendly:
                    if (NumberOfTimesTalkedTo == 1)
                        return "古月巡逻蛊师向你点头：" + "\"欢迎来到古月山寨，外乡人。\"";
                    else if (NumberOfTimesTalkedTo <= 3)
                        return "古月巡逻蛊师微笑着说：" + "\"最近山寨周围不太平，你也要小心些。\"";
                    else
                        return "古月巡逻蛊师拍了拍你的肩膀：" + "\"有你在，我们山寨安全多了！\"";

                case GuAttitude.Respectful:
                    return "古月巡逻蛊师恭敬地行礼：" + "\"大人，有什么吩咐？\"";

                case GuAttitude.Contemptuous:
                    return "古月巡逻蛊师轻蔑地扫了你一眼：" + "\"哼，又一个不知天高地厚的小子。\"";

                case GuAttitude.Fearful:
                    return "古月巡逻蛊师颤抖着说：" + "\"你...你想干什么？别过来！\"";

                default:
                    if (NumberOfTimesTalkedTo == 1)
                        return "古月巡逻蛊师看了你一眼：" + "\"你是新来的？注意安全，别走太远。\"";
                    else
                        return "古月巡逻蛊师点点头：" + "\"嗯。\"";
            }
        }

        public override void SetChatButtons(ref string button, ref string button2)
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

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                Main.npcChatText = GetDialogue(NPC, CurrentAttitude);
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
        // 商店系统
        // ============================================================

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);

            shop.Add(new Item(ItemID.Mushroom), Condition.TimeDay);
            shop.Add(new Item(ItemID.Gel));
            shop.Add(new Item(ItemID.Rope));

            shop.Register();
        }

        // ============================================================
        // 生成条件
        // ============================================================

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiPlayer = spawnInfo.Player.GetModPlayer<QiPlayer>();
            if (!qiPlayer.qiEnabled) return 0f;

            // 只在古月驻地生物群系中刷新
            if (spawnInfo.Player.InModBiome<GuYueCompoundBiome>())
                return 0.3f;

            return 0f;
        }

        // ============================================================
        // 保存/加载巡逻状态 + 信念数据（基类已处理信念）
        // ============================================================

        /// <summary>
        /// 计算瞄准提前量：根据目标当前位置、速度和弹幕速度，预测命中位置
        /// </summary>
        private static Vector2 AimAhead(Vector2 sourcePos, Vector2 targetPos, Vector2 targetVel, float projectileSpeed)
        {
            Vector2 delta = targetPos - sourcePos;
            float dist = delta.Length();
            if (dist < 1f) return Vector2.Normalize(delta);

            // 预估飞行时间
            float flightTime = dist / MathF.Max(projectileSpeed, 0.1f);

            // 预测目标位置
            Vector2 predictedPos = targetPos + targetVel * flightTime * 0.5f;

            // 重新计算方向
            Vector2 aimDir = predictedPos - sourcePos;
            if (aimDir.LengthSquared() < 1f)
                aimDir = delta;

            aimDir.Normalize();
            return aimDir;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["spawnX"] = _spawnPoint.X;
            tag["spawnY"] = _spawnPoint.Y;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.ContainsKey("spawnX"))
                _spawnPoint = new Vector2(tag.GetFloat("spawnX"), tag.GetFloat("spawnY"));
        }
    }
}
