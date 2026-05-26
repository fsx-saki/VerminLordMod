using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GuanWangBuff : ModBuff
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
            player.GetCritChance(DamageClass.Generic) += 8f;
            player.GetModPlayer<GuanWangPlayer>().HasGuanWang = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class GuanWangPlayer : ModPlayer
    {
        public bool HasGuanWang { get; set; }

        public override void ResetEffects()
        {
            HasGuanWang = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasGuanWang && Main.rand.NextFloat() < 0.05f)
            {
                modifiers.FinalDamage *= 0f;
                CombatText.NewText(Player.Hitbox, new Color(100, 200, 255), "闪避!");
            }
        }
    }
}
