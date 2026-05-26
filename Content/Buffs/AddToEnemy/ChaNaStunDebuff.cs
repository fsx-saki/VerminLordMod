using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToEnemy
{
    public class ChaNaStunDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.persistentBuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity = Vector2.Zero;
            npc.frameCounter = 0;
            npc.ai[0] = 0;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.15f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
