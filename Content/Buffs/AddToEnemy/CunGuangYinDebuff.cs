using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToEnemy
{
    public class CunGuangYinDebuff : ModBuff
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
            npc.velocity *= 0.7f;

            npc.ai[0] = npc.ai[0];

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.MagicMirror);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.velocity *= 0.2f;
                d.velocity.Y -= 0.5f;
            }
        }
    }
}
