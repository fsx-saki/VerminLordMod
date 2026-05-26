using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class DianYanBuff : ModBuff
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
            player.GetCritChance(DamageClass.Generic) += 0.15f;
            player.GetModPlayer<DianYanPlayer>().HasDianYan = true;

            if (Main.rand.NextBool(6))
            {
                var headPos = player.position + new Microsoft.Xna.Framework.Vector2(player.width / 2f, 0f);
                var d = Dust.NewDustDirect(headPos, 4, 4, DustID.Electric);
                d.velocity = new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class DianYanPlayer : ModPlayer
    {
        public bool HasDianYan { get; set; }
        private const int RevealRange = 600;

        public override void ResetEffects()
        {
            HasDianYan = false;
        }

        public override void PostUpdate()
        {
            if (!HasDianYan)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Microsoft.Xna.Framework.Vector2.Distance(Player.Center, npc.Center);
                if (dist <= RevealRange)
                {
                    npc.alpha = 0;
                }
            }
        }
    }
}
