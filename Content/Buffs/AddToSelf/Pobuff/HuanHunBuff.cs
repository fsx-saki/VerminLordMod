using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HuanHunBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetModPlayer<HuanHunPlayer>().HasHuanHun = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Ghost);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.1f);
            }
        }
    }

    public class HuanHunPlayer : ModPlayer
    {
        public bool HasHuanHun { get; set; }

        public override void ResetEffects()
        {
            HasHuanHun = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasHuanHun && Main.rand.NextFloat() < 0.15f)
            {
                modifiers.FinalDamage *= 0f;
                CombatText.NewText(Player.Hitbox, new Color(150, 130, 255), "幻魂闪避!");
            }
        }
    }
}
