using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// GuYueNPCBase — 古月家族NPC抽象基类
    /// 
    /// 继承自 GuMasterBase，整合了：
    /// - 古月家族通用贴图（全部使用古月巡逻蛊师贴图）
    /// - 角色类型系统（GuYueNPCType）
    /// - 数据驱动配置（GuYueNPCConfig）
    /// - 同家族保护（防止误伤）
    /// - 通用对话模板
    /// 
    /// 子类只需重写：
    /// - GetNPCType() - 返回角色类型
    /// - 可选重写 GetDialogue() / AddShops() / GetCombatAI() 等
    /// 
    /// 设计原则：
    /// - 所有古月NPC共享同一套贴图（古月巡逻蛊师）
    /// - 通过 GuYueNPCType 区分身份和功能
    /// - 属性由 GuYueNPCConfig 数据驱动
    /// - 后续扩展只需新增 GuYueNPCType 枚举值和对应配置
    /// </summary>
    public abstract class GuYueNPCBase : GuMasterBase
    {
        // ============================================================
        // 抽象方法：子类必须返回自己的角色类型
        // ============================================================
        public abstract GuYueNPCType GetNPCType();

        // ============================================================
        // 贴图：所有古月NPC统一使用古月巡逻蛊师的贴图
        // 子类无需重写，如需自定义贴图可重写 Texture/HeadTexture
        // ============================================================
        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster_Head";

        // ============================================================
        // 配置缓存
        // ============================================================
        private GuYueNPCConfig _config;
        public GuYueNPCConfig Config
        {
            get
            {
                if (_config == null)
                    _config = GuYueNPCConfig.GetDefaultConfig(GetNPCType());
                return _config;
            }
        }

        // ============================================================
        // GuMasterBase 抽象实现（委托到配置）
        // ============================================================
        public override FactionID GetFaction() => FactionID.GuYue;
        public override GuRank GetRank() => Config.Rank;
        public override GuPersonality GetPersonality() => Config.Personality;

        public override string GuMasterDisplayName => Config.DisplayName;
        public override int GuMasterDamage => Config.BaseDamage;
        public override int GuMasterLife => Config.BaseLife;
        public override int GuMasterDefense => Config.BaseDefense;

        // ============================================================
        // 静态配置
        // ============================================================
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

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
            // 基类设置基础属性
            base.SetDefaults();

            // 测试阶段：所有古月NPC都表现为普通城镇NPC
            // 后续可根据场景（主世界/小世界）动态切换
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.aiStyle = 7; // 城镇NPC AI
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;
        }

        // ============================================================
        // 同家族保护
        // ============================================================
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

        // ============================================================
        // 对话系统（模板方法）
        // ============================================================
        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;

            // 根据角色类型和态度返回不同对话
            string roleText = GetRoleDialoguePrefix();

            return attitude switch
            {
                GuAttitude.Hostile => roleText + "冷冷地盯着你：\"滚开，否则别怪我不客气！\"",
                GuAttitude.Wary => roleText + "警惕地看着你：\"你最好离远点。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => roleText + "恭敬地向你行礼：\"见过大人。\"",
                GuAttitude.Contemptuous => roleText + "轻蔑地扫了你一眼：\"哼。\"",
                GuAttitude.Fearful => roleText + "颤抖着后退了几步：\"你...你想干什么？\"",
                _ => roleText + "看了你一眼，没有理会。"
            };
        }

        /// <summary>
        /// 获取角色对话前缀（如"古月族长"、"学堂家老"等）
        /// </summary>
        protected virtual string GetRoleDialoguePrefix()
        {
            return Config.DisplayName;
        }

        /// <summary>
        /// 友好态度下的对话 - 子类可重写以提供个性化对话
        /// </summary>
        protected virtual string GetFriendlyDialogue()
        {
            if (NumberOfTimesTalkedTo == 1)
                return GetRoleDialoguePrefix() + "向你点头：\"欢迎来到古月山寨。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return GetRoleDialoguePrefix() + "微笑着说：\"在古月山寨还习惯吗？\"";
            else
                return GetRoleDialoguePrefix() + "拍了拍你的肩膀：\"好好干！\"";
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "对话";
            if (Config.IsTownNPC)
            {
                button2 = "商店";
            }
            else
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

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                Main.npcChatText = GetDialogue(NPC, CurrentAttitude);
            }
            else
            {
                if (Config.IsTownNPC)
                {
                    shop = ShopName;
                }
                else
                {
                    ProjectileProtectionEnabled = !ProjectileProtectionEnabled;
                    if (ProjectileProtectionEnabled)
                        Main.npcChatText = "你重新开启了弹幕保护。";
                    else
                        Main.npcChatText = "你悄悄去除了弹幕保护。";
                }
            }
        }

        // ============================================================
        // 生成条件
        // ============================================================
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;

            // 城镇NPC由系统自动管理生成，不需要SpawnChance
            if (Config.IsTownNPC) return 0f;

            return 0f;
        }

        // ============================================================
        // 通用名称列表
        // ============================================================
        public static List<string> GuYueNameList = new List<string>()
        {
            "古月翰墨","古月庆雪","古月伶俐","古月新","古月昆皓","古月映雪","古月安娴",
            "古月文乐","古月乐章","古月娅童","古月冷松","古月麦冬","古月碧春","古月觅露",
            "古月嘉玉","古月妙晴","古月从筠","古月焱","古月锐锋","古月书萱","古月香春",
            "古月采白","古月舒","古月馨兰","古月梦桐","古月宏壮","古月承","古月香彤",
            "古月碧菡","古月寄南","古月绣文","古月大","古月问夏","古月吉玟","古月含桃",
            "古月清韵","古月亦绿","古月阳阳","古月初阳","古月博厚","古月婉然","古月安荷",
            "古月衍","古月秋蝶","古月思天","古月初兰","古月建树","古月景铄","古月慕蕊",
            "古月晴画","古月绮山","古月绿海","古月浩言","古月晴波","古月思源","古月嘉云",
            "古月秀竹","古月蔼","古月格","古月浩阔","古月懿轩","古月姣","古月访彤",
            "古月轩","古月涵育","古月舞","古月斯琪","古月彦珺","古月晴曦","古月之玉",
            "古月映寒","古月白容","古月乐蓉","古月悦恺","古月傲之","古月央","古月俊逸",
            "古月婉淑","古月德辉","古月如","古月颜","古月芷雪","古月孤容","古月半雪",
            "古月古韵","古月阳煦","古月嘉美","古月善静","古月元绿","古月筠溪","古月谷蕊",
            "古月痴梅","古月荷","古月蔓菁","古月雪羽","古月宁","古月雅静","古月清怡",
            "古月访曼","古月安吉","古月嘉熙","古月良奥","古月峻","古月景龙","古月涵易",
            "古月夜梅","古月千儿","古月晴丽","古月建同","古月恨风"
        };

        public override List<string> SetNPCNameList()
        {
            return GuYueNameList;
        }
    }
}
