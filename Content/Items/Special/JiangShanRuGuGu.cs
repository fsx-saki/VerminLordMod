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
    [ImplStatus(ImplStatus.Implemented, "三转人道辅助蛊", "三转", "人")]
    public class JiangShanRuGuGu : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int CooldownFrames = 1800;
        private const int HealAmount = 50;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 20000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            if (player.HasBuff(ModContent.BuffType<JiangShanRuGuCDBuff>()))
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.Heal(HealAmount);

            for (int i = 0; i < 323; i++)
            {
                if (Main.debuff[i] && player.buffTime[i] > 0)
                {
                    if (!BuffID.Sets.NurseCannotRemoveDebuff[i])
                    {
                        player.buffTime[i] = 0;
                    }
                }
            }

            player.AddBuff(ModContent.BuffType<JiangShanRuGuCDBuff>(), CooldownFrames);

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PortalBoltTrail);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                d.velocity = new Microsoft.Xna.Framework.Vector2(
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-3f, -1f));
            }

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(200, 180, 100), "江山如故！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiangShanRuGuEffect", $"恢复{HealAmount}点生命值，清除所有可移除的减益效果"));
            tooltips.Add(new TooltipLine(Mod, "JiangShanRuGuCD", $"冷却：{CooldownFrames / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "JiangShanRuGuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class JiangShanRuGuCDBuff : ModBuff
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
        }
    }
}
