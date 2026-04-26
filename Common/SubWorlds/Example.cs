using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.WorldBuilding;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.SubWorlds
{
	public class ExampleSubworld : Subworld
	{
		public override int Width => 1000;
		public override int Height => 1000;

		public override bool ShouldSave => true;
		public override bool NoPlayerSaving => true;

		public override List<GenPass> Tasks => new List<GenPass>()
		{
		new ExampleGenPass()
	};

		// Sets the time to the middle of the day whenever the subworld loads
		public override void OnLoad() {
			Main.dayTime = true;
			Main.time = 27000;
		}
	}

	public class ExampleGenPass : GenPass
	{
		//TODO: remove this once tML changes generation passes
		public ExampleGenPass() : base("Terrain", 1) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			progress.Message = "Generating terrain"; // Sets the text displayed for this pass
			Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
			Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds
			for (int i = 0; i < Main.maxTilesX; i++) {
				for (int j = Main.maxTilesY / 2; j < Main.maxTilesY; j++) {
					progress.Set((j + i * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = TileID.Dirt;
				}
			}
		}
	}

	public class UpdateSubworldSystem : ModSystem
	{
		public override void PreUpdateWorld() {
			if (SubworldSystem.IsActive<ExampleSubworld>()) {
				// Update mechanisms
				Wiring.UpdateMech();

				// Update tile entities
				TileEntity.UpdateStart();
				foreach (TileEntity te in TileEntity.ByID.Values) {
					te.Update();
				}
				TileEntity.UpdateEnd();

				// Update liquid
				if (++Liquid.skipCount > 1) {
					Liquid.UpdateLiquid();
					Liquid.skipCount = 0;
				}
			}
		}
	}
}
