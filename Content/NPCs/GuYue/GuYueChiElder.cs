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
    public class GuYueChiElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.ChiElder;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 60, 0);
        }

        protected override string GetRoleDialoguePrefix() => "赤脉家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "赤脉家老傲然道：\"赤脉乃古月之剑，攻伐之术无人能出其右。\"",
                "赤脉家老握紧拳头：\"进攻是最好的防御！畏首畏尾只会错失良机。\"",
                "赤脉家老冷笑道：\"漠脉那帮守旧派，只知道缩在壳里。\"",
                "赤脉家老目光锐利：\"年轻人，你若有胆识，赤脉欢迎你。\"",
                "赤脉家老低声道：\"族长太过保守了。若由赤脉主导，古月早已称霸南疆。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "赤脉家老怒目圆睁：\"找死！\"",
                GuAttitude.Wary => "赤脉家老冷冷地看着你：\"你最好别挡赤脉的路。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "赤脉家老点头：\"有点实力，不错。\"",
                GuAttitude.Contemptuous => "赤脉家老嗤笑：\"弱者不配与我对话。\"",
                GuAttitude.Fearful => "赤脉家老咬牙：\"你……你等着！\"",
                _ => "赤脉家老正在练功，浑身散发着灼热的气息。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_ChiElder", "greeting");

            b.StartNode("greeting",
                "赤脉家老傲慢地看着你，周身隐隐有赤红真元流转。")
                .AddOption("关于赤脉", "ask_chi_pulse", DialogueOptionType.Informative)
                .AddOption("关于攻伐之道", "ask_attack", DialogueOptionType.Informative)
                .AddOption("关于家族内争", "ask_internal", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_chi_pulse",
                "赤脉家老：\"赤脉传承攻伐之术，以火属性蛊虫为核心。赤脉弟子战斗意志最强，是古月的利刃。\"")
                .AddOption("火属性蛊虫有什么优势？", "ask_fire_gu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fire_gu",
                "赤脉家老：\"火属性蛊虫攻击力最强，且附带灼烧效果。配合赤脉独有的'焚身诀'，可令敌人痛不欲生。\"")
                .AddOption("厉害", "greeting");

            b.StartNode("ask_attack",
                "赤脉家老来了兴致：\"攻伐之道，讲究'先发制人'。一击必杀，不留后患。犹豫不决是蛊师的大忌。\"")
                .AddOption("如何提升攻击力？", "ask_power_up")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_power_up",
                "赤脉家老：\"选对蛊虫是关键。攻击型蛊虫配合攻击型道域，威力倍增。此外，真元越充沛，蛊虫发挥越强。\"")
                .AddOption("我记下了", "greeting");

            b.StartNode("ask_internal",
                "赤脉家老压低声音：\"赤脉与漠脉之争，由来已久。他们主张防守，我们主张进攻。族长……偏袒漠脉。\"")
                .AddOption("你打算怎么办？", "ask_plan")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_plan",
                "赤脉家老冷笑：\"等时机成熟，赤脉自然会证明谁才是对的。你若识时务，最好站在正确的一边。\"")
                .AddOption("……", "greeting");

            b.StartNode("trade",
                "赤脉家老：\"赤脉的战斗物资，都是上等货色。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "赤脉家老挥手：\"去吧，别浪费我时间。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
