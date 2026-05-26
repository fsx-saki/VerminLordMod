using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShuiFuSleepingBuff : ModBuff
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
            player.velocity.X = 0f;
            player.velocity.Y = 0f;
            player.gravDir = 1f;
            player.controlLeft = false;
            player.controlRight = false;
            player.controlUp = false;
            player.controlDown = false;
            player.controlJump = false;

            player.statLife += 5;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;

            player.GetModPlayer<global::VerminLordMod.Common.Players.QiResourcePlayer>().ExtraQiRegen +=
                player.GetModPlayer<global::VerminLordMod.Common.Players.QiResourcePlayer>().BaseQiRegenRate * 2f;

            if (Main.rand.NextBool(10))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.PurpleTorch, 0, -1f, Scale: 1.2f);
                d.noGravity = true;
            }

            if (player.buffTime[buffIndex] <= 2)
            {
                player.AddBuff(ModContent.BuffType<ShuiFuBuff>(), 300);
            }
        }
    }
}
