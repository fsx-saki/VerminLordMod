using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YuShengBuff : ModBuff
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
            player.GetModPlayer<YuShengPlayer>().HasYuSheng = true;

            player.moveSpeed += 0.20f;
            player.wingTimeMax = 200;
            player.wingTime = player.wingTimeMax;
        }
    }

    public class YuShengPlayer : ModPlayer
    {
        public bool HasYuSheng { get; set; }
        private const float PushRange = 300f;
        private const float PushForce = 4f;

        public override void ResetEffects()
        {
            HasYuSheng = false;
        }

        public override void PostUpdate()
        {
            if (!HasYuSheng || Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= PushRange && dist > 0f)
                {
                    Vector2 pushDir = Vector2.Normalize(npc.Center - Player.Center);
                    float forceFactor = 1f - (dist / PushRange);
                    npc.velocity += pushDir * PushForce * forceFactor;

                    if (Main.rand.NextBool(10))
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Cloud);
                        d.noGravity = true;
                        d.velocity = pushDir * 2f;
                        d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    }
                }
            }

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(30f, PushRange);
                var pos = Player.Center + new Vector2(
                    (float)System.Math.Cos(angle) * dist,
                    (float)System.Math.Sin(angle) * dist
                );
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Cloud);
                d.noGravity = true;
                d.velocity = Vector2.Normalize(pos - Player.Center) * 1f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
