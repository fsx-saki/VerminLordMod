using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TianYuanBuff : ModBuff
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
            player.GetModPlayer<TianYuanPlayer>().HasTianYuanBuff = true;

            for (int i = 0; i < 3; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.15f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
                d.color = new Color(255, 200, 50);
            }

            if (player.buffTime[buffIndex] <= 2)
            {
                int exhaustBuff = ModContent.BuffType<TianYuanExhaustBuff>();
                player.AddBuff(exhaustBuff, 300);
                Text.ShowTextRed(player, "天元宝皇炼仙之力消散...反噬降临！");
            }
        }
    }

    public class TianYuanExhaustBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) -= 0.20f;
            player.moveSpeed *= 0.7f;
            player.maxRunSpeed *= 0.7f;
            player.statDefense -= 10;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.1f;
                d.noGravity = true;
                d.scale = 0.5f;
                d.alpha = 150;
            }
        }
    }

    public class TianYuanPlayer : ModPlayer
    {
        public bool HasTianYuanBuff { get; set; }
        public int CooldownTimer { get; set; }
        public bool IsOnCooldown => CooldownTimer > 0;

        public override void ResetEffects()
        {
            HasTianYuanBuff = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTianYuanBuff)
                return;

            Player.GetDamage(DamageClass.Generic) += 0.15f;
            Player.statDefense += (int)(Player.statDefense * 0.15f);
            Player.GetCritChance(DamageClass.Generic) += 15f;
            Player.moveSpeed *= 1.15f;

            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.ExtraQiRegen += 0.15f * qiResource.BaseQiRegenRate;
        }

        public override void PostUpdate()
        {
            if (CooldownTimer > 0)
                CooldownTimer--;
        }
    }
}
