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
    public class GuYueChief : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Chief;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 1, 0, 0);
        }

        protected override string GetRoleDialoguePrefix() => "古月博";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "古月博沉稳地说：\"古月一族，传承数百年，不会在我手中断绝。\"",
                "古月博目光深邃：\"蛊师之道，在于取舍。你准备好了吗？\"",
                "古月博叹了口气：\"赤脉与漠脉之争，已非一日。但家族存亡面前，一切内斗都应放下。\"",
                "古月博正色道：\"年轻人，记住——蛊师最大的敌人不是外人，而是自己的贪欲。\"",
                "古月博望向远方：\"南疆局势日益紧张，青茅山已非世外桃源。\"",
                "古月博低声道：\"我虽为族长，但有些事……身不由己。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "古月博目光如电：\"你胆敢在古月山寨撒野？！\"",
                GuAttitude.Wary => "古月博冷冷地看着你：\"你的行为已引起我的注意。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "古月博微微颔首：\"你为古月一族立下了功劳。\"",
                GuAttitude.Contemptuous => "古月博不怒自威：\"不知天高地厚。\"",
                GuAttitude.Fearful => "古月博退后一步：\"你……你到底是谁？\"",
                _ => "古月博端坐于主位，不怒自威。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_Chief", "greeting");

            b.StartNode("greeting",
                "古月博端坐于主位，目光深邃地看着你。")
                .AddOption("请教族长", "ask_wisdom", DialogueOptionType.Informative)
                .AddOption("关于家族局势", "ask_situation", DialogueOptionType.Informative)
                .AddOption("关于赤脉与漠脉", "ask_factions", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_wisdom",
                "古月博缓缓开口：\"蛊师之道，首在修心。心若不正，蛊必反噬。\"")
                .AddOption("如何修心？", "ask_cultivation")
                .AddOption("蛊虫反噬会怎样？", "ask_backlash")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cultivation",
                "古月博：\"空窍是蛊师的根基。开辟空窍需要真元灌注，而真元来自日积月累的修炼。切忌急功近利。\"")
                .AddOption("我明白了", "greeting");

            b.StartNode("ask_backlash",
                "古月博面色凝重：\"轻则修为尽失，重则形神俱灭。历史上不乏天才蛊师因贪功冒进而陨落的先例。\"")
                .AddOption("令人警醒", "greeting");

            b.StartNode("ask_situation",
                "古月博叹道：\"南疆各方势力蠢蠢欲动。青茅山虽偏安一隅，但已无法独善其身。我们必须未雨绸缪。\"")
                .AddOption("有什么我可以帮忙的？", "ask_help")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_help",
                "古月博审视着你：\"若你真心为古月效力，先去学堂和药堂历练一番。待你实力足够，我自有安排。\"")
                .AddOption("遵命", "greeting");

            b.StartNode("ask_factions",
                "古月博沉默片刻：\"赤脉主攻，漠脉主守，药脉主医。三脉本应相辅相成，但……权力使人盲目。\"")
                .AddOption("族长不能力挽狂澜吗？", "ask_power")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_power",
                "古月博苦笑：\"我虽为族长，但各脉家老各有根基。牵一发而动全身，只能徐徐图之。\"")
                .AddOption("我理解了", "greeting");

            b.StartNode("trade",
                "古月博：\"这些是家族珍藏，看在你为古月效力的份上，特许你选购。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "古月博微微点头：\"去吧，莫要辜负我的期望。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
