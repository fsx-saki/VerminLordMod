using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class LangTaiBuff : ModBuff
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
            player.statDefense += 10;
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.GetModPlayer<LangTaiPlayer>().HasLangTai = true;

            if (Main.rand.NextBool(10))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.BrownMoss);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class LangTaiPlayer : ModPlayer
    {
        public bool HasLangTai { get; set; }
        private int healTimer;

        public override void ResetEffects()
        {
            HasLangTai = false;
        }

        public override void PostUpdate()
        {
            if (!HasLangTai)
            {
                healTimer = 0;
                return;
            }

            healTimer++;
            if (healTimer >= 30)
            {
                healTimer = 0;
                if (Player.statLife < Player.statLifeMax2)
                {
                    Player.Heal(2);
                }
            }
        }
    }
}
