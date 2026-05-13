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
    public class YuTangJiaLao : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.DefenseElder;

        public override string Texture => "VerminLordMod/Content/NPCs/Town/YuTangJiaLao";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Town/YuTangJiaLao_Head";

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 70, 0);
        }

        protected override string GetRoleDialoguePrefix() => "御堂家老";

        protected override string GetFriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "御堂家老正在检查防务：\"山寨安全，是我御堂的首要职责。\"",
                "御堂家老沉声道：\"最近山外不太平，巡逻队已加强警戒。\"",
                "御堂家老拍了拍盔甲：\"好的防御胜过最好的进攻。\"",
                "御堂家老审视着你：\"你若想加入巡逻队，需先通过我的考核。\"",
                "御堂家老低声说：\"有消息说，有散修在山寨附近出没……\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "御堂家老拔出武器：\"来者不善！御堂弟子何在！\"",
                GuAttitude.Wary => "御堂家老手按剑柄：\"你最好解释一下你的行为。\"",
                GuAttitude.Friendly => GetFriendlyDialogue(),
                GuAttitude.Respectful => "御堂家老行军礼：\"你的实力令人敬佩。\"",
                GuAttitude.Contemptuous => "御堂家老冷哼：\"不堪一击。\"",
                GuAttitude.Fearful => "御堂家老后退一步，吹响警哨：\"来人！\"",
                _ => "御堂家老正在巡视防务。"
            };
        }

        protected override DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("YuTangJiaLao", "greeting");

            b.StartNode("greeting",
                "御堂家老正色看着你，手按剑柄。")
                .AddOption("关于山寨防务", "ask_defense", DialogueOptionType.Informative)
                .AddOption("关于巡逻队", "ask_patrol", DialogueOptionType.Informative)
                .AddOption("关于散修威胁", "ask_rogue", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_defense",
                "御堂家老：\"古月山寨三面环山，只有正面和两条小路可通。御堂在关键位置都布有阵法和暗哨。\"")
                .AddOption("阵法？", "ask_formations")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_formations",
                "御堂家老：\"迷踪阵是御堂的得意之作。外人踏入阵中，感知范围大幅缩减，而我方人员不受影响。攻守兼备。\"")
                .AddOption("厉害", "greeting");

            b.StartNode("ask_patrol",
                "御堂家老：\"巡逻蛊师分三班轮值，昼夜不停。他们都是一转以上的精锐，不可小觑。\"")
                .AddOption("我能加入巡逻吗？", "ask_join_patrol")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_join_patrol",
                "御堂家老审视着你：\"加入巡逻队需要至少一转修为，且通过实战考核。你若有意，先去训练场磨练一番。\"")
                .AddOption("我会准备的", "greeting");

            b.StartNode("ask_rogue",
                "御堂家老面色凝重：\"近来有不少散修在青茅山出没。有些只是路过，但有些……不怀好意。你若在山外遇到他们，务必小心。\"")
                .AddOption("多谢提醒", "greeting");

            b.StartNode("trade",
                "御堂家老：\"御堂的武器装备，品质上乘。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "御堂家老点头：\"注意安全。\"")
                .AddOption("告退", "bye", DialogueOptionType.Exit);

            return b.Build();
        }
    }
}
