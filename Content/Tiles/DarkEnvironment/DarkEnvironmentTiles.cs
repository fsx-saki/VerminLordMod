using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Tiles.DarkEnvironment
{
    /// <summary>
    /// 尸块 — 尸体矿化形成的方块
    /// </summary>
        /// <summary>
    /// 尸块 — 尸体矿化形成的方块
    /// </summary>
        public class CorpseBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(140, 110, 100), CreateMapEntryName());

            DustType = DustID.Ash;
            HitSound = SoundID.NPCHit2;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.CorpseBlock>());
        }
    }

    /// <summary>
    /// 蛊尸块 — 蛊虫与尸体融合的方块
    /// </summary>
        /// <summary>
    /// 蛊尸块 — 蛊虫与尸体融合的方块
    /// </summary>
        public class GuCorpseBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 50, 40), CreateMapEntryName());

            DustType = DustID.CorruptGibs;
            HitSound = SoundID.NPCHit2;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.GuCorpseBlock>());
        }
    }

    /// <summary>
    /// 骨块 — 骨骼制成的方块
    /// </summary>
        /// <summary>
    /// 骨块 — 骨骼制成的方块
    /// </summary>
        public class BoneBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 195, 180), CreateMapEntryName());

            DustType = DustID.Bone;
            HitSound = SoundID.NPCHit2;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.BoneBlock>());
        }
    }

    /// <summary>
    /// 血石 — 血液凝结形成的石头
    /// </summary>
        /// <summary>
    /// 血石 — 血液凝结形成的石头
    /// </summary>
        public class BloodStone : ModTile
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

            AddMapEntry(new Color(160, 30, 30), CreateMapEntryName());

            DustType = DustID.Crimson;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.02f;
            b = 0.02f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.BloodStone>());
        }
    }

    /// <summary>
    /// 腐木 — 腐烂的木材
    /// </summary>
        /// <summary>
    /// 腐木 — 腐烂的木材
    /// </summary>
        public class RotWood : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(90, 70, 40), CreateMapEntryName());

            DustType = DustID.Ash;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.RotWood>());
        }
    }

    /// <summary>
    /// 毒晶 — 毒素结晶体
    /// </summary>
        /// <summary>
    /// 毒晶 — 毒素结晶体
    /// </summary>
        public class PoisonCrystal : ModTile
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

            AddMapEntry(new Color(50, 150, 80), CreateMapEntryName());

            DustType = DustID.Poisoned;
            HitSound = SoundID.Shatter;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.05f;
            g = 0.2f;
            b = 0.1f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.PoisonCrystal>());
        }
    }

    /// <summary>
    /// 魂残块 — 灵魂残余凝结的方块
    /// </summary>
        /// <summary>
    /// 魂残块 — 灵魂残余凝结的方块
    /// </summary>
        public class SoulRemnantBlock : ModTile
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

            AddMapEntry(new Color(100, 100, 130), CreateMapEntryName());

            DustType = DustID.Ghost;
            HitSound = SoundID.NPCHit5;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.1f;
            b = 0.2f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.SoulRemnantBlock>());
        }
    }

    /// <summary>
    /// 甲壳块 — 蛊虫甲壳制成的方块
    /// </summary>
        /// <summary>
    /// 甲壳块 — 蛊虫甲壳制成的方块
    /// </summary>
        public class ChitinShell : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 80, 50), CreateMapEntryName());

            DustType = DustID.CorruptGibs;
            HitSound = SoundID.NPCHit1;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.ChitinShell>());
        }
    }

    /// <summary>
    /// 蛊丝网 — 蛊虫吐丝结成的网
    /// </summary>
        /// <summary>
    /// 蛊丝网 — 蛊虫吐丝结成的网
    /// </summary>
        public class GuSilkWeb : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());

            DustType = DustID.Web;
            HitSound = SoundID.Grass;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.GuSilkWeb>());
        }
    }

    /// <summary>
    /// 诅咒之土 — 被诅咒的土地
    /// </summary>
        /// <summary>
    /// 诅咒之土 — 被诅咒的土地
    /// </summary>
        public class CursedEarth : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(60, 40, 50), CreateMapEntryName());

            DustType = DustID.Corruption;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.DarkEnvironment.CursedEarth>());
        }
    }
}
