using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JiGuiYunBuff : ModBuff
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
            player.GetModPlayer<JiGuiYunPlayer>().HasJiGuiYun = true;

            player.maxRunSpeed += 8f;
            player.accRunSpeed += 8f;
            player.moveSpeed += 0.5f;

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Cloud);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                d.alpha = 100;
            }
        }
    }

    public class JiGuiYunPlayer : ModPlayer
    {
        public bool HasJiGuiYun { get; set; }

        public override void ResetEffects()
        {
            HasJiGuiYun = false;
        }

        public override void PostUpdate()
        {
            if (HasJiGuiYun)
            {
                Player.rocketTimeMax = 600;
                Player.rocketBoots = 2;
                if (Player.rocketTime < Player.rocketTimeMax)
                    Player.rocketTime = Player.rocketTimeMax;
            }
        }
    }
}
