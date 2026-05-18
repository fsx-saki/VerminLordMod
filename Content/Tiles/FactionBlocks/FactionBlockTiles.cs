using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Tiles.FactionBlocks
{
    /// <summary>
    /// 古月木块 — 古月宗的木制方块
    /// </summary>
        /// <summary>
    /// 古月木块 — 古月宗的木制方块
    /// </summary>
        public class GuYueWoodBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(139, 90, 43), CreateMapEntryName());

            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.GuYueWoodBlock>());
        }
    }

    /// <summary>
    /// 古月竹台 — 古月宗的竹制平台
    /// </summary>
        /// <summary>
    /// 古月竹台 — 古月宗的竹制平台
    /// </summary>
        public class GuYueBambooPlatform : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 140, 60), CreateMapEntryName());

            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.GuYueBambooPlatform>());
        }
    }

    /// <summary>
    /// 白玉方块 — 白玉制成的方块
    /// </summary>
        /// <summary>
    /// 白玉方块 — 白玉制成的方块
    /// </summary>
        public class BaiJadeBlock : ModTile
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

            AddMapEntry(new Color(220, 230, 240), CreateMapEntryName());

            DustType = DustID.Ice;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.12f;
            b = 0.15f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.BaiJadeBlock>());
        }
    }

    /// <summary>
    /// 白银砖 — 白银色的砖块
    /// </summary>
        /// <summary>
    /// 白银砖 — 白银色的砖块
    /// </summary>
        public class BaiSilverBrick : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(190, 195, 210), CreateMapEntryName());

            DustType = DustID.Silver;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.BaiSilverBrick>());
        }
    }

    /// <summary>
    /// 熊家暗石 — 熊家的暗色石块
    /// </summary>
        /// <summary>
    /// 熊家暗石 — 熊家的暗色石块
    /// </summary>
        public class XiongDarkStone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 75, 70), CreateMapEntryName());

            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
            MinPick = 40;
            MineResist = 1.5f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.XiongDarkStone>());
        }
    }

    /// <summary>
    /// 熊家铁梁 — 熊家的铁制梁柱
    /// </summary>
        /// <summary>
    /// 熊家铁梁 — 熊家的铁制梁柱
    /// </summary>
        public class XiongIronBeam : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(100, 100, 110), CreateMapEntryName());

            DustType = DustID.Iron;
            HitSound = SoundID.Tink;
            MinPick = 50;
            MineResist = 2f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.XiongIronBeam>());
        }
    }

    /// <summary>
    /// 铁家锻造石 — 铁家的锻造石
    /// </summary>
        /// <summary>
    /// 铁家锻造石 — 铁家的锻造石
    /// </summary>
        public class TieForgeStone : ModTile
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

            AddMapEntry(new Color(60, 55, 55), CreateMapEntryName());

            DustType = DustID.Ash;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.05f;
            b = 0.02f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.TieForgeStone>());
        }
    }

    /// <summary>
    /// 王家水晶砖 — 王家的水晶砖
    /// </summary>
        /// <summary>
    /// 王家水晶砖 — 王家的水晶砖
    /// </summary>
        public class WangCrystalBlock : ModTile
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

            AddMapEntry(new Color(80, 150, 220), CreateMapEntryName());

            DustType = DustID.CrystalSerpent;
            HitSound = SoundID.Shatter;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.2f;
            b = 0.35f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.WangCrystalBlock>());
        }
    }

    /// <summary>
    /// 赵家影砖 — 赵家的暗影砖
    /// </summary>
        /// <summary>
    /// 赵家影砖 — 赵家的暗影砖
    /// </summary>
        public class ZhaoShadowBrick : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(50, 45, 55), CreateMapEntryName());

            DustType = DustID.Obsidian;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.ZhaoShadowBrick>());
        }
    }

    /// <summary>
    /// 贾家金丝砖 — 贾家的金丝装饰砖
    /// </summary>
        /// <summary>
    /// 贾家金丝砖 — 贾家的金丝装饰砖
    /// </summary>
        public class JiaGoldSilkBlock : ModTile
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

            AddMapEntry(new Color(220, 190, 80), CreateMapEntryName());

            DustType = DustID.Gold;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.12f;
            b = 0.03f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.JiaGoldSilkBlock>());
        }
    }

    /// <summary>
    /// 散修泥砖 — 散修使用的泥砖
    /// </summary>
        /// <summary>
    /// 散修泥砖 — 散修使用的泥砖
    /// </summary>
        public class ScatteredMudBrick : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 130, 90), CreateMapEntryName());

            DustType = DustID.Mud;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.ScatteredMudBrick>());
        }
    }

    /// <summary>
    /// 古月竹墙 — 古月宗的竹墙
    /// </summary>
        /// <summary>
    /// 古月竹墙 — 古月宗的竹墙
    /// </summary>
        public class GuYueBambooWall : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 130, 55), CreateMapEntryName());

            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.GuYueBambooWall>());
        }
    }

    /// <summary>
    /// 白玉柱 — 白玉制成的柱子
    /// </summary>
        /// <summary>
    /// 白玉柱 — 白玉制成的柱子
    /// </summary>
        public class BaiJadePillar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 210, 225), CreateMapEntryName());

            DustType = DustID.Ice;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.08f;
            g = 0.1f;
            b = 0.12f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.BaiJadePillar>());
        }
    }

    /// <summary>
    /// 铁家锻造墙 — 铁家的锻造墙壁
    /// </summary>
        /// <summary>
    /// 铁家锻造墙 — 铁家的锻造墙壁
    /// </summary>
        public class TieForgeWall : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(70, 60, 55), CreateMapEntryName());

            DustType = DustID.Ash;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.04f;
            b = 0.01f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.TieForgeWall>());
        }
    }

    /// <summary>
    /// 王家水晶柱 — 王家的水晶柱
    /// </summary>
        /// <summary>
    /// 王家水晶柱 — 王家的水晶柱
    /// </summary>
        public class WangCrystalPillar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(90, 160, 230), CreateMapEntryName());

            DustType = DustID.CrystalSerpent;
            HitSound = SoundID.Shatter;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.08f;
            g = 0.18f;
            b = 0.3f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.WangCrystalPillar>());
        }
    }

    /// <summary>
    /// 赵家影幕 — 赵家的暗影幕帘
    /// </summary>
        /// <summary>
    /// 赵家影幕 — 赵家的暗影幕帘
    /// </summary>
        public class ZhaoShadowCurtain : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(45, 40, 50), CreateMapEntryName());

            DustType = DustID.Obsidian;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.ZhaoShadowCurtain>());
        }
    }

    /// <summary>
    /// 贾家丝绸幕 — 贾家的丝绸幕帘
    /// </summary>
        /// <summary>
    /// 贾家丝绸幕 — 贾家的丝绸幕帘
    /// </summary>
        public class JiaSilkCurtain : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(210, 180, 70), CreateMapEntryName());

            DustType = DustID.Gold;
            HitSound = SoundID.Tink;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.12f;
            g = 0.1f;
            b = 0.02f;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.FactionBlocks.JiaSilkCurtain>());
        }
    }
}
