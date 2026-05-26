using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class XingSuBuff : ModBuff
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
            player.GetModPlayer<XingSuPlayer>().HasXingSu = true;

            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetDamage(DamageClass.Generic) += 0.10f;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.ShimmerSpark);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class XingSuPlayer : ModPlayer
    {
        public bool HasXingSu { get; set; }
        public Vector2 CastPosition { get; set; } = Vector2.Zero;
        private int _starTimer;

        public override void ResetEffects()
        {
            HasXingSu = false;
        }

        public override void PostUpdate()
        {
            if (!HasXingSu || Player.whoAmI != Main.myPlayer)
            {
                _starTimer = 0;
                return;
            }

            _starTimer++;

            if (_starTimer % 60 == 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Player.Center, npc.Center);
                    if (dist <= 400f)
                    {
                        npc.SimpleStrikeNPC(30, 0, false, 0f, DamageClass.Default);

                        for (int j = 0; j < 5; j++)
                        {
                            var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.ShimmerSpark);
                            d.noGravity = true;
                            d.velocity *= 0.3f;
                            d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }
                    }
                }
            }

            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(50f, 400f);
                var pos = Player.Center + new Vector2(
                    (float)System.Math.Cos(angle) * dist,
                    (float)System.Math.Sin(angle) * dist
                );
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.ShimmerSpark);
                d.noGravity = true;
                d.velocity = new Vector2(0f, 2f);
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
