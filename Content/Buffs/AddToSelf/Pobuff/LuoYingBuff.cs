using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class LuoYingBuff : ModBuff
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
            player.GetModPlayer<LuoYingPlayer>().HasLuoYing = true;
        }
    }

    public class LuoYingPlayer : ModPlayer
    {
        public bool HasLuoYing { get; set; }
        private int _damageTimer;
        private const float StormRadius = 300f;

        public override void ResetEffects()
        {
            HasLuoYing = false;
        }

        public override void PostUpdate()
        {
            if (!HasLuoYing)
            {
                _damageTimer = 0;
                return;
            }

            _damageTimer++;

            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(80f, StormRadius);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                int dustType = Main.rand.NextBool() ? 261 : DustID.ChlorophyteWeapon;
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, dustType);
                d.noGravity = true;
                d.velocity += new Vector2((float)System.Math.Cos(angle + MathHelper.PiOver2) * 1.5f, -0.5f);
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }

            if (_damageTimer % 60 == 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist <= StormRadius)
                    {
                        npc.SimpleStrikeNPC(8, 0, false, 0f, DamageClass.Default);

                        npc.AddBuff(ModContent.BuffType<LuoYingNPC>(), 120);

                        for (int j = 0; j < 3; j++)
                        {
                            int dustType = Main.rand.NextBool() ? 261 : DustID.ChlorophyteWeapon;
                            var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType);
                            d.noGravity = true;
                            d.velocity *= 0.3f;
                            d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }
                    }
                }
            }
        }
    }

    public class LuoYingNPC : ModBuff
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
            npc.defense = (int)(npc.defense * 0.9f);

            if (Main.rand.NextBool(4))
            {
                int dustType = Main.rand.NextBool() ? 261 : DustID.ChlorophyteWeapon;
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
