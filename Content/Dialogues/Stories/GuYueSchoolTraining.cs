using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    // ============================================================
    // GuYueSchoolTraining — 学堂历练剧情（StoryPhase.SchoolTraining）
    //
    // 剧情：玩家进入山寨内部，学堂教头考验修为。
    // 需完成拳脚训练和蛊虫操控训练。
    //
    // TODO:
    //   - 填充完整的对话树节点
    //   - 添加剧情推进条件（完成拳脚训练 + 蛊虫训练）
    //   - 添加对话效果（获得学堂认可标记）
    // ============================================================

    public class GuYueSchoolTraining : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueSchoolTrial";
        protected override string DisplayName => "拳脚教头";
        protected override string GreetingText =>
            "拳脚教头叉着腰看着你：\"新来的？学堂的规矩先搞清楚！\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "学堂的庭院中，一名身材魁梧的教头正在指导弟子们练习拳脚。" +
                "他看到你走来，停下动作，叉着腰审视着你。" +
                "\"又来一个新人？先别急着进学堂，让我看看你有没有基础。\"")
                .AddOption("我准备好了", "ready_for_training", DialogueOptionType.Combat,
                    tooltip: "接受训练考验")
                .AddOption("关于学堂规矩", "ask_rules", DialogueOptionType.Informative)
                .AddOption("我想先了解一下", "ask_info", DialogueOptionType.Informative)
                .AddOption("我还有事，改天再来", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_rules",
                "拳脚教头清了清嗓子：\"学堂规矩三条——" +
                "一、不得私斗；二、按时操练；三、尊师重道。" +
                "违反任何一条，轻则罚做杂役，重则逐出山寨。\"")
                .AddOption("明白了，我接受考验", "ready_for_training")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_info",
                "拳脚教头指着学堂方向：\"古月学堂分两个阶段——" +
                "先是拳脚功夫，这是蛊师的基本功。" +
                "然后是蛊虫操控，考验你对真元和蛊虫的协调能力。" +
                "通过了这两个考验，才算正式入门。\"")
                .AddOption("拳脚考验是什么？", "ask_martial")
                .AddOption("蛊虫考验是什么？", "ask_gu_control")
                .AddOption("我准备好了", "ready_for_training");

            b.StartNode("ask_martial",
                "拳脚教头拍了拍手掌：\"拳脚考验很简单——" +
                "在训练场上击败三个木桩傀儡。" +
                "傀儡虽然动作迟缓，但防御结实，你需要找到破防的节奏。" +
                "这是考验你的基本功和观察力。\"")
                .AddOption("蛊虫考验呢？", "ask_gu_control")
                .AddOption("我接受考验", "ready_for_training");

            b.StartNode("ask_gu_control",
                "拳脚教头表情严肃起来：\"蛊虫操控考验才是真正的难关。" +
                "你需要同时操控至少一只蛊虫完成指定攻击序列。" +
                "考验的不是蛮力，而是你对蛊虫和真元的协调能力。" +
                "很多新人在这一关上栽了跟头。\"")
                .AddOption("拳脚考验呢？", "ask_martial")
                .AddOption("我接受考验", "ready_for_training");

            b.StartNode("ready_for_training",
                "拳脚教头点头：\"好！先去训练场完成拳脚考验。" +
                "击败三个木桩傀儡后回来找我，我再安排蛊虫考验。\"")
                .EndsDialogue();

            // TODO: 添加拳脚考验完成后的对话节点
            // TODO: 添加蛊虫考验对话节点
            // TODO: 添加考验通过后的推进效果

            b.StartNode("bye",
                "拳脚教头挥手：\"去吧去吧，想好了再来。\"")
                .EndsDialogue();

            return b.Build();
        }
    }
}