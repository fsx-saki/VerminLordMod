using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FeiShaBuff : ModBuff
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
            player.GetModPlayer<FeiShaPlayer>().HasFeiSha = true;
        }
    }

    public class FeiShaPlayer : ModPlayer
    {
        public bool HasFeiSha { get; set; }
        private int _damageTimer;

        public override void ResetEffects()
        {
            HasFeiSha = false;
        }

        public override void PostUpdate()
        {
            if (!HasFeiSha)
            {
                _damageTimer = 0;
                return;
            }

            _damageTimer++;

            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(100f, 300f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Sandstorm);
                d.noGravity = true;
                d.velocity *= 0.5f;
                d.velocity += new Vector2((float)System.Math.Cos(angle + MathHelper.PiOver2) * 2f, -1f);
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            if (_damageTimer % 60 == 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist <= 300f)
                    {
                        npc.SimpleStrikeNPC(5, 0, false, 0f, DamageClass.Default);

                        npc.AddBuff(ModContent.BuffType<FeiShaNPC>(), 60);

                        for (int j = 0; j < 3; j++)
                        {
                            var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Sandstorm);
                            d.noGravity = true;
                            d.velocity *= 0.3f;
                            d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }
                    }
                }
            }
        }
    }

    public class FeiShaNPC : ModBuff
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
            npc.damage = (int)(npc.defDamage * 0.9f);

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Sandstorm);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
