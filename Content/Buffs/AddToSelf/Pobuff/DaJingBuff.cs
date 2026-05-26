using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class DaJingBuff : ModBuff
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
            player.GetModPlayer<DaJingPlayer>().HasDaJing = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<SuccessFailureDust>());
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class DaJingPlayer : ModPlayer
    {
        public bool HasDaJing { get; set; }

        public override void ResetEffects()
        {
            HasDaJing = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasDaJing)
            {
                modifiers.FinalDamage *= 1.50f;
                ConsumeDaJing();
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasDaJing)
            {
                modifiers.FinalDamage *= 1.50f;
                ConsumeDaJing();
            }
        }

        private void ConsumeDaJing()
        {
            for (int i = 0; i < Player.buffType.Length; i++)
            {
                if (Player.buffType[i] == ModContent.BuffType<DaJingBuff>())
                {
                    Player.DelBuff(i);
                    break;
                }
            }

            HasDaJing = false;

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, Terraria.ID.DustID.GoldFlame);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 2.0f);
                d.velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
            }
        }
    }
}
