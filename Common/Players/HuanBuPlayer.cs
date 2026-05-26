using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class HuanBuPlayer : ModPlayer
    {
        public bool HuanBuActive { get; set; }

        public override void ResetEffects()
        {
            HuanBuActive = false;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (HuanBuActive && npc.active)
            {
                npc.AddBuff(BuffID.Chilled, 120);
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (HuanBuActive)
            {
                NPC ownerNpc = null;
                if (proj.owner >= 0 && proj.owner < Main.maxNPCs)
                {
                    ownerNpc = Main.npc[proj.owner];
                }

                if (ownerNpc != null && ownerNpc.active)
                {
                    ownerNpc.AddBuff(BuffID.Chilled, 120);
                }
            }
        }
    }
}
