using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TeYiBuff : ModBuff
    {
        public enum MutationType
        {
            None = 0,
            Special = 1,
            Variable = 2,
            Sharp = 3
        }

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
            var teYiPlayer = player.GetModPlayer<TeYiPlayer>();

            switch (teYiPlayer.CurrentMutation)
            {
                case MutationType.Special:
                    player.GetDamage(DamageClass.Generic) += 0.20f;
                    break;
                case MutationType.Variable:
                    player.statDefense += (int)(player.statDefense * 0.20f);
                    break;
                case MutationType.Sharp:
                    player.GetCritChance(DamageClass.Generic) += 20f;
                    break;
            }

            if (Main.rand.NextBool(5))
            {
                int dustType = teYiPlayer.CurrentMutation switch
                {
                    MutationType.Special => 261,
                    MutationType.Variable => 176,
                    MutationType.Sharp => 261,
                    _ => 261
                };
                var d = Dust.NewDustDirect(player.position, player.width, player.height, dustType);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class TeYiPlayer : ModPlayer
    {
        public TeYiBuff.MutationType CurrentMutation { get; set; }

        public override void ResetEffects()
        {
            CurrentMutation = TeYiBuff.MutationType.None;
        }
    }
}
