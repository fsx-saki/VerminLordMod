using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.Dialogues.Stories
{
    // ============================================================
    // GuYueArrival — 初入古月山寨剧情（StoryPhase.Arrival）
    //
    // 剧情：玩家首次来到古月山寨地界，遇到守门蛊师盘问。
    // 玩家需通过盘问才能进入山寨内部。
    //
    // TODO:
    //   - 填充完整的对话树节点
    //   - 添加剧情推进条件检查
    //   - 添加对话效果（声望变化、剧情标记）
    // ============================================================

    public class GuYueArrival : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueVillageEntry";
        protected override string DisplayName => "古月守门蛊师";
        protected override string GreetingText =>
            "守门蛊师拦住了你：\"站住！你是何人？为何来到古月山寨？\"";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "古月山寨的石门前，一名身穿灰袍的蛊师拦住了你的去路。" +
                "他目光如鹰，审视着你身上的气息。" +
                "\"来者何人？古月山寨不是什么人都能进的。\"")
                .AddOption("我是蛊师，慕名而来", "claim_gu_master", DialogueOptionType.Informative,
                    tooltip: "表明身份")
                .AddOption("我只是路过", "just_passing", DialogueOptionType.Normal,
                    tooltip: "含糊回应")
                .AddOption("有人介绍我来这里", "has_referral", DialogueOptionType.Informative,
                    tooltip: "提及引荐人")
                .AddOption("我听说这里能学习蛊术", "want_to_learn", DialogueOptionType.Informative,
                    tooltip: "表明学习意图");

            b.StartNode("claim_gu_master",
                "守门蛊师眉头微松：\"蛊师？让我看看你的空窍。\" " +
                "他走近你，凝神感知你体内的真元流转。" +
                "\"嗯……确实有真元波动，虽然微弱，但不是凡人伪装的。\"")
                .AddOption("我可以证明自己", "prove_self", DialogueOptionType.Combat,
                    tooltip: "接受考验")
                .AddOption("谢谢认可", "thank_recognition", DialogueOptionType.Normal);

            b.StartNode("prove_self",
                "守门蛊师点头：\"好，那就在门前的训练桩上展示一下你的实力。" +
                "击倒三个训练桩，我就放你进去。\"")
                .AddOption("我接受考验！", "accept_trial", DialogueOptionType.Combat)
                .AddOption("算了，我改天再来", "decline_trial", DialogueOptionType.Exit);

            b.StartNode("accept_trial",
                "你走向训练桩，准备展示自己的实力。" +
                "守门蛊师退开几步，冷眼旁观。")
                .EndsDialogue();

            b.StartNode("thank_recognition",
                "守门蛊师沉吟片刻：\"既然你确是蛊师，可以进入外围。" +
                "但内围需族长特许，你先去学堂家老那里报到吧。\"")
                .AddOption("多谢！", "enter_compound", DialogueOptionType.Normal);

            b.StartNode("enter_compound",
                "守门蛊师挥手放行：\"进去吧，别惹事。" +
                "古月山寨虽名义安全，但惹了家老们，你连出都出不了。\"")
                .EndsDialogue();

            b.StartNode("just_passing",
                "守门蛊师冷笑：\"路过的？这青茅山上没有'路过'的人。" +
                "要么你是来求学的蛊师，要么你是来偷蛊方的贼。\"")
                .AddOption("我不是贼！", "claim_gu_master")
                .AddOption("那我走", "leave", DialogueOptionType.Exit);

            b.StartNode("has_referral",
                "守门蛊师态度稍缓：\"有人引荐？谁？\" " +
                "他等着你说出引荐者的名字。" +
                "（TODO：根据玩家是否持有引荐信道具分支）")
                .AddOption("是贾家的商队管事", "jia_referral", DialogueOptionType.Informative)
                .AddOption("……我记不清了", "claim_gu_master");

            b.StartNode("jia_referral",
                "守门蛊师点头：\"贾家的人……他们和我们有些生意往来。" +
                "既然是商队管事介绍的，可以放你进去外围。\"")
                .AddOption("多谢放行", "enter_compound");

            b.StartNode("want_to_learn",
                "守门蛊师表情微微变化：\"想学蛊术？" +
                "古月学堂确实收外姓弟子，但需通过入门考验。" +
                "你有真元吗？\"")
                .AddOption("有，我已经开窍了", "claim_gu_master")
                .AddOption("还没有", "no_qi_yet", DialogueOptionType.Normal);

            b.StartNode("no_qi_yet",
                "守门蛊师摇头：\"连空窍都没开，谈什么学蛊术？" +
                "你先去山下的觉醒台试试吧，开了窍再来。\"")
                .AddOption("好，我去试试", "leave", DialogueOptionType.Exit);

            b.StartNode("leave",
                "你转身离开古月山寨的石门。" +
                "守门蛊师的声音从身后传来：\"下次再来，最好带着真元。\"")
                .EndsDialogue();

            b.StartNode("decline_trial",
                "守门蛊师无所谓地说：\"不愿考验？那就等你想好了再来。\"")
                .EndsDialogue();

            return b.Build();
        }
    }
}