using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class WanXiangBuff : ModBuff
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
            player.GetModPlayer<WanXiangPlayer>().HasWanXiang = true;

            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.statDefense += (int)(player.statDefense * 0.10f);

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.PurpleTorch);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class WanXiangPlayer : ModPlayer
    {
        public bool HasWanXiang { get; set; }

        public override void ResetEffects()
        {
            HasWanXiang = false;
        }
    }

    public class WanXiangNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasWanXiangShrink { get; set; }

        public override void ResetEffects(NPC npc)
        {
            HasWanXiangShrink = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasWanXiangShrink)
            {
                npc.scale = 0.7f;
            }
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (HasWanXiangShrink)
            {
                modifiers.FinalDamage *= 0.75f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (HasWanXiangShrink)
            {
                modifiers.DefenseEffectiveness *= 0.85f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (HasWanXiangShrink)
            {
                modifiers.DefenseEffectiveness *= 0.85f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (HasWanXiangShrink)
            {
                drawColor = Color.Lerp(drawColor, Color.Purple, 0.3f);

                if (Main.rand.NextBool(6))
                {
                    var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch);
                    d.noGravity = true;
                    d.velocity *= 0.2f;
                    d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                }
            }
        }
    }

    public class WanXiangShrinkDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<WanXiangNPC>().HasWanXiangShrink = true;
        }
    }
}
