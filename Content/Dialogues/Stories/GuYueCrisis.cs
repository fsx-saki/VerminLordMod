using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    public class GuYueCrisis : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueCrisis";
        protected override string DisplayName => "御堂家老";
        protected override string GreetingText =>
            "御堂家老面色凝重：\"出大事了！山寨外围发现了大量散修蛊师的踪迹！\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "御堂家老急匆匆地走来：\"不好了！巡逻队发现山寨周围聚集了大量散修蛊师，" +
                "看样子来者不善！族长已经召集了所有家老商议对策。\"")
                .AddOption("有多少敌人？", "enemy_info", DialogueOptionType.Informative,
                    tooltip: "询问敌情详情")
                .AddOption("我能做什么？", "how_to_help", DialogueOptionType.Combat,
                    tooltip: "主动请缨参战")
                .AddOption("先看看情况再说", "wait_and_see", DialogueOptionType.Normal,
                    tooltip: "谨慎观望");

            b.StartNode("enemy_info",
                "御堂家老沉声道：\"至少有二十名散修蛊师，其中不乏二转高手。" +
                "他们似乎在寻找什么东西……有人说是古月家族的传承蛊方。\"")
                .AddOption("我去迎敌！", "join_battle", DialogueOptionType.Combat)
                .AddOption("需要加强防御", "defense_plan", DialogueOptionType.Informative);

            b.StartNode("how_to_help",
                "御堂家老审视着你：\"你愿意为家族而战？好！\"他递给你一枚令牌，" +
                "\"持此令牌，你可以调动山寨外围的防御阵法。记住，保护族人和蛊方是最优先的。\"")
                .AddOption("明白！", "join_battle", DialogueOptionType.Combat);

            b.StartNode("wait_and_see",
                "御堂家老皱眉：\"现在不是犹豫的时候！不过……也好，先观察敌人的动向再行动，" +
                "也不失为一种策略。但若山寨有难，你必须站出来。\"")
                .AddOption("我会的", "join_battle", DialogueOptionType.Combat);

            b.StartNode("defense_plan",
                "御堂家老点头：\"御堂已经在各要道布置了陷阱和蛊阵。" +
                "赤脉的战士负责正面迎敌，漠脉的防御蛊师守护内围。" +
                "药脉则在后方准备救治。你……就去支援赤脉吧。\"")
                .AddOption("好，我去支援赤脉", "join_battle", DialogueOptionType.Combat);

            b.StartNode("join_battle",
                "你握紧了手中的武器，朝山寨外围走去。远处，散修蛊师的喊杀声已经隐约可闻。" +
                "古月山寨的危机，正式开始了……")
                .EndsDialogue();

            return b.Build();
        }
    }
}
