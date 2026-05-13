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
    public class GuYueMedicinePulseElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MedicinePulseElder;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 65, 0);
        }

        protected override string GetRoleDialoguePrefix() => "药脉家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "药脉家老温和地说：\"药脉不争权势，只求救人济世。\"",
                "药脉家老整理着药材：\"赤脉漠脉之争，与我们药脉无关。我们只管救人。\"",
                "药脉家老微笑道：\"蛊师修炼，身体是根本。药脉的丹药，能让你走得更远。\"",
                "药脉家老叹道：\"可惜族里愿意学医的人越来越少。都去追求战斗了……\"",
                "药脉家老认真地说：\"一个家族若没有医者，再强的战士也不过是昙花一现。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "药脉家老叹息道：\"暴力解决不了任何问题。\"",
                GuAttitude.Wary => "药脉家老退后一步：\"药脉不参与争斗，请自重。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "药脉家老欣慰地说：\"懂得尊重医者的人，值得尊重。\"",
                GuAttitude.Contemptuous => "药脉家老不为所动：\"你迟早会需要药脉的帮助。\"",
                GuAttitude.Fearful => "药脉家老护住药箱：\"求求你，这些药材是用来救命的……\"",
                _ => "药脉家老正在整理药柜。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_MedicinePulseElder", "greeting");

            b.StartNode("greeting",
                "药脉家老温和地看着你，手中还拿着一株草药。")
                .AddOption("关于药脉", "ask_medicine_pulse", DialogueOptionType.Informative)
                .AddOption("关于医道", "ask_healing", DialogueOptionType.Informative)
                .AddOption("关于三脉关系", "ask_three_pulses", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_medicine_pulse",
                "药脉家老：\"药脉传承医道，以木属性蛊虫为核心。药脉弟子擅长治疗和辅助，是古月的后盾。\"")
                .AddOption("木属性蛊虫有什么优势？", "ask_wood_gu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_wood_gu",
                "药脉家老：\"木属性蛊虫擅长治疗和恢复，能加速伤口愈合、恢复真元。配合药脉独有的'回春诀'，可起死回生。\"")
                .AddOption("了不起", "greeting");

            b.StartNode("ask_healing",
                "药脉家老：\"医道有三层——治伤、治蛊、治命。治伤最易，治蛊次之，治命最难。药脉目前只掌握了前两层。\"")
                .AddOption("治命是什么？", "ask_life_healing")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_life_healing",
                "药脉家老摇头：\"治命之术，传说能延续寿命、逆转衰老。但那只是传说……至少目前是。\"")
                .AddOption("也许将来能实现", "greeting");

            b.StartNode("ask_three_pulses",
                "药脉家老叹道：\"赤脉主攻，漠脉主守，药脉主医。三脉本应相辅相成，但赤脉与漠脉的权力之争……唉。\"")
                .AddOption("药脉不参与吗？", "ask_neutral")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_neutral",
                "药脉家老：\"药脉保持中立，只管救人。但若家族有难，药脉绝不会袖手旁观。\"")
                .AddOption("令人敬佩", "greeting");

            b.StartNode("trade",
                "药脉家老：\"药脉的丹药和药材，种类齐全。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "药脉家老微笑：\"保重身体，有需要随时来。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
