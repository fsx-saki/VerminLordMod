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
    public class GuYueFistInstructor : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.FistInstructor;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.2f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 30, 0);
        }

        protected override string GetRoleDialoguePrefix() => "拳脚教头";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "拳脚教头虎目一瞪：\"站直了！蛊师不能只靠蛊虫，拳脚功夫也不能落下！\"",
                "拳脚教头活动着筋骨：\"身体是蛊师的根本。没有强健的体魄，再好的蛊虫也发挥不出威力。\"",
                "拳脚教头拍着木人桩：\"来，跟我过两招！\"",
                "拳脚教头严肃道：\"蛊虫可能会反噬、可能会被克制，但你的拳头永远属于你自己。\"",
                "拳脚教头咧嘴一笑：\"别小看凡人的拳脚功夫。一拳打在要害上，蛊师也得倒！\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "拳脚教头摆出架势：\"想动手？来！\"",
                GuAttitude.Wary => "拳脚教头握紧拳头：\"你最好别在训练场闹事。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "拳脚教头拍拍你的肩膀：\"好身手！\"",
                GuAttitude.Contemptuous => "拳脚教头嗤笑：\"花拳绣腿。\"",
                GuAttitude.Fearful => "拳脚教头后退一步：\"你……你这是何苦？\"",
                _ => "拳脚教头正在训练场上指导弟子。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_FistInstructor", "greeting");

            b.StartNode("greeting",
                "拳脚教头站在训练场中央，浑身肌肉虬结。")
                .AddOption("请教拳脚功夫", "ask_fist", DialogueOptionType.Informative)
                .AddOption("关于体魄修炼", "ask_body", DialogueOptionType.Informative)
                .AddOption("关于实战技巧", "ask_combat", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_fist",
                "拳脚教头：\"拳脚功夫分三路——拳、掌、指。拳走刚猛，掌走绵柔，指走精巧。各有千秋，但根基都是力量和速度。\"")
                .AddOption("哪种最适合蛊师？", "ask_best_style")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_best_style",
                "拳脚教头：\"蛊师近身格斗少，但掌法最实用。掌法灵活，能配合蛊虫施展，攻守兼备。\"")
                .AddOption("我学掌法", "greeting");

            b.StartNode("ask_body",
                "拳脚教头：\"体魄修炼讲究'外练筋骨皮，内练一口气'。蛊师以真元为主，但身体素质也不能忽视。\"")
                .AddOption("如何锻炼？", "ask_exercise")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_exercise",
                "拳脚教头：\"每天扎马步一个时辰，打木人桩五百下，绕山寨跑三圈。坚持三个月，你的体魄就能超过大多数蛊师。\"")
                .AddOption("听起来很辛苦", "greeting");

            b.StartNode("ask_combat",
                "拳脚教头认真地说：\"实战中，最重要的是判断时机。敌人出招的瞬间，就是破绽出现的瞬间。抓住那一瞬，一击制胜。\"")
                .AddOption("如何判断时机？", "ask_timing")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_timing",
                "拳脚教头：\"这只能靠实战积累。我安排的实战对练，就是为了培养这种直觉。多打，多看，多想。\"")
                .AddOption("我明白了", "greeting");

            b.StartNode("trade",
                "拳脚教头：\"训练用的装备和药物，都在这里。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "拳脚教头挥手：\"去吧，别忘了每天练功！\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
