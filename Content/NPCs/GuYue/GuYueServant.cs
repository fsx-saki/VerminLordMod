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
    public class GuYueServant : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Servant;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.7f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
        }

        protected override string GetRoleDialoguePrefix() => "杂役";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "杂役恭敬地说：\"大人有什么吩咐？\"",
                "杂役擦着汗：\"今天还有好多活要干呢……\"",
                "杂役小声道：\"能留在山寨做杂役，已经比外面好太多了。\"",
                "杂役叹气：\"我虽然不是蛊师，但在这里也能学到不少东西。\"",
                "杂役微笑道：\"药堂的丹药味儿，闻多了也就习惯了。\"",
                "杂役认真地说：\"山寨里的人都对我很好，我会好好干活的。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "杂役吓得跪下：\"大人饶命！\"",
                GuAttitude.Wary => "杂役紧张地后退：\"我、我什么都没做……\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "杂役受宠若惊：\"大人太客气了！\"",
                GuAttitude.Contemptuous => "杂役低下头，默默忍受。",
                GuAttitude.Fearful => "杂役瑟瑟发抖：\"求求你别伤害我……\"",
                _ => "杂役正在忙碌地干活。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_Servant", "greeting");

            b.StartNode("greeting",
                "杂役停下手中的活，恭敬地看着你。")
                .AddOption("聊聊", "ask_chat", DialogueOptionType.Informative)
                .AddOption("关于山寨", "ask_compound", DialogueOptionType.Informative)
                .AddOption("关于凡人生活", "ask_mortal_life", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_chat",
                "杂役：\"我每天负责打扫、搬运、做饭……虽然辛苦，但比在外面好多了。至少不用担心野兽和散修。\"")
                .AddOption("你是怎么来山寨的？", "ask_origin")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_origin",
                "杂役：\"我原本是山下村子的孤儿。古月山寨收留了我，让我做杂役换取食宿。虽然不是蛊师，但至少有个安身之处。\"")
                .AddOption("不容易", "greeting");

            b.StartNode("ask_compound",
                "杂役：\"山寨里规矩多，但都是为大家好。最重要的一条——不准在寨内私斗。违反的人会被赶出去。\"")
                .AddOption("还有什么规矩？", "ask_rules")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_rules",
                "杂役掰着手指数：\"不准偷盗、不准欺凌弱小、不准私藏蛊虫……还有，议事厅和藏书阁是禁地，没有许可不能进。\"")
                .AddOption("我记住了", "greeting");

            b.StartNode("ask_mortal_life",
                "杂役：\"凡人在蛊师的世界里很渺小。但古月山寨对凡人还算不错，只要勤快干活，就能安稳度日。\"")
                .AddOption("有没有想过修炼？", "ask_dream")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream",
                "杂役苦笑：\"修炼需要天赋。我试过开辟空窍，但失败了。学堂家老说，不是每个人都能成为蛊师的。不过没关系，做好本职工作也是一种修行。\"")
                .AddOption("说得对", "greeting");

            b.StartNode("trade",
                "杂役：\"我有些日常用品，你需要吗？\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "杂役鞠躬：\"大人慢走。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
