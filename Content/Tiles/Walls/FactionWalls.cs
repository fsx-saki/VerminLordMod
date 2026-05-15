using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Tiles.Walls
{
    public class GuYueWoodWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(110, 75, 35));
            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.GuYueWoodWall>());
        }
    }

    public class BaiJadeWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(190, 200, 210));
            DustType = DustID.Ice;
            HitSound = SoundID.Tink;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.BaiJadeWall>());
        }
    }

    public class XiongStoneWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(90, 85, 80));
            DustType = DustID.Stone;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.XiongStoneWall>());
        }
    }

    public class TieIronWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(70, 70, 75));
            DustType = DustID.Iron;
            HitSound = SoundID.Tink;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.TieIronWall>());
        }
    }

    public class WangCrystalWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(100, 140, 200));
            DustType = DustID.CrystalSerpent;
            HitSound = SoundID.Shatter;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.WangCrystalWall>());
        }
    }

    public class ZhaoShadowWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(45, 40, 50));
            DustType = DustID.Obsidian;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.ZhaoShadowWall>());
        }
    }

    public class JiaGoldSilkWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(170, 140, 50));
            DustType = DustID.Gold;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.JiaGoldSilkWall>());
        }
    }

    public class ScatteredMudWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(100, 80, 55));
            DustType = DustID.Mud;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.ScatteredMudWall>());
        }
    }

    public class BoneWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(180, 175, 170));
            DustType = DustID.WhiteTorch;
            HitSound = SoundID.Dig;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.BoneWall>());
        }
    }

    public class FleshWallUnsafe : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(140, 60, 60));
            DustType = DustID.Crimson;
            HitSound = SoundID.NPCHit2;
        }

        public override void KillWall(int i, int j, ref bool fail)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.Placeable.Walls.FleshWallItem>());
        }
    }
}
