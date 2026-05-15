// ============================================================
// DialogueTreeDemoNPC - 对话树演示员NPC
// 一个友好的城镇NPC，用于展示和测试对话树系统
// 不依赖信念系统，直接使用预设对话树
// 根据 DebugAttitude 展示不同的对话树（7种态度 x 多种分支）
// 使用自定义 DialogueTreeUI 展示所有选项（突破原版2按钮限制）
// ============================================================
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.DialogueTreeUI;
using VerminLordMod.Common.UI.UIUtils;
using VerminLordMod.Content.Items.Debuggers;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs;

/// <summary>
/// 对话树演示员 — 用于展示和测试对话树功能
/// 根据 DebugAttitude 展示不同的对话树
/// </summary>
[AutoloadHead]
public class DialogueTreeDemoNPC : ModNPC
{
    // ===== 调试用状态（可被道具修改）=====
    public BeliefState DebugBelief;
    public GuAttitude DebugAttitude = GuAttitude.Friendly;
    public GuPersonality DebugPersonality = GuPersonality.Benevolent;

    // ===== 对话树（每种态度一个）=====
    private readonly Dictionary<GuAttitude, DialogueTree> _attitudeTrees = new();
    private bool _treeInitialized;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 25;
        NPCID.Sets.ExtraFramesCount[Type] = 9;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 700;
        NPCID.Sets.AttackType[Type] = 0;
        NPCID.Sets.AttackTime[Type] = 30;
        NPCID.Sets.AttackAverageChance[Type] = 10;
        NPCID.Sets.HatOffsetY[Type] = 4;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers
        {
            Velocity = 1f,
            Direction = 1
        });
    }

    public override void SetDefaults()
    {
        NPC.width = 18;
        NPC.height = 40;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.damage = 10;
        NPC.lifeMax = 250;
        NPC.defense = 15;
        NPC.knockBackResist = 0.5f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = 7;
        AnimationType = NPCID.Guide;
        NPC.value = Item.buyPrice(0, 0, 50, 0);

        DebugBelief = new BeliefState
        {
            PlayerName = "",
            RiskThreshold = 0.5f,
            ConfidenceLevel = 0.5f,
            EstimatedPower = 0.5f,
            ObservationCount = 1,
            HasTraded = false,
            HasFought = false,
            WasDefeated = false,
            HasDefeatedPlayer = false,
            LastInteractionDay = 0
        };
    }

    public override void AddShops()
    {
        var shop = new NPCShop(Type, "DemoShop");

        shop.Add(new Item(ItemID.HealingPotion) { shopCustomPrice = 50 }, Condition.TimeDay);
        shop.Add(new Item(ItemID.GreaterHealingPotion) { shopCustomPrice = 150 });
        shop.Add(new Item(ItemID.ManaPotion) { shopCustomPrice = 50 });
        shop.Add(new Item(ItemID.RegenerationPotion) { shopCustomPrice = 80 });
        shop.Add(new Item(ItemID.IronskinPotion) { shopCustomPrice = 60 });
        shop.Add(new Item(ItemID.SwiftnessPotion) { shopCustomPrice = 40 });
        shop.Add(new Item(ItemID.RecallPotion) { shopCustomPrice = 20 });
        shop.Add(new Item(ItemID.Torch) { shopCustomPrice = 1 });
        shop.Add(new Item(ItemID.Rope) { shopCustomPrice = 2 });
        shop.Add(new Item(ItemID.WoodenArrow) { shopCustomPrice = 1 });
        shop.Add(new Item(ItemID.Shuriken) { shopCustomPrice = 3 });
        shop.Add(new Item(ItemID.Bomb) { shopCustomPrice = 15 });
        shop.Add(new Item(ItemID.BugNet) { shopCustomPrice = 25 });

        shop.Add(new Item(ModContent.ItemType<YuanS>()) { shopCustomPrice = 1 });

        shop.Add(new Item(ModContent.ItemType<TenLifeGu>()) { shopCustomPrice = 100 });
        shop.Add(new Item(ModContent.ItemType<HundredLifeGu>()) { shopCustomPrice = 500 });
        shop.Add(new Item(ModContent.ItemType<StrengthLongicorn>()) { shopCustomPrice = 200 });
        shop.Add(new Item(ModContent.ItemType<HuangLuoLongicorn>()) { shopCustomPrice = 200 });
        shop.Add(new Item(ModContent.ItemType<BronzeShari>()) { shopCustomPrice = 300 });
        shop.Add(new Item(ModContent.ItemType<SliverShari>()) { shopCustomPrice = 800 });
        shop.Add(new Item(ModContent.ItemType<WineBug>()) { shopCustomPrice = 150 });
        shop.Add(new Item(ModContent.ItemType<SevenWineBug>()) { shopCustomPrice = 600 });
        shop.Add(new Item(ModContent.ItemType<LivingLeaf>()) { shopCustomPrice = 50 });
        shop.Add(new Item(ModContent.ItemType<KsitigarbhaFlowerGu>()) { shopCustomPrice = 400 });
        shop.Add(new Item(ModContent.ItemType<OneMinion>()) { shopCustomPrice = 250 });
        shop.Add(new Item(ModContent.ItemType<JinLiGu>()) { shopCustomPrice = 180 });
        shop.Add(new Item(ModContent.ItemType<WolfWaveCard>()) { shopCustomPrice = 120 });

        shop.Register();
    }

    public override void AI()
    {
        base.AI();

        if (!_treeInitialized)
        {
            InitializeDialogueTrees();
            _treeInitialized = true;
        }
    }

    // ============================================================
    // 对话树初始化
    // ============================================================

    private void InitializeDialogueTrees()
    {
        var manager = DialogueTreeManager.Instance;

        // 为每种态度创建并注册对话树
        _attitudeTrees[GuAttitude.Friendly] = BuildFriendlyTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Friendly]);

        _attitudeTrees[GuAttitude.Wary] = BuildWaryTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Wary]);

        _attitudeTrees[GuAttitude.Respectful] = BuildRespectfulTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Respectful]);

        _attitudeTrees[GuAttitude.Fearful] = BuildFearfulTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Fearful]);

        _attitudeTrees[GuAttitude.Hostile] = BuildHostileTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Hostile]);

        _attitudeTrees[GuAttitude.Contemptuous] = BuildContemptuousTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Contemptuous]);

        _attitudeTrees[GuAttitude.Ignore] = BuildIgnoreTree();
        manager.RegisterTree<DialogueTreeDemoNPC>(_attitudeTrees[GuAttitude.Ignore]);
    }

    /// <summary>
    /// 获取当前态度对应的对话树
    /// </summary>
    private DialogueTree GetCurrentTree()
    {
        if (_attitudeTrees.TryGetValue(DebugAttitude, out var tree))
            return tree;
        return _attitudeTrees.GetValueOrDefault(GuAttitude.Friendly);
    }

    // ============================================================
    // 态度对话树构建
    // ============================================================

    #region Friendly - 友好对话树

    private DialogueTree BuildFriendlyTree()
    {
        var b = new DialogueTreeBuilder("Demo_Friendly", "greeting");

        // ===== 主菜单 =====
        b.StartNode("greeting",
            "{npcName}热情地向你打招呼：\"欢迎来到对话树演示！我是专门用来展示对话树功能的。你想了解什么？\"")
            .AddOption("这是什么？", "what_is_this", DialogueOptionType.Informative)
            .AddOption("展示对话树", "show_tree")
            .AddOption("交易与购买", "trade_menu", DialogueOptionType.Trade)
            .AddOption("炼制与合成", "craft_menu", DialogueOptionType.Craft)
            .AddOption("修炼指导", "training_menu", DialogueOptionType.Teach)
            .AddOption("社交与互动", "social_menu", DialogueOptionType.Social)
            .AddOption("任务与委托", "quest_menu", DialogueOptionType.Quest)
            .AddOption("挑战与试炼", "challenge_menu", DialogueOptionType.Combat)
            .AddOption("深度世界观", "lore_menu", DialogueOptionType.Informative)
            .AddOption("秘密与传说", "secrets_menu", DialogueOptionType.Informative)
            .AddOption("调试工具", "debug_tools", DialogueOptionType.Special)
            .AddOption("帮助", "help")
            .AddOption("再见", "bye", DialogueOptionType.Exit);

        // ===== 介绍 =====
        b.StartNode("what_is_this",
            "{npcName}解释道：\"对话树是一种多分支对话系统。\n\n传统的 Terraria 对话只有两个按钮，而对话树可以有任意多个选项，每个选项可以：\n* 跳转到不同的对话节点\n* 需要特定条件才能显示（如信念、声望、物品）\n* 触发各种效果（修改信念、给予物品等）\n\n你想看看具体效果吗？\"")
            .AddOption("好的，展示给我看", "show_tree")
            .AddOption("回到主菜单", "greeting");

        // ===== 对话树展示 =====
        b.StartNode("show_tree",
            "{npcName}拍了拍手：\"对话树有三种主要类型，你想看哪种？\"")
            .AddOption("基础对话树", "tree_basic")
            .AddOption("条件分支演示", "tree_conditional", DialogueOptionType.Informative)
            .AddOption("对话效果演示", "tree_effect", DialogueOptionType.Special)
            .AddOption("返回", "greeting", DialogueOptionType.Exit);

        b.StartNode("tree_basic",
            "{npcName}说：\"基础对话树有三种选项类型：\n\n1. 单选项 - 只有一个选项，必须选择\n2. 多选项 - 多个选项自由选择\n3. 条件选项 - 满足条件才显示\n\n你想体验哪种？\"")
            .AddOption("单选项演示", "single_option")
            .AddOption("多选项演示", "multi_option")
            .AddOption("条件选项演示", "conditional_option", DialogueOptionType.Informative)
            .AddOption("返回", "show_tree", DialogueOptionType.Exit);

        b.StartNode("single_option",
            "{npcName}点点头：\"这是单选项演示。你只有一个选择：继续。\"")
            .AddOption("继续", "single_option_2");

        b.StartNode("single_option_2",
            "{npcName}微笑着说：\"很好！你通过了单选项演示。这就是单选项对话树——线性推进，没有分支。\"")
            .AddOption("返回基础菜单", "tree_basic");

        b.StartNode("multi_option",
            "{npcName}张开双臂：\"多选项演示！你可以从多个选项中选择：\"")
            .AddOption("选项 A - 红色的药丸", "multi_a")
            .AddOption("选项 B - 蓝色的药丸", "multi_b")
            .AddOption("选项 C - 自己造一个", "multi_c");

        b.StartNode("multi_a",
            "{npcName}递给你一颗红色药丸：\"你选择了红色。故事继续...但这是演示，所以回到菜单吧。\"")
            .AddOption("返回", "tree_basic", DialogueOptionType.Exit);

        b.StartNode("multi_b",
            "{npcName}递给你一颗蓝色药丸：\"你选择了蓝色。故事结束...但这是演示，所以回到菜单吧。\"")
            .AddOption("返回", "tree_basic", DialogueOptionType.Exit);

        b.StartNode("multi_c",
            "{npcName}大笑：\"哈哈，有创意！但抱歉，我只准备了红蓝两色。回到菜单吧。\"")
            .AddOption("返回", "tree_basic", DialogueOptionType.Exit);

        int debuggerItemType = ModContent.ItemType<DialogueDebugger>();
        b.StartNode("conditional_option",
            "{npcName}解释道：\"条件选项需要特定条件才会显示。\n\n例如：\n* 持有特定道具 -> 显示特殊选项\n* 声望达到阈值 -> 显示尊敬选项\n* 信念状态不同 -> 显示不同选项\n\n你可以用调试道具修改状态，看看选项如何变化！\"")
            .AddOption("特殊选项（需要持有信念调试器）", "has_debugger",
                DialogueOptionType.Special,
                condition: new HasItemCondition(debuggerItemType))
            .AddOption("尊敬选项（需要声望>=50）", "high_reputation",
                DialogueOptionType.Special,
                condition: new ReputationCondition(FactionID.GuYue, 50))
            .AddOption("返回", "tree_basic", DialogueOptionType.Exit);

        b.StartNode("has_debugger",
            "{npcName}眼睛一亮：\"哦！你带着信念调试器！太好了，你可以用它来修改我的状态，看看对话树如何响应。\"")
            .AddOption("好的", "debug_tools");

        b.StartNode("high_reputation",
            "{npcName}恭敬地行礼：\"尊敬的大人！没想到您在本地的声望如此之高。请允许我为您提供更多服务。\"")
            .AddOption("打开商店", null, DialogueOptionType.Trade, opensShop: "DemoShop")
            .AddOption("继续对话", "greeting");

        int beliefModifierType = ModContent.ItemType<BeliefModifier>();
        b.StartNode("tree_effect",
            "{npcName}兴奋地说：\"对话效果是对话树最强大的功能！选择选项可以触发各种效果：\"")
            .AddOptionWithEffects("修改信念（增加好感）", "effect_belief",
                DialogueOptionType.Normal, null, null,
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.1f),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                    ModifyBeliefEffect.ModifyOp.Add, 0.15f),
                new ShowMessageEffect("NPC对你的好感度提升了！", Color.Green))
            .AddOptionWithEffects("获得演示道具", "effect_item",
                DialogueOptionType.Special, null, null,
                new GiveItemEffect(beliefModifierType, 1),
                new ShowMessageEffect("你获得了信念修改卡！", Color.Gold))
            .AddOptionWithEffects("触发警告（演示效果）", "effect_warning",
                DialogueOptionType.Risky, null, null,
                new ShowMessageEffect("警告！NPC态度恶化！", Color.Red),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, 0.2f))
            .AddOption("返回", "show_tree", DialogueOptionType.Exit);

        b.StartNode("effect_belief",
            "{npcName}微笑着说：\"信念已修改！你可以用信念调试器查看变化。\n\nRiskThreshold 降低了（NPC更信任你）\nConfidenceLevel 提升了（NPC更了解你）\"")
            .AddOption("继续", "tree_effect");

        b.StartNode("effect_item",
            "{npcName}递给你一张卡片：\"这是信念修改卡，对准任何蛊师NPC使用，可以修改其信念状态。试试看！\"")
            .AddOption("谢谢！", "tree_effect");

        b.StartNode("effect_warning",
            "{npcName}后退一步：\"你触发了警告效果！NPC的 RiskThreshold 上升了，这意味着他变得更警惕。\n\n这就是对话效果的实际应用——每个选择都有后果！\"")
            .AddOption("明白了", "tree_effect");

        // ===== 炼制与合成菜单 =====
        b.StartNode("craft_menu",
            "{npcName}拿出一个小型炼炉：\"我这里可以帮你炼制一些基础物品。你想炼制什么？\"")
            .AddOptionWithEffects("炼制疗伤丹药（需要5个蘑菇+10元石）", "craft_done",
                DialogueOptionType.Craft, new HasItemCondition(ItemID.Mushroom, 5), "消耗5个蘑菇和10元石",
                new RemoveItemEffect(ItemID.Mushroom, 5),
                new BuyItemEffect(ItemID.HealingPotion, 10, 3),
                new ShowMessageEffect("炼制成功！获得3瓶疗伤丹药！", Color.Green))
            .AddOptionWithEffects("炼制铁锭（需要3个铁矿石+5元石）", "craft_done",
                DialogueOptionType.Craft, new HasItemCondition(ItemID.IronOre, 3), "消耗3个铁矿石和5元石",
                new RemoveItemEffect(ItemID.IronOre, 3),
                new GiveItemEffect(ItemID.IronBar, 1),
                new ShowMessageEffect("炼制成功！获得1个铁锭！", Color.Green))
            .AddOptionWithEffects("炼制金锭（需要4个金矿石+10元石）", "craft_done",
                DialogueOptionType.Craft, new HasItemCondition(ItemID.GoldOre, 4), "消耗4个金矿石和10元石",
                new RemoveItemEffect(ItemID.GoldOre, 4),
                new GiveItemEffect(ItemID.GoldBar, 1),
                new ShowMessageEffect("炼制成功！获得1个金锭！", Color.Gold))
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("craft_done",
            "{npcName}擦了擦汗：\"炼制完成！修真界的炼器之道博大精深，这只是皮毛而已。\"")
            .AddOption("继续炼制", "craft_menu", DialogueOptionType.Craft)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        // ===== 修炼指导菜单 =====
        b.StartNode("training_menu",
            "{npcName}正襟危坐：\"修炼之道，贵在坚持。你想了解哪方面的修炼知识？\"")
            .AddOption("请教战斗技巧", "training_combat", DialogueOptionType.Teach)
            .AddOption("请教资源获取", "training_resource", DialogueOptionType.Teach)
            .AddOption("请教蛊虫培养", "training_gu", DialogueOptionType.Teach)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("training_combat",
            "{npcName}摆出架势：\"战斗技巧方面：\n\n1. 善用环境——利用地形优势\n2. 合理搭配——不同蛊虫配合使用\n3. 知己知彼——了解敌人弱点\n4. 保持真元——不要过度消耗\n\n记住，活着才有输出！\"")
            .AddOption("受益匪浅！", "training_menu")
            .AddOption("返回主菜单", "greeting");

        b.StartNode("training_resource",
            "{npcName}拿出一张地图：\"资源获取方面：\n\n1. 青茅山盛产草药和矿石\n2. 夜晚有更多稀有资源出现\n3. 完成委托可以获得元石奖励\n4. 与商人交易是获取稀有物品的好方法\n\n记住，元石是修真界的硬通货！\"")
            .AddOption("明白了！", "training_menu")
            .AddOption("返回主菜单", "greeting");

        b.StartNode("training_gu",
            "{npcName}小心翼翼地拿出一只蛊虫：\"蛊虫培养方面：\n\n1. 蛊虫需要真元喂养，不要饿着它们\n2. 不同蛊虫有不同的进化路线\n3. 合炼可以产生更强大的蛊虫\n4. 本命蛊与主人性命相连，务必珍惜\n\n蛊虫是蛊师的根本，切记！\"")
            .AddOption("受教了！", "training_menu")
            .AddOption("返回主菜单", "greeting");

        // ===== 秘密与传说菜单 =====
        b.StartNode("secrets_menu",
            "{npcName}压低声音，四处张望后说：\"你想知道一些不为人知的秘密？小心隔墙有耳...\"")
            .AddOption("关于这个世界的真相", "secret_world", DialogueOptionType.Informative)
            .AddOption("关于古月家族的秘密", "secret_guyue", DialogueOptionType.Informative)
            .AddOption("关于远古遗迹", "secret_ruins", DialogueOptionType.Informative)
            .AddOption("算了，太危险了", "greeting", DialogueOptionType.Exit);

        b.StartNode("secret_world",
            "{npcName}神秘地说：\"这个世界远比表面看起来复杂。据说在远古时代，蛊师们拥有移山填海之能。但一场大劫之后，大部分传承都失落了。\n\n现在残存的蛊师家族，不过是当年辉煌的余烬罢了...\"")
            .AddOption("还有更多吗？", "secrets_menu")
            .AddOption("令人震惊...", "greeting");

        b.StartNode("secret_guyue",
            "{npcName}声音更低了：\"古月家族...表面上是青茅山的主宰，但内部派系林立。漠脉、赤脉、药脉各怀心思。\n\n据说族长手中掌握着一件上古至宝，但没人见过真面目。\"")
            .AddOption("有意思...", "secrets_menu")
            .AddOption("我知道了", "greeting");

        b.StartNode("secret_ruins",
            "{npcName}眼中闪过一丝向往：\"传说在青茅山深处，有一座远古蛊师的洞府。里面藏有无数珍宝和失传的蛊方。\n\n但洞府外有强大的禁制，非有缘人不得入内。据说每百年才会开启一次...\"")
            .AddOption("我要去寻找！", "secrets_menu")
            .AddOption("太遥远了", "greeting");

        // ===== 挑战与试炼菜单 =====
        b.StartNode("challenge_menu",
            "{npcName}眼中燃起斗志：\"你想挑战我？很好！修真之路，不进则退。你想挑战什么？\"")
            .AddOption("知识问答挑战", "challenge_quiz", DialogueOptionType.Teach)
            .AddOption("运气赌博挑战", "challenge_gamble", DialogueOptionType.Risky)
            .AddOption("战斗模拟挑战", "challenge_combat", DialogueOptionType.Combat)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("challenge_quiz",
            "{npcName}拿出一本古籍：\"修真知识问答！答对了有奖励，答错了...嘿嘿。准备好了吗？\"")
            .AddOption("第一题：蛊师修炼的核心是什么？", "quiz_q1")
            .AddOption("算了，我不擅长答题", "challenge_menu");

        b.StartNode("quiz_q1",
            "{npcName}问道：\"蛊师修炼的核心是什么？\"")
            .AddOptionWithEffects("A. 真元", "quiz_q1_correct",
                DialogueOptionType.Teach, null, "选择A",
                new ShowMessageEffect("正确！真元是蛊师修炼的根本！", Color.Green),
                new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 10))
            .AddOptionWithEffects("B. 体力", "quiz_q1_wrong",
                DialogueOptionType.Teach, null, "选择B",
                new ShowMessageEffect("错误！蛊师修炼的核心是真元，不是体力。", Color.Red))
            .AddOptionWithEffects("C. 运气", "quiz_q1_wrong",
                DialogueOptionType.Teach, null, "选择C",
                new ShowMessageEffect("错误！虽然运气很重要，但核心是真元。", Color.Red));

        b.StartNode("quiz_q1_correct",
            "{npcName}鼓掌：\"答对了！奖励你10元石。下一题：蛊虫分为几个等级？\"")
            .AddOptionWithEffects("A. 三个", "quiz_q2_wrong",
                DialogueOptionType.Teach, null, "选择A",
                new ShowMessageEffect("错误！蛊虫通常分为五个等级。", Color.Red))
            .AddOptionWithEffects("B. 五个", "quiz_q2_correct",
                DialogueOptionType.Teach, null, "选择B",
                new ShowMessageEffect("正确！蛊虫分为凡、灵、宝、道、仙五个等级！", Color.Green),
                new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 20))
            .AddOptionWithEffects("C. 七个", "quiz_q2_wrong",
                DialogueOptionType.Teach, null, "选择C",
                new ShowMessageEffect("错误！蛊虫通常分为五个等级。", Color.Red));

        b.StartNode("quiz_q1_wrong",
            "{npcName}摇头：\"答错了！不过没关系，再来一次？\"")
            .AddOption("重新挑战", "challenge_quiz")
            .AddOption("算了", "challenge_menu");

        b.StartNode("quiz_q2_correct",
            "{npcName}赞叹道：\"厉害！两题全对！奖励你20元石。看来你对修真知识很了解啊！\"")
            .AddOption("继续挑战", "challenge_menu")
            .AddOption("返回主菜单", "greeting");

        b.StartNode("quiz_q2_wrong",
            "{npcName}惋惜地说：\"可惜，第二题答错了。不过第一题答对了，还是给你10元石吧。\"")
            .AddOption("重新挑战", "challenge_quiz")
            .AddOption("返回主菜单", "greeting");

        b.StartNode("challenge_gamble",
            "{npcName}拿出一个骰盅：\"来赌一把！押注元石，赢了翻倍，输了全没。敢不敢？\"")
            .AddOptionWithEffects("押注10元石（50%胜率）", "gamble_result",
                DialogueOptionType.Risky, new HasYuanSCondition(10), "押注10元石，50%胜率",
                new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 10, 0),
                new RandomEffect(
                    new CompositeEffect(
                        new ShowMessageEffect("你赢了！获得20元石！", Color.Gold),
                        new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 20)),
                    new ShowMessageEffect("你输了！10元石没了...", Color.Red),
                    0.5f))
            .AddOptionWithEffects("押注50元石（30%胜率）", "gamble_result",
                DialogueOptionType.Risky, new HasYuanSCondition(50), "押注50元石，30%胜率",
                new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 50, 0),
                new RandomEffect(
                    new CompositeEffect(
                        new ShowMessageEffect("大赢！获得150元石！", Color.Gold),
                        new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 150)),
                    new ShowMessageEffect("你输了！50元石没了...", Color.Red),
                    0.3f))
            .AddOptionWithEffects("押注100元石（10%胜率）", "gamble_result",
                DialogueOptionType.Risky, new HasYuanSCondition(100), "押注100元石，10%胜率",
                new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 100, 0),
                new RandomEffect(
                    new CompositeEffect(
                        new ShowMessageEffect("超级大奖！获得500元石！！", Color.Gold),
                        new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 500)),
                    new ShowMessageEffect("你输了！100元石没了...", Color.Red),
                    0.1f))
            .AddOption("不赌了", "challenge_menu");

        b.StartNode("gamble_result",
            "{npcName}收起骰盅：\"赌博有风险，下注需谨慎！还要继续吗？\"")
            .AddOption("再来一局", "challenge_gamble", DialogueOptionType.Risky)
            .AddOption("见好就收", "challenge_menu");

        b.StartNode("challenge_combat",
            "{npcName}摆出战斗姿态：\"战斗模拟！我会控制力道，不会真的伤到你。准备好了吗？\"")
            .AddOptionWithEffects("初级挑战（简单）", "combat_result",
                DialogueOptionType.Combat, null, "初级战斗模拟",
                new ShowMessageEffect("你完成了初级战斗模拟！获得20元石！", Color.Green),
                new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 20))
            .AddOptionWithEffects("中级挑战（普通）", "combat_result",
                DialogueOptionType.Combat, null, "中级战斗模拟",
                new ShowMessageEffect("你完成了中级战斗模拟！获得50元石！", Color.Green),
                new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 50))
            .AddOptionWithEffects("高级挑战（困难）", "combat_result",
                DialogueOptionType.Combat, null, "高级战斗模拟",
                new ShowMessageEffect("你完成了高级战斗模拟！获得100元石！", Color.Gold),
                new GiveItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 100))
            .AddOption("返回", "challenge_menu");

        b.StartNode("combat_result",
            "{npcName}擦了擦汗：\"不错不错！你的实力有所提升。还要继续挑战吗？\"")
            .AddOption("继续挑战", "challenge_combat", DialogueOptionType.Combat)
            .AddOption("今天就到这里", "challenge_menu");

        // ===== 深度世界观菜单 =====
        b.StartNode("lore_menu",
            "{npcName}拿出一本厚重的古籍：\"你想深入了解这个世界的真相？很好，求知欲是修真者最重要的品质。\"")
            .AddOption("蛊师的起源", "lore_origin", DialogueOptionType.Informative)
            .AddOption("五大势力的历史", "lore_factions", DialogueOptionType.Informative)
            .AddOption("蛊虫的奥秘", "lore_gu", DialogueOptionType.Informative)
            .AddOption("上古大劫的真相", "lore_cataclysm", DialogueOptionType.Informative)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("lore_origin",
            "{npcName}缓缓道来：\"蛊师的起源可以追溯到上古时期。传说第一位蛊师是一位名叫'蛊祖'的奇人，他在深山中发现了第一只蛊虫。\n\n蛊祖发现，通过培养和炼化蛊虫，人类可以获得超乎想象的力量。他将这门技艺传授给了十二位弟子，这十二人后来建立了最初的十二蛊师家族。\n\n然而，经过数千年的变迁，大部分家族已经消亡或没落。如今只剩下五大势力还在传承蛊师之道...\"")
            .AddOption("继续听", "lore_menu")
            .AddOption("太长了，下次再听", "greeting");

        b.StartNode("lore_factions",
            "{npcName}翻开古籍的另一页：\"五大势力分别是：\n\n1. 古月家族——青茅山的主宰，以月之蛊闻名\n2. 白家——北方雪原的统治者，擅长冰系蛊术\n3. 商家——遍布天下的商业帝国，掌控资源流通\n4. 铁家——以锻造蛊器闻名，战力强悍\n5. 天鹤宗——最古老的蛊师宗门，传承最为完整\n\n每个势力都有自己的独门蛊术和传承体系。\"")
            .AddOption("继续听", "lore_menu")
            .AddOption("很有意思", "greeting");

        b.StartNode("lore_gu",
            "{npcName}小心翼翼地取出一只蛊虫标本：\"蛊虫分为五个等级：\n\n凡级——最常见的蛊虫，如月光蛊、治疗蛊\n灵级——拥有灵智的蛊虫，如剑影蛊、盾甲蛊\n宝级——稀世珍宝级别的蛊虫，如天元蛊、时空蛊\n道级——蕴含天地法则的蛊虫，极其罕见\n仙级——传说中的存在，据说只有蛊祖拥有过\n\n蛊虫可以通过喂养、合炼、进化来提升等级。\"")
            .AddOption("继续听", "lore_menu")
            .AddOption("受益匪浅", "greeting");

        b.StartNode("lore_cataclysm",
            "{npcName}压低声音，神色凝重：\"上古大劫...这是蛊师世界最大的禁忌话题。\n\n据说在数千年前，蛊师文明达到了巅峰。但一场突如其来的大劫摧毁了一切。没有人知道大劫的具体原因，但流传着几种说法：\n\n1. 天罚说——蛊师的力量触怒了天道\n2. 内乱说——蛊师家族之间的战争引发了灾难\n3. 蛊虫反噬说——某位蛊师试图炼制仙级蛊虫失败\n\n无论真相如何，大劫之后，蛊师文明倒退了几千年。许多传承永远失传了...\"")
            .AddOption("令人唏嘘", "lore_menu")
            .AddOption("我要探寻真相！", "greeting");

        // ===== 交易与购买菜单 =====
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();
        b.StartNode("trade_menu",
            "{npcName}搓了搓手：\"我这里有一些好东西，不过需要元石来交换。你想看看什么？\"")
            .AddOptionWithEffects("购买疗伤丹药（50元石）", "trade_done",
                DialogueOptionType.Trade, new HasYuanSCondition(50), "消耗50元石恢复50点生命",
                new BuyItemEffect(ItemID.LesserHealingPotion, 50, 3),
                new ShowMessageEffect("你购买了3瓶疗伤丹药！", Color.Gold))
            .AddOptionWithEffects("购买铁甲（200元石）", "trade_done",
                DialogueOptionType.Trade, new HasYuanSCondition(200), "消耗200元石获得铁甲",
                new BuyItemEffect(ItemID.IronChainmail, 200, 1),
                new ShowMessageEffect("你购买了铁甲！", Color.Gold))
            .AddOptionWithEffects("出售铁矿石（每个5元石）", "trade_done",
                DialogueOptionType.Barter, new HasItemCondition(ItemID.IronOre, 5), "出售5个铁矿石获得25元石",
                new SellItemEffect(ItemID.IronOre, 5, 5),
                new ShowMessageEffect("出售了5个铁矿石！", Color.Gold))
            .AddOptionWithEffects("出售铜矿石（每个3元石）", "trade_done",
                DialogueOptionType.Barter, new HasItemCondition(ItemID.CopperOre, 5), "出售5个铜矿石获得15元石",
                new SellItemEffect(ItemID.CopperOre, 3, 5),
                new ShowMessageEffect("出售了5个铜矿石！", Color.Gold))
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("trade_done",
            "{npcName}点点头：\"交易完成！欢迎下次再来。还有什么需要吗？\"")
            .AddOption("继续交易", "trade_menu", DialogueOptionType.Trade)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        // ===== 社交与互动菜单 =====
        b.StartNode("social_menu",
            "{npcName}微笑着说：\"社交互动是修真世界的重要部分。你想做什么？\"")
            .AddOptionWithEffects("闲聊（增进关系）", "social_done",
                DialogueOptionType.Social, null, "与NPC闲聊，增进好感",
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.05f),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                    ModifyBeliefEffect.ModifyOp.Add, 0.1f),
                new ShowMessageEffect("你和NPC聊得很愉快！好感度略微提升。", Color.LightBlue))
            .AddOptionWithEffects("请教修炼心得", "social_done",
                DialogueOptionType.Teach, null, "向NPC请教修炼经验",
                new ShowMessageEffect("NPC分享了一些修炼心得，你感觉受益匪浅！", Color.Cyan))
            .AddOptionWithEffects("赠送礼物（提升声望）", "social_done",
                DialogueOptionType.Social, new HasItemCondition(ItemID.GoldBar, 3), "消耗3个金锭提升声望",
                new RemoveItemEffect(ItemID.GoldBar, 3),
                new ModifyReputationEffect(FactionID.GuYue, 20, "赠送礼物"),
                new ShowMessageEffect("NPC很高兴！与古月势力的声望提升了！", Color.Gold))
            .AddOptionWithEffects("切磋武艺", "social_done",
                DialogueOptionType.Combat, null, "与NPC切磋，不伤性命",
                new ShowMessageEffect("你和NPC切磋了一番，双方都受益良多！", Color.Orange),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                    ModifyBeliefEffect.ModifyOp.Add, 0.2f))
            .AddOptionWithEffects("结盟提议", "social_done",
                DialogueOptionType.Ally, new ReputationCondition(FactionID.GuYue, 100), "需要声望>=100",
                new ShowMessageEffect("你与NPC结成了盟友关系！", Color.Gold),
                new ModifyReputationEffect(FactionID.GuYue, 50, "结盟"))
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("social_done",
            "{npcName}笑着说：\"社交互动完成！人际关系需要慢慢培养。\"")
            .AddOption("继续社交", "social_menu", DialogueOptionType.Social)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        // ===== 任务与委托菜单 =====
        b.StartNode("quest_menu",
            "{npcName}拿出一卷卷轴：\"我这里有几种委托，你有兴趣接吗？\"")
            .AddOptionWithEffects("采集任务：收集10个木材", "quest_done",
                DialogueOptionType.Quest, new HasItemCondition(ItemID.Wood, 10), "需要10个木材",
                new RemoveItemEffect(ItemID.Wood, 10),
                new GiveItemEffect(yuanSItemType, 30),
                new ShowMessageEffect("任务完成！获得30元石奖励！", Color.Gold))
            .AddOptionWithEffects("狩猎任务：收集5个晶状体", "quest_done",
                DialogueOptionType.Quest, new HasItemCondition(ItemID.Lens, 5), "需要5个晶状体",
                new RemoveItemEffect(ItemID.Lens, 5),
                new GiveItemEffect(yuanSItemType, 50),
                new ShowMessageEffect("任务完成！获得50元石奖励！", Color.Gold))
            .AddOptionWithEffects("讨伐任务：击杀5只史莱姆", "quest_done",
                DialogueOptionType.Quest, null, "接受讨伐史莱姆的任务",
                new GiveQuestEffect("讨伐史莱姆", "击杀5只史莱姆，回来领取奖励", 50))
            .AddOptionWithEffects("探索任务：寻找隐藏宝藏", "quest_done",
                DialogueOptionType.Quest, null, "接受探索任务",
                new RevealMapEffect("隐藏宝藏", Main.spawnTileX * 16 + 500, Main.spawnTileY * 16 - 200),
                new ShowMessageEffect("地图上标记了一个宝藏位置！", Color.Cyan))
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        b.StartNode("quest_done",
            "{npcName}点头赞许：\"任务处理完毕！完成更多委托可以提升声望。\"")
            .AddOption("查看其他委托", "quest_menu", DialogueOptionType.Quest)
            .AddOption("返回主菜单", "greeting", DialogueOptionType.Exit);

        // ===== 调试工具 =====
        b.StartNode("debug_tools",
            "{npcName}拿出一个工具箱：\"调试工具可以让你修改我的各种状态，观察对话树的变化。你想修改什么？\"")
            .AddOptionWithEffects("设置 RiskThreshold = 0.1（自信）", "debug_result",
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Set, 0.1f))
            .AddOptionWithEffects("设置 RiskThreshold = 0.9（恐惧）", "debug_result",
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Set, 0.9f))
            .AddOptionWithEffects("设置态度为 Wary", "debug_result",
                new SetAttitudeEffect(GuAttitude.Wary))
            .AddOptionWithEffects("设置态度为 Respectful", "debug_result",
                new SetAttitudeEffect(GuAttitude.Respectful))
            .AddOptionWithEffects("重置所有状态", "debug_result",
                new ResetStateEffect())
            .AddOption("返回", "greeting", DialogueOptionType.Exit);

        b.StartNode("debug_result",
            "{npcName}点点头：\"状态已修改！请关闭对话界面后重新打开，查看对话树的变化。\n\n提示：你可以用信念调试器查看修改后的具体数值。\"")
            .AddOption("好的", "greeting");

        // ===== 帮助 =====
        b.StartNode("help",
            "{npcName}拿出一本手册：\"对话树演示员使用指南：\n\n1. 使用'对话树召唤令'生成我\n2. 右键与我对话，浏览各个菜单\n3. 使用调试道具修改我的状态\n4. 观察不同状态下的对话树变化\n5. 体验对话效果的实际影响\n\n需要什么帮助吗？\"")
            .AddOption("回到主菜单", "greeting")
            .AddOption("再见", "bye", DialogueOptionType.Exit);

        // ===== 退出 =====
        b.StartNode("bye",
            "{npcName}向你挥手告别：\"感谢体验对话树演示！希望这个系统能为你的Mod增添更多深度。再见！\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Wary - 警惕对话树

    private DialogueTree BuildWaryTree()
    {
        var b = new DialogueTreeBuilder("Demo_Wary", "greeting");

        b.StartNode("greeting",
            "{npcName}警惕地盯着你，手按在武器上：\"你是谁？想干什么？我警告你，别耍花样！\"")
            .AddOption("我没有恶意", "explain", DialogueOptionType.Informative)
            .AddOption("我只是路过", "pass_by")
            .AddOption("我想打听点消息", "wary_info", DialogueOptionType.Informative)
            .AddOption("我想交易", "trade_attempt", DialogueOptionType.Trade)
            .AddOption("...（沉默离开）", "bye", DialogueOptionType.Exit);

        b.StartNode("explain",
            "{npcName}稍微放松了一点，但仍然保持警惕：\"哼，每个这么说的人都不可信。你有什么证明？\"")
            .AddOption("我可以给你看我的身份证明", "show_proof")
            .AddOption("算了，你不信拉倒", "bye", DialogueOptionType.Exit);

        b.StartNode("show_proof",
            "{npcName}仔细打量了你一番：\"好吧...你看起来确实不像坏人。但我还是要盯着你。\"")
            .AddOptionWithEffects("谢谢你的信任", "greeting",
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.1f))
            .AddOption("哼", "bye", DialogueOptionType.Exit);

        b.StartNode("pass_by",
            "{npcName}侧身让开一条路：\"快走快走，别在这里晃悠。\"")
            .AddOption("好的，再见", "bye", DialogueOptionType.Exit);

        b.StartNode("trade_attempt",
            "{npcName}冷笑一声：\"交易？我凭什么相信你不会趁机偷袭？\"")
            .AddOption("我可以先给你看货物", "trade_show")
            .AddOption("那算了", "bye", DialogueOptionType.Exit);

        b.StartNode("trade_show",
            "{npcName}犹豫了一下：\"...好吧，让我看看你有什么。但别想耍花招！\"")
            .AddOption("打开商店", null, DialogueOptionType.Trade, opensShop: "DemoShop")
            .AddOption("算了", "bye", DialogueOptionType.Exit);

        b.StartNode("wary_info",
            "{npcName}犹豫了一下，还是开口了：\"你想知道什么？不过我不保证说的都是真的。\"")
            .AddOption("附近有什么危险？", "wary_danger")
            .AddOption("你在这里做什么？", "wary_why_here")
            .AddOption("算了，不问了", "greeting");

        b.StartNode("wary_danger",
            "{npcName}指了指东边：\"那边有妖兽出没，我劝你别去送死。不过...你死了也不关我的事。\"")
            .AddOption("多谢提醒", "greeting")
            .AddOption("哼", "bye", DialogueOptionType.Exit);

        b.StartNode("wary_why_here",
            "{npcName}警惕地看了你一眼：\"这不关你的事。每个人都有自己的理由。\"")
            .AddOption("好吧", "greeting")
            .AddOption("（离开）", "bye", DialogueOptionType.Exit);

        b.StartNode("bye",
            "{npcName}一直盯着你直到你走远：\"呼...总算走了。\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Respectful - 尊敬对话树

    private DialogueTree BuildRespectfulTree()
    {
        var b = new DialogueTreeBuilder("Demo_Respectful", "greeting");

        b.StartNode("greeting",
            "{npcName}恭敬地九十度鞠躬：\"大人！不知您大驾光临，有失远迎！请问有什么可以为您效劳的？\"")
            .AddOption("我需要一些物资", "shop", DialogueOptionType.Trade)
            .AddOption("给我讲讲这里的情况", "tell_story", DialogueOptionType.Informative)
            .AddOption("有什么特殊服务吗？", "special_service", DialogueOptionType.Special)
            .AddOption("我要赏赐你", "respect_gift", DialogueOptionType.Social)
            .AddOption("你太客气了", "humble")
            .AddOption("我先走了", "bye", DialogueOptionType.Exit);

        b.StartNode("shop",
            "{npcName}连忙打开货架：\"大人请随意挑选，小人这里的东西虽然不算珍贵，但都是精选好货！\"")
            .AddOption("打开商店", null, DialogueOptionType.Trade, opensShop: "DemoShop")
            .AddOption("下次再说", "greeting");

        b.StartNode("tell_story",
            "{npcName}清了清嗓子：\"大人想听哪方面的？小人虽然见识有限，但在这片地界也混了几十年了。\"")
            .AddOption("附近的势力分布", "story_factions")
            .AddOption("有什么危险的地方", "story_dangers")
            .AddOption("有没有宝藏传说", "story_treasure")
            .AddOption("够了，谢谢", "greeting");

        b.StartNode("story_factions",
            "{npcName}压低声音：\"这片地界主要有三大势力...（以下省略三千字详细情报）\"")
            .AddOption("很有价值的情报", "greeting")
            .AddOption("还有别的吗？", "tell_story");

        b.StartNode("story_dangers",
            "{npcName}神色凝重：\"东边的山谷最近不太平，据说有妖兽出没。大人如果要过去，务必小心。\"")
            .AddOption("多谢提醒", "greeting")
            .AddOption("还有别的吗？", "tell_story");

        b.StartNode("story_treasure",
            "{npcName}神秘兮兮地说：\"据说北边的古墓里藏着一件神器，但机关重重，至今没人能取出来。以大人的实力，或许可以一试。\"")
            .AddOption("有意思", "greeting")
            .AddOption("还有别的吗？", "tell_story");

        b.StartNode("special_service",
            "{npcName}眼睛一亮：\"大人果然慧眼！小人这里确实有些特殊门路...\"")
            .AddOption("什么门路？", "special_detail")
            .AddOption("算了，下次吧", "greeting");

        b.StartNode("special_detail",
            "{npcName}凑近低语：\"小人认识一些...特殊渠道的商人。如果大人需要一些市面上买不到的东西，小人可以代为引荐。\"")
            .AddOption("好，下次带我去", "greeting")
            .AddOption("我不需要", "greeting");

        b.StartNode("respect_gift",
            "{npcName}受宠若惊：\"大人要赏赐小人？这...这怎么好意思！\"")
            .AddOptionWithEffects("给你一些元石（50元石）", "respect_gift_done",
                DialogueOptionType.Social, new HasYuanSCondition(50), "赠送50元石",
                new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 50, 0),
                new ModifyReputationEffect(FactionID.GuYue, 30, "慷慨赏赐"),
                new ShowMessageEffect("NPC感激涕零！声望大幅提升！", Color.Gold))
            .AddOption("算了，下次吧", "greeting");

        b.StartNode("respect_gift_done",
            "{npcName}激动得热泪盈眶：\"大人恩德，小人没齿难忘！以后有用得着小人的地方，尽管吩咐！\"")
            .AddOption("好好干", "greeting")
            .AddOption("我走了", "bye", DialogueOptionType.Exit);

        b.StartNode("humble",
            "{npcName}惶恐地说：\"大人折煞小人了！能为您效劳是小人的福分！\"")
            .AddOption("好吧，那我要买东西", "shop", DialogueOptionType.Trade)
            .AddOption("你忙吧，我走了", "bye", DialogueOptionType.Exit);

        b.StartNode("bye",
            "{npcName}再次鞠躬：\"大人慢走！随时欢迎您再来！\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Fearful - 恐惧对话树

    private DialogueTree BuildFearfulTree()
    {
        var b = new DialogueTreeBuilder("Demo_Fearful", "greeting");

        b.StartNode("greeting",
            "{npcName}看到你吓得脸色发白，声音颤抖：\"别...别杀我！我什么都说！我什么都给你！\"")
            .AddOption("别怕，我不伤害你", "calm_down", DialogueOptionType.Informative)
            .AddOption("把值钱的东西交出来", "rob", DialogueOptionType.Risky)
            .AddOption("我问你几个问题", "question")
            .AddOption("你需要帮助吗？", "fearful_help", DialogueOptionType.Social)
            .AddOption("...（转身离开）", "bye", DialogueOptionType.Exit);

        b.StartNode("calm_down",
            "{npcName}稍微平静了一点，但还在发抖：\"真...真的吗？你不会骗我吧？\"")
            .AddOptionWithEffects("真的，我说话算话", "calm_down_2",
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.2f))
            .AddOption("骗你的，我就是要杀你", "scare", DialogueOptionType.Risky);

        b.StartNode("calm_down_2",
            "{npcName}松了口气，擦了擦冷汗：\"谢...谢谢大人不杀之恩！有什么需要尽管吩咐！\"")
            .AddOption("打开商店", null, DialogueOptionType.Trade, opensShop: "DemoShop")
            .AddOption("没事了", "bye", DialogueOptionType.Exit);

        b.StartNode("scare",
            "{npcName}吓得瘫软在地：\"饶命啊！！！\"")
            .AddOptionWithEffects("哈哈，开玩笑的", "calm_down_2",
                new ShowMessageEffect("你吓唬了NPC，RiskThreshold 上升了！", Color.Red),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, 0.3f))
            .AddOption("...我真的是坏人", "rob", DialogueOptionType.Risky);

        b.StartNode("rob",
            "{npcName}慌忙把身上的东西都掏出来：\"都给你！都给你！求你别杀我！\"")
            .AddOptionWithEffects("拿走所有东西", "bye",
                new ShowMessageEffect("你抢走了NPC的物品！", Color.Red),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Set, 1.0f))
            .AddOption("算了，我不需要", "calm_down");

        b.StartNode("question",
            "{npcName}连忙点头：\"大人请问！小人知无不言！\"")
            .AddOption("这里安全吗？", "q_safe")
            .AddOption("有没有见过可疑的人？", "q_stranger")
            .AddOption("没别的了", "bye", DialogueOptionType.Exit);

        b.StartNode("q_safe",
            "{npcName}四处张望了一下：\"最近不太平啊大人！听说东边有强盗出没，晚上千万别走夜路！\"")
            .AddOption("知道了", "question")
            .AddOption("还有呢？", "q_stranger");

        b.StartNode("q_stranger",
            "{npcName}压低声音：\"昨天确实有个穿黑袍的人经过，看起来不像本地人...\"")
            .AddOption("有意思", "question")
            .AddOption("够了", "bye", DialogueOptionType.Exit);

        b.StartNode("fearful_help",
            "{npcName}眼睛一亮：\"大人愿意帮我？真的吗？我...我需要一些食物和水，我已经好几天没吃东西了...\"")
            .AddOptionWithEffects("给你一些食物", "fearful_help_done",
                DialogueOptionType.Social, new HasItemCondition(ItemID.Mushroom, 3), "需要3个蘑菇",
                new RemoveItemEffect(ItemID.Mushroom, 3),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.3f),
                new ShowMessageEffect("NPC感激涕零！对你的信任大幅提升！", Color.Green))
            .AddOption("我没有食物", "greeting");

        b.StartNode("fearful_help_done",
            "{npcName}狼吞虎咽地吃着，眼泪都流出来了：\"谢谢大人！谢谢大人！您是我的救命恩人！\"")
            .AddOption("不用谢", "greeting")
            .AddOption("我走了", "bye", DialogueOptionType.Exit);

        b.StartNode("bye",
            "{npcName}看着你离开，如释重负地瘫坐在地上：\"活...活下来了...\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Hostile - 敌对对话树

    private DialogueTree BuildHostileTree()
    {
        var b = new DialogueTreeBuilder("Demo_Hostile", "greeting");

        b.StartNode("greeting",
            "{npcName}拔出武器，眼中充满杀意：\"你还有胆子来？找死！\"")
            .AddOption("等等！我是来谈判的！", "negotiate", DialogueOptionType.Risky)
            .AddOption("你想怎样？", "what_you_want")
            .AddOption("我有话问你", "hostile_info", DialogueOptionType.Informative)
            .AddOption("你打不过我的", "hostile_taunt", DialogueOptionType.Risky)
            .AddOption("（默默离开）", "bye", DialogueOptionType.Exit);

        b.StartNode("negotiate",
            "{npcName}冷笑：\"谈判？你有什么资格跟我谈判？\"")
            .AddOption("我可以给你补偿", "compensate", DialogueOptionType.Trade)
            .AddOption("那你想打一架？", "fight", DialogueOptionType.Risky)
            .AddOption("...我走", "bye", DialogueOptionType.Exit);

        b.StartNode("compensate",
            "{npcName}稍微收敛了杀意：\"补偿？哼，那要看你拿得出什么了。\"")
            .AddOption("打开商店（试图交易）", null, DialogueOptionType.Trade, opensShop: "DemoShop")
            .AddOption("算了，你太贪心了", "fight", DialogueOptionType.Risky);

        b.StartNode("what_you_want",
            "{npcName}咬牙切齿：\"我想让你滚出我的地盘！永远别再回来！\"")
            .AddOption("好，我走", "bye", DialogueOptionType.Exit)
            .AddOption("这是你的地盘？笑话", "fight", DialogueOptionType.Risky);

        b.StartNode("fight",
            "{npcName}举起武器：\"那就别怪我不客气了！\"")
            .AddOptionWithEffects("（准备战斗）", "bye",
                new ShowMessageEffect("NPC进入战斗状态！", Color.Red),
                new SetAttitudeEffect(GuAttitude.Hostile))
            .AddOption("等等！我走！", "bye", DialogueOptionType.Exit);

        b.StartNode("hostile_taunt",
            "{npcName}狂笑道：\"哈哈哈！就凭你也配跟我打？你连我的一根手指都打不过！\"")
            .AddOption("那就试试看！", "fight", DialogueOptionType.Risky)
            .AddOption("你等着，我会回来的", "bye", DialogueOptionType.Exit);

        b.StartNode("hostile_info",
            "{npcName}不耐烦地说：\"你想知道什么？说完赶紧滚！\"")
            .AddOption("你为什么这么恨我？", "hostile_why")
            .AddOption("算了", "bye", DialogueOptionType.Exit);

        b.StartNode("hostile_why",
            "{npcName}咬牙切齿：\"你还有脸问？上次你抢了我的东西，还打伤了我的兄弟！这笔账我记着呢！\"")
            .AddOption("那是个误会", "negotiate")
            .AddOption("我没错", "fight", DialogueOptionType.Risky);

        b.StartNode("bye",
            "{npcName}在你身后喊道：\"滚！别再让我看见你！下次见面就是你的死期！\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Contemptuous - 轻蔑对话树

    private DialogueTree BuildContemptuousTree()
    {
        var b = new DialogueTreeBuilder("Demo_Contemptuous", "greeting");

        b.StartNode("greeting",
            "{npcName}上下打量了你一番，嗤笑一声：\"哟，这不是那个谁吗？怎么，又来找虐了？\"")
            .AddOption("你什么意思？", "what_mean")
            .AddOption("我不想跟你吵", "ignore_him")
            .AddOption("敢不敢打个赌？", "contempt_bet", DialogueOptionType.Risky)
            .AddOption("有本事再说一遍！", "provoke", DialogueOptionType.Risky)
            .AddOption("（转身就走）", "bye", DialogueOptionType.Exit);

        b.StartNode("what_mean",
            "{npcName}嘲讽地笑着：\"字面意思。就你这样的，也敢在我面前晃悠？\"")
            .AddOption("你...！", "provoke", DialogueOptionType.Risky)
            .AddOption("我不跟你一般见识", "ignore_him")
            .AddOption("（走人）", "bye", DialogueOptionType.Exit);

        b.StartNode("ignore_him",
            "{npcName}在你背后喊道：\"怎么？怂了？哈哈，我就知道！\"")
            .AddOption("（忍气吞声地走了）", "bye", DialogueOptionType.Exit)
            .AddOption("（回头怒视）", "provoke", DialogueOptionType.Risky);

        b.StartNode("provoke",
            "{npcName}眼神一冷：\"呵，有点意思。你想怎么着？划下道来！\"")
            .AddOption("来啊！谁怕谁！", "fight", DialogueOptionType.Risky)
            .AddOption("...算了", "bye", DialogueOptionType.Exit);

        b.StartNode("fight",
            "{npcName}摆出战斗姿态：\"那就让你见识见识，什么叫实力碾压！\"")
            .AddOptionWithEffects("（进入战斗）", "bye",
                new ShowMessageEffect("NPC被激怒，进入战斗状态！", Color.Red),
                new SetAttitudeEffect(GuAttitude.Hostile))
            .AddOption("我认输...", "bye", DialogueOptionType.Exit);

        b.StartNode("contempt_bet",
            "{npcName}挑了挑眉：\"哦？你想跟我打赌？赌什么？\"")
            .AddOptionWithEffects("赌100元石，我能在战斗中赢你", "contempt_bet_fight",
                DialogueOptionType.Risky, new HasYuanSCondition(100), "需要100元石",
                new ShowMessageEffect("赌约成立！", Color.Gold))
            .AddOption("算了，不赌了", "greeting");

        b.StartNode("contempt_bet_fight",
            "{npcName}大笑道：\"哈哈哈！有胆量！那就来吧！输了可别哭鼻子！\"")
            .AddOptionWithEffects("（进入战斗）", "bye",
                new ShowMessageEffect("赌约战斗开始！", Color.Red),
                new SetAttitudeEffect(GuAttitude.Hostile))
            .AddOption("我改变主意了", "bye", DialogueOptionType.Exit);

        b.StartNode("bye",
            "{npcName}不屑地啐了一口：\"哼，废物。\"")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    #region Ignore - 无视对话树

    private DialogueTree BuildIgnoreTree()
    {
        var b = new DialogueTreeBuilder("Demo_Ignore", "greeting");

        b.StartNode("greeting",
            "{npcName}瞥了你一眼，继续做自己的事，完全当你不存在。")
            .AddOption("喂！我在跟你说话！", "hey")
            .AddOption("...（尴尬地站着）", "stand")
            .AddOption("给你点好处，理我一下", "ignore_bribe", DialogueOptionType.Social)
            .AddOption("（走人）", "bye", DialogueOptionType.Exit);

        b.StartNode("hey",
            "{npcName}头也不抬，冷淡地说：\"听见了。所以呢？\"")
            .AddOption("你能不能尊重一下人？", "respect")
            .AddOption("...没事了", "bye", DialogueOptionType.Exit);

        b.StartNode("stand",
            "你站了一会儿，{npcName}完全无视你，空气安静得尴尬。")
            .AddOption("我再试试", "hey")
            .AddOption("算了，走吧", "bye", DialogueOptionType.Exit);

        b.StartNode("respect",
            "{npcName}终于抬起头，面无表情地看着你：\"尊重？你配吗？\"说完又低下头继续做事。")
            .AddOption("你...！", "angry", DialogueOptionType.Risky)
            .AddOption("（无言以对）", "bye", DialogueOptionType.Exit);

        b.StartNode("angry",
            "{npcName}依然头也不抬：\"吵死了。要打就打，不打滚。\"")
            .AddOption("打就打！", "fight", DialogueOptionType.Risky)
            .AddOption("...我滚", "bye", DialogueOptionType.Exit);

        b.StartNode("fight",
            "{npcName}终于放下手中的事，站起身：\"早这样不就完了？浪费时间。\"")
            .AddOptionWithEffects("（进入战斗）", "bye",
                new ShowMessageEffect("NPC被激怒，进入战斗状态！", Color.Red),
                new SetAttitudeEffect(GuAttitude.Hostile))
            .AddOption("我错了...", "bye", DialogueOptionType.Exit);

        b.StartNode("ignore_bribe",
            "{npcName}终于有了点反应，瞥了你手中的东西一眼：\"...什么东西？\"")
            .AddOptionWithEffects("给你50元石，跟我说句话", "ignore_bribe_done",
                DialogueOptionType.Social, new HasYuanSCondition(50), "消耗50元石",
                new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 50, 0),
                new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.15f),
                new ShowMessageEffect("NPC终于正眼看你了！", Color.Green))
            .AddOption("算了", "bye", DialogueOptionType.Exit);

        b.StartNode("ignore_bribe_done",
            "{npcName}收起元石，语气依然冷淡但至少愿意说话了：\"行吧，你想说什么？\"")
            .AddOption("终于肯理我了", "greeting")
            .AddOption("没什么，就是确认一下你还活着", "bye", DialogueOptionType.Exit);

        b.StartNode("bye",
            "你离开时，{npcName}自始至终没有多看你一眼。")
            .EndsDialogue();

        return b.Build();
    }

    #endregion

    // ============================================================
    // 对话系统
    // ============================================================

    public override bool CanChat()
    {
        return DebugAttitude != GuAttitude.Hostile;
    }

    public override string GetChat()
    {
        // 当自定义UI打开时，返回空文本（原版对话内容被自定义UI覆盖）
        if (DialogueTreeUI.Instance.IsOpen)
            return "";

        string npcName = NPC.GivenName ?? NPC.TypeName;
        string text = DebugAttitude switch
        {
            GuAttitude.Friendly => npcName + "微笑着向你点头：\"你好！我是对话树演示员，想体验一下对话树系统吗？\"",
            GuAttitude.Wary => npcName + "警惕地看着你：\"你...你想干什么？\"",
            GuAttitude.Respectful => npcName + "恭敬地行礼：\"大人，欢迎光临！\"",
            GuAttitude.Fearful => npcName + "颤抖着说：\"别...别过来！\"",
            GuAttitude.Hostile => npcName + "拔出武器：\"滚开！\"",
            GuAttitude.Contemptuous => npcName + "轻蔑地扫了你一眼：\"呵，又是你。\"",
            GuAttitude.Ignore => npcName + "完全无视你的存在，自顾自地做自己的事。",
            _ => npcName + "看了你一眼：\"有事吗？\""
        };
        return text;
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        // 当自定义UI打开时，隐藏原版按钮
        if (DialogueTreeUI.Instance.IsOpen)
        {
            button = null;
            button2 = null;
            return;
        }

        button = "开始体验";
        button2 = DebugAttitude switch
        {
            GuAttitude.Hostile => null,
            GuAttitude.Wary => null,
            _ => "查看状态"
        };
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shop)
    {
        var manager = DialogueTreeManager.Instance;

        if (firstButton)
        {
            // 根据当前态度选择对应的对话树
            var tree = GetCurrentTree();
            if (tree != null)
            {
                // 重新注册当前态度的对话树（覆盖之前的注册）
                // 这样 manager.StartDialogue(NPC, player) 就能找到正确的树
                manager.RegisterTree<DialogueTreeDemoNPC>(tree);
                if (manager.StartDialogue(NPC, Main.LocalPlayer))
                {
                    var options = manager.GetCurrentOptions();
                    string npcName = NPC.GivenName ?? NPC.TypeName;
                    DialogueTreeUI.Instance.Open(npcName, NPC.type, manager.GetCurrentNPCText(), options);

                    // 不关闭原版对话界面，让自定义UI覆盖在上面
                    Main.npcChatText = "";
                }
                else
                {
                    Main.npcChatText = "对话树启动失败...";
                }
            }
            else
            {
                Main.npcChatText = "对话树启动失败...";
            }
        }
        else
        {
            Main.npcChatText = GetStatusText();
        }
    }

    private string GetStatusText()
    {
        return "===== 对话树演示员状态 =====\n" +
               "态度: " + GuiEnumHelper.GetAttitudeName(DebugAttitude) + "\n" +
               "性格: " + GuiEnumHelper.GetPersonalityName(DebugPersonality) + "\n" +
               "RiskThreshold: " + DebugBelief.RiskThreshold.ToString("F2") + "\n" +
               "ConfidenceLevel: " + DebugBelief.ConfidenceLevel.ToString("F2") + "\n" +
               "EstimatedPower: " + DebugBelief.EstimatedPower.ToString("F2") + "\n" +
               "已交易: " + DebugBelief.HasTraded + "\n" +
               "已战斗: " + DebugBelief.HasFought + "\n" +
               "ObservationCount: " + DebugBelief.ObservationCount;
    }
}
