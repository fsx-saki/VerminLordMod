using System;
using Terraria.ModLoader;
using VerminLordMod.Content.Tiles;

namespace VerminLordMod.Common.Systems
{
	public class QingMaoBiomeTileCount : ModSystem
	{
		public int exampleBlockCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			exampleBlockCount = tileCounts[ModContent.TileType<QingMaoStoneBlock>()];
		}
	}
}
