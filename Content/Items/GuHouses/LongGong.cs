using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.ImplementationTracker;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.GuHouses
{
    /// <summary>
    /// 龙宫 (LongGong)
    /// 八转仙蛊屋，以水道仙蛊为核心构建，气势恢宏，可召唤水龙攻击。
    /// 具备强大的水道攻击和防御能力。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "龙宫 — 八转仙蛊屋", "八转", "水")]
    public class LongGong : GuHouseItem
    {
        public override int HouseLevel => 8;
        public override GuHouseType HouseType => GuHouseType.Combat;
        public override int ActivationQiCost => 900;
        public override int SustainQiCostPerSecond => 35;
        public override float Range => 900f;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Cyan;
            Item.value = 2000000;
            Item.damage = 500;
            Item.knockBack = 15f;
        }

        public override void OnActivate(Player player)
        {
            base.OnActivate(player);
            player.AddBuff(BuffID.Wet, 600);
            player.AddBuff(BuffID.IceBarrier, 600);
            Main.NewText("龙宫降临！万水汇聚，水龙腾空！", Color.DeepSkyBlue);
        }
    }
}
