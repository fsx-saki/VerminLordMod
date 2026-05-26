using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class KuXinBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<KuXinPlayer>().HasKuXin = true;
            player.GetDamage(DamageClass.Generic) += 0.12f;
        }
    }

    public class KuXinPlayer : ModPlayer
    {
        public bool HasKuXin { get; set; }
        private int _damageTimer;

        public override void ResetEffects()
        {
            HasKuXin = false;
        }

        public override void PostUpdate()
        {
            if (!HasKuXin)
            {
                _damageTimer = 0;
                return;
            }

            _damageTimer++;

            if (_damageTimer % 60 == 0)
            {
                Player.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), 1, 0);
            }

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Crimson);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
