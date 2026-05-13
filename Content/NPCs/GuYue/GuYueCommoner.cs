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
    public class GuYueCommoner : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Commoner;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.7f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 2, 0);
        }

        protected override string GetRoleDialoguePrefix() => "古月族人";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "古月族人微笑着说：\"今天天气不错，适合出门走走。\"",
                "古月族人低声道：\"最近寨子里气氛有些紧张……\"",
                "古月族人热情地说：\"你是新来的吧？有什么不懂的可以问我。\"",
                "古月族人叹道：\"日子虽然平淡，但平安就是福。\"",
                "古月族人望着远方：\"山外的世界……不知道是什么样的。\"",
                "古月族人认真地说：\"古月山寨虽小，但这里的人都很团结。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "古月族人惊恐地后退：\"你、你干什么！\"",
                GuAttitude.Wary => "古月族人警惕地看着你：\"你最好别惹事。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "古月族人点头：\"大人好。\"",
                GuAttitude.Contemptuous => "古月族人沉默不语。",
                GuAttitude.Fearful => "古月族人颤抖着后退：\"别、别过来……\"",
                _ => "古月族人正在做自己的事情。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_Commoner", "greeting");

            b.StartNode("greeting",
                "古月族人看到你，微微点头致意。")
                .AddOption("闲聊", "ask_chat", DialogueOptionType.Informative)
                .AddOption("关于古月山寨", "ask_compound", DialogueOptionType.Informative)
                .AddOption("关于日常生活", "ask_daily", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_chat",
                "古月族人：\"最近寨子里来了不少新人。族长似乎在筹备什么大事，但具体是什么，我们这些普通族人就不得而知了。\"")
                .AddOption("什么大事？", "ask_big_event")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_big_event",
                "古月族人摇头：\"我也不清楚。只是看到各脉家老频繁出入议事厅，气氛不太寻常。\"")
                .AddOption("令人好奇", "greeting");

            b.StartNode("ask_compound",
                "古月族人：\"古月山寨已经存在好几百年了。据说最初只是一群蛊师为了躲避战乱而建的避难所，后来慢慢发展成了现在的规模。\"")
                .AddOption("山寨有多大？", "ask_size")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_size",
                "古月族人：\"山寨不算大，住着几百号人。蛊师大概有几十个，其余都是凡人。凡人负责日常杂务，蛊师负责保护山寨。\"")
                .AddOption("我明白了", "greeting");

            b.StartNode("ask_daily",
                "古月族人：\"日常生活嘛……种地、养殖、做饭、打扫。虽然不像蛊师那样精彩，但至少安稳。\"")
                .AddOption("凡人和蛊师相处得怎么样？", "ask_relation")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_relation",
                "古月族人：\"大多数蛊师对我们还算客气。当然也有少数傲慢的……但族长管教严格，不会让蛊师欺负凡人。\"")
                .AddOption("那就好", "greeting");

            b.StartNode("trade",
                "古月族人：\"我有些日常用品，你需要吗？\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "古月族人点头：\"再见。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
