using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using VerminLordMod.Backgrounds;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.Biomes
{
	/// <summary>
	/// 古月一族驻地生物群系
	/// 替换原有的青茅山群系，使用相同的贴图资源
	/// 古月巡逻蛊师在此群系中刷新
	/// </summary>
	public class GuYueCompoundBiome : ModBiome
	{
		// 使用原有的青茅山群系资源（贴图不换）
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<QingMaoWaterStyle>();
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<QingMaoSurfaceBackgroundStyle>();
		public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Crimson;

		// 音乐
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/SkyCity");

		// Bestiary
		public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
		public override Color? BackgroundColor => base.BackgroundColor;
		public override string MapBackground => BackgroundPath;

		/// <summary>
		/// 检测玩家是否在古月驻地中
		/// 条件：地表 + 足够多的青茅石方块（后续可由世界生成自动放置）
		/// </summary>
		public override bool IsBiomeActive(Player player)
		{
			// 青茅石方块数量 >= 40 即激活
			bool hasEnoughBlocks = ModContent.GetInstance<QingMaoBiomeTileCount>().exampleBlockCount >= 40;

			// 必须在地表或天空层
			bool isSurface = player.ZoneSkyHeight || player.ZoneOverworldHeight;

			return hasEnoughBlocks && isSurface;
		}

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
	}
}
