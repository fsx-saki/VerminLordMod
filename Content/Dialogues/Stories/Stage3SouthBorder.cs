using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.DialogueTree.Actions;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：南疆初到
    /// 玩家到达南疆散修营地
    /// </summary>
    public class Stage3SouthBorderArrival : StoryDialogueProvider
    {
        protected override string TreeID => "Story_SouthBorderArrival";
        protected override string DisplayName => "太白云生";
        protected override string GreetingText =>
            "一位白发苍苍的老者坐在篝火旁，看到你走来，露出了和蔼的笑容。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "老者站起身来，向你拱手：\"老夫太白云生，散修一枚。" +
                "看小友风尘仆仆，是从北边来的吧？\"\n\n" +
                "他指了指篝火旁的空位：\"坐下歇歇吧。" +
                "南疆不比青茅山，这里鱼龙混杂，但机会也多。\"")
                .AddOption("太白前辈，南疆有什么势力？", "ask_factions", DialogueOptionType.Informative,
                    tooltip: "了解南疆局势")
                .AddOption("我在寻找机缘", "ask_opportunities", DialogueOptionType.Quest,
                    tooltip: "询问修炼机缘")
                .AddOption("多谢前辈", "thank_elder", DialogueOptionType.Exit,
                    tooltip: "简单道谢");

            b.StartNode("ask_factions",
                "太白云生捋了捋胡须：\"南疆最大的势力有三——" +
                "散修联盟、南疆三王遗迹、以及暗中的影宗分部。\"\n\n" +
                "\"散修联盟由各路散修组成，没有固定组织，" +
                "但消息灵通。三王遗迹是远古三王的传承之地，" +
                "里面有机缘也有凶险。影宗……呵，" +
                "那是个连名字都不能随便提的存在。\"")
                .AddOption("三王遗迹在哪里？", "ask_relics", DialogueOptionType.Quest)
                .AddOption("影宗是什么？", "ask_shadow", DialogueOptionType.Risky);

            b.StartNode("ask_relics",
                "太白云生压低声音：\"三王遗迹在南疆荒野深处。" +
                "入口被三道封印保护，只有达到三转修为才能进入。\"\n\n" +
                "\"里面的传承分为三种——铁骨王的炼体传承、" +
                "毒蛇王的毒道传承、幻影王的幻术传承。" +
                "每种传承都有守护者，不是好拿的。\"")
                .AddOption("我想要挑战三王传承", "accept_relics", DialogueOptionType.Quest)
                .AddOption("我再考虑考虑", "defer_relics", DialogueOptionType.Exit);

            b.StartNode("accept_relics",
                "太白云生眼中闪过一丝赞赏：\"好！有志气！" +
                "老夫给你画一张地图，标注了三王遗迹的入口位置。\"\n\n" +
                "他递给你一张泛黄的地图：\"小心行事。" +
                "传承之地凶险万分，但若能获得真传，" +
                "你的修为将突飞猛进。\"")
                .EndsDialogue();

            b.StartNode("defer_relics",
                "太白云生点了点头：\"也好，谋定而后动。" +
                "营地里有不少散修，你可以先和他们交流交流。\"")
                .EndsDialogue();

            b.StartNode("ask_shadow",
                "太白云生脸色一变，四下看了看：\"嘘！" +
                "影宗的事不要在这里说。\"\n\n" +
                "他凑近你耳边：\"影宗是南疆最神秘的组织。" +
                "据说他们的首领能操控影子，杀人于无形。" +
                "最近南疆发生的好几起失踪案，" +
                "都和影宗脱不了干系。\"\n\n" +
                "\"记住，在南疆，不要相信任何人。\"")
                .AddOption("我记住了", "shadow_ack", DialogueOptionType.Exit);

            b.StartNode("shadow_ack",
                "太白云生拍了拍你的肩膀：\"好孩子。" +
                "在这乱世中，能保持警觉是好事。\"")
                .EndsDialogue();

            b.StartNode("ask_opportunities",
                "太白云生笑了：\"机缘？南疆最不缺的就是机缘。" +
                "三王传承、散修集市、荒兽猎场……\"\n\n" +
                "\"但最大的机缘，莫过于春秋蝉。\"" +
                "他说到这里，突然停住了，" +
                "似乎意识到自己说多了。")
                .AddOption("春秋蝉是什么？", "ask_cicada", DialogueOptionType.Risky)
                .AddOption("散修集市在哪里？", "ask_market", DialogueOptionType.Informative);

            b.StartNode("ask_cicada",
                "太白云生摇了摇头：\"这个……老夫不能多说。" +
                "你只需要知道，春秋蝉是蛊世界最神秘的蛊虫之一，" +
                "据说能逆转时间。\"\n\n" +
                "\"如果你在南疆深处感受到时间的波动，" +
                "那可能就是春秋蝉的气息。\"")
                .AddOption("我会留意的", "cicada_ack", DialogueOptionType.Exit);

            b.StartNode("cicada_ack",
                "太白云生意味深长地看了你一眼：\"好。" +
                "也许……你和春秋蝉有缘。\"")
                .EndsDialogue();

            b.StartNode("ask_market",
                "太白云生指了指营地东边：\"散修集市就在那边。" +
                "黑市商人、药贩子、蛊虫贩子……什么都有。\"\n\n" +
                "\"不过要小心，南疆的商人可不都是善茬。\"")
                .EndsDialogue();

            b.StartNode("thank_elder",
                "太白云生微笑着点头：\"去吧，南疆虽乱，" +
                "但对有实力的人来说，正是大展拳脚的好地方。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：遇见商心慈
    /// 玩家在南疆荒野中遇到商心慈
    /// </summary>
    public class Stage3ShangXinCiMeet : StoryDialogueProvider
    {
        protected override string TreeID => "Story_ShangXinCiMeet";
        protected override string DisplayName => "商心慈";
        protected override string GreetingText =>
            "一个年轻女子独自坐在路边，衣衫上沾满了泥土和草叶。她看起来疲惫而警惕。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "女子抬头看到你，本能地后退了一步，" +
                "但随即注意到你没有敌意，稍微放松了一些。\n\n" +
                "\"你……你是散修营地的蛊师吗？我叫商心慈。" +
                "我在南疆迷路了，已经好几天没吃东西了……\"")
                .AddOption("你需要帮助吗？", "help_her", DialogueOptionType.Social,
                    tooltip: "帮助商心慈，好感度+15")
                .AddOption("你有什么值钱的东西？", "exploit_her", DialogueOptionType.Barter,
                    tooltip: "利用商心慈，获得50元石")
                .AddOption("……", "ignore_her", DialogueOptionType.Exit,
                    tooltip: "无视她，继续赶路");

            b.StartNode("help_her",
                "你从行囊中取出干粮和水递给商心慈。" +
                "她接过食物，眼眶微微泛红：\"谢谢你……" +
                "这年头，愿意帮助陌生人的人不多了。\"\n\n" +
                "她吃完后精神好了很多：\"我是商家的女儿，" +
                "因为家族变故流落到南疆。" +
                "如果你需要南疆的情报，我可以帮你。\"")
                .AddOption("告诉我南疆的情况", "info_south", DialogueOptionType.Informative)
                .AddOption("跟我回散修营地", "bring_camp", DialogueOptionType.Social);

            b.StartNode("info_south",
                "商心慈整理了一下思绪：\"南疆目前最值得关注的有三件事——" +
                "第一，三王传承即将开启；" +
                "第二，影宗在南疆活动频繁；" +
                "第三，有人在散修中散布春秋蝉的消息。\"\n\n" +
                "\"第三点最可疑。春秋蝉的消息不应该出现在散修中，" +
                "除非有人故意放出来的。\"")
                .AddOption("谁在散布消息？", "ask_spreader", DialogueOptionType.Risky)
                .AddOption("多谢你的情报", "thank_info", DialogueOptionType.Exit);

            b.StartNode("ask_spreader",
                "商心慈犹豫了一下：\"我只听说是一个叫'黑土'的人。" +
                "他在散修营地中出现了一段时间，" +
                "然后又消失了。\"\n\n" +
                "她压低声音：\"我觉得黑土不是他的真名。" +
                "这个人……太神秘了。\"")
                .EndsDialogue();

            b.StartNode("thank_info",
                "商心慈微笑：\"不客气。如果你需要更多情报，" +
                "可以来散修营地找我。\"")
                .EndsDialogue();

            b.StartNode("bring_camp",
                "商心慈感激地点头：\"太好了！有你的陪伴，" +
                "路上安全多了。\"\n\n" +
                "你们一起走回散修营地。路上，" +
                "商心慈向你讲述了她家族的遭遇，" +
                "以及她为什么独自来到南疆。")
                .EndsDialogue();

            b.StartNode("exploit_her",
                "商心慈的表情变得警惕：\"我……我身上没什么值钱的东西。" +
                "只有这个……\"她从怀中取出一块元石。\n\n" +
                "\"这是最后的了。你真的要拿走吗？\"")
                .AddOption("算了，留着吧", "reconsider", DialogueOptionType.Social)
                .AddOption("给我", "take_it", DialogueOptionType.Barter);

            b.StartNode("reconsider",
                "你叹了口气，把元石还给她。" +
                "商心慈松了口气：\"谢谢……你比我想象的要好。\"")
                .EndsDialogue();

            b.StartNode("take_it",
                "你拿走了元石。商心慈低着头，" +
                "不再说话。她转身离去，" +
                "背影显得格外孤独。")
                .EndsDialogue();

            b.StartNode("ignore_her",
                "你从她身边走过，没有停留。" +
                "身后传来一声轻叹，" +
                "但很快就被荒野的风声淹没了。")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：义天山异变
    /// Stage4开头，义天山出现
    /// </summary>
    public class Stage4YiTianShanAppears : StoryDialogueProvider
    {
        protected override string TreeID => "Story_YiTianShanAppears";
        protected override string DisplayName => "太白云生";
        protected override string GreetingText =>
            "太白云生急匆匆地跑来：\"出大事了！义天山异变！\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "太白云生气喘吁吁：\"义天山——南疆最大的灵山，" +
                "突然出现了异变！灵气暴涌，各方势力都在赶往那里！\"\n\n" +
                "\"据说义天山深处藏着远古蛊仙的传承，" +
                "这次异变可能就是传承现世的征兆！" +
                "我们必须尽快赶过去！\"")
                .AddOption("我跟你去！", "go_together", DialogueOptionType.Quest,
                    tooltip: "与太白云生一起前往义天山")
                .AddOption("让我先准备一下", "prepare_first", DialogueOptionType.Exit,
                    tooltip: "先整理装备再出发")
                .AddOption("义天山有什么危险？", "ask_danger", DialogueOptionType.Informative);

            b.StartNode("go_together",
                "太白云生点头：\"好！义天山令我已经拿到了，" +
                "这是进入义天山副本的钥匙。\"\n\n" +
                "他递给你一枚令牌：\"记住，义天山内部正道和魔道都在争夺传承。" +
                "我们必须小心行事，不要轻易暴露实力。\"")
                .EndsDialogue();

            b.StartNode("prepare_first",
                "太白云生：\"也好，磨刀不误砍柴工。" +
                "但不要耽搁太久，义天山的机缘稍纵即逝。\"")
                .EndsDialogue();

            b.StartNode("ask_danger",
                "太白云生面色凝重：\"义天山的危险主要有三个——" +
                "第一，正道和魔道蛊师会互相厮杀；" +
                "第二，义天山内部的守护阵法和机关；" +
                "第三……大同风。\"\n\n" +
                "\"大同风是义天山最可怕的灾害，" +
                "一旦刮起，连六转蛊师都难以幸存。\"")
                .AddOption("大同风是什么？", "ask_datongfeng", DialogueOptionType.Informative);

            b.StartNode("ask_datongfeng",
                "太白云生：\"大同风是天地间最狂暴的风。" +
                "它不分敌我，将一切吹散。" +
                "据说只有找到风眼，才能在大同风中存活。\"\n\n" +
                "\"如果义天山异变引发大同风……" +
                "那将是一场浩劫。\"")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：北原初到
    /// Stage5开头，到达北原
    /// </summary>
    public class Stage5NorthDesertArrival : StoryDialogueProvider
    {
        protected override string TreeID => "Story_NorthDesertArrival";
        protected override string DisplayName => "黑楼兰";
        protected override string GreetingText =>
            "一位威严的女子端坐在王座上，目光如鹰般锐利。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "黑楼兰打量着你：\"又一个从南疆来的蛊师。" +
                "最近南疆人来得不少，但能在北原活下来的不多。\"\n\n" +
                "她站起身来：\"我是黑楼兰，北原王庭之主。" +
                "你来到我的地盘，是想效忠王庭，还是另有所图？\"")
                .AddOption("效忠王庭", "swear_allegiance", DialogueOptionType.Quest,
                    tooltip: "效忠黑楼兰，获得王庭任务线")
                .AddOption("合作平等", "equal_cooperation", DialogueOptionType.Social,
                    tooltip: "以平等身份合作")
                .AddOption("我只是路过", "just_passing", DialogueOptionType.Exit,
                    tooltip: "拒绝加入王庭");

            b.StartNode("swear_allegiance",
                "黑楼兰露出满意的笑容：\"很好。" +
                "王庭不养闲人，但也不会亏待忠心之人。\"\n\n" +
                "她挥手，一名侍从递来一枚令牌：" +
                "\"这是王庭令，凭此可在王庭商店购买物资，" +
                "也可接取王庭任务。不要让我失望。\"")
                .EndsDialogue();

            b.StartNode("equal_cooperation",
                "黑楼兰眯起眼睛：\"平等合作？" +
                "在北原，实力就是话语权。\"\n\n" +
                "她审视了你片刻：\"也罢。你若能证明自己的实力，" +
                "我可以给你有限的合作机会。" +
                "先去冰塞川那里报到吧。\"")
                .EndsDialogue();

            b.StartNode("just_passing",
                "黑楼兰冷哼一声：\"路过？北原不是什么好地方。" +
                "暴风雪、冻土尸鬼、还有长生天的追杀……" +
                "你最好想清楚自己要做什么。\"\n\n" +
                "她转身离去：\"王庭的大门不会永远为你敞开。\"")
                .EndsDialogue();

            return b.Build();
        }
    }
}
