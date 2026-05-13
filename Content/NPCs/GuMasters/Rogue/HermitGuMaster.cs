using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public class HermitGuMaster : RogueGuMasterBase
    {
        public override GuRank GetRank() => GuRank.Zhuan2_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Benevolent;
        public override string GuMasterDisplayName => "隐修蛊师";
        public override int GuMasterDamage => 25;
        public override int GuMasterLife => 400;
        public override int GuMasterDefense => 12;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.value = Item.buyPrice(0, 0, 80, 0);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            return attitude switch
            {
                GuAttitude.Hostile => "隐修蛊师叹息：\"本想与世无争...也罢，来吧！\"",
                GuAttitude.Wary => "隐修蛊师平静地说：\"老朽在此隐居多年，不喜打扰。\"",
                GuAttitude.Friendly => "隐修蛊师微笑道：\"年轻人，蛊道漫漫，需静心修炼。\"",
                GuAttitude.Respectful => "隐修蛊师颔首：\"你的修为不错，假以时日必成大器。\"",
                GuAttitude.Fearful => "隐修蛊师后退一步：\"你...你身上有魔道的气息！\"",
                _ => "隐修蛊师闭目养神，没有理会你。"
            };
        }
    }
}