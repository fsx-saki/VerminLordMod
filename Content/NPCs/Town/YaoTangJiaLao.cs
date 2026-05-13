using VerminLordMod.Common.DialogueTree;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;
using VerminLordMod.Content.NPCs.GuYue;

namespace VerminLordMod.Content.NPCs.Town
{
    [AutoloadHead]
    public class YaoTangJiaLao : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MedicineElder;

        public override string Texture => "VerminLordMod/Content/NPCs/Town/YaoTangJiaLao";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Town/YaoTangJiaLao_Head";

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 60, 0);
        }

        protected override string GetRoleDialoguePrefix() => "药堂家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "药堂家老正在研磨草药：\"受伤了就来药堂，我给你看看。\"",
                "药堂家老温和地说：\"修行路上，保重身体要紧。\"",
                "药堂家老递给你一包药粉：\"这是止血散，随身带着以防万一。\"",
                "药堂家老叹道：\"最近受伤的蛊师越来越多……山外不太平啊。\"",
                "药堂家老认真地说：\"药道与蛊道相辅相成，不懂药理的蛊师走不远。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "药堂家老皱眉：\"你若想在此撒野，先问问御堂答不答应！\"",
                GuAttitude.Wary => "药堂家老警惕地看着你：\"你最好别在药堂惹事。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "药堂家老微笑着点头：\"你的医术有进步。\"",
                GuAttitude.Contemptuous => "药堂家老摇头：\"不知惜身之人，不值得医治。\"",
                GuAttitude.Fearful => "药堂家老后退几步：\"你……你要做什么？\"",
                _ => "药堂家老正在整理药材。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("YaoTangJiaLao", "greeting");

            b.StartNode("greeting",
                "药堂家老放下手中的药臼，温和地看着你。")
                .AddOption("关于药道", "ask_medicine", DialogueOptionType.Informative)
                .AddOption("关于治疗", "ask_healing", DialogueOptionType.Informative)
                .AddOption("关于丹药", "ask_pills", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_medicine",
                "药堂家老：\"药道讲究君臣佐使，配伍得当则药到病除，配伍不当则害人害己。蛊师用药，更需谨慎。\"")
                .AddOption("蛊师用药有何不同？", "ask_gu_medicine")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_gu_medicine",
                "药堂家老：\"蛊师体质与凡人不同，真元运行会改变药性吸收。有些凡人用的良药，对蛊师反而有害。因此药堂专门研究蛊师药理。\"")
                .AddOption("原来如此", "greeting");

            b.StartNode("ask_healing",
                "药堂家老：\"治疗之术分三等——凡医用药，蛊医用蛊，上医用心。药堂以药为主，辅以治疗类蛊虫。\"")
                .AddOption("治疗类蛊虫？", "ask_healing_gu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_healing_gu",
                "药堂家老：\"如月蛾蛊，可在夜间缓慢恢复宿主伤势。又如青叶蛊，能加速伤口愈合。但治疗类蛊虫往往战斗力不足，需配合药道使用。\"")
                .AddOption("受教了", "greeting");

            b.StartNode("ask_pills",
                "药堂家老：\"丹药是药堂的核心产出。从最基础的疗伤丹，到高级的回元丹，每一颗都经过严格炼制。品质有保障。\"")
                .AddOption("我需要一些丹药", "trade")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "药堂家老：\"药堂的丹药和药材，种类齐全。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "药堂家老点头：\"保重身体。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
