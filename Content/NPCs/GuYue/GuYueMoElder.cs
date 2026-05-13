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
    public class GuYueMoElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MoElder;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.35f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 60, 0);
        }

        protected override string GetRoleDialoguePrefix() => "漠脉家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "漠脉家老平静地说：\"漠脉之道，在于坚忍。活到最后的人，才是赢家。\"",
                "漠脉家老淡淡道：\"赤脉那帮莽夫，只知进攻不知防守。战场上，活得久的才是强者。\"",
                "漠脉家老闭目养神：\"防御之道，非一朝一夕可成。需日积月累，方能固若金汤。\"",
                "漠脉家老睁开眼：\"你若想学防御之术，需先学会忍耐。\"",
                "漠脉家老低声道：\"族长的决策是明智的。贸然出击只会招致祸患。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "漠脉家老面色不变：\"无谓的挑衅。\"",
                GuAttitude.Wary => "漠脉家老冷冷地看着你：\"你的举动已被我记录在案。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "漠脉家老微微颔首：\"沉稳之人，可堪大用。\"",
                GuAttitude.Contemptuous => "漠脉家老不予理会：\"不值一提。\"",
                GuAttitude.Fearful => "漠脉家老退后一步，真元护盾自动展开：\"你疯了！\"",
                _ => "漠脉家老正在冥想修炼。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYue_MoElder", "greeting");

            b.StartNode("greeting",
                "漠脉家老面无表情地看着你，周身有一层淡灰真元护罩。")
                .AddOption("关于漠脉", "ask_mo_pulse", DialogueOptionType.Informative)
                .AddOption("关于防御之道", "ask_defense_art", DialogueOptionType.Informative)
                .AddOption("关于赤脉", "ask_chi_pulse", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_mo_pulse",
                "漠脉家老：\"漠脉传承防御之术，以土属性蛊虫为核心。漠脉弟子防御力最强，是古月的盾。\"")
                .AddOption("土属性蛊虫有什么优势？", "ask_earth_gu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_earth_gu",
                "漠脉家老：\"土属性蛊虫防御最高，且能生成护盾。配合漠脉独有的'磐石诀'，可抵御绝大多数攻击。\"")
                .AddOption("我明白了", "greeting");

            b.StartNode("ask_defense_art",
                "漠脉家老：\"防御之道，在于'以不变应万变'。敌人攻势越猛，消耗越大。待其力竭，便是反击之时。\"")
                .AddOption("如何提升防御？", "ask_defense_up")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_defense_up",
                "漠脉家老：\"防御型蛊虫配合防御型道域，效果最佳。此外，铜皮蛊、铁皮蛊等辅助蛊虫也能大幅提升防御。\"")
                .AddOption("多谢指教", "greeting");

            b.StartNode("ask_chi_pulse",
                "漠脉家老冷哼：\"赤脉只知进攻，不懂得保存实力。战场上，莽撞者死得最快。\"")
                .AddOption("两脉能否合作？", "ask_cooperation")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cooperation",
                "漠脉家老沉默片刻：\"理论上攻守互补最为理想。但赤脉家老……太过激进。除非族长出面，否则难以调和。\"")
                .AddOption("我理解了", "greeting");

            b.StartNode("trade",
                "漠脉家老：\"漠脉的防具和护盾道具，品质一流。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "漠脉家老点头：\"谨慎行事。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
