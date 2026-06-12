using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Biomes
{
    public class NorthDesertBiome : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override int Music => 48; // Snow biome music

        public override string BestiaryIcon => "Content/Biomes/NorthDesertIcon";
        public override string BackgroundPath => "Content/Biomes/NorthDesertBackground";
        public override Color? BackgroundColor => new Color(200, 220, 255);

        public override bool IsBiomeActive(Player player)
        {
            // 北原群系条件：
            // 1. 玩家已到达北原阶段
            // 2. 在雪地地区
            bool hasReachedNorthDesert = false;
            var storyPhase = Common.DialogueTree.StoryManager.Instance.GetPhase(player);
            if ((int)storyPhase >= (int)Common.DialogueTree.StoryPhase.NorthDesertArrival)
                hasReachedNorthDesert = true;

            bool inSnow = player.ZoneSnow;
            return hasReachedNorthDesert && inSnow;
        }
    }
}
