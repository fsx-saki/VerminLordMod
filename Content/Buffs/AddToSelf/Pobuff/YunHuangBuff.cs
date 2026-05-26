using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YunHuangBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetModPlayer<YunHuangPlayer>().HasYunHuang = true;

            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Torch);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.velocity.Y -= 0.8f;
                d.scale = Main.rand.NextFloat(0.6f, 1.2f);
            }
        }
    }

    public class YunHuangPlayer : ModPlayer
    {
        public bool HasYunHuang { get; set; }
        private bool _hasRevivedThisBuff;

        public override void ResetEffects()
        {
            HasYunHuang = false;
        }

        public override void UpdateDead()
        {
            if (!HasYunHuang || _hasRevivedThisBuff)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            _hasRevivedThisBuff = true;

            Player.statLife = (int)(Player.statLifeMax2 * 0.30f);
            Player.dead = false;
            Player.respawnTimer = 0;

            for (int i = 0; i < 40; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Torch,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 0, default, 1.8f);
                d.noGravity = true;
            }

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.FireworkFountain_Red,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -1f), 0, default, 1.5f);
                d.noGravity = true;
            }

            CombatText.NewText(Player.Hitbox, new Color(255, 100, 50), "云凰涅槃，浴火重生！");
        }

        public override void SaveData(TagCompound tag)
        {
            tag["HasRevivedThisBuff"] = _hasRevivedThisBuff;
        }

        public override void LoadData(TagCompound tag)
        {
            _hasRevivedThisBuff = tag.GetBool("HasRevivedThisBuff");
        }
    }
}
