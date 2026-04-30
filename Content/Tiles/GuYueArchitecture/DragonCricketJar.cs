using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 龙丸蛐蛐罐 - 养龙丸蛐蛐的罐子
    /// 原文: "龙丸蛐蛐，也唱响了窸窸窣窣的生命之歌。它们是夜间才出来活动，身上都散发红光的昆虫" (gzr_part001.txt:1053-1055)
    /// 功能: 装饰性物块，发出微弱的红光
    /// </summary>
    public class DragonCricketJar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateHeights = new[] { 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 80, 80), CreateMapEntryName());

            DustType = DustID.Firework_Red;
            HitSound = SoundID.Shatter;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.05f;
            b = 0.05f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.DragonCricketJar>());
        }
    }
}
