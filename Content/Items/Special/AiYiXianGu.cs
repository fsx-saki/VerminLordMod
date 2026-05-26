using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转人道仙蛊", "五转", "人")]
    public class AiYiXianGu : ModItem
    {
        private const int QiCostPerUse = 40;
        private const int BuffDuration = 600;
        private const int AllyRange = 300;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.Heal(80);
            player.AddBuff(BuffID.Regeneration, BuffDuration);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || i == player.whoAmI || other.dead)
                    continue;
                float dist = Vector2.Distance(player.Center, other.Center);
                if (dist <= AllyRange)
                {
                    other.AddBuff(BuffID.Regeneration, BuffDuration);
                    other.GetModPlayer<AiYiAllyPlayer>().HasAiYiBuff = true;
                }
            }

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<LoveDust>());
                d.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "AiYiEffect", "爱意仙蛊：回复80HP，获得生命回复，附近友方+10%伤害"));
            tooltips.Add(new TooltipLine(Mod, "AiYiDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "AiYiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class AiYiAllyPlayer : ModPlayer
    {
        public bool HasAiYiBuff { get; set; }

        public override void ResetEffects()
        {
            HasAiYiBuff = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasAiYiBuff)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasAiYiBuff)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }
    }
}
