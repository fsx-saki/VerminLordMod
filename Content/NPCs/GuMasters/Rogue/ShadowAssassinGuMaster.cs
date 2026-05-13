using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public class ShadowAssassinGuMaster : RogueGuMasterBase
    {
        public override GuRank GetRank() => GuRank.Zhuan2_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Cold;
        public override string GuMasterDisplayName => "暗影蛊师";
        public override int GuMasterDamage => 70;
        public override int GuMasterLife => 600;
        public override int GuMasterDefense => 12;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.value = Item.buyPrice(0, 2, 50, 0);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            var hostileLines = new List<string>
            {
                "暗影蛊师无声地拔出武器：\"……死。\"",
                "暗影蛊师：\"你看到了不该看到的东西。\"",
                "暗影蛊师低语：\"没有人能活着离开。\"",
            };
            var waryLines = new List<string>
            {
                "暗影蛊师警惕地注视着你：\"……\"",
                "暗影蛊师：\"别多管闲事。\"",
            };

            return attitude switch
            {
                GuAttitude.Hostile => hostileLines[Main.rand.Next(hostileLines.Count)],
                GuAttitude.Wary => waryLines[Main.rand.Next(waryLines.Count)],
                GuAttitude.Contemptuous => "暗影蛊师不屑一顾：\"弱。\"",
                GuAttitude.Fearful => "暗影蛊师面色微变：\"……撤。\"",
                _ => "暗影蛊师隐匿在阴影中，几乎无法察觉。"
            };
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            if (qiRealm.GuLevel < 2) return 0f;
            if (!Main.hardMode) return 0f;
            return 0.008f;
        }
    }
}
