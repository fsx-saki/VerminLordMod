using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Prototypes
{
    public class HostileBanditNPC : NPCBehaviorHost
    {
        public override string Texture => "VerminLordMod/Content/NPCs/Prototypes/HostileBanditNPC";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Prototypes/HostileBanditNPC_Head";

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DialogueBehavior((talkCount) =>
            {
                if (talkCount == 1) return "悍匪：\"看什么看？不想死就滚远点！\"";
                if (talkCount <= 3) return "悍匪：\"这片地盘归我管，识相的交出钱财。\"";
                return "悍匪：\"哼，算你识相。\"";
            }, false, null));

            Behaviors.Add(new CombatBehavior(
                damage: 25, knockback: 5f,
                cooldown: 20, randExtraCooldown: 5,
                projType: 0, attackDelay: 1, projSpeed: 0f
            ));

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 50);
            loot.AddItem(ItemID.IronBroadsword, 1, 1, 0.2f);
            loot.AddItem(ItemID.IronBow, 1, 1, 0.15f);
            loot.AddItem(ItemID.Leather, 2, 5, 0.5f);
            loot.AddItem(ItemID.Torch, 5, 15, 0.6f);
            Behaviors.Add(loot);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 500;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 40;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 25;
            NPC.lifeMax = 200;
            NPC.defense = 10;
            NPC.knockBackResist = 0.3f;
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
                "黑风", "铁手", "独眼", "疤脸", "血刀",
                "鬼面", "毒蛇", "恶狼", "秃鹰", "野狗"
            };
        }
    }
}