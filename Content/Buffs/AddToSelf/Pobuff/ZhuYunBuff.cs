using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZhuYunBuff : ModBuff
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
            player.GetModPlayer<ZhuYunPlayer>().ZhuYunActive = true;

            int dustId = player.GetModPlayer<ZhuYunPlayer>().FortuneType switch
            {
                0 => DustID.GoldFlame,
                1 => DustID.BlueFairy,
                2 => DustID.YellowStarDust,
                _ => DustID.RainbowMk2
            };

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, dustId);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class ZhuYunPlayer : ModPlayer
    {
        public bool ZhuYunActive { get; set; }
        public int FortuneType { get; set; }

        public override void ResetEffects()
        {
            ZhuYunActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (!ZhuYunActive)
                return;

            switch (FortuneType)
            {
                case 0:
                    Player.GetDamage(DamageClass.Generic) += 0.20f;
                    break;
                case 1:
                    Player.statDefense += (int)(Player.statDefense * 0.20f);
                    break;
                case 2:
                    Player.GetCritChance(DamageClass.Generic) += 20f;
                    break;
                case 3:
                    Player.GetDamage(DamageClass.Generic) += 0.08f;
                    Player.statDefense += (int)(Player.statDefense * 0.08f);
                    Player.GetCritChance(DamageClass.Generic) += 8f;
                    break;
            }
        }

        public override void UpdateDead()
        {
            ZhuYunActive = false;
        }
    }
}
