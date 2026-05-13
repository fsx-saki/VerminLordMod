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
    public class GuYueSecondTurnGuMaster : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.SecondTurnGuMaster;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 40, 0);
        }

        protected override string GetRoleDialoguePrefix() => "二转蛊师";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "二转蛊师沉稳地说：\"修炼之路漫长，不可懈怠。\"",
                "二转蛊师擦拭着蛊虫：\"二转之后，才真正理解了蛊师之道。一转时不过是在打基础罢了。\"",
                "二转蛊师感慨道：\"当年我也像那些一转弟子一样，觉得修炼遥遥无期。坚持下来就好了。\"",
                "二转蛊师正色道：\"巡逻任务不轻松，但这是为家族出力的机会。\"",
                "二转蛊师低声道：\"最近山外的局势越来越复杂了……你要多加小心。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "二转蛊师拔出蛊虫：\"不自量力！\"",
                GuAttitude.Wary => "二转蛊师警惕地看着你：\"你最好别惹事。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "二转蛊师点头：\"你的实力值得尊敬。\"",
                GuAttitude.Contemptuous => "二转蛊师不屑：\"哼，不过如此。\"",
                GuAttitude.Fearful => "二转蛊师后退一步：\"你……你到底是什么人？\"",
                _ => "二转蛊师正在维护装备。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_SecondTurnGuMaster", "greeting");

            b.StartNode("greeting",
                "二转蛊师向你点了点头，神情沉稳。")
                .AddOption("请教经验", "ask_experience", DialogueOptionType.Informative)
                .AddOption("关于巡逻任务", "ask_patrol", DialogueOptionType.Informative)
                .AddOption("关于突破二转", "ask_breakthrough", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_experience",
                "二转蛊师：\"一转到二转是最大的坎。很多人卡在一转一辈子。关键在于真元积累是否足够，以及对蛊虫的理解是否到位。\"")
                .AddOption("有什么诀窍吗？", "ask_tips")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_tips",
                "二转蛊师：\"诀窍就是没有诀窍。日复一日的修炼，加上实战经验的积累。不过，选择适合自己道域的蛊虫确实很重要。\"")
                .AddOption("我记下了", "greeting");

            b.StartNode("ask_patrol",
                "二转蛊师：\"巡逻任务主要是巡视青茅山周边，驱赶野兽和散修。偶尔也会遇到危险，但二转蛊师足以应对大多数情况。\"")
                .AddOption("遇到过什么危险吗？", "ask_danger")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_danger",
                "二转蛊师面色微变：\"上个月，巡逻队在外围发现了一具三转蛊师的尸体……死因不明。御堂家老已经加强了警戒。\"")
                .AddOption("令人不安", "greeting");

            b.StartNode("ask_breakthrough",
                "二转蛊师回忆道：\"突破二转时，感觉空窍猛然扩张，真元如洪流般涌过。那一瞬间，世界仿佛都不同了。\"")
                .AddOption("听起来很震撼", "greeting");

            b.StartNode("trade",
                "二转蛊师：\"我有些用不上的物资，你可以看看。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "二转蛊师点头：\"保重。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
