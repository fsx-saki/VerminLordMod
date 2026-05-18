using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.Resources
{
    /// <summary>
    /// 魂银矿石 — 散发幽光的银色矿石，含有魂道道痕
    /// </summary>
    public class 魂银矿石 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 160, 200), CreateMapEntryName());

            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
            MinPick = 30;
            MineResist = 2f;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.25f;
            g = 0.25f;
            b = 0.31f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.魂银矿石>());
        }
    }
}
