using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Biomes;
using VerminLordMod.Content.Projectiles;
using System;
using Terraria.GameContent;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    // ============================================================
    // GuYuePatrolGuMaster - 古月巡逻蛊师（掠夺型原型）
    //
    // 简化版（信任值驱动）：
    // - 野外敌对NPC，信任值系统驱动行为
    // - 击败玩家后搜刮背包（元石+未炼化蛊虫）
    // - 被击败后记住玩家，下次遇到更谨慎（信任值更低）
    // - 保留对话能力（态度友好时可对话）
    // - 巡逻AI：在出生点附近来回走动
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

        // ===== 呼叫支援冷却 =====
        private int _callForHelpCooldown = 0;

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

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
            NPC.value = Item.buyPrice(0, 0, 15, 0);
            NPC.townNPC = false;
            NPC.friendly = false;
            AnimationType = NPCID.Guide;

            ApplyRankBonuses();
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (attacker.ModNPC is GuMasterBase otherMaster && otherMaster.GetFaction() == GetFaction())
                return false;
            return base.CanBeHitByNPC(attacker);
        }

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

            // 掠夺型NPC：初始信任值更低（30，比默认50低）
            // 这样第一次见到玩家时更可能处于 Wary 态度
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                var player = Main.player[p];
                if (player != null && player.active)
                {
                    TrustPerPlayer[player.name] = 30f;
                }
            }
        }

        // ============================================================
        // AI 主循环（整合巡逻逻辑）
        // ============================================================

        public override void AI()
        {
            if (!_spawnPointInitialized)
            {
                _spawnPoint = NPC.Center;
                _spawnPointInitialized = true;
            }

            if (_callForHelpCooldown > 0) _callForHelpCooldown--;

            // 调用基类AI（信任值→态度→行为）
            base.AI();

            // 基类AI处理了战斗/对话/警戒，这里补充巡逻逻辑
            // 当非敌对且玩家不在附近时，执行巡逻
            if (CurrentAttitude != GuAttitude.Hostile && AggroTimer <= 0)
            {
                float distToPlayer = Vector2.Distance(NPC.Center, Main.LocalPlayer.Center);
                if (distToPlayer > 400f)
                {
                    ExecutePatrolAI(NPC);
                }
            }
        }

        /// <summary> 巡逻AI：在出生点附近来回走动 </summary>
        private void ExecutePatrolAI(NPC npc)
        {
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
                CombatText.NewText(npc.getRect(), Color.OrangeRed, combatLines[Main.rand.Next(combatLines.Length)], true);
            }

            // 被攻击后的震惊反击阶段（前5秒站定射击）
            if (AggroTimer > 1500)
            {
                npc.velocity.X *= 0.9f;

                if (Main.rand.NextBool(30))
                {
                    Vector2 aimDir = AimAhead(npc.Center, target.Center, target.velocity, 8f);
                    aimDir *= 8f;

                    int projType = ModContent.ProjectileType<MoonlightProjEnemy>();
                    Projectile.NewProjectile(
                        npc.GetSource_FromAI(),
                        npc.Center + aimDir * 10f,
                        aimDir,
                        projType,
                        npc.damage,
                        3f,
                        Main.myPlayer
                    );
                }
                return;
            }

            // 正常战斗阶段：保持距离 + 远程射击
            if (dist < 100f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 2f;
            }
            else if (dist > 250f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 1.5f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -6f;

            // 发射月光蛊弹幕
            if (dist < 400f && Main.rand.NextBool(45))
            {
                Vector2 aimDir = AimAhead(npc.Center, target.Center, target.velocity, 7f);
                aimDir *= 7f;

                int projType = ModContent.ProjectileType<MoonlightProjEnemy>();
                Projectile.NewProjectile(
                    npc.GetSource_FromAI(),
                    npc.Center,
                    aimDir,
                    projType,
                    npc.damage,
                    3f,
                    Main.myPlayer
                );
            }

            // 低血量时呼叫支援（冷却300帧=5秒）
            if (NPC.life < NPC.lifeMax * 0.3f && _callForHelpCooldown <= 0)
            {
                ExecuteCallForHelp(npc);
                _callForHelpCooldown = 600;
            }
        }

        /// <summary> 呼叫支援：生成2-3个友方巡逻蛊师 </summary>
        private void ExecuteCallForHelp(NPC npc)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.Firework_Red,
                    Main.rand.NextFloat(-6f, 6f),
                    Main.rand.NextFloat(-8f, -2f),
                    100, Color.Red, 2f);
            }
            Main.NewText($"{GuMasterDisplayName}发出了求救信号！", Color.Red);

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
                        patrolNPC.HasBeenHitByPlayer = true;
                        patrolNPC.AggroTimer = 1800;
                    }
                }
            }
        }

        // ============================================================
        // 击败玩家后的搜尸逻辑
        // ============================================================

        public void OnPlayerDeath(Player player)
        {
            if (_hasLootedPlayerThisEncounter) return;
            _hasLootedPlayerThisEncounter = true;

            // 击败玩家：信任值进一步降低（更肆无忌惮）
            ModifyTrust(player.name, -20f);

            // 搜刮逻辑：从玩家背包取走元石和未炼化蛊虫
            int lootedValue = 0;
            for (int i = 0; i < 50; i++)
            {
                var item = player.inventory[i];
                if (item == null || item.IsAir) continue;

                if (item.type == ModContent.ItemType<Content.Items.Consumables.YuanS>())
                {
                    int takeAmount = item.stack / 2;
                    if (takeAmount > 0)
                    {
                        item.stack -= takeAmount;
                        lootedValue += takeAmount;
                    }
                }
                else if (item.ModItem is Content.Items.Consumables.GuConsumableItem)
                {
                    int takeAmount = item.stack / 2;
                    if (takeAmount > 0)
                    {
                        item.stack -= takeAmount;
                        lootedValue += 5;
                    }
                }
            }

            if (lootedValue > 0)
            {
                Main.NewText($"{GuMasterDisplayName}从你身上搜走了价值 {lootedValue} 元石的物品！", Color.OrangeRed);
            }
        }

        // ============================================================
        // 被击杀时处理（信任值 + 目击者）
        // ============================================================

        public override void OnKill()
        {
            // 被击败：NPC死后此实例销毁，但其他同阵营NPC的信任值不变
            // 目击者检查
            int? killerPlayerId = WhoKilledMe();
            if (killerPlayerId.HasValue)
            {
                var killer = Main.player[killerPlayerId.Value];
                if (killer.active)
                {
                    var worldPlayer = killer.GetModPlayer<GuWorldPlayer>();

                    if (HasWitnesses(400f))
                    {
                        worldPlayer.AddInfamy(15);
                        worldPlayer.RemoveReputation(FactionID.GuYue, 30, "击杀巡逻蛊师");
                        Main.NewText("你击杀了一名古月巡逻蛊师！古月家族声望下降。", Color.OrangeRed);
                    }
                    else
                    {
                        Main.NewText("你秘密击杀了一名古月巡逻蛊师，无人知晓。", Color.Gray);
                    }
                }
            }
        }

        // ============================================================
        // 对话系统
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
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;

            if (spawnInfo.Player.InModBiome<GuYueCompoundBiome>())
                return 0.3f;

            return 0f;
        }

        // ============================================================
        // 瞄准提前量 + 持久化
        // ============================================================

        private static Vector2 AimAhead(Vector2 sourcePos, Vector2 targetPos, Vector2 targetVel, float projectileSpeed)
        {
            Vector2 delta = targetPos - sourcePos;
            float dist = delta.Length();
            if (dist < 1f) return Vector2.Normalize(delta);

            float flightTime = dist / MathF.Max(projectileSpeed, 0.1f);
            Vector2 predictedPos = targetPos + targetVel * flightTime * 0.5f;
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
