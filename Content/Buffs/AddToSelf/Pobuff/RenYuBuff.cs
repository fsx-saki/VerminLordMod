using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RenYuBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.statDefense += (int)(player.statDefense * 0.08f);
            player.GetModPlayer<RenYuPlayer>().HasRenYu = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<PersonDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class RenYuPlayer : ModPlayer
    {
        public bool HasRenYu { get; set; }
        private int _healTimer;

        public override void ResetEffects()
        {
            HasRenYu = false;
        }

        public override void PostUpdate()
        {
            if (!HasRenYu)
            {
                _healTimer = 0;
                return;
            }

            _healTimer++;
            if (_healTimer >= 60)
            {
                _healTimer = 0;
                if (Player.statLife < Player.statLifeMax2)
                {
                    Player.statLife += 1;
                    Player.HealEffect(1);
                }
            }
        }
    }
}
