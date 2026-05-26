using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class MaoCaoWuBuff : ModBuff
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
            player.GetModPlayer<MaoCaoWuPlayer>().HasMaoCaoWu = true;
            player.statDefense += 3;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.WoodFurniture);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class MaoCaoWuPlayer : ModPlayer
    {
        public bool HasMaoCaoWu { get; set; }
        private const float RepelRadius = 200f;
        private const float RepelStrength = 8f;

        public override void ResetEffects()
        {
            HasMaoCaoWu = false;
        }

        public override void PostUpdate()
        {
            if (!HasMaoCaoWu)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            Vector2 playerCenter = Player.Center;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.townNPC)
                    continue;

                float dist = Vector2.Distance(npc.Center, playerCenter);
                if (dist < RepelRadius && dist > 0f)
                {
                    Vector2 dir = npc.Center - playerCenter;
                    dir.Normalize();
                    float force = RepelStrength * (1f - dist / RepelRadius);
                    npc.velocity += dir * force;
                }
            }
        }
    }
}
