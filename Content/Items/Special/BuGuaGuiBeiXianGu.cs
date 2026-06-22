using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转智道防御仙蛊", "五转", "智")]
    class BuGuaGuiBeiXianGu : GuBaseItem
    {
        protected override int qiCost => 36;
        protected override int _guLevel => 5;

        public new static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRate { get; private set; }
        public static LocalizedText GuLevel { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRate = this.GetLocalization("ControlRate");
            GuLevel = this.GetLocalization("GuLevel");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
            }
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.accessory = true;
            Item.defense = 16;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void OnActiveTick(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxCurrent -= qiCost;
            player.GetCritChance(DamageClass.Generic) += 0.08f;
            player.GetModPlayer<BuGuaDodgePlayer>().HasBuGuaEquipped = true;
        }
    }

    class BuGuaDodgePlayer : ModPlayer
    {
        public bool HasBuGuaEquipped { get; set; }

        public override void ResetEffects()
        {
            HasBuGuaEquipped = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasBuGuaEquipped && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
                Player.immuneTime = (int)(60);
                CombatText.NewText(Player.Hitbox, new Microsoft.Xna.Framework.Color(100, 200, 255), "卜卦闪避!", true);
            }
        }
    }
}
