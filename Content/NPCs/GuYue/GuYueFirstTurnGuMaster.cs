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
    public class GuYueFirstTurnGuMaster : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.FirstTurnGuMaster;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.5f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 20, 0);
        }

        protected override string GetRoleDialoguePrefix() => "一转蛊师";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "一转蛊师热情地说：\"你也刚入门吗？一起加油吧！\"",
                "一转蛊师挠了挠头：\"修炼真难啊……空窍开辟了好久才有一点进展。\"",
                "一转蛊师兴奋道：\"我刚刚炼化了一只蛊虫！虽然只是最低级的，但很有成就感！\"",
                "一转蛊师小声说：\"学堂家老讲课太枯燥了，但我又不敢打瞌睡……\"",
                "一转蛊师憧憬道：\"等我升到二转，就能参加巡逻队了！\"",
                "一转蛊师叹气：\"真元总是不够用，修炼好慢啊……\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "一转蛊师惊恐地后退：\"你、你想干什么！来人啊！\"",
                GuAttitude.Wary => "一转蛊师紧张地看着你：\"你……你不会是来捣乱的吧？\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "一转蛊师崇拜地看着你：\"前辈好厉害！\"",
                GuAttitude.Contemptuous => "一转蛊师委屈地说：\"我、我会变强的……\"",
                GuAttitude.Fearful => "一转蛊师瑟瑟发抖：\"别、别伤害我……\"",
                _ => "一转蛊师正在练习基础功法。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_FirstTurnGuMaster", "greeting");

            b.StartNode("greeting",
                "一转蛊师看到你，露出友好的微笑。")
                .AddOption("聊聊修炼", "ask_training", DialogueOptionType.Informative)
                .AddOption("关于蛊虫", "ask_gu", DialogueOptionType.Informative)
                .AddOption("关于山寨生活", "ask_life", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_training",
                "一转蛊师：\"修炼真的好难啊。每天都要打坐运气，真元增长慢得像蜗牛。不过学堂家老说，根基最重要，不能急。\"")
                .AddOption("加油", "ask_encourage")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_encourage",
                "一转蛊师感激地说：\"谢谢鼓励！我一定会坚持的！\"")
                .AddOption("嗯", "greeting");

            b.StartNode("ask_gu",
                "一转蛊师：\"我目前只炼化了一只蛊虫，还是最普通的。不过学堂家老说，一转蛊师有一只就不错了。\"")
                .AddOption("你的蛊虫是什么？", "ask_my_gu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_my_gu",
                "一转蛊师有些不好意思：\"是一只……酒虫。虽然没什么战斗力，但能帮我辨别酒的好坏。嘿嘿。\"")
                .AddOption("也挺好的", "greeting");

            b.StartNode("ask_life",
                "一转蛊师：\"山寨里的生活还算安稳。每天修炼、上课、做杂务……虽然枯燥，但至少安全。不像山外，听说到处都是危险。\"")
                .AddOption("山外有什么危险？", "ask_danger")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_danger",
                "一转蛊师压低声音：\"听说山外有散修劫掠，还有野兽袭击。御堂家老说，没有二转以上修为最好不要单独外出。\"")
                .AddOption("我会小心的", "greeting");

            b.StartNode("trade",
                "一转蛊师：\"我有些多余的低级材料，你需要吗？\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "一转蛊师挥手：\"下次再聊！\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
