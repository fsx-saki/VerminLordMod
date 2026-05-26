using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TuoYuBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.statDefense += 10;
            player.GetModPlayer<TuoYuPlayer>().HasTuoYu = true;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<SpaceDust>());
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class TuoYuPlayer : ModPlayer
    {
        public bool HasTuoYu { get; set; }

        public override void ResetEffects()
        {
            HasTuoYu = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTuoYu)
                return;

            Player.moveSpeed += 0.05f;
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.ExtraQiRegen += qiResource.BaseQiRegenRate * 0.10f;
        }

        public override void UpdateBadLifeRegen()
        {
            if (HasTuoYu)
            {
                Player.buffImmune[BuffID.Suffocation] = true;
                Player.buffImmune[BuffID.Chilled] = true;
            }
        }
    }
}
