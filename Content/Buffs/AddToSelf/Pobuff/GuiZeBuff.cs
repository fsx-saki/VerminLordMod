using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GuiZeBuff : ModBuff
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
            player.GetModPlayer<GuiZePlayer>().HasGuiZe = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                d.color = new Color(200, 200, 255);
            }
        }
    }

    public class GuiZePlayer : ModPlayer
    {
        public bool HasGuiZe { get; set; }
        private const int RuleRange = 400;

        public override void ResetEffects()
        {
            HasGuiZe = false;
        }

        public override void PostUpdate()
        {
            if (!HasGuiZe)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= RuleRange)
                {
                    npc.GetGlobalNPC<GuiZeNPC>().IsRuleBound = true;
                }
            }
        }
    }

    public class GuiZeNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsRuleBound { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsRuleBound = false;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (IsRuleBound)
            {
                modifiers.FinalDamage *= 0.9f;
            }
        }
    }
}
