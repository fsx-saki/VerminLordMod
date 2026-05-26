using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HaiJiaoBuff : ModBuff
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
            player.GetModPlayer<HaiJiaoPlayer>().HasHaiJiao = true;
            player.gills = true;
            player.statDefense += 10;
        }
    }

    public class HaiJiaoPlayer : ModPlayer
    {
        public bool HasHaiJiao { get; set; }
        private const float DomeRadius = 120f;
        private const int DomeDamage = 10;
        private const float KnockbackStrength = 8f;
        private int _damageTimer;

        public override void ResetEffects()
        {
            HasHaiJiao = false;
        }

        public override void PostUpdate()
        {
            if (!HasHaiJiao)
            {
                _damageTimer = 0;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * DomeRadius, (float)System.Math.Sin(angle) * DomeRadius);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Water);
                d.noGravity = true;
                d.velocity = (Player.Center - pos).SafeNormalize(Vector2.Zero) * 0.5f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            _damageTimer++;
            if (_damageTimer % 10 != 0)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.townNPC)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= DomeRadius && dist > 0f)
                {
                    npc.SimpleStrikeNPC(DomeDamage, 0, false, KnockbackStrength, DamageClass.Default);

                    Vector2 dir = npc.Center - Player.Center;
                    dir.Normalize();
                    npc.velocity += dir * KnockbackStrength;

                    for (int j = 0; j < 5; j++)
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Water);
                        d.noGravity = true;
                        d.velocity = dir * 2f;
                        d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                    }
                }
            }
        }
    }
}
