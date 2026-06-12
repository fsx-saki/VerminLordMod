using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Biomes
{
    public class SouthBorderBiome : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override int Music => 2; // Jungle biome music

        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Terraria/SnowWater");

        public override string BestiaryIcon => "Content/Biomes/SouthBorderIcon";
        public override string BackgroundPath => "Content/Biomes/SouthBorderBackground";
        public override Color? BackgroundColor => new Color(80, 120, 60);

        public override bool IsBiomeActive(Player player)
        {
            // 南疆群系条件：
            // 1. 玩家已到达南疆阶段
            // 2. 在丛林地区
            // 3. 有足够的南疆特有方块
            bool hasReachedSouthBorder = false;
            var storyPhase = Common.DialogueTree.StoryManager.Instance.GetPhase(player);
            if ((int)storyPhase >= (int)Common.DialogueTree.StoryPhase.SouthBorderArrival)
                hasReachedSouthBorder = true;

            bool inJungle = player.ZoneJungle;
            return hasReachedSouthBorder && inJungle;
        }
    }
}
