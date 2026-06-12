using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.DialogueTree.Actions;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：血祭之夜
    /// 方源在青茅山发动血祭，玩家面临关键选择
    /// </summary>
    public class Stage2BloodSacrifice : StoryDialogueProvider
    {
        protected override string TreeID => "Story_BloodSacrifice";
        protected override string DisplayName => "方源";
        protected override string GreetingText =>
            "方源站在血色的月光下，周围是倒下的族人。他的眼神冰冷而深邃：" +
            "\"你来了。我一直在等你。\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "方源转过身来，嘴角微微上扬：\"你来得正好。" +
                "血祭已经完成，青茅山的气运已被我抽取。\"\n\n" +
                "他伸出手：\"现在，你有两个选择。加入我，或者……成为我前进路上的垫脚石。\"")
                .AddOption("加入方源", "join_fangyuan", DialogueOptionType.Quest,
                    tooltip: "加入方源，走上魔道")
                .AddOption("对抗方源", "oppose_fangyuan", DialogueOptionType.Combat,
                    tooltip: "与方源战斗，保卫青茅山")
                .AddOption("逃离青茅山", "flee_qingmao", DialogueOptionType.Exit,
                    tooltip: "保命要紧，离开这个是非之地");

            b.StartNode("join_fangyuan",
                "方源微微点头：\"明智的选择。在这个弱肉强食的世界里，" +
                "只有力量才是永恒的真理。\"\n\n" +
                "他递给你一枚暗红色的蛊虫：\"这是血道蛊虫，它能增强你的力量。" +
                "但记住——选择了这条路，就没有回头。\"")
                .AddOption("接过蛊虫", "join_confirm", DialogueOptionType.Quest);

            b.StartNode("join_confirm",
                "你接过了血道蛊虫，感受到一股强大的力量涌入体内。" +
                "方源转身离去：\"地脉守护者还在沉睡。去击败它，" +
                "打破青茅山的封印，然后……离开这里。\"")
                .EndsDialogue();

            b.StartNode("oppose_fangyuan",
                "方源冷笑：\"勇气可嘉，但愚蠢至极。\"\n\n" +
                "他挥手，一道血光向你袭来——")
                .TriggersCombat();

            b.StartNode("flee_qingmao",
                "你转身向山寨外跑去。身后传来方源的声音：" +
                "\"逃跑也是一种选择。但记住——你逃不掉命运的。\"\n\n" +
                "青茅山在你身后渐渐远去，你知道，这里的一切都变了。")
                .EndsDialogue();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：白凝冰的抉择
    /// 白凝冰空窍无法支撑，面临自爆
    /// </summary>
    public class Stage2BaiNingBing : StoryDialogueProvider
    {
        protected override string TreeID => "Story_BaiNingBingIceSeal";
        protected override string DisplayName => "白凝冰";
        protected override string GreetingText =>
            "白凝冰浑身散发着刺骨的寒气，她的空窍正在崩溃。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "白凝冰咬着嘴唇，冰霜从她的指尖蔓延开来：" +
                "\"我的空窍……撑不住了。冰道反噬越来越严重。\"\n\n" +
                "她抬起头，眼中闪过一丝决绝：\"我有一个办法——" +
                "冰封自己。但需要外力帮助。你愿意帮我吗？\"")
                .AddOption("帮助白凝冰冰封", "help_bainingbing", DialogueOptionType.Quest,
                    tooltip: "帮助白凝冰冰封自身，她将存活但沉睡")
                .AddOption("旁观", "watch_bainingbing", DialogueOptionType.Exit,
                    tooltip: "不介入，看白凝冰如何选择")
                .AddOption("敌对", "oppose_bainingbing", DialogueOptionType.Combat,
                    tooltip: "趁机攻击白凝冰");

            b.StartNode("help_bainingbing",
                "你将真元注入白凝冰的空窍，帮助她稳定冰道。" +
                "冰霜逐渐收敛，白凝冰的身体被一层薄冰包裹。\n\n" +
                "白凝冰在冰封前最后看了你一眼：\"……谢谢你。\"")
                .EndsDialogue();

            b.StartNode("watch_bainingbing",
                "白凝冰深吸一口气：\"既然没人帮我……" +
                "那就用我自己的方式！\"\n\n" +
                "她猛然爆发，冰道之力席卷四周——" +
                "她选择了自爆冰封，将自己和周围的一切冻结。")
                .EndsDialogue();

            b.StartNode("oppose_bainingbing",
                "白凝冰眼中闪过一丝寒光：\"你……！\"\n\n" +
                "冰霜在她周围爆发——")
                .TriggersCombat();

            return b.Build();
        }
    }

    /// <summary>
    /// 剧情：天鹤来袭
    /// 天鹤上人袭击青茅山
    /// </summary>
    public class Stage2TianHeAttack : StoryDialogueProvider
    {
        protected override string TreeID => "Story_TianHeAttack";
        protected override string DisplayName => "古月御堂家老";
        protected override string GreetingText =>
            "御堂家老神色凝重：\"天鹤上人来了！所有人准备战斗！\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "天空中传来鹤鸣声，一只巨大的白鹤掠过山寨上空。" +
                "御堂家老大喊：\"是天鹤上人！他在攻击山寨的护山大阵！\"\n\n" +
                "\"所有蛊师，跟我上寨墙！\"")
                .AddOption("随御堂家老上寨墙", "defend_wall", DialogueOptionType.Combat,
                    tooltip: "参与山寨防御战")
                .AddOption("去通知族长", "notify_chief", DialogueOptionType.Quest,
                    tooltip: "向古月博报告敌情")
                .AddOption("保护平民撤离", "protect_civilians", DialogueOptionType.Social,
                    tooltip: "帮助寨民躲进地窖");

            b.StartNode("defend_wall",
                "你冲上寨墙，看到天鹤上人驾驭白鹤在空中盘旋。" +
                "他挥手间，风刃如刀，寨墙上的蛊师纷纷受伤。\n\n" +
                "御堂家老：\"集中攻击他的鹤！没有坐骑他就无法飞行！\"")
                .TriggersCombat();

            b.StartNode("notify_chief",
                "你冲进议事厅，古月博正在调兵遣将。" +
                "他看到你来：\"好，你来得正好。" +
                "我需要你带一队人去东寨门支援，那里防守最薄弱。\"")
                .EndsDialogue();

            b.StartNode("protect_civilians",
                "你帮助老人和孩子躲进地窖。一个小孩拉着你的衣角：" +
                "\"大哥哥，外面会发生什么？\"\n\n" +
                "你安慰了他，然后转身走向寨墙。你知道，" +
                "只有击退天鹤上人，山寨才能安全。")
                .EndsDialogue();

            return b.Build();
        }
    }
}
