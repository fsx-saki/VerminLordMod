using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToEnemy
{
    class GuanGuSealBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.persistentBuff[Type] = true;
            Main.vanityPet[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.SilverCoin);
                d.velocity *= 0.3f;
                d.velocity.Y -= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            npc.defense = npc.defDefense - (int)(npc.defDefense * 0.3f);
        }
    }
}
