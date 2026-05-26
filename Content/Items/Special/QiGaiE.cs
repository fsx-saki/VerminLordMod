using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "储道辅助蛊-真元储存", "二转", "储")]
    public class QiGaiE : ModItem
    {
        private float _storedQi;
        private int _guLevel = 2;
        private float MaxStoredQi => 100 * _guLevel;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            return _storedQi > 0;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            if (_storedQi <= 0)
            {
                Text.ShowTextRed(player, "乞丐蛾内无储存的真元");
                return false;
            }

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            float toRelease = _storedQi;
            float canAbsorb = qiResource.QiMaxCurrent - qiResource.QiCurrent;
            float actual = System.Math.Min(toRelease, canAbsorb);

            qiResource.RefundQi(actual);
            _storedQi -= actual;

            if (actual < toRelease)
            {
                _storedQi += (toRelease - actual);
            }

            Text.ShowTextGreen(player, $"乞丐蛾释放真元: {(int)actual}，剩余储存: {(int)_storedQi}/{(int)MaxStoredQi}");
            return true;
        }

        public override void RightClick(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            float availableQi = qiResource.QiCurrent;
            float canStore = MaxStoredQi - _storedQi;

            if (canStore <= 0)
            {
                Text.ShowTextRed(player, "乞丐蛾已满，无法继续储存真元");
                return;
            }

            if (availableQi <= 0)
            {
                Text.ShowTextRed(player, "当前真元不足，无法储存");
                return;
            }

            float toStore = System.Math.Min(availableQi, canStore);
            qiResource.ConsumeQi(toStore);
            _storedQi += toStore;

            Text.ShowTextGreen(player, $"乞丐蛾储存真元: {(int)toStore}，当前储存: {(int)_storedQi}/{(int)MaxStoredQi}");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "QiGaiEDesc", "储道辅助蛊：可储存真元，如同真元电池"));
            tooltips.Add(new TooltipLine(Mod, "QiGaiEStore", $"[c/BB5BC9:储存真元: {(int)_storedQi}/{(int)MaxStoredQi}]"));
            tooltips.Add(new TooltipLine(Mod, "QiGaiECapacity", $"[c/BB5BC9:容量: {(int)MaxStoredQi} (100 × 转数)]"));
            tooltips.Add(new TooltipLine(Mod, "QiGaiELeftClick", "左键：释放储存的真元"));
            tooltips.Add(new TooltipLine(Mod, "QiGaiERightClick", "右键：将当前真元存入乞丐蛾"));
        }

        public override void SaveData(TagCompound tag)
        {
            tag["storedQi"] = _storedQi;
            tag["guLevel"] = _guLevel;
        }

        public override void LoadData(TagCompound tag)
        {
            _storedQi = tag.GetFloat("storedQi");
            _guLevel = tag.GetInt("guLevel");
            if (_guLevel <= 0)
                _guLevel = 2;
        }
    }
}
