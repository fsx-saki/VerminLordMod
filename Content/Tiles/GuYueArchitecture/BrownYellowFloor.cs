using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 棕黄色地板 - 古月山寨宗族祠堂内的棕黄色地板
    /// 原文: "跪在棕黄色的地板上" (gzr_part001.txt:115)
    /// </summary>
    public class BrownYellowFloor : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            AddMapEntry(new Color(180, 140, 80), CreateMapEntryName());
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
