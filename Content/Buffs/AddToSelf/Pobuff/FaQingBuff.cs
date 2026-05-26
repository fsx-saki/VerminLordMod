using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FaQingBuff : ModBuff
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
            player.GetModPlayer<FaQingBuffPlayer>().FaQingActive = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<Dusts.CharmDust>());
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                d.color = new Color(255, 105, 180);
            }
        }
    }

    public class FaQingBuffPlayer : ModPlayer
    {
        public bool FaQingActive { get; set; }

        public override void ResetEffects()
        {
            FaQingActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (!FaQingActive)
                return;

            Player.moveSpeed += 0.15f;
            Player.GetCritChance(DamageClass.Generic) += 5f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (FaQingActive)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }

        public override void UpdateDead()
        {
            FaQingActive = false;
        }
    }
}
