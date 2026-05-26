using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YiHouBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.20f;

            Lighting.AddLight(player.Center, 1.0f, 0.85f, 0.3f);

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<PersonDust>());
                d.velocity = new Vector2(0, -1.5f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class YiHouPlayer : ModPlayer
    {
        public bool HasLegacy { get; set; }
        public bool LegacyActive { get; set; }
        private bool _lastFrameLowHp = false;

        public override void ResetEffects()
        {
            LegacyActive = false;
        }

        public override void PostUpdate()
        {
            if (!HasLegacy) return;

            float hpThreshold = Player.statLifeMax2 * 0.25f;
            bool isLowHp = Player.statLife <= hpThreshold;

            if (isLowHp && !_lastFrameLowHp)
            {
                ActivateLegacy();
            }

            _lastFrameLowHp = isLowHp;
        }

        private void ActivateLegacy()
        {
            HasLegacy = false;
            LegacyActive = true;

            Player.statLife += 50;
            Player.HealEffect(50);
            Player.AddBuff(ModContent.BuffType<YiHouBuff>(), 300);

            CombatText.NewText(Player.getRect(), new Color(255, 215, 0), "遗后之力激活！");

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<PersonDust>());
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["HasLegacy"] = HasLegacy;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            HasLegacy = tag.GetBool("HasLegacy");
        }
    }
}
