using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HuanJingBuff : ModBuff
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
            player.GetModPlayer<HuanJingPlayer>().HasHuanJing = true;
        }
    }

    public class HuanJingPlayer : ModPlayer
    {
        public bool HasHuanJing { get; set; }
        private const float MissChance = 0.30f;

        public override void ResetEffects()
        {
            HasHuanJing = false;
        }

        public override void PostUpdate()
        {
            if (!HasHuanJing)
                return;

            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(30f, 60f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (HasHuanJing && Main.rand.NextFloat() < MissChance)
            {
                Player.NinjaDodge();
                CombatText.NewText(Player.Hitbox, new Color(200, 150, 255), "闪避!", true);
                return true;
            }
            return false;
        }
    }
}
