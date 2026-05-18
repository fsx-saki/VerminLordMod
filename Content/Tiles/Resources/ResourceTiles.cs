using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.Resources
{        /// <summary>
    /// 元石矿 — 蕴含灵气的元石矿石，是蛊师修炼的基础资源
    /// </summary>
        public class YuanStoneOre : ModTile
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

            AddMapEntry(new Color(80, 160, 240), CreateMapEntryName());

            DustType = DustID.MagicMirror;
            HitSound = SoundID.Tink;
            MinPick = 50;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.2f;
            b = 0.5f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.YuanStoneOre>());
        }
    }        /// <summary>
    /// 灵脉石 — 灵脉中凝结的矿石，灵气含量极高
    /// </summary>
        public class SpiritVeinStone : ModTile
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

            AddMapEntry(new Color(180, 100, 255), CreateMapEntryName());

            DustType = DustID.PurpleTorch;
            HitSound = SoundID.Tink;
            MinPick = 80;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.2f;
            g = 0.08f;
            b = 0.35f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.SpiritVeinStone>());
        }
    }        /// <summary>
    /// 药园土壤 — 适合种植灵草的特殊土壤
    /// </summary>
        public class HerbGardenSoil : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(90, 130, 60), CreateMapEntryName());

            DustType = DustID.JungleGrass;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.HerbGardenSoil>());
        }
    }        /// <summary>
    /// 毒晶矿石 — 毒素结晶形成的矿石，危险但珍贵
    /// </summary>
        public class PoisonCrystalOre : ModTile
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

            AddMapEntry(new Color(80, 200, 60), CreateMapEntryName());

            DustType = DustID.Poisoned;
            HitSound = SoundID.Tink;
            MinPick = 60;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.08f;
            g = 0.3f;
            b = 0.05f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.PoisonCrystalOre>());
        }
    }        /// <summary>
    /// 魂精矿石 — 蕴含魂力的矿石，散发幽蓝光芒
    /// </summary>
        public class SoulEssenceOre : ModTile
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

            AddMapEntry(new Color(200, 200, 255), CreateMapEntryName());

            DustType = DustID.BlueTorch;
            HitSound = SoundID.Tink;
            MinPick = 100;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.2f;
            g = 0.2f;
            b = 0.4f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.SoulEssenceOre>());
        }
    }        /// <summary>
    /// 赤铜矿脉 — 赤铜矿的矿脉形态
    /// </summary>
        public class ResourceRedCopperOre : ModTile
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

            AddMapEntry(new Color(200, 100, 50), CreateMapEntryName());

            DustType = DustID.Copper;
            HitSound = SoundID.Tink;
            MinPick = 40;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.06f;
            b = 0.02f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.ResourceRedCopperOre>());
        }
    }        /// <summary>
    /// 暗铁矿石 — 暗属性铁矿石，用于锻造暗系蛊器
    /// </summary>
        public class DarkIronOre : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(60, 55, 65), CreateMapEntryName());

            DustType = DustID.Obsidian;
            HitSound = SoundID.Tink;
            MinPick = 70;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.DarkIronOre>());
        }
    }        /// <summary>
    /// 灵骨矿石 — 灵兽骨骼矿化后的矿石
    /// </summary>
        public class SpiritBoneOre : ModTile
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

            AddMapEntry(new Color(220, 210, 190), CreateMapEntryName());

            DustType = DustID.WhiteTorch;
            HitSound = SoundID.Dig;
            MinPick = 90;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.14f;
            b = 0.1f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.SpiritBoneOre>());
        }
    }        /// <summary>
    /// 蛊卵簇 — 蛊虫卵聚集的矿脉，可能孵化出蛊虫
    /// </summary>
        public class GuEggCluster : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 160, 100), CreateMapEntryName());

            DustType = DustID.Hive;
            HitSound = SoundID.NPCHit1;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.12f;
            g = 0.1f;
            b = 0.04f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Resources.GuEggCluster>());
        }
    }
}
