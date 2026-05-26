using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GongPingBuff : ModBuff
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
            player.GetModPlayer<GongPingPlayer>().HasGongPing = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                d.color = new Color(200, 200, 255);
            }
        }
    }

    public class GongPingPlayer : ModPlayer
    {
        public bool HasGongPing { get; set; }
        private const int NormalizedDamage = 50;

        public override void ResetEffects()
        {
            HasGongPing = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasGongPing)
            {
                modifiers.FinalDamage.Flat = NormalizedDamage - modifiers.FinalDamage.Base;
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasGongPing)
            {
                modifiers.FinalDamage.Flat = NormalizedDamage - modifiers.FinalDamage.Base;
            }
        }
    }
}
