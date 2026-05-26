using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RenPiBuff : ModBuff
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
            player.GetModPlayer<RenPiPlayer>().HasRenPi = true;
            player.aggro -= 750;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class RenPiPlayer : ModPlayer
    {
        public bool HasRenPi { get; set; }

        public override void ResetEffects()
        {
            HasRenPi = false;
        }

        public override void PostUpdate()
        {
            if (!HasRenPi)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Microsoft.Xna.Framework.Vector2.Distance(Player.Center, npc.Center);
                float aggroRange = npc.lifeMax > 0 ? 600f : 400f;
                float reducedRange = aggroRange * 0.7f;

                if (dist > reducedRange && dist <= aggroRange)
                {
                    npc.GetGlobalNPC<RenPiNPC>().AggroReduced = true;
                }
            }
        }
    }

    public class RenPiNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool AggroReduced { get; set; }

        public override void ResetEffects(NPC npc)
        {
            AggroReduced = false;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (AggroReduced)
            {
                modifiers.FinalDamage *= 0.7f;
            }
        }
    }
}
