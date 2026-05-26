using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转金道防御仙蛊", "五转", "金")]
    public class FangBeiXianGu : GuAccessoryItem
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 36;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.accessory = true;
            Item.defense = 19;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            player.endurance += 0.08f;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<FangBeiXianGuPlayer>().FangBeiActive = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FangBeiEffect", "增加12点防御力，减少8%受到的伤害"));
            tooltips.Add(new TooltipLine(Mod, "FangBeiVigilant", "5秒未受伤害时，额外增加5点防御（警觉防御）"));
            tooltips.Add(new TooltipLine(Mod, "FangBeiQiCost", $"占据真元：{qiCost}"));
        }
    }

    public class FangBeiXianGuPlayer : ModPlayer
    {
        public bool FangBeiActive { get; set; }
        private int _noDamageTimer = 0;
        private const int VigilantThreshold = 300;
        private const int ExtraDefense = 5;

        public override void ResetEffects()
        {
            FangBeiActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (!FangBeiActive)
            {
                _noDamageTimer = 0;
                return;
            }

            _noDamageTimer++;

            if (_noDamageTimer >= VigilantThreshold)
            {
                Player.statDefense += ExtraDefense;
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            _noDamageTimer = 0;
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            _noDamageTimer = 0;
        }
    }
}
