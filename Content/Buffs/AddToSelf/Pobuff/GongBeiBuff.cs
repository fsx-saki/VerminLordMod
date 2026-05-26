using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GongBeiBuff : ModBuff
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
            player.statDefense += (int)(player.statDefense * 0.10f);
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.GetModPlayer<GongBeiPlayer>().HasGongBei = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.AncientLight);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class GongBeiPlayer : ModPlayer
    {
        public bool HasGongBei { get; set; }
        private const float WeakenRange = 200f;
        private int _applyTimer;

        public override void ResetEffects()
        {
            HasGongBei = false;
        }

        public override void PostUpdate()
        {
            if (!HasGongBei)
            {
                _applyTimer = 0;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            _applyTimer++;
            if (_applyTimer >= 30)
            {
                _applyTimer = 0;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist <= WeakenRange)
                    {
                        npc.AddBuff(ModContent.BuffType<GongBeiNPC>(), 60);
                    }
                }
            }
        }
    }

    public class GongBeiNPC : ModBuff
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
            npc.damage = (int)(npc.damage * 0.95f);

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.AncientLight);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.5f, 0.8f);
            }
        }
    }
}
