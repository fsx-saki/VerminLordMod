using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    public class GuYueFinale : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFinale";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月博站在议事厅前，望着远处的山峦，神情复杂：" +
            "\"危机已解，但古月的未来……还需要你来守护。\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "古月博转过身来，眼中满是欣慰：\"你做得很好。若非你挺身而出，" +
                "古月山寨恐怕已经沦陷。从今以后，你便是古月的座上宾。\"")
                .AddOption("这是我应该做的", "duty_response", DialogueOptionType.Social,
                    tooltip: "谦逊地回应")
                .AddOption("那些散修是什么来头？", "enemy_origin", DialogueOptionType.Informative,
                    tooltip: "询问敌人的背景")
                .AddOption("古月的未来会怎样？", "future", DialogueOptionType.Informative,
                    tooltip: "询问家族的前景");

            b.StartNode("duty_response",
                "古月博微微一笑：\"不骄不躁，这才是蛊师应有的品质。\"他沉吟片刻，" +
                "\"古月山寨虽然暂时安全，但南疆的局势越来越复杂。" +
                "你需要继续修炼，将来才能在这乱世中立足。\"")
                .AddOption("我会继续努力的", "finale_end", DialogueOptionType.Exit);

            b.StartNode("enemy_origin",
                "古月博面色凝重：\"那些散修……背后有人指使。我怀疑是南疆的某个大势力" +
                "在暗中觊觎古月的传承蛊方。这次只是试探，更大的风暴还在后面。\"")
                .AddOption("我该如何准备？", "future")
                .AddOption("我会保护古月的", "finale_end", DialogueOptionType.Exit);

            b.StartNode("future",
                "古月博望向远方：\"古月的未来，取决于每一个族人的选择。" +
                "赤脉主张主动出击，漠脉主张固守防御，药脉则只想救人。" +
                "而我……只希望家族能够延续下去。\"他转向你，" +
                "\"无论你做出什么选择，古月都会支持你。\"")
                .AddOption("多谢族长", "finale_end", DialogueOptionType.Exit);

            b.StartNode("finale_end",
                "夕阳西下，古月山寨恢复了往日的宁静。你站在山寨门口，" +
                "望着远方的山峦，心中既有释然，也有不安。" +
                "南疆的风云变幻，你的故事才刚刚开始……")
                .EndsDialogue();

            return b.Build();
        }
    }
}
