using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YiQueBuff : ModBuff
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
            var yiQuePlayer = player.GetModPlayer<YiQuePlayer>();
            yiQuePlayer.HasYiQue = true;

            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 20;
            player.statDefense += (int)(player.statDefense * 0.20f);
            player.moveSpeed += 0.20f;

            switch (yiQuePlayer.FlawType)
            {
                case 1:
                    player.GetDamage(DamageClass.Generic) -= 0.30f;
                    break;
                case 2:
                    player.statDefense -= (int)(player.statDefense * 0.30f);
                    break;
                case 3:
                    player.moveSpeed -= 0.30f;
                    break;
                case 4:
                    player.GetCritChance(DamageClass.Generic) -= 30;
                    break;
            }

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class YiQuePlayer : ModPlayer
    {
        public bool HasYiQue { get; set; }
        public int FlawType { get; set; }

        public override void ResetEffects()
        {
            HasYiQue = false;
            FlawType = 0;
        }
    }
}
