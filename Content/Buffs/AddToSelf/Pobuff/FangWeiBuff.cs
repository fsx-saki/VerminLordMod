using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FangWeiBuff : ModBuff
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
            player.GetModPlayer<FangWeiPlayer>().HasFangWei = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.IceTorch);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.8f);
            }
        }
    }

    public class FangWeiPlayer : ModPlayer
    {
        public bool HasFangWei { get; set; }
        private int _pulseTimer;

        public override void ResetEffects()
        {
            HasFangWei = false;
        }

        public override void PostUpdate()
        {
            if (!HasFangWei)
                return;

            _pulseTimer++;
            if (_pulseTimer % 10 != 0)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist > 1500f)
                    continue;

                if (npc.alpha > 0 || npc.hide)
                {
                    npc.alpha = 0;
                    npc.hide = false;
                    npc.netUpdate = true;

                    for (int j = 0; j < 3; j++)
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.IceTorch);
                        d.noGravity = true;
                        d.velocity *= 0.5f;
                        d.scale = 1.2f;
                    }
                }
            }
        }
    }
}
