using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：山寨来客
    ///
    /// 外乡人来到古月山寨门前，与守门蛊师对话。
    /// 纯对话树，无游戏系统交互 — 验证 Layer 0~1 的基础设施。
    ///
    /// 结构：greeting → 3 分支（友善/直接/挑衅）→ 各分支结束
    /// </summary>
    public class GuYueVillageEntry : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueVillageEntry";
        protected override string DisplayName => "古月守门蛊师";
        protected override string GreetingText =>
            "古月守门蛊师打量着你，手按在腰间的蛊虫袋上：" +
            "\"站住。你是何人？来我古月山寨有何贵干？\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "{npcName}目光锐利地扫过你全身：" +
                "\"外乡人，报上名来。古月山寨不欢迎不明来历之人。\"")
                .AddOption("友善地问候", "friendly_greet", DialogueOptionType.Social,
                    tooltip: "以礼相待，表明自己没有恶意")
                .AddOption("直接说明来意", "direct_ask", DialogueOptionType.Informative,
                    tooltip: "不卑不亢，说明自己是路过的旅人")
                .AddOption("挑衅对方", "threat_response", DialogueOptionType.Risky,
                    tooltip: "\"区区一个守门的，也敢拦我？\"");

            b.StartNode("friendly_greet",
                "{npcName}神色稍缓：\"倒是个懂礼数的。我叫古月青。" +
                "山寨里有学堂、药堂、演武场，你若有事可以去看看。\"")
                .AddOption("多谢指点", "friendly_end", DialogueOptionType.Exit);

            b.StartNode("friendly_end",
                "{npcName}点了点头：\"去吧。记住，在山寨里不要惹事。\"")
                .EndsDialogue();

            b.StartNode("direct_ask",
                "{npcName}眯起眼睛：\"路过的旅人？最近山寨周围不太平。" +
                "你一个人能走到这里，看来也不是泛泛之辈。\"\n\n" +
                "他侧身让开：\"进去吧。药堂和学堂在东边，演武场在西边。\"")
                .AddOption("道谢后进入山寨", "direct_end", DialogueOptionType.Exit);

            b.StartNode("direct_end",
                "{npcName}在你身后喊道：\"天黑之前最好找个地方落脚。\"")
                .EndsDialogue();

            b.StartNode("threat_response",
                "{npcName}脸色一沉，手重新按上蛊虫袋：" +
                "\"好大的口气！我倒要看看你有什么本事！\"\n\n" +
                "他摆出了战斗姿态，周围的几个蛊师也注意到了这边的动静。")
                .AddOption("退让一步", "threat_backdown", DialogueOptionType.Exit,
                    tooltip: "\"开个玩笑而已，何必当真？\"")
                .AddOption("继续挑衅", "threat_fight", DialogueOptionType.Combat,
                    tooltip: "与守门蛊师战斗");

            b.StartNode("threat_backdown",
                "{npcName}冷哼一声：\"算你识相。进去吧，别让我再看到你惹事。\"")
                .EndsDialogue();

            b.StartNode("threat_fight",
                "{npcName}大喝一声：\"找死！\"")
                .TriggersCombat();

            return b.Build();
        }
    }
}