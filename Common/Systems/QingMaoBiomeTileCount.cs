using Terraria.ModLoader;
using VerminLordMod.Content.Tiles;
using System;

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
