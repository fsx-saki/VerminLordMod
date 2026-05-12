using Terraria.ID;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：学堂考验
    ///
    /// 玩家来到古月学堂，请求教头指导炼蛊。
    /// 教头要求玩家展示修为，根据修为高低给予不同回应。
    ///
    /// 使用的层级特性：
    /// - Layer 0: DialogueNode, DialogueOption, DialogueTree
    /// - Layer 1: StoryDialogueProvider (继承)
    /// - Layer 2: DialogueCondition (RealmCondition 修为门槛)
    /// - Layer 3: DialogueEffect (ModifyReputationEffect, GiveItemEffect)
    /// - Layer 4: IDialogueIntegration (通过 DialogueIntegration 访问游戏系统)
    ///
    /// 结构：
    ///   greeting → 请求指导
    ///     ├─ 修为达标 → training → 获得声望+物品 → 结束
    ///     └─ 修为不足 → too_weak → 被拒绝 → 结束
    /// </summary>
    public class GuYueSchoolTrial : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueSchoolTrial";
        protected override string DisplayName => "古月学堂教头";
        protected override string GreetingText =>
            "古月学堂教头正在指导几个年轻蛊师练习控蛊术。" +
            "看到你走近，他示意其他人继续练习，转身面向你。";

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            // ===== 根节点 =====
            b.StartNode("greeting",
                "{npcName}打量了你一番：\"外乡人也对炼蛊感兴趣？" +
                "我是古月学堂的教头，古月铁。想学东西，先让我看看你的本事。\"")
                .AddOption("请求指导", "request_training", DialogueOptionType.Teach,
                    tooltip: "\"请前辈指点一二。\"")
                .AddOption("我只是看看", "just_looking", DialogueOptionType.Exit,
                    tooltip: "婉拒，转身离开");

            // ===== 修为达标分支 =====
            b.StartNode("request_training",
                "{npcName}点了点头：\"好，有上进心。让我看看你的修为如何。" +
                "释放你的蛊力，让我感受一下。\"")
                .AddOption("展示修为", "show_power", DialogueOptionType.Normal,
                    condition: new RealmCondition(3),
                    tooltip: "释放蛊力，展示自己三转以上的修为")
                .AddOption("（修为不足，默默退下）", "too_weak", DialogueOptionType.Exit,
                    condition: new RealmCondition(3) { Invert = true },
                    tooltip: "你的修为还不够，贸然展示只会丢脸");

            // ===== 修为达标 → 获得指导 =====
            b.StartNode("show_power",
                "{npcName}感受到你的蛊力，眼中闪过一丝赞许：" +
                "\"不错！三转以上的修为，在年轻一辈中算是佼佼者了。" +
                "既然你有这份实力，我可以教你一些进阶的控蛊技巧。\"\n\n" +
                "他取出一本手札递给你：\"这是我年轻时整理的炼蛊心得，你拿去看吧。" +
                "另外，既然你通过了考验，我会在族长面前为你美言几句。\"")
                .AddOptionWithEffects("多谢教头！", "training_end", DialogueOptionType.Exit,
                    null, null,
                    new ModifyReputationEffect(FactionID.GuYue, 30, "通过学堂考验"),
                    new GiveItemEffect(ItemID.FallenStar, 3),
                    new ShowMessageEffect("你在古月家族的声望提升了！", Microsoft.Xna.Framework.Color.Gold));

            b.StartNode("training_end",
                "{npcName}拍了拍你的肩膀：\"好好修炼，日后必成大器。" +
                "若遇到什么困难，可以随时来学堂找我。\"")
                .EndsDialogue();

            // ===== 修为不足 =====
            b.StartNode("too_weak",
                "{npcName}摇了摇头：\"你的修为还不够火候。" +
                "贸然学习高阶技巧，反而会伤及自身根基。\"\n\n" +
                "他语气稍缓：\"先回去好好打基础，等修为到了三转再来找我。" +
                "到时候，我亲自教你。\"")
                .AddOption("告辞离开", "weak_end", DialogueOptionType.Exit);

            b.StartNode("weak_end",
                "{npcName}转身继续指导其他学生，不再理会你。")
                .EndsDialogue();

            // ===== 只是看看 =====
            b.StartNode("just_looking",
                "{npcName}耸了耸肩：\"随你便。不过记住，古月山寨的规矩——" +
                "不惹事，不偷学。被我发现偷师，可别怪我不客气。\"")
                .EndsDialogue();

            return b.Build();
        }
    }
}