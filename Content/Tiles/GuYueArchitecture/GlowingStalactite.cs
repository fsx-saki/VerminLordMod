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
    /// 七彩钟乳石 - 地下溶洞中的发光钟乳石
    /// 原文: "地下溶洞美轮美奂，钟乳石散发着赤橙黄绿青蓝紫七色光华" (gzr_part001.txt:324)
    /// 功能: 装饰性发光物块
    /// </summary>
    public class GlowingStalactite : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Stone, TileID.Mud, TileID.Dirt };
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 100, 255), CreateMapEntryName());

            DustType = DustID.PinkFairy;
            HitSound = SoundID.Shatter;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // 七彩渐变效果
            double time = Main.time * 0.001;
            r = (float)(System.Math.Sin(time) * 0.3 + 0.5);
            g = (float)(System.Math.Sin(time + 2.094) * 0.3 + 0.5);
            b = (float)(System.Math.Sin(time + 4.188) * 0.3 + 0.5);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.GlowingStalactite>());
        }
    }
}
