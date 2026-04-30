using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 青竹酒坛 - 装青竹酒的酒坛装饰
    /// 原文: "给我上坛青竹酒" (gzr_part001.txt:974)
    /// 原文: "青竹酒，可以说是古月山寨中的第一好酒" (gzr_part001.txt:1020)
    /// 功能: 装饰性物块
    /// </summary>
    public class GreenBambooWineJar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(60, 140, 60), CreateMapEntryName());

            DustType = DustID.GreenTorch;
            HitSound = SoundID.Shatter;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.GreenBambooWineJar>());
        }
    }
}
