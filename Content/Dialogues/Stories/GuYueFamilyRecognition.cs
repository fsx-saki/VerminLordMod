using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    public class GuYueFamilyRecognition : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueFamilyRecognition";
        protected override string DisplayName => "古月族长";
        protected override string GreetingText =>
            "古月博端坐于主位，目光深邃地看着你：" +
            "\"你为山寨做了不少事，今日召你来，是有要事相商。\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "古月博缓缓开口：\"你在学堂和药堂的表现，我都看在眼里。\"")
                .AddOption("族长过奖了", "humble", DialogueOptionType.Social,
                    tooltip: "谦虚回应")
                .AddOption("这是我应该做的", "duty", DialogueOptionType.Informative,
                    tooltip: "表示为家族效力是分内之事")
                .AddOption("有什么赏赐吗？", "greedy", DialogueOptionType.Trade,
                    tooltip: "直白地询问奖励");

            b.StartNode("humble",
                "古月博微微点头：\"谦虚是好事，但过度的谦虚便是虚伪。\"他站起身来，" +
                "\"从今日起，你便是古月家族的正式成员。家族的资源和机密，你都有权知晓。\"")
                .AddOption("多谢族长！", "recognition_end", DialogueOptionType.Exit);

            b.StartNode("duty",
                "古月博露出一丝笑意：\"好！有这份心，便是我古月之人。\"他拍了拍你的肩膀，" +
                "\"从今日起，你便是古月家族的正式成员。望你继续为家族效力。\"")
                .AddOption("定不辱命！", "recognition_end", DialogueOptionType.Exit);

            b.StartNode("greedy",
                "古月博眉头微皱：\"年轻人，莫要被利益蒙蔽了双眼。\"他顿了顿，" +
                "\"不过，直率也算是一种品质。从今日起，你是古月家族的正式成员了。\"")
                .AddOption("多谢族长", "recognition_end", DialogueOptionType.Exit);

            b.StartNode("recognition_end",
                "古月博挥手示意你可以退下了。你感到一股暖流涌上心头——" +
                "从此，古月山寨的大门将永远为你敞开。")
                .EndsDialogue();

            return b.Build();
        }
    }
}
