using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.Items;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 仙蛊屋玩家数据 — 管理当前激活的仙蛊屋（单槽位法器模式）。
    /// 玩家同一时间只能激活一个仙蛊屋。
    /// </summary>
    public class GuHousePlayer : ModPlayer
    {
        /// <summary>当前激活的仙蛊屋物品（IsAir = 无激活）</summary>
        public Item ActiveHouse = new Item();

        /// <summary>是否有激活的仙蛊屋</summary>
        public bool IsActive => !ActiveHouse.IsAir;

        /// <summary>持续消耗计时器（每秒扣一次）</summary>
        public int SustainTimer = 0;

        /// <summary>
        /// 尝试激活仙蛊屋。若已有激活的，先停用旧的。
        /// </summary>
        public bool TryActivate(Item houseItem)
        {
            if (IsActive)
            {
                // 已有激活的，先停用
                Deactivate();
            }

            if (houseItem.ModItem is GuHouseItem gh)
            {
                ActiveHouse = houseItem.Clone();
                gh.OnActivate(Player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 停用当前仙蛊屋。
        /// </summary>
        public void Deactivate()
        {
            if (IsActive && ActiveHouse.ModItem is GuHouseItem gh)
            {
                gh.OnDeactivate(Player);
            }
            ActiveHouse.TurnToAir();
            SustainTimer = 0;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["ActiveHouse"] = ActiveHouse;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("ActiveHouse"))
                ActiveHouse = tag.Get<Item>("ActiveHouse") ?? new Item();
            else
                ActiveHouse = new Item();
            SustainTimer = 0;
        }
    }
}
