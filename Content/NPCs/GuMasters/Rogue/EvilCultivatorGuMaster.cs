using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters.Rogue
{
    public class EvilCultivatorGuMaster : RogueGuMasterBase
    {
        public override GuRank GetRank() => GuRank.Zhuan2_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cruel;
        public override string GuMasterDisplayName => "邪修蛊师";
        public override int GuMasterDamage => 55;
        public override int GuMasterLife => 800;
        public override int GuMasterDefense => 18;

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.value = Item.buyPrice(0, 2, 0, 0);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            var hostileLines = new List<string>
            {
                "邪修蛊师阴笑道：\"你的蛊虫……归我了。\"",
                "邪修蛊师舔了舔嘴唇：\"又来了一个送死的。\"",
                "邪修蛊师眼中闪过贪婪：\"你的空窍……让我看看里面藏着什么好东西。\"",
            };
            var waryLines = new List<string>
            {
                "邪修蛊师冷冷地看着你：\"别挡我的道。\"",
                "邪修蛊师眯起眼睛：\"你身上有股……令人不悦的正气。\"",
            };
            var fearfulLines = new List<string>
            {
                "邪修蛊师面露惧色：\"你……你是谁！\"",
                "邪修蛊师后退几步：\"不可能……这种力量……\"",
            };

            return attitude switch
            {
                GuAttitude.Hostile => hostileLines[Main.rand.Next(hostileLines.Count)],
                GuAttitude.Wary => waryLines[Main.rand.Next(waryLines.Count)],
                GuAttitude.Contemptuous => "邪修蛊师嗤笑：\"蝼蚁而已。\"",
                GuAttitude.Fearful => fearfulLines[Main.rand.Next(fearfulLines.Count)],
                _ => "邪修蛊师周身散发着诡异的气息。"
            };
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            if (qiRealm.GuLevel < 2) return 0f;
            return 0.01f;
        }
    }
}
