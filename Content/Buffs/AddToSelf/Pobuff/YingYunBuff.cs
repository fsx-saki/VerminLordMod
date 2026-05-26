using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YingYunBuff : ModBuff
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
            player.GetModPlayer<YingYunPlayer>().HasYingYun = true;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<LuckDust>());
                d.velocity = new Vector2(0, -1f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class YingYunPlayer : ModPlayer
    {
        public bool HasYingYun { get; set; }

        public override void ResetEffects()
        {
            HasYingYun = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasYingYun && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 2f;
                CombatText.NewText(Player.getRect(), new Color(255, 215, 0), "天运！");
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasYingYun && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 2f;
                CombatText.NewText(Player.getRect(), new Color(255, 215, 0), "天运！");
            }
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (HasYingYun && Main.rand.NextFloat() < 0.10f)
            {
                CombatText.NewText(Player.getRect(), new Color(100, 255, 100), "闪避！");
                return false;
            }
            return true;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (HasYingYun && Main.rand.NextFloat() < 0.10f)
            {
                CombatText.NewText(Player.getRect(), new Color(100, 255, 100), "闪避！");
                return false;
            }
            return true;
        }
    }

    public class YingYunNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            var yingYunPlayer = player.GetModPlayer<YingYunPlayer>();
            if (yingYunPlayer.HasYingYun)
            {
                npc.value *= 1.15f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                if (player.active && !player.dead)
                {
                    var yingYunPlayer = player.GetModPlayer<YingYunPlayer>();
                    if (yingYunPlayer.HasYingYun)
                    {
                        npc.value *= 1.15f;
                    }
                }
            }
        }
    }
}
