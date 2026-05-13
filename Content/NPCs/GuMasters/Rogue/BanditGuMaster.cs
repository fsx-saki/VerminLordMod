using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public class BanditGuMaster : RogueGuMasterBase
    {
        public override GuRank GetRank() => GuRank.Zhuan1_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Aggressive;
        public override string GuMasterDisplayName => "悍匪蛊师";
        public override int GuMasterDamage => 35;
        public override int GuMasterLife => 500;
        public override int GuMasterDefense => 15;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.value = Item.buyPrice(0, 1, 0, 0);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            return attitude switch
            {
                GuAttitude.Hostile => "悍匪蛊师大喝：\"交出你的蛊虫，饶你不死！\"",
                GuAttitude.Wary => "悍匪蛊师冷笑：\"识相的就滚远点。\"",
                GuAttitude.Contemptuous => "悍匪蛊师轻蔑地说：\"就凭你也配做蛊师？\"",
                GuAttitude.Fearful => "悍匪蛊师色厉内荏：\"你...你别过来！\"",
                _ => "悍匪蛊师恶狠狠地盯着你。"
            };
        }
    }
}