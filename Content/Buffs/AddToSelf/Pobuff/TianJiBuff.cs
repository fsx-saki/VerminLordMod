using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TianJiBuff : ModBuff
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
            player.GetModPlayer<TianJiPlayer>().HasTianJi = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                d.color = new Color(200, 220, 255);
            }
        }
    }

    public class TianJiPlayer : ModPlayer
    {
        public bool HasTianJi { get; set; }
        private const float DodgeChance = 0.05f;
        private const int RevealRange = 600;

        public override void ResetEffects()
        {
            HasTianJi = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTianJi)
                return;

            Player.GetCritChance(DamageClass.Generic) += 10f;
        }

        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            if (!HasTianJi)
                return false;

            if (Main.rand.NextFloat() < DodgeChance)
            {
                return true;
            }

            return false;
        }

        public override void PostUpdate()
        {
            if (!HasTianJi || Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= RevealRange)
                {
                    npc.GetGlobalNPC<TianJiNPC>().IsRevealed = true;
                }
            }
        }
    }

    public class TianJiNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsRevealed { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsRevealed = false;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (IsRevealed)
            {
                drawColor = Color.Lerp(drawColor, Color.Gold, 0.3f);
            }
        }
    }
}
