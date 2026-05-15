using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Prototypes
{
    public class PassiveVillagerNPC : NPCBehaviorHost
    {
        public override string Texture => "VerminLordMod/Content/NPCs/Prototypes/PassiveVillagerNPC";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Prototypes/PassiveVillagerNPC_Head";

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DialogueBehavior((talkCount) =>
            {
                if (talkCount == 1) return "村民：\"您好，旅行者。我们这里很太平。\"";
                if (talkCount <= 3) return "村民：\"今天天气不错，适合出门走走。\"";
                return "村民：\"有空常来坐坐。\"";
            }, false, null));

            var loot = new LootBehavior(ItemID.CopperCoin, 5, 20);
            loot.AddItem(ItemID.Wood, 3, 8, 0.5f);
            loot.AddItem(ItemID.Gel, 1, 3, 0.3f);
            Behaviors.Add(loot);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 300;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 60;
            NPCID.Sets.AttackAverageChance[Type] = 0;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 0;
            NPC.lifeMax = 50;
            NPC.defense = 2;
            NPC.knockBackResist = 0.8f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>
            {
                "张三", "李四", "王五", "赵六", "陈七",
                "老刘", "阿福", "小翠", "春兰", "秋菊"
            };
        }
    }
}