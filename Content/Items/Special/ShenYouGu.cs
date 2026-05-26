using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转梦道功能蛊", "三转", "梦")]
    public class ShenYouGu : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int SpiritDuration = 600;
        private const int SpiritDamage = 30;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 15000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<ShenYouProj>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<ShenYouProj>() && Main.projectile[i].owner == player.whoAmI)
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

            int buffType = ModContent.BuffType<ShenYouImmobilizeBuff>();
            player.AddBuff(buffType, SpiritDuration);

            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                Main.MouseWorld,
                Vector2.Zero,
                ModContent.ProjectileType<ShenYouProj>(),
                SpiritDamage,
                0f,
                player.whoAmI
            );

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ShenYouEffect", "神游出窍：释放灵体跟随鼠标，触碰敌人造成30点伤害"));
            tooltips.Add(new TooltipLine(Mod, "ShenYouDuration", $"灵体持续：{SpiritDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "ShenYouImmobilize", "期间本体无法移动"));
            tooltips.Add(new TooltipLine(Mod, "ShenYouQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class ShenYouImmobilizeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
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
            player.velocity = Vector2.Zero;
            player.GetModPlayer<ShenYouPlayer>().IsImmobilized = true;

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, 261);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = 1.2f;
            }
        }
    }

    public class ShenYouPlayer : ModPlayer
    {
        public bool IsImmobilized { get; set; }

        public override void ResetEffects()
        {
            IsImmobilized = false;
        }

        public override void PreUpdateMovement()
        {
            if (IsImmobilized)
            {
                Player.velocity = Vector2.Zero;
                Player.maxRunSpeed = 0f;
                Player.runAcceleration = 0f;
            }
        }
    }
}
