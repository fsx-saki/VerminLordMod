using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.Terrain
{
    /// <summary>
    /// 古石径 — 古老的石板路
    /// </summary>
        /// <summary>
    /// 古石径 — 古老的石板路
    /// </summary>
        public class AncientStonePath : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(140, 135, 120), CreateMapEntryName());

            DustType = DustID.Stone;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.AncientStonePath>());
        }
    }

    /// <summary>
    /// 灵壤 — 灵气充盈的土壤
    /// </summary>
        /// <summary>
    /// 灵壤 — 灵气充盈的土壤
    /// </summary>
        public class SpiritSoil : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 140, 80), CreateMapEntryName());

            DustType = DustID.JungleGrass;
            HitSound = SoundID.Dig;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.03f;
            g = 0.06f;
            b = 0.02f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.SpiritSoil>());
        }
    }

    /// <summary>
    /// 沼泽泥 — 毒沼泽中的淤泥
    /// </summary>
        /// <summary>
    /// 沼泽泥 — 毒沼泽中的淤泥
    /// </summary>
        public class SwampMud : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 70, 50), CreateMapEntryName());

            DustType = DustID.Mud;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.SwampMud>());
        }
    }

    /// <summary>
    /// 荒漠沙 — 荒芜之地的沙土
    /// </summary>
        /// <summary>
    /// 荒漠沙 — 荒芜之地的沙土
    /// </summary>
        public class WastelandSand : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 150, 100), CreateMapEntryName());

            DustType = DustID.Sand;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.WastelandSand>());
        }
    }

    /// <summary>
    /// 寒铁地板 — 寒铁制成的地板，冰冷刺骨
    /// </summary>
        /// <summary>
    /// 寒铁地板 — 寒铁制成的地板，冰冷刺骨
    /// </summary>
        public class ColdIronFloor : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 105, 115), CreateMapEntryName());

            DustType = DustID.Iron;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.ColdIronFloor>());
        }
    }

    /// <summary>
    /// 玉石砖径 — 玉石铺就的小径
    /// </summary>
        /// <summary>
    /// 玉石砖径 — 玉石铺就的小径
    /// </summary>
        public class JadeBrickPath : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 220, 210), CreateMapEntryName());

            DustType = DustID.Ice;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.04f;
            g = 0.06f;
            b = 0.05f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.JadeBrickPath>());
        }
    }

    /// <summary>
    /// 血沙 — 被鲜血浸染的沙土，暗红色
    /// </summary>
        /// <summary>
    /// 血沙 — 被鲜血浸染的沙土，暗红色
    /// </summary>
        public class BloodSand : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 60, 50), CreateMapEntryName());

            DustType = DustID.Crimson;
            HitSound = SoundID.Dig;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.08f;
            g = 0.02f;
            b = 0.01f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.BloodSand>());
        }
    }

    /// <summary>
    /// 暗苔石 — 覆盖暗色苔藓的石头
    /// </summary>
        /// <summary>
    /// 暗苔石 — 覆盖暗色苔藓的石头
    /// </summary>
        public class DarkMossStone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(60, 70, 55), CreateMapEntryName());

            DustType = DustID.Stone;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Terrain.DarkMossStone>());
        }
    }
}
