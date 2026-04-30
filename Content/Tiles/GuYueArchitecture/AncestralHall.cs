using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 宗族祠堂 - 古月山寨最中央的大气辉煌楼阁，举办祭祀大典的场所
    /// 原文: "古月山寨的最中央，是一座大气辉煌的楼阁。此时正举办着祭祀大典" (gzr_part001.txt:114)
    /// "宗族祠堂中尽是额头碰撞地板的轻响" (gzr_part001.txt:120)
    /// 功能: 大型功能性建筑，可用于开窍/祭祀相关事件
    /// </summary>
    public class AncestralHall : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;

            // 5x5 大型建筑
            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.Origin = new Point16(2, 4);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 50, 50), Language.GetText("Mods.VerminLordMod.MapObject.AncestralHall"));

            AdjTiles = new int[] { TileID.Tables, TileID.Chairs, TileID.WorkBenches };
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 80, 80,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.AncestralHall>());
        }
    }
}
