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
    /// 青矛竹 - 青茅山特有的竹子，笔直如矛，玉质光泽
    /// 原文: "青矛竹挺得笔直，如根根玉竿" (gzr_part001.txt:1018)
    /// 原文: "碧玉的青矛竹在月色下，闪现着莹润如玉的光华" (gzr_part001.txt:1056)
    /// </summary>
    public class SpearBamboo : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileCut[Type] = false;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Grass, TileID.Dirt, TileID.Mud };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 160, 60), CreateMapEntryName());

            DustType = DustID.Grass;
            HitSound = SoundID.Grass;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.SpearBamboo>());
        }
    }
}
