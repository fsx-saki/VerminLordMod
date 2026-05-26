using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShangHenBuff : ModBuff
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
            player.GetModPlayer<ShangHenPlayer>().HasShangHen = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<LifeDeathDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class ShangHenPlayer : ModPlayer
    {
        public bool HasShangHen { get; set; }
        private const float DamageBonusPer10Percent = 0.05f;
        private const float MaxDamageBonus = 0.45f;

        public override void ResetEffects()
        {
            HasShangHen = false;
        }

        public override void PostUpdate()
        {
            if (!HasShangHen)
                return;

            float hpPercent = (float)Player.statLife / Player.statLifeMax2;
            float missingPercent = 1f - hpPercent;
            float bonusPer10 = (int)(missingPercent * 10f) * DamageBonusPer10Percent;
            float damageBonus = System.Math.Min(bonusPer10, MaxDamageBonus);

            Player.GetDamage(DamageClass.Generic) += damageBonus;
        }
    }
}
