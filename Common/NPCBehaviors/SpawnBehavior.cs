using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class SpawnBehavior : BaseNPCBehavior
    {
        public override string Name => "SpawnBehavior";

        private readonly int _minGuLevel;
        private readonly int _minLife;

        public SpawnBehavior(int minGuLevel = 1, int minLife = 100)
        {
            _minGuLevel = minGuLevel;
            _minLife = minLife;
        }

        public override bool? CanTownNPCSpawn(NPC npc, int numTownNPCs)
        {
            foreach (var player in Main.ActivePlayers)
            {
                var qiRealm = player.GetModPlayer<Common.Players.QiRealmPlayer>();
                if (qiRealm.GuLevel >= _minGuLevel && player.statLifeMax2 >= _minLife)
                    return true;
            }
            return false;
        }
    }
}