using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    public abstract class CommonerNPCBase : NPCBehaviorHost
    {
        public override string Texture => "VerminLordMod/Content/NPCs/Commoners/" + GetType().Name;
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Commoners/" + GetType().Name + "_Head";

        protected abstract string ProfessionName { get; }
        protected abstract Func<int, string> GetDialogueFunc();
        protected virtual bool HasShop => false;
        protected virtual string ShopName => null;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers
            {
                Velocity = 1f,
                Direction = 1
            });
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 10;
            NPC.lifeMax = 250;
            NPC.defense = 15;
            NPC.knockBackResist = 0.5f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = 7;
            AnimationType = NPCID.Guide;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DialogueBehavior(GetDialogueFunc(), HasShop, ShopName));

            Behaviors.Add(new CombatBehavior(
                damage: 10, knockback: 3f,
                cooldown: 30, randExtraCooldown: 10,
                projType: 0, attackDelay: 1, projSpeed: 0f
            ));

            Behaviors.Add(new SpawnBehavior(0, 100));

            RegisterProfessionBehaviors();
        }

        protected virtual void RegisterProfessionBehaviors() { }
    }
}