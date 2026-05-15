using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Prototypes
{
    public class GuardPrototypeNPC : NPCBehaviorHost
    {
        public override string Texture => "VerminLordMod/Content/NPCs/Prototypes/GuardPrototypeNPC";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Prototypes/GuardPrototypeNPC_Head";

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DialogueBehavior((talkCount) =>
            {
                if (talkCount == 1) return "守卫：\"站住！出示你的通行证。\"";
                if (talkCount <= 3) return "守卫：\"这里由我守护，闲杂人等不得靠近。\"";
                return "守卫：\"嗯，你可以过去了。\"";
            }, false, null));

            Behaviors.Add(new CombatBehavior(
                damage: 40, knockback: 8f,
                cooldown: 15, randExtraCooldown: 0,
                projType: 0, attackDelay: 1, projSpeed: 0f
            ));

            var loot = new LootBehavior(ItemID.GoldCoin, 1, 3);
            loot.AddItem(ItemID.IronHelmet, 1, 1, 0.3f);
            loot.AddItem(ItemID.IronChainmail, 1, 1, 0.3f);
            loot.AddItem(ItemID.IronGreaves, 1, 1, 0.3f);
            loot.AddItem(ItemID.HealingPotion, 1, 3, 0.5f);
            Behaviors.Add(loot);

            Behaviors.Add(new SpawnBehavior(2, 200));
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 20;
            NPCID.Sets.AttackAverageChance[Type] = 50;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 40;
            NPC.lifeMax = 400;
            NPC.defense = 20;
            NPC.knockBackResist = 0.2f;
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
                "铁壁", "钢盾", "石磊", "坚城", "磐石",
                "虎威", "龙骧", "熊罴", "鹰扬", "豹韬"
            };
        }
    }
}