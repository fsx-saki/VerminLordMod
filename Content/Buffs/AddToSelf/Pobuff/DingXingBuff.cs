using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class DingXingBuff : ModBuff
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
            player.GetModPlayer<DingXingPlayer>().HasDingXing = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                d.color = new Color(180, 220, 255);
            }
        }
    }

    public class DingXingPlayer : ModPlayer
    {
        public bool HasDingXing { get; set; }

        public override void ResetEffects()
        {
            HasDingXing = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasDingXing)
                return;

            Player.buffImmune[BuffID.Confused] = true;
            Player.buffImmune[BuffID.Cursed] = true;
            Player.buffImmune[BuffID.Silenced] = true;

            Player.statDefense += (int)(Player.statDefense * 0.05f);
        }
    }
}
