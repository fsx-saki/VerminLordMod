using Microsoft.Xna.Framework;
using Terraria;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Abstractions
{
    public interface IFactionMember
    {
        FactionID Faction { get; }
        RepLevel GetReputationWith(FactionID otherFaction);
        int GetReputationPointsWith(FactionID otherFaction);
        void ModifyReputation(FactionID targetFaction, int delta, string reason = "");
        bool IsHostileTo(FactionID otherFaction);
        bool IsAlliedWith(FactionID otherFaction);
    }

    public interface IFactionTerritory
    {
        FactionID OwnerFaction { get; }
        string TerritoryName { get; }
        Rectangle Bounds { get; }
        bool IsContested { get; }
        FactionID ControllingFaction { get; }
    }

    public interface IReputationModifier
    {
        string SourceName { get; }
        int CalculateRepDelta(FactionID targetFaction, InteractionType interaction);
        bool ShouldTriggerChainReaction(FactionID sourceFaction, FactionID targetFaction, int delta);
    }

    public static class FactionMemberHelper
    {
        public static bool IsSameFaction(IFactionMember a, IFactionMember b)
        {
            return a.Faction == b.Faction && a.Faction != FactionID.None;
        }

        public static bool IsNeutralOrFriendly(IFactionMember member, FactionID otherFaction)
        {
            var rep = member.GetReputationWith(otherFaction);
            return rep >= RepLevel.Neutral;
        }

        public static string GetRelationshipDescription(IFactionMember member, FactionID otherFaction)
        {
            var rep = member.GetReputationWith(otherFaction);
            var factionName = WorldStateMachine.GetFactionDisplayName(otherFaction);
            return rep switch
            {
                RepLevel.Hostile => $"与{factionName}势不两立",
                RepLevel.Unfriendly => $"与{factionName}关系紧张",
                RepLevel.Neutral => $"与{factionName}保持中立",
                RepLevel.Friendly => $"与{factionName}关系友好",
                RepLevel.Allied => $"与{factionName}结为盟友",
                _ => $"与{factionName}关系未知",
            };
        }
    }
}
