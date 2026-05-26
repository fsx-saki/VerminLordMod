using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转成败道辅助蛊", "二转", "成败")]
    public class JiangRuGuGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int BuffDuration = 480;
        private const float HpLossRatio = 0.20f;
        private const float CritBonus = 0.25f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            if (player.statLife <= 1)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int hpLoss = (int)(player.statLife * HpLossRatio);
            player.statLife -= hpLoss;
            if (player.statLife < 1)
                player.statLife = 1;

            player.GetCritChance(DamageClass.Generic) += CritBonus * 100;
            player.AddBuff(ModContent.BuffType<JiangRuGuBuff>(), BuffDuration);

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
                d.noGravity = true;
                d.velocity = new Microsoft.Xna.Framework.Vector2(
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-3f, -1f));
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 50, 50), $"-{hpLoss}HP");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiangRuEffect", "降入蛊蛊：失去20%当前生命值，获得25%暴击率"));
            tooltips.Add(new TooltipLine(Mod, "JiangRuDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "JiangRuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class JiangRuGuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetCritChance(DamageClass.Generic) += 25f;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
                d.noGravity = true;
                d.velocity *= 0.1f;
                d.scale = Main.rand.NextFloat(0.5f, 0.8f);
            }
        }
    }
}
