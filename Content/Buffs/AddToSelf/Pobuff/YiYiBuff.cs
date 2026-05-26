using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YiYiBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.GetModPlayer<YiYiPlayer>().HasYiYi = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<LoveDust>());
                d.velocity = new Vector2(0, -1f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class YiYiPlayer : ModPlayer
    {
        public bool HasYiYi { get; set; }
        private const float LifestealPercent = 0.03f;

        public override void ResetEffects()
        {
            HasYiYi = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyLifesteal(damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyLifesteal(damageDone);
        }

        private void ApplyLifesteal(int damageDone)
        {
            if (!HasYiYi) return;

            int healAmount = (int)(damageDone * LifestealPercent);
            if (healAmount > 0)
            {
                Player.statLife += healAmount;
                if (Player.statLife > Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
                Player.HealEffect(healAmount);
            }
        }
    }
}
