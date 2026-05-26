using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TianWangBuff : ModBuff
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
            player.GetModPlayer<TianWangPlayer>().HasTianWangBuff = true;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
                d.color = new Color(255, 215, 0);
            }
        }
    }

    public class TianWangPlayer : ModPlayer
    {
        public bool HasTianWangBuff { get; set; }
        private const int AuraRange = 300;

        public override void ResetEffects()
        {
            HasTianWangBuff = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTianWangBuff)
                return;

            Player.GetDamage(DamageClass.Generic) += 0.15f;
            Player.statDefense += 10;
        }

        public override void PostUpdate()
        {
            if (!HasTianWangBuff || Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= AuraRange)
                {
                    npc.GetGlobalNPC<TianWangNPC>().IsSuppressed = true;
                }
            }
        }
    }

    public class TianWangNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsSuppressed { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsSuppressed = false;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (IsSuppressed)
                modifiers.FinalDamage *= 0.90f;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (IsSuppressed)
            {
                drawColor = Color.Lerp(drawColor, Color.Gold, 0.25f);
            }
        }
    }
}
