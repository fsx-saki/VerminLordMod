using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZuoMengBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.05f;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ExtraQiRegen += qiResource.BaseQiRegenRate * 0.05f;

            Lighting.AddLight(player.Center, new Vector3(0.15f, 0.1f, 0.3f));

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                d.velocity *= 0.15f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }
}
