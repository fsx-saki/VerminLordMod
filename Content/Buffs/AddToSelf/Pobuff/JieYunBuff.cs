using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JieYunBuff : ModBuff
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
            player.GetModPlayer<JieYunPlayer>().HasJieYun = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class JieYunPlayer : ModPlayer
    {
        public bool HasJieYun { get; set; }
        private const float ConvertChance = 0.25f;

        public override void ResetEffects()
        {
            HasJieYun = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!HasJieYun)
                return;

            if (Main.rand.NextFloat() < ConvertChance)
            {
                int healAmount = info.Damage;
                Player.statLife += healAmount;
                if (Player.statLife > Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;

                CombatText.NewText(Player.Hitbox, new Color(100, 255, 100), $"+{healAmount}", true);

                for (int i = 0; i < 8; i++)
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.PurpleTorch);
                    d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.0f, 1.5f);
                }
            }
        }
    }
}
