using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public class WildGuMaster : RogueGuMasterBase
    {
        public override GuRank GetRank() => GuRank.Zhuan1_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cautious;
        public override string GuMasterDisplayName => "野生蛊师";
        public override int GuMasterDamage => 20;
        public override int GuMasterLife => 300;
        public override int GuMasterDefense => 10;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.value = Item.buyPrice(0, 0, 30, 0);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            return attitude switch
            {
                GuAttitude.Hostile => "野生蛊师怒吼：\"找死！\"",
                GuAttitude.Wary => "野生蛊师警惕地盯着你：\"别过来...\"",
                GuAttitude.Friendly => "野生蛊师放松了一些：\"嗯...你看起来不像坏人。\"",
                GuAttitude.Fearful => "野生蛊师后退了几步：\"你...你是什么人？！\"",
                _ => "野生蛊师没有理会你。"
            };
        }
    }
}