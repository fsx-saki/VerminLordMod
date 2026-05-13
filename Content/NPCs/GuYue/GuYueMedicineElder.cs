using VerminLordMod.Common.DialogueTree;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.GuYue
{
    [AutoloadHead]
    public class GuYueMedicineElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MedicineElder;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 75, 0);
        }

        protected override string GetRoleDialoguePrefix() => "药堂家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "药堂家老正在研磨药材：\"良药苦口，蛊师修炼更是如此。\"",
                "药堂家老端起一碗汤药：\"这副药能温养经脉，对开辟空窍有辅助之效。\"",
                "药堂家老叹道：\"蛊虫虽强，但若不注重调养，身体迟早会垮。\"",
                "药堂家老认真地叮嘱：\"修炼之余别忘了休息。过犹不及，此乃医道至理。\"",
                "药堂家老微笑道：\"药堂的丹药供应充足，有需要尽管来。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "药堂家老厉声道：\"药堂重地，岂容你放肆！\"",
                GuAttitude.Wary => "药堂家老警惕地看着你：\"你若敢在药堂动手脚，后果自负。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "药堂家老和蔼地说：\"你的身体调养得不错，继续保持。\"",
                GuAttitude.Contemptuous => "药堂家老摇头：\"不懂得珍惜身体的人，走不远。\"",
                GuAttitude.Fearful => "药堂家老护住药柜：\"你……你想干什么？这些药材不能乱动！\"",
                _ => "药堂家老正在忙碌地配药，没有注意到你。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_MedicineElder", "greeting");

            b.StartNode("greeting",
                "药堂家老放下手中的药杵，擦了擦手。")
                .AddOption("求医问药", "ask_medicine", DialogueOptionType.Informative)
                .AddOption("关于蛊虫调养", "ask_gu_care", DialogueOptionType.Informative)
                .AddOption("关于丹药炼制", "ask_alchemy", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_medicine",
                "药堂家老：\"蛊师修炼伤身，这是不争的事实。真元消耗过度、蛊虫反噬、突破失败……都需要药石调理。\"")
                .AddOption("有什么推荐的药物？", "ask_remedies")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_remedies",
                "药堂家老：\"日常修炼可用温养丹调理经脉，战斗后需用回元散恢复真元。至于突破失败……那需要天元续命丹，珍贵得很。\"")
                .AddOption("我记下了", "greeting");

            b.StartNode("ask_gu_care",
                "药堂家老：\"蛊虫虽是武器，但也是活物。定期用灵液喂养，保持空窍通畅，蛊虫才能维持最佳状态。\"")
                .AddOption("灵液怎么获取？", "ask_spirit_liquid")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_spirit_liquid",
                "药堂家老：\"灵液可从元泉中提取，也可用特定蛊虫炼制。药堂有现成的灵液出售，不过价格不菲。\"")
                .AddOption("我看看", "trade")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_alchemy",
                "药堂家老眼中闪过一丝骄傲：\"炼丹之道，讲究火候与配伍。一味之差，效果天壤之别。药堂的丹药，品质有保证。\"")
                .AddOption("能教我炼丹吗？", "ask_learn_alchemy")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_learn_alchemy",
                "药堂家老犹豫了一下：\"炼丹非一朝一夕之功。你若真有心，先从辨识药材开始吧。药堂后院有些常见药材，你可以去认认。\"")
                .AddOption("好的", "greeting");

            b.StartNode("trade",
                "药堂家老：\"药堂的丹药和药材，应有尽有。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "药堂家老点点头：\"保重身体，莫要逞强。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
