using Microsoft.Xna.Framework;
using Terraria.ID;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.Dialogues.Stories
{
    /// <summary>
    /// 剧情：药堂秘方
    ///
    /// 古月药堂的家老正在研制一种新的疗伤蛊药，需要一味稀有药材。
    /// 玩家可以选择帮助寻找，或用已有药材换取报酬。
    ///
    /// 使用的层级特性：
    /// - Layer 0: DialogueNode, DialogueOption, DialogueTree
    /// - Layer 1: StoryDialogueProvider (继承)
    /// - Layer 2: DialogueCondition (HasItemCondition, RealmCondition)
    /// - Layer 3: DialogueEffect (ModifyReputationEffect, GiveItemEffect, RemoveItemEffect, ShowMessageEffect)
    /// - Layer 4: IDialogueIntegration (通过 DialogueIntegration 访问游戏系统)
    ///
    /// 结构：
    ///   greeting → 药老求助
    ///     ├─ 有药材 → trade → 交易成功 → 结束
    ///     ├─ 愿意帮忙 → accept_quest → 指引 → 结束
    ///     └─ 拒绝 → refuse → 结束
    /// </summary>
    public class GuYueMedicineRequest : StoryDialogueProvider
    {
        protected override string TreeID => "Story_GuYueMedicineRequest";
        protected override string DisplayName => "古月药堂家老";
        protected override string GreetingText =>
            "古月药堂家老正在捣药，看到你进来，放下手中的药杵。";

        // 稀有药材的物品种类（月光草）
        private const int RareHerbType = ItemID.Moonglow;

        protected override DialogueTree BuildTree()
        {
            var b = NewBuilder("greeting");

            b.StartNode("greeting",
                "{npcName}叹了口气：\"年轻人，你来得正好。" +
                "我正在炼制一味疗伤蛊药，缺了一味主药——月光草。" +
                "没有它，这炉药就废了。\"\n\n" +
                "他期待地看着你：\"你身上可有月光草？或者愿意帮我去采一些来？\"")
                .AddOption("我有月光草", "has_herb", DialogueOptionType.Trade,
                    condition: new HasItemCondition(RareHerbType, 3),
                    tooltip: "交出 3 株月光草")
                .AddOption("我愿意帮忙去找", "accept_quest", DialogueOptionType.Quest,
                    tooltip: "接受任务，去采集月光草")
                .AddOption("抱歉，我帮不上忙", "refuse", DialogueOptionType.Exit,
                    tooltip: "婉拒并离开");

            b.StartNode("has_herb",
                "{npcName}眼睛一亮：\"太好了！快给我看看！\"\n\n" +
                "他接过月光草，仔细端详后连连点头：" +
                "\"品质上乘，年份也够。有了这味药，这炉疗伤蛊药就成了！\"\n\n" +
                "他转身从架子上取下一个布袋：\"我不能白拿你的东西。" +
                "这些元石你拿着，算是我的一点心意。" +
                "另外，我会在族长面前替你美言几句。\"")
                .AddOptionWithEffects("成交", "trade_end", DialogueOptionType.Trade,
                    null, null,
                    new RemoveItemEffect(RareHerbType, 3),
                    new ModifyReputationEffect(FactionID.GuYue, 50, "帮助药堂炼制蛊药"),
                    new ShowMessageEffect("你获得了 50 点古月家族声望！", Color.Gold));

            b.StartNode("trade_end",
                "{npcName}满意地收起月光草：\"以后若采到好药材，尽管拿来药堂。" +
                "我古月药堂从不亏待帮忙的人。\"")
                .EndsDialogue();

            b.StartNode("accept_quest",
                "{npcName}露出欣慰的笑容：\"好！月光草生长在寨子东边的密林深处，" +
                "靠近溪水的地方最容易找到。不过那里常有野狼出没，你要小心。\"\n\n" +
                "他补充道：\"采到三株以上就够用了。回来找我，我必有重谢。\"")
                .AddOption("我这就去", "quest_accept_end", DialogueOptionType.Exit,
                    tooltip: "出发前往密林");

            b.StartNode("quest_accept_end",
                "{npcName}在你身后喊道：\"记住，选月光充足的地方找！" +
                "月光草顾名思义，只在月光照耀下生长！\"")
                .EndsDialogue();

            b.StartNode("refuse",
                "{npcName}摆了摆手：\"罢了罢了，各人有各人的事。" +
                "我再想想别的办法。\"\n\n" +
                "他转身继续捣药，不再理会你。")
                .EndsDialogue();

            return b.Build();
        }
    }
}