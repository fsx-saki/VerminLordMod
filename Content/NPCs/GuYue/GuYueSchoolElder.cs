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
    public class GuYueSchoolElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.SchoolElder;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 80, 0);
        }

        protected override string GetRoleDialoguePrefix() => "学堂家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "学堂家老捋了捋胡须：\"学问之道，无捷径可走。\"",
                "学堂家老翻开一本古籍：\"蛊师修炼，首重悟性，次重勤奋。\"",
                "学堂家老严肃地说：\"空窍之道，一窍通则百窍通。你若能领悟此理，前途不可限量。\"",
                "学堂家老叹道：\"如今愿意静心读书的年轻人越来越少了……\"",
                "学堂家老指了指书架：\"这些典籍，都是历代先辈的心血。好好珍惜。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "学堂家老怒道：\"无知狂徒！休要在学堂放肆！\"",
                GuAttitude.Wary => "学堂家老皱眉：\"你若不安分，老夫自会禀报族长。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "学堂家老欣慰地点头：\"孺子可教也。\"",
                GuAttitude.Contemptuous => "学堂家老摇头：\"朽木不可雕。\"",
                GuAttitude.Fearful => "学堂家老后退几步：\"你……你这是何意？\"",
                _ => "学堂家老正在翻阅古籍，没有注意到你。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_SchoolElder", "greeting");

            b.StartNode("greeting",
                "学堂家老放下手中的书卷，抬头看着你。")
                .AddOption("请教修炼之道", "ask_training", DialogueOptionType.Informative)
                .AddOption("关于蛊虫知识", "ask_gu_knowledge", DialogueOptionType.Informative)
                .AddOption("关于学堂", "ask_school", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_training",
                "学堂家老：\"修炼之道，分三步——开辟空窍、炼化蛊虫、突破境界。每一步都需稳扎稳打。\"")
                .AddOption("如何开辟空窍？", "ask_kongqiao")
                .AddOption("如何炼化蛊虫？", "ask_refine")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_kongqiao",
                "学堂家老：\"空窍乃人体与天地灵气沟通的通道。使用空窍石可辅助开辟，但根基仍在自身真元的积累。\"")
                .AddOption("我明白了", "greeting");

            b.StartNode("ask_refine",
                "学堂家老：\"炼化蛊虫需消耗真元，将蛊虫纳入空窍。炼化进度不足时，蛊虫会逐渐脱离控制。切记量力而行。\"")
                .AddOption("多谢指教", "greeting");

            b.StartNode("ask_gu_knowledge",
                "学堂家老翻开一本图鉴：\"蛊虫分天地人三等，每等又分九品。低品蛊虫易得易控，高品蛊虫威力惊人但难以驾驭。\"")
                .AddOption("蛊虫有哪些属性？", "ask_gu_attributes")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_gu_attributes",
                "学堂家老：\"蛊虫属性与道域对应——金木水火土、风雷光暗、血骨魂梦……每种属性都有克制关系。选蛊需结合自身道域。\"")
                .AddOption("受教了", "greeting");

            b.StartNode("ask_school",
                "学堂家老自豪地说：\"古月学堂传承三百年，培养了无数优秀蛊师。学堂的藏书阁是南疆最丰富的蛊术典籍库之一。\"")
                .AddOption("我能借阅吗？", "ask_library")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_library",
                "学堂家老犹豫了一下：\"外层书架可以随意翻阅，但内层典籍需族长特许。你若想深入研读，需先证明自己的价值。\"")
                .AddOption("我会努力的", "greeting");

            b.StartNode("trade",
                "学堂家老：\"这些是学堂的常用物资，你若有需要可以选购。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "学堂家老点点头：\"学无止境，好自为之。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
