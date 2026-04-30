using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 灵泉 - 产出元石的灵泉，古月一族的修行资源
    /// 原文: "我族有灵泉产出元石" (gzr_part001.txt:1189)
    /// 功能: 装饰性水源物块，发出微光
    /// </summary>
    public class SpiritSpring : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 200, 255), CreateMapEntryName());

            DustType = DustID.MagicMirror;
            HitSound = SoundID.Splash;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.2f;
            g = 0.5f;
            b = 0.8f;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.gamePaused || !Main.instance.IsActive)
                return;

            Tile tile = Main.tile[i, j];
            if (!TileDrawing.IsVisible(tile))
                return;

            if (tile.TileFrameX == 0 && tile.TileFrameY == 0 && Main.rand.NextBool(5))
            {
                Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 4), 4, 4, DustID.MagicMirror, 0f, -0.3f, 50, default, 0.8f);
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.SpiritSpring>());
        }
    }
}
