using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FengHuoBuff : ModBuff
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
            player.GetModPlayer<FengHuoPlayer>().HasFengHuo = true;
        }
    }

    public class FengHuoPlayer : ModPlayer
    {
        public bool HasFengHuo { get; set; }
        private const float AlertRange = 800f;
        private int _alertCooldown;

        public override void ResetEffects()
        {
            HasFengHuo = false;
        }

        public override void PostUpdate()
        {
            if (!HasFengHuo)
            {
                _alertCooldown = 0;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            Player.AddBuff(BuffID.Hunter, 2);

            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Torch);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.velocity.Y -= 1f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            if (_alertCooldown > 0)
            {
                _alertCooldown--;
                return;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= AlertRange)
                {
                    CombatText.NewText(Player.Hitbox, Color.OrangeRed, "⚠ 敌袭!", true);
                    _alertCooldown = 60;
                    break;
                }
            }
        }
    }
}
