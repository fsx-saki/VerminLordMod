using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：方源暴露真面目（Stage4高潮）
    /// 大同风后，方源说出"我乃方源！"的名场面
    /// </summary>
    public class Stage4FangYuanReveal : StoryDialogueProvider
    {
        protected override string TreeID => "Story_FangYuanReveal";
        protected override string DisplayName => "方源";
        protected override string GreetingText =>
            "大同风刚刚停歇，烟尘弥漫中，一个身影缓缓走出。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "风沙渐息，方源从废墟中走出，拍了拍身上的灰尘。" +
                "他环顾四周，嘴角浮现一丝嘲讽的笑意。\n\n" +
                "\"大同风……不过如此。\"他轻声说道，" +
                "然后目光扫过在场的所有人，" +
                "最后定格在你身上。")
                .AddOption("方源！你到底是谁？", "ask_identity", DialogueOptionType.Social)
                .AddOption("你策划了这一切？", "accuse", DialogueOptionType.Risky)
                .AddOption("……", "silent", DialogueOptionType.Exit);

            b.StartNode("ask_identity",
                "方源大笑，笑声在废墟中回荡：\n\n" +
                "\"我乃方源！\"\n\n" +
                "这三个字如惊雷般炸响，在场所有人都愣住了。" +
                "方源——那个被整个蛊世界追杀的人，" +
                "竟然一直隐藏在青茅山，隐藏在所有人的眼皮底下！\n\n" +
                "\"没错，从一开始，我就在你们中间。" +
                "贾金生、血祭、义天山……一切都在我的计划之中。\"")
                .AddOption("追击方源！", "chase", DialogueOptionType.Combat,
                    tooltip: "正道倾向+20，追击方源")
                .AddOption("放他走", "let_go", DialogueOptionType.Exit,
                    tooltip: "方源好感度+10，魔道倾向+10")
                .AddOption("加入方源", "join", DialogueOptionType.Quest,
                    tooltip: "方源好感度+30，魔道倾向+30");

            b.StartNode("accuse",
                "方源微微挑眉：\"策划？你说得太简单了。\"\n\n" +
                "\"这不是策划，这是生存。在蛊世界里，" +
                "不主动出击的人，只会被别人吃掉。\"\n\n" +
                "他转身离去：\"义天山的传承我已经拿到了。" +
                "你们继续在这里争吧——与我无关了。\"")
                .AddOption("不能让他走！", "chase", DialogueOptionType.Combat)
                .AddOption("……让他走吧", "let_go", DialogueOptionType.Exit);

            b.StartNode("silent",
                "方源看了你一眼，似乎在你的沉默中读出了什么。\n\n" +
                "\"有意思……\"他低声说道，" +
                "然后转身走向义天山的出口。\n\n" +
                "\"我们还会再见面的。\"")
                .EndsDialogue();

            b.StartNode("chase",
                "你冲向方源，但他早已有所准备。" +
                "一道血光闪过，方源的身影消失在义天山的深处。\n\n" +
                "你没能追上他。但你知道——" +
                "方源这个名字，将永远刻在你的记忆中。")
                .EndsDialogue();

            b.StartNode("let_go",
                "你看着方源远去的背影，心中五味杂陈。\n\n" +
                "也许，在这个弱肉强食的世界里，" +
                "他的选择并没有错。只是……" +
                "代价太大了。")
                .EndsDialogue();

            b.StartNode("join",
                "方源停下脚步，回头看了你一眼：" +
                "\"加入我？你知道这意味着什么吗？\"\n\n" +
                "\"意味着与整个蛊世界为敌。" +
                "意味着永远不能回头。\"\n\n" +
                "他伸出手：\"你确定？\"")
                .AddOption("我确定", "join_confirm", DialogueOptionType.Quest)
                .AddOption("……我再想想", "join_hesitate", DialogueOptionType.Exit);

            b.StartNode("join_confirm",
                "方源嘴角微微上扬：\"好。从现在起，" +
                "你就是我的人。\"\n\n" +
                "他递给你一枚暗红色的令牌：" +
                "\"这是影宗的联络令。拿着它，" +
                "在需要的时候，会有人来接应你。\"\n\n" +
                "\"记住——在暗处，比在明处更安全。\"")
                .EndsDialogue();

            b.StartNode("join_hesitate",
                "方源收回手：\"犹豫是正常的。" +
                "但记住，机会不会等人。\"\n\n" +
                "他转身离去：\"下次见面，" +
                "希望你已经做出了决定。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：太白云生之死（Stage5最催泪场景）
    /// </summary>
    public class Stage5TaiBaiYunShengDeath : StoryDialogueProvider
    {
        protected override string TreeID => "Story_TaiBaiYunShengDeath";
        protected override string DisplayName => "太白云生";
        protected override string GreetingText =>
            "太白云生浑身浴血，但仍然挡在众人面前。他的白发在风中飘散，却依然挺立不倒。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "太白云生回过头来，脸上带着温和的笑容，" +
                "仿佛身上的伤势不存在一般：\n\n" +
                "\"小友……老夫这辈子，活得不算精彩，" +
                "但也算问心无愧。唯一遗憾的，" +
                "就是没能看到你们成长到更高的境界。\"\n\n" +
                "他咳出一口血：\"方源……太强了。" +
                "你们先走，老夫来断后。\"")
                .AddOption("太白前辈！我来帮你！", "help_taibai", DialogueOptionType.Quest,
                    tooltip: "救助太白云生，因果业力-30")
                .AddOption("太白前辈……", "watch_taibai", DialogueOptionType.Exit,
                    tooltip: "旁观，太白云生可能死亡")
                .AddOption("趁乱偷袭方源！", "ambush_fangyuan", DialogueOptionType.Combat,
                    tooltip: "偷袭方源，但可能害死太白云生");

            b.StartNode("help_taibai",
                "你冲上前去，将真元注入太白云生的体内。" +
                "老人的气息逐渐稳定下来。\n\n" +
                "太白云生惊讶地看着你：\"你……" +
                "为什么要救老夫？这很危险啊……\"\n\n" +
                "他眼眶微红：\"谢谢你……" +
                "老夫还欠你一条命。\"\n\n" +
                "方源冷眼旁观：\"多管闲事。\"然后转身离去。")
                .EndsDialogue();

            b.StartNode("watch_taibai",
                "太白云生看出了你的犹豫，他微微一笑：" +
                "\"没关系，小友。老夫理解。\"\n\n" +
                "他转身面对方源，白发在风中飞扬：" +
                "\"这就是……蛊师的世界啊。\"\n\n" +
                "太白云生全力爆发，为众人争取了撤退的时间。" +
                "但当他力竭倒下时，没有人能再救他了。\n\n" +
                "他的最后一句话是：\"……值得。\"")
                .EndsDialogue();

            b.StartNode("ambush_fangyuan",
                "你趁方源注意力在太白云生身上时发动偷袭！" +
                "但方源的反应超乎想象——\n\n" +
                "\"你以为你能伤到我？\"" +
                "方源随手一挥，你被击飞出去，" +
                "重创倒地。\n\n" +
                "太白云生为了保护你，不得不放弃防守，" +
                "被方源一击击中要害……\n\n" +
                "太白云生倒下了。他的眼中没有怨恨，" +
                "只有深深的遗憾。")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：宿命大战开始（Stage6开场）
    /// </summary>
    public class Stage6DestinyWarBegin : StoryDialogueProvider
    {
        protected override string TreeID => "Story_DestinyWarBegin";
        protected override string DisplayName => "天庭使者";
        protected override string GreetingText =>
            "金色的光芒从天而降，一位天庭使者缓缓落地。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "天庭使者环顾四周，声音如同天雷滚滚：" +
                "\"蛊世界的修士们，听好了！\"\n\n" +
                "\"天庭奉宿命之令，降临此地。" +
                "四柱封印即将解除，届时天地将回归正轨。\"\n\n" +
                "\"顺天者昌，逆天者亡。" +
                "现在，做出你们的选择。\"")
                .AddOption("我愿追随天庭", "follow_heaven", DialogueOptionType.Quest,
                    tooltip: "正道路线，与天庭合作")
                .AddOption("天庭算什么东西？", "oppose_heaven", DialogueOptionType.Risky,
                    tooltip: "魔道路线，对抗天庭")
                .AddOption("我需要考虑", "consider", DialogueOptionType.Exit,
                    tooltip: "中立路线，暂不选择");

            b.StartNode("follow_heaven",
                "天庭使者微微点头：\"明智的选择。" +
                "天庭从不亏待忠心之人。\"\n\n" +
                "\"四柱浮岛已经出现，你需要逐一攻破。" +
                "每座浮岛都有守护者，击败它们，" +
                "才能解除封印。\"\n\n" +
                "\"去吧，天庭会注视着你。\"")
                .EndsDialogue();

            b.StartNode("oppose_heaven",
                "天庭使者眼中闪过一丝寒光：" +
                "\"不知天高地厚。\"\n\n" +
                "他挥手，一道金色光芒将你击退：" +
                "\"你以为你能对抗宿命？" +
                "宿命是不可逆转的。\"\n\n" +
                "\"但……如果你执意如此，" +
                "那就用你的实力来证明吧。\"")
                .EndsDialogue();

            b.StartNode("consider",
                "天庭使者面无表情：\"犹豫不决……" +
                "这是弱者的表现。\"\n\n" +
                "\"但天庭给你这个机会。" +
                "四柱浮岛出现后，" +
                "你自然会做出选择。\"\n\n" +
                "他转身飞向天空：\"记住——" +
                "时间不等人。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：春秋蝉回溯（Stage6关键转折）
    /// </summary>
    public class Stage6ChunQiuRebirth : StoryDialogueProvider
    {
        protected override string TreeID => "Story_ChunQiuRebirth";
        protected override string DisplayName => "春秋蝉";
        protected override string GreetingText =>
            "时间仿佛凝固了。你感到胸口传来一阵温热——春秋蝉在共鸣！";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "龙公的最后一击即将落下，" +
                "你感到意识在消散……\n\n" +
                "但就在这一刻，春秋蝉发出了耀眼的光芒！" +
                "时间的洪流在你眼前倒转——" +
                "你看到了自己走过的每一步路，" +
                "每一个选择，每一次战斗……\n\n" +
                "然后，一切回到了战斗开始之前。")
                .AddOption("我回来了……", "reborn", DialogueOptionType.Quest);

            b.StartNode("reborn",
                "你睁开眼睛，发现自己回到了龙公战开始前的位置。" +
                "龙公依然站在那里，似乎什么都没发生。\n\n" +
                "但你知道——你已经经历过一次战斗。" +
                "龙公的攻击模式、弱点、节奏……" +
                "你全都记住了。\n\n" +
                "春秋蝉赋予了你\"宿命回溯\"的力量。" +
                "这一次，你不会再输。")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：宿命碎裂（Stage6高潮）
    /// </summary>
    public class Stage6DestinyShattered : StoryDialogueProvider
    {
        protected override string TreeID => "Story_DestinyShattered";
        protected override string DisplayName => "宿命蛊";
        protected override string GreetingText =>
            "龙公倒下了。天空出现了一道巨大的裂缝——那是宿命的裂痕。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "龙公的身体缓缓倒下，" +
                "他难以置信地看着你：\n\n" +
                "\"你……竟然逆转了宿命……" +
                "这不可能……\"\n\n" +
                "他的声音越来越弱：\"宿命……碎了？\"" +
                "随即化为光点消散。\n\n" +
                "天空中的裂缝越来越大，" +
                "金色的丝线从裂缝中断裂，" +
                "如同命运的丝线被一一剪断。")
                .AddOption("这就是……宿命碎裂？", "observe", DialogueOptionType.Informative)
                .AddOption("终于结束了", "relief", DialogueOptionType.Exit);

            b.StartNode("observe",
                "你仰望天空，看着宿命的碎片如流星般坠落。" +
                "每一片碎片都代表着一条被改写的命运。\n\n" +
                "世界似乎在颤抖——" +
                "没有了宿命的约束，" +
                "一切都将变得不可预测。\n\n" +
                "但也许……这才是真正的自由。")
                .AddOption("我准备好了", "ready", DialogueOptionType.Quest);

            b.StartNode("ready",
                "宿命碎裂带来的能量波动席卷大地。" +
                "天劫的威力增强了三成——" +
                "天意不再约束，反而变得更加狂暴。\n\n" +
                "但你知道，这是升仙的必经之路。" +
                "只有通过天劫，才能真正踏入蛊仙之境。")
                .EndsDialogue();

            b.StartNode("relief",
                "你长出一口气。龙公之战终于结束了。" +
                "但你知道，这只是开始。\n\n" +
                "宿命碎裂意味着新的时代来临——" +
                "一个没有宿命约束的时代。" +
                "而你，将成为这个时代的先驱。")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：升仙（Stage6结尾/Stage7开头）
    /// </summary>
    public class Stage6Ascension : StoryDialogueProvider
    {
        protected override string TreeID => "Story_Ascension";
        protected override string DisplayName => "天劫";
        protected override string GreetingText =>
            "天劫降临！六道天雷劈下，这是升仙的最终考验！";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "天空乌云密布，雷电交加。" +
                "六道天劫接踵而至——\n\n" +
                "第一劫：雷劫。天雷轰顶，考验肉身。\n" +
                "第二劫：火劫。天火焚身，考验意志。\n" +
                "第三劫：风劫。罡风刮骨，考验根基。\n" +
                "第四劫：心劫。心魔入侵，考验道心。\n" +
                "第五劫：血劫。血海翻涌，考验战力。\n" +
                "第六劫：道劫。天道质问，考验悟性。\n\n" +
                "六劫过后，便是升仙！")
                .AddOption("我准备好了！", "ready_ascend", DialogueOptionType.Quest)
                .AddOption("让我再准备一下", "not_ready", DialogueOptionType.Exit);

            b.StartNode("ready_ascend",
                "你挺身面对天劫，真元全力运转。" +
                "每一道天劫都是生死考验，" +
                "但你一一渡过。\n\n" +
                "当最后一道天劫消散时，" +
                "你的身体开始发光——" +
                "真元转化为仙元，" +
                "空窍扩展为仙窍。\n\n" +
                "天地共鸣，万物朝贺。" +
                "你——已升仙！")
                .EndsDialogue();

            b.StartNode("not_ready",
                "天劫的乌云暂时散去，但不会消失。" +
                "当你准备好的时候，" +
                "天劫会再次降临。\n\n" +
                "升仙不可逆，请务必做好万全准备。")
                .EndsDialogue();

            return b.Build();
        }
    }
}
