using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Debuff
{
    // ============================================================
    // MiZongBuff — 迷踪阵区域效果Buff大框
    //
    // 功能：在迷踪阵范围内的玩家获得此Buff，
    //       降低NPC对玩家的感知距离。
    //
    // TODO:
    //   - 实现Buff效果（降低NPC感知）
    //   - 完善贴图
    // ============================================================

    public class MiZongBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = true;
            DisplayName.SetDefault("迷踪阵庇护");
            Description.SetDefault("NPC对你的感知距离降低了50%");
        }
    }
}