using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JuGuShieldBuff : ModBuff
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
            player.immune = true;
            player.immuneTime = 2;
            player.endurance = 1f;
            player.GetModPlayer<JuGuShieldPlayer>().HasJuGuShield = true;

            for (int i = 0; i < 3; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = 1.5f;
            }

            if (player.buffTime[buffIndex] <= 2)
            {
                int exhaustBuff = ModContent.BuffType<JuGuExhaustBuff>();
                player.AddBuff(exhaustBuff, 600);
                Text.ShowTextRed(player, "矩蛊护盾消散...力竭降临！");
            }
        }
    }

    public class JuGuShieldPlayer : ModPlayer
    {
        public bool HasJuGuShield { get; set; }

        public override void ResetEffects()
        {
            HasJuGuShield = false;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (HasJuGuShield)
                return false;
            return base.CanBeHitByNPC(npc, ref cooldownSlot);
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (HasJuGuShield)
                return false;
            return base.CanBeHitByProjectile(proj);
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (HasJuGuShield)
                modifiers.FinalDamage *= 0;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (HasJuGuShield)
                modifiers.FinalDamage *= 0;
        }
    }
}
