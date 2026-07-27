using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Items
{
    public abstract class GuBaseItem : ModItem
    {
        public virtual int GuLevel => 1;
        public virtual int QiCost => 7;
        public bool IsRefined { get; set; }

        public override void SetDefaults()
        {
            Item.maxStack = 1;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (IsRefined)
                {
                    var yh = player.GetModPlayer<YuanHaiSystem.YuanHaiPlayer>();
                    if (yh != null) { yh.AddGu(Item.type); Item.TurnToAir(); }
                }
                return false;
            }

            if (!IsRefined)
            {
                Main.NewText("该蛊虫尚未炼化，按 U 打开炼化界面", Color.Red);
                return false;
            }

            var qi = player.GetModPlayer<QiResourcePlayer>();
            if (qi == null || qi.QiCurrent < QiCost)
            {
                Main.NewText("真元不足！", Color.OrangeRed);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var qi = player.GetModPlayer<QiResourcePlayer>();
            qi?.ConsumeQi(QiCost);
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tools)
        {
            if (!IsRefined)
                tools.Add(new TooltipLine(Mod, "GuUnrefined", "[c/ff4444:未炼化 - 按 U 打开炼化界面]"));
        }

        public override void SaveData(TagCompound tag) { tag["GuiRefined"] = IsRefined; }
        public override void LoadData(TagCompound tag) { if (tag.TryGet("GuiRefined", out bool r)) IsRefined = r; }
    }
}
