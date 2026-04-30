using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 古月学堂书桌 - 学堂中学员使用的书桌
    /// 原文: 古月山寨学堂中学员上课使用的书桌
    /// 功能: 装饰性家具，可作为工作台使用
    /// </summary>
    public class SchoolDesk : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(120, 80, 40), Language.GetText("Mods.VerminLordMod.MapObject.SchoolDesk"));

            AdjTiles = new int[] { TileID.Tables, TileID.WorkBenches };

            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 32,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.SchoolDesk>());
        }
    }
}
