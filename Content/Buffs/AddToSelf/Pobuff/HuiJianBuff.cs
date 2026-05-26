using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HuiJianBuff : ModBuff
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
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;

            var huiJianPlayer = player.GetModPlayer<HuiJianPlayer>();
            huiJianPlayer.HasHuiJianBuff = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.SilverFlame);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
            }
        }
    }

    public class HuiJianPlayer : ModPlayer
    {
        public bool HasHuiJianBuff { get; set; }

        public override void ResetEffects()
        {
            HasHuiJianBuff = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasHuiJianBuff)
            {
                modifiers.CritDamage *= 1f;
                modifiers.SetCrit();
                HasHuiJianBuff = false;
                Player.ClearBuff(ModContent.BuffType<HuiJianBuff>());
            }
        }
    }
}
