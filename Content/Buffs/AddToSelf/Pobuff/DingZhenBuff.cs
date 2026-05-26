using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class DingZhenBuff : ModBuff
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
            player.GetModPlayer<DingZhenPlayer>().HasDingZhen = true;
        }
    }

    public class DingZhenPlayer : ModPlayer
    {
        public bool HasDingZhen { get; set; }

        public override void ResetEffects()
        {
            HasDingZhen = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasDingZhen)
                return;

            Player.statDefense += 20;
            Player.noKnockback = true;
            Player.velocity.X = 0f;
            Player.velocity.Y = 0f;
            Player.gravDir = 1f;
            Player.jump = 0;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.WoodFurniture);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }
}
