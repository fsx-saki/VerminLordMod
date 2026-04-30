using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 饭袋草蛊 - 一转草蛊，结出香喷喷的米饭
    /// 原文: "蛊的形态，有很多种...还有花草形态，就是方源眼前的...饭袋草蛊" (gzr_part001.txt:1148-1150)
    /// 原文: "草袋中会生长出香喷喷的米饭" (gzr_part001.txt:1150)
    /// </summary>
    public class RiceBagGrass : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateHeights = new[] { 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = -2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Grass, TileID.Dirt, TileID.Mud, TileID.Stone };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 200, 80), CreateMapEntryName());

            DustType = DustID.Grass;
            HitSound = SoundID.Grass;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.RiceBagGrass>());
        }
    }
}
