using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转成败道辅助蛊", "二转", "成败")]
    public class DuoTaiGu : ModItem
    {
        private const int QiCostPerUse = 8;
        private const int SelfDamage = 50;
        private const int BuffDuration = 300;
        private const float DamageBonus = 0.30f;

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
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动堕胎蛊");
                return false;
            }
            if (player.statLife <= SelfDamage)
            {
                Text.ShowTextRed(player, "生命值过低，无法献祭");
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.Hurt(PlayerDeathReason.LegacyDefault(), SelfDamage, 0);

            player.GetModPlayer<DuoTaiGuPlayer>().DuoTaiActive = true;
            player.GetModPlayer<DuoTaiGuPlayer>().DuoTaiTimer = BuffDuration;

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<Dusts.SuccessFailureDust>(), 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 1.6f);
                dust.color = new Color(180, 40, 40);
            }

            Text.ShowTextRed(player, "堕胎蛊：献祭生命力换取力量！+30%伤害");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DuoTaiDesc", "二转成败道辅助蛊：堕胎"));
            tooltips.Add(new TooltipLine(Mod, "DuoTaiEffect", $"对自身造成{SelfDamage}伤害，换取+30%伤害持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "DuoTaiWarning", "[c/FF4444:警告：以生命力为代价换取力量]"));
            tooltips.Add(new TooltipLine(Mod, "DuoTaiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class DuoTaiGuPlayer : ModPlayer
    {
        public bool DuoTaiActive { get; set; }
        public int DuoTaiTimer { get; set; }

        public override void ResetEffects()
        {
            if (DuoTaiTimer > 0)
            {
                DuoTaiTimer--;
                if (DuoTaiTimer <= 0)
                    DuoTaiActive = false;
            }
            else
            {
                DuoTaiActive = false;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (DuoTaiActive)
            {
                modifiers.FinalDamage *= 1.30f;
            }
        }
    }
}
