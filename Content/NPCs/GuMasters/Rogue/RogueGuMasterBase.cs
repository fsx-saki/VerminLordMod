using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public abstract class RogueGuMasterBase : GuMasterBase
    {
        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/Rogue/" + GetType().Name;
        public override string HeadTexture => "VerminLordMod/Content/NPCs/GuMasters/Rogue/" + GetType().Name + "_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override string GuMasterDisplayName => "散修蛊师";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.townNPC = false;
            NPC.friendly = false;
            NPC.width = 18;
            NPC.height = 40;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            AnimationType = NPCID.Guide;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.02f;
        }
    }
}