using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToEnemy
{
    public class ZhuMoDebuff : ModBuff
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
            npc.GetGlobalNPC<ZhuMoNPC>().ZhuMoMarked = true;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.CrimsonTorch);
                d.velocity *= 0.15f;
                d.velocity.Y -= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                d.color = new Color(255, 50, 50);
            }
        }
    }

    public class ZhuMoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool ZhuMoMarked { get; set; }

        public override void ResetEffects(NPC npc)
        {
            ZhuMoMarked = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (ZhuMoMarked)
            {
                modifiers.FinalDamage *= 1.20f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (ZhuMoMarked)
            {
                modifiers.FinalDamage *= 1.20f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (ZhuMoMarked)
            {
                drawColor = Color.Lerp(drawColor, new Color(255, 80, 80), 0.3f);
            }
        }
    }
}
