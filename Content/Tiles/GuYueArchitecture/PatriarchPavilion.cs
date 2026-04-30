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
    /// 家主阁 - 古月山寨中央的权力中枢，高达五层，飞檐翘角
    /// 原文: "家主阁就处在山寨的正中央，高达五层，飞檐翘角，重兵把守" (gzr_part001.txt:319-320)
    /// 原文: "阁前就是广场，阁内供奉着古月先人的牌位" (gzr_part001.txt:320)
    /// 功能: 大型装饰建筑
    /// </summary>
    public class PatriarchPavilion : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.Origin = new Point16(2, 4);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 80, 40), Language.GetText("Mods.VerminLordMod.MapObject.PatriarchPavilion"));

            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 80, 80,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.PatriarchPavilion>());
        }
    }
}
