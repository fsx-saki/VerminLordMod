using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class LanQueBuff : ModBuff
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
            player.GetModPlayer<LanQuePlayer>().HasLanQue = true;
        }
    }

    public class LanQuePlayer : ModPlayer
    {
        public bool HasLanQue { get; set; }
        private const float MagnetRadius = 500f;
        private const float MagnetStrength = 8f;

        public override void ResetEffects()
        {
            HasLanQue = false;
        }

        public override void PostUpdate()
        {
            if (!HasLanQue)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            Vector2 playerCenter = Player.Center;

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.noGrabDelay > 0)
                    continue;

                float dist = Vector2.Distance(item.Center, playerCenter);
                if (dist < MagnetRadius && dist > 0f)
                {
                    Vector2 dir = playerCenter - item.Center;
                    dir.Normalize();
                    float force = MagnetStrength * (1f - dist / MagnetRadius);
                    item.velocity += dir * force;

                    if (Main.rand.NextBool(10))
                    {
                        var d = Dust.NewDustDirect(item.position, item.width, item.height, DustID.MagicMirror);
                        d.noGravity = true;
                        d.velocity *= 0.2f;
                        d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                    }
                }
            }

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(50f, 200f);
                var pos = playerCenter + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
