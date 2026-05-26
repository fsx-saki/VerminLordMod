using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HaoZhuBuff : ModBuff
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
            player.GetModPlayer<HaoZhuPlayer>().HasHaoZhu = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(15f, 30f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Iron);
                d.noGravity = true;
                d.velocity *= 0.1f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class HaoZhuPlayer : ModPlayer
    {
        public bool HasHaoZhu { get; set; }
        private const float ThornsPercent = 0.10f;

        public override void ResetEffects()
        {
            HasHaoZhu = false;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (!HasHaoZhu || !npc.active)
                return;

            int thornsDamage = (int)(hurtInfo.SourceDamage * ThornsPercent);
            if (thornsDamage < 1)
                thornsDamage = 1;

            npc.SimpleStrikeNPC(thornsDamage, Player.direction, false, 0f, DamageClass.Default);

            for (int i = 0; i < 3; i++)
            {
                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Iron);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }
}
