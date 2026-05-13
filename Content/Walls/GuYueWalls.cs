using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Walls
{
    // ============================================================
    // Walls 大框 — 自定义墙壁
    //
    // 当前项目方块有但墙壁缺失，需要配套墙壁。
    // 所有自定义方块都需要对应的墙壁才能构成完整建筑。
    //
    // TODO:
    //   - QingMaoStoneWall（青茅石墙）
    //   - BoneBanbooWall（骨竹墙）
    //   - GuYueBrownYellowWall（古月棕黄墙）
    //   - 青砖建筑系列墙壁
    //   - 红铜装饰墙壁
    //   - 灵泉装饰墙壁
    // ============================================================

    public class QingMaoStoneWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            DustType = DustID.Stone;
            AddMapEntry(new Microsoft.Xna.Framework.Color(150, 140, 120));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 1;
        }
    }

    public class BoneBanbooWall : ModWall
    {
        public override void SetStaticDefaults()
        {
            DustType = DustID.Bone;
            AddMapEntry(new Microsoft.Xna.Framework.Color(200, 190, 170));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 2 : 1;
        }
    }
}