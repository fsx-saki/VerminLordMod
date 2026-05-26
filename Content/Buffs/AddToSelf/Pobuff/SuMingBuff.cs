using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class SuMingBuff : ModBuff
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
            player.GetModPlayer<SuMingPlayer>().HasSuMingProtection = true;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.color = new Color(180, 100, 255);
            }
        }
    }

    public class SuMingPlayer : ModPlayer
    {
        public bool HasSuMingProtection { get; set; }

        public override void ResetEffects()
        {
            HasSuMingProtection = false;
        }

        public override void PostUpdate()
        {
            if (!HasSuMingProtection)
                return;

            if (Player.statLife < 1)
            {
                Player.statLife = 1;
            }

            if (Player.dead && Player.statLife > 0)
            {
                Player.dead = false;
            }
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (HasSuMingProtection)
            {
                Player.statLife = 1;
                Player.dead = false;
                Player.immune = true;
                Player.immuneTime = 30;
                Player.SetImmuneTimeForAllTypes(30);
                return false;
            }
            return true;
        }
    }

    public class SuMingBuffEndSystem : ModPlayer
    {
        private int suMingBuffType = -1;

        public override void PreUpdateBuffs()
        {
            if (suMingBuffType == -1)
                suMingBuffType = ModContent.BuffType<SuMingBuff>();

            int buffIndex = Player.FindBuffIndex(suMingBuffType);
            if (buffIndex != -1 && Player.buffTime[buffIndex] == 1)
            {
                Player.AddBuff(BuffID.Weak, 600);
            }
        }
    }
}
