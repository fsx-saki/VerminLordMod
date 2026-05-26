using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ChouHenBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.20f;

            if (player.whoAmI == Main.myPlayer)
            {
                ChouHenPlayer chouHenPlayer = player.GetModPlayer<ChouHenPlayer>();
                chouHenPlayer.HasChouHen = true;
            }

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<DarkDust>());
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class ChouHenPlayer : ModPlayer
    {
        public bool HasChouHen { get; set; }
        private int damageTimer = 0;

        public override void ResetEffects()
        {
            HasChouHen = false;
        }

        public override void PostUpdate()
        {
            if (!HasChouHen)
            {
                damageTimer = 0;
                return;
            }

            damageTimer++;
            if (damageTimer >= 60)
            {
                damageTimer = 0;
                Player.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), 2, 0);
            }
        }
    }
}
