using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class MengDieBuff : ModBuff
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
            player.GetModPlayer<MengDiePlayer>().HasMengDie = true;
        }
    }

    public class MengDiePlayer : ModPlayer
    {
        public bool HasMengDie { get; set; }
        private const float DodgeChance = 0.10f;

        public override void ResetEffects()
        {
            HasMengDie = false;
        }

        public override void PostUpdate()
        {
            if (!HasMengDie)
                return;

            Player.GetDamage(DamageClass.Generic) += 0.15f;
            Player.GetCritChance(DamageClass.Generic) += 8f;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<DreamDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (HasMengDie && Main.rand.NextFloat() < DodgeChance)
            {
                Player.NinjaDodge();
                CombatText.NewText(Player.Hitbox, new Color(200, 150, 255), "蝶梦!", true);
                return true;
            }
            return false;
        }
    }
}
