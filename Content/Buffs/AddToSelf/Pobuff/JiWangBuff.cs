using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JiWangBuff : ModBuff
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
            player.GetCritChance(DamageClass.Generic) += 10f;
            player.GetModPlayer<JiWangPlayer>().HasJiWang = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class JiWangPlayer : ModPlayer
    {
        public bool HasJiWang { get; set; }

        public override void ResetEffects()
        {
            HasJiWang = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasJiWang && Main.rand.NextFloat() < 0.05f)
            {
                modifiers.FinalDamage *= 0f;
                CombatText.NewText(Player.Hitbox, new Color(100, 200, 255), "洞察闪避!");
            }
        }

        public override void PostUpdate()
        {
            if (!HasJiWang || Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= 600f)
                {
                    npc.GetGlobalNPC<JiWangNPC>().IsRevealed = true;
                }
            }
        }
    }

    public class JiWangNPC : GlobalNPC
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
                drawColor = Color.Lerp(drawColor, Color.Cyan, 0.15f);
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsRevealed)
            {
                modifiers.CritDamage *= 1.05f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (IsRevealed)
            {
                modifiers.CritDamage *= 1.05f;
            }
        }
    }
}
