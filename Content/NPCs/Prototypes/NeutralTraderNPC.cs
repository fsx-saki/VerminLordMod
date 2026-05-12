using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Prototypes
{
    public class NeutralTraderNPC : NPCBehaviorHost
    {
        public override string Texture => "VerminLordMod/Content/NPCs/Town/JiasTravelingMerchant";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Town/JiasTravelingMerchant_Head";

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DialogueBehavior((talkCount) =>
            {
                if (talkCount == 1) return "行商：\"欢迎欢迎！看看有什么需要的？\"";
                if (talkCount <= 3) return "行商：\"我走南闯北，什么稀奇玩意儿都见过。\"";
                return "行商：\"下次再来，给你算便宜点。\"";
            }, true, "TraderShop"));

            Behaviors.Add(new CombatBehavior(
                damage: 15, knockback: 3f,
                cooldown: 30, randExtraCooldown: 10,
                projType: 0, attackDelay: 1, projSpeed: 0f
            ));

            var shop = new ShopBehavior("TraderShop");
            shop.AddItem(ItemID.HealingPotion, 10);
            shop.AddItem(ItemID.ManaPotion, 5);
            shop.AddItem(ItemID.Torch, 1);
            shop.AddItem(ItemID.Rope, 1);
            shop.AddItem(ItemID.WoodenArrow, 1);
            shop.AddItem(ItemID.Shuriken, 3);
            shop.AddItem(ItemID.RecallPotion, 15);
            shop.AddItem(ItemID.IronskinPotion, 20);
            Behaviors.Add(shop);

            Behaviors.Add(new SpawnBehavior(1, 100));
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 400;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 20;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 15;
            NPC.lifeMax = 150;
            NPC.defense = 8;
            NPC.knockBackResist = 0.5f;
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
                "贾富", "钱多", "金宝", "万利", "财源",
                "商通", "货郎", "掌柜", "老板", "东家"
            };
        }
    }
}